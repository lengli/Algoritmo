using System;
using System.Collections.Generic;
using System.Linq;
using Functions;
using AlgoResult;
using LocalCore.HillClimbing;
using LocalCore.LSChains;
using LocalCore.BuscaMutacao;

namespace AlgoCore
{
    public abstract class RotinaAlgo : IFunctions
    {
        #region IFunctions

        public FuncAptidao FuncApt { get; set; }
        public FuncRepopRestricao FuncRestr { get; set; }

        #endregion

        // geração atual
        protected int g;

        protected int _avaliacoes;
        protected int _maxAval;
        protected int _geracoesMAx;
        protected int _tamanhoPop;
        protected Bound _min;
        protected Bound _max;
        protected int _nAtributos;
        protected double _distTabu;
        protected int _gerSemMelhorar;
        protected double _margemComp;
        protected List<IndividuoBin> individuosTabu = new List<IndividuoBin>();
        protected AlgoInfo _agInfo;
        protected ListAptidao _gs;
        protected ListAptidao _hs;
        private List<double> _gmax = new List<double>();
        private List<double> _hmax = new List<double>();
        protected FuncValidarRestricao _validarRestricao;
        protected FuncValidarFronteira _validarFronteira;
        private List<IndividuoBin> populacao;
        private int _popValida;
        private double _popMax;
        private double _popMin;

        public double MinGlogal { get; set; }
        public double ErroAceitavel { get; set; }

        protected RotinaAlgo(FuncAptidao aptidao, FuncRepopRestricao restricao,
            ListAptidao gs, ListAptidao hs,
            FuncValidarRestricao validarRestricao, FuncValidarFronteira validarFronteira)
        {
            FuncApt = aptidao;
            FuncRestr = restricao;
            _gs = gs;
            _hs = hs;
            _validarRestricao = validarRestricao;
            _validarFronteira = validarFronteira;
            g = 0;
        }

        protected double FuncaoAptidaoVirtual(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            double f = FuncApt(atributos);
            return f;
        }

        double piorAval = double.MinValue;
        protected double FuncaoAptidao(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            double f = FuncApt(atributos);
            if (_validarRestricao == null || _validarRestricao(atributos))
            {
                // penalidade: deve garantir que os individuos invalidos 
                // sejam piores do que qualquer individuo valido

                if (f > piorAval) piorAval = f;
                return f;
            }

            List<double> gValues = _gs == null ? new List<double>() :
                _gs().Select(gi => Math.Max(0, gi(atributos))).ToList();

            List<double> hValues = _hs == null ? new List<double>() :
                _hs().Select(hi => Math.Abs(hi(atributos))).ToList();

            // v(X)
            double v = 1;

            // Gmax
            if (_gs != null)
                for (int i = 0; i < _gs().Count; i++)
                {
                    if (_gmax.Count < i + 1) _gmax.Add(0);
                    if (_gmax[i] < gValues[i]) _gmax[i] = gValues[i];
                    if (_gmax[i] == 0) continue;
                    v += gValues[i] / _gmax[i];
                }
            if (_hs != null)
                for (int i = 0; i < _hs().Count; i++)
                {
                    if (_hmax.Count < i + 1) _hmax.Add(0);
                    if (_hmax[i] < hValues[i]) _hmax[i] = hValues[i];
                    if (_hmax[i] == 0) continue;
                    v += hValues[i] / _hmax[i];
                }

            double gSum = _gmax.Sum(g => g != 0 ? 1 / g : 0);
            double hSum = _hmax.Sum(h => h != 0 ? 1 / h : 0);
            if (gSum + hSum != 0)
                v /= (gSum + hSum);

            double rf = (double)_popValida / _tamanhoPop;

            double fc = _popMax == _popMin ? 1 : (f - _popMin) / (_popMax - _popMin);

            double d = Math.Sqrt(fc * fc + v * v);

            double p = (1 - rf) * v + rf * fc;

            return d + p + piorAval;
        }

        public AlgoInfo Rodar(int geracoesMAx, int tamanhoPop, Bound min,
            Bound max, int nAtributos, int precisao, //double erroAceitavel, double minGlobal,
            int maxAvaliacoes, bool usarTabu, double distTabu, int nMaxRepop, int gerSemMelhorar, double margemComp, bool tabuNaPop, ParametrosHillClimbing hillClimbing = null,
            ParametrosLSChains lsChains = null, int qtdMutLocal = 0)
        {
            IndividuoBin.Precisao = precisao;
            _maxAval = maxAvaliacoes;
            _geracoesMAx = geracoesMAx == 0 ? int.MaxValue : geracoesMAx;
            _tamanhoPop = tamanhoPop;
            _min = min;
            _max = max;
            _nAtributos = nAtributos;
            _distTabu = distTabu;
            _margemComp = margemComp;
            _gerSemMelhorar = gerSemMelhorar;

            _agInfo = new AlgoInfo();
            if (geracoesMAx > -1)
                populacao = PopulacaoAleatoria(tamanhoPop, min, max, nAtributos, precisao, lsChains != null);

            InicializarAlgoritmo(populacao);

            for (g = 0; g < geracoesMAx || geracoesMAx == 0; g++)
            {
                // epidemia - mata 95% da população
                //if (rand.NextDouble() <= 0.1)
                //    populacao = populacao.Take((int)(tamanhoPop * .05)).ToList();

                // n. de individuos validos, se for o caso
                if (_validarRestricao != null)
                {
                    _popValida = populacao.Count(ind => _validarRestricao(ind.Atributos));
                    _popMax = populacao.Max(ind => ind.Aptidao);
                    _popMin = populacao.Min(ind => ind.Aptidao);
                }

                // Limite do nr. de avaliações
                if ((_maxAval != 0 && _avaliacoes >= _maxAval)
                    //|| (_agInfo.MelhorIndividuo != null && Math.Abs(_agInfo.MelhorIndividuo.Aptidao - MinGlogal) < ErroAceitavel)
                    ) break;

                //if (erroAceitavel > 0 && Math.Abs(_agInfo.MelhorIndividuo.Aptidao - minGlobal) < erroAceitavel) break;

                // avaliará individuos novos
                for (int i =0; i<populacao.Count;i++)
                {
                     IndividuoBin individuo = populacao[i];
                    if (usarTabu)
                    {
                        foreach (IndividuoBin inTabu in individuosTabu)
                        {
                            if(inTabu.Proximo(distTabu,individuo))
                            {
                                List<IndividuoBin> inds = IndividuoBin.GerarPopulacao<IndividuoBin>(1, _min, _max,
                                    _nAtributos, precisao, individuosTabu, distTabu);
                                if (inds.Count > 0) populacao[i] = inds[0];
                            }
                        }
                    }

                    if(individuo.Aptidao == double.MaxValue)
                        individuo.Aptidao = FuncaoAptidao(individuo.Atributos);
                }

                ExecutarAlgoritmo(populacao);

                populacao = populacao.OrderBy(ind => ind.Aptidao).Take(tamanhoPop).ToList();

                BuscaLocal(populacao, max, min, precisao, qtdMutLocal, hillClimbing, lsChains);

                populacao = populacao.OrderBy(ind => ind.Aptidao).Take(tamanhoPop).ToList();

                int ultimaRepop = 0;
                int nRepopulacao = 0;
                //criterio de repopulação, evitar mínimos locais devido ao elitismo
                if (usarTabu)
                {
                    if (CriterioRepopular(_agInfo, ultimaRepop))
                    {
                        // criterio de parada: repopular n vezes
                        if (nRepopulacao >= nMaxRepop) break;
                        nRepopulacao++;

                        individuosTabu.Add(populacao[0].Clone());
                        if (individuosTabu.Count > 5)
                            individuosTabu = individuosTabu.OrderBy(c => c.Aptidao).Take(5).ToList();

                        populacao = PopulacaoAleatoria(tamanhoPop - (tabuNaPop ? individuosTabu.Count : 0), min, max, nAtributos, precisao,
                            lsChains != null, individuosTabu);

                        // adicionando os individuos tabus
                        if (tabuNaPop)
                            populacao.AddRange(individuosTabu);

                        ultimaRepop = g;
                    }
                }
                else
                {
                    // criterio de parada
                    if (CriterioDeParada(_agInfo)) break;
                }

                _agInfo.AdicionarInfo(populacao, g, _avaliacoes);

                // se todos os individuos convergirem 
                //int gr = populacao.GroupBy(ind => ind.Aptidao).Count();
                //if (g > 400 && gr == 1) break;
                Console.WriteLine("Ger:" + g + " / Apt:" + _agInfo.MelhorIndividuo.Aptidao + " / aval:" + _avaliacoes);
            }
            return _agInfo;
        }

        List<int> falhas;
        List<double> ultAptidao;
        List<double> mutExplt;
        List<double> mutExplr;
        // método destinado a inserir algorítmos meméticos
        // considerando que a lista populacao já está ordenada por aptidao
        private void BuscaLocal(List<IndividuoBin> populacao,
            Bound max, Bound min, int precisao, // parametros do algoritmo
            int qtdMutLocal, ParametrosHillClimbing hillClimbing, ParametrosLSChains lsChains) // parametros das buscas locais
        {
            // busca local
            if (mutExplr == null || mutExplt == null)
            {
                mutExplt = new List<double>();
                mutExplr = new List<double>();
                falhas = new List<int>();
                ultAptidao = new List<double>();
                for (int i = 0; i < _nAtributos; i++)
                {
                    mutExplr.Add((max(0) - min(0)) / 8);
                    mutExplt.Add((max(0) - min(0)) / 15);
                    falhas.Add(0);
                    ultAptidao.Add(double.MaxValue);
                }
            }

            for (int z = 0; z < qtdMutLocal; z++)
            {
                for (int j = 0; j < _nAtributos; j++)
                {
                    IndividuoBin ind1 = populacao[z].Clone();
                    if (falhas[j] >= 4)
                    {
                        mutExplt[j] /= 1 + new Random(AlgoUtil.GetSeed()).NextDouble();
                        mutExplr[j] /= 1 + new Random(AlgoUtil.GetSeed()).NextDouble();
                        if (mutExplt[j] < Math.Pow(10, -(precisao + 2)))
                            mutExplt[j] = Math.Pow(10, -(precisao - 2));
                        if (mutExplr[j] < (max(0) - min(0)) / 200)
                            mutExplr[j] = (max(0) - min(0)) / 100;
                        falhas[j] = 0;
                        ultAptidao[j] = double.MaxValue;
                    }

                    double explt = mutExplt[j];
                    double explr = mutExplr[j];

                    if (new Random(AlgoUtil.GetSeed()).NextDouble() < 0.5)
                    {
                        explt *= -1;
                        explr *= -1;
                    }

                    List<IndividuoBin> tempInds = new List<IndividuoBin>();
                    for (int k = 1; k < 4; k++)
                    {
                        ind1 = MutacaoReal.Executar(ind1, FuncaoAptidao, FuncaoAptidaoVirtual, explt, _validarRestricao, j);
                        tempInds.Add(ind1.Clone());
                        ind1 = MutacaoReal.Executar(ind1, FuncaoAptidao, FuncaoAptidaoVirtual, explr, _validarRestricao, j);
                        tempInds.Add(ind1.Clone());
                    }

                    int melhor = 0;
                    double melhorApt = double.MaxValue;
                    for (int m = 0; m < tempInds.Count; m++)
                    {
                        if (tempInds[m].Aptidao < melhorApt)
                        {
                            melhorApt = tempInds[m].Aptidao;
                            melhor = m;
                        }
                    }

                    if (tempInds[melhor].Aptidao < populacao[z].Aptidao)
                    {
                        ultAptidao[j] = tempInds[melhor].Aptidao;
                        populacao[z] = tempInds[melhor];
                    }
                    else
                    {
                        falhas[j]++;
                        populacao.Add(tempInds[melhor]);
                    }

                    if (falhas[j] != 0) continue;

                    //Console.WriteLine("Mut-Exploração:" + mutExplr);
                    //Console.WriteLine("Mut-Explotação:" + mutExplt);
                    //Console.WriteLine("Apt:" + populacao[0].Aptidao);
                    //Console.WriteLine();
                }
            }


            // busca por hill climbing
            if (hillClimbing != null)
            {
                for (int i = 0; i < hillClimbing.NIndividuos; i++)
                {
                    IndividuoBin indivTemp = new RotinaHillClimbing(FuncaoAptidao).Rodar(populacao[i], hillClimbing);
                    if (indivTemp != null)
                        populacao[i] = indivTemp;
                }
            }

            if (lsChains != null)
            {
                // pega o melhor individuo sempre
                IndividuoLSChains individuoSelecionado = populacao[0] as IndividuoLSChains;
                if (individuoSelecionado != null)
                {
                    IndividuoBin indivTemp = new RotinaLSChains(FuncaoAptidao).Rodar(individuoSelecionado, lsChains);
                    if (indivTemp != null)
                        populacao[0] = indivTemp;
                }
            }
        }

        protected abstract bool CriterioDeParada(AlgoInfo agInfo);
        protected abstract void InicializarAlgoritmo(List<IndividuoBin> populacao);
        public abstract void ExecutarAlgoritmo(List<IndividuoBin> populacao);

        private bool CriterioRepopular(AlgoInfo agInfo, int ultimaRepop)
        {
            // n gerações sem melhoras do melhor
            if (agInfo.Informacoes.Count == 0) return false;
            double melhorAptidao = agInfo.Informacoes.Last().MelhorAptidao;
            int contarMelhor = 1;
            for (int j = agInfo.Informacoes.Count - 2; j >= ultimaRepop; j--)
            {
                if (agInfo.Informacoes[j].MelhorAptidao <= melhorAptidao + _margemComp) contarMelhor++;
                else break;
            }

            if (contarMelhor >= _gerSemMelhorar) return true;
            return false;
        }

        private List<IndividuoBin> PopulacaoAleatoria<T>(int nPop, Bound min, Bound max, int nAtributos, int precisao,
            List<IndividuoBin> tabu = null) where T : IndividuoBin, new()
        {
            List<T> _populacao;

            if (FuncRestr == null)
                _populacao = IndividuoBin.GerarPopulacao<T>(nPop, min, max, nAtributos, precisao, tabu, _distTabu);
            else
            {
                IndividuoBin.Maximo = max;
                IndividuoBin.Minimo = min;
                _populacao = FuncRestr(nPop).Select(atrs => new T { Atributos = atrs }).ToList();
            }

            // n. de individuos validos, se for o caso
            if (_validarRestricao != null)
            {
                _popValida = _populacao.Count(ind => _validarRestricao(ind.Atributos));
                _popMax = _populacao.Max(ind => ind.Aptidao);
                _popMin = _populacao.Min(ind => ind.Aptidao);
            }

            // avaliação da população inicial
            foreach (T individuo in _populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

            List<IndividuoBin> populacaoRetorno = new List<IndividuoBin>();
            foreach (T ind in _populacao)
                populacaoRetorno.Add(ind);
            _populacao.Clear();

            return populacaoRetorno;
        }

        private List<IndividuoBin> PopulacaoAleatoria(int nPop, Bound min, Bound max, int nAtributos, int precisao,
            bool isLSChains, List<IndividuoBin> tabu = null)
        {
            if (isLSChains)
                return PopulacaoAleatoria<IndividuoLSChains>(nPop, min, max, nAtributos, precisao, tabu);
            return PopulacaoAleatoria<IndividuoBin>(nPop, min, max, nAtributos, precisao, tabu);
        }
    }
}
