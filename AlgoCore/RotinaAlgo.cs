using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected int _avaliacoes = 0;
        protected int _maxAval = 0;
        protected int _geracoesMAx;
        protected int _tamanhoPop;
        protected double _min;
        protected double _max;
        protected int _nAtributos;
        protected double _distTabu;
        protected int _gerSemMelhorar;
        protected double _margemComp;
        protected Random rand = new Random(DateTime.Now.Millisecond);
        protected List<IndividuoBin> individuosTabu = new List<IndividuoBin>();
        protected AlgoInfo _agInfo;
        protected List<FuncAptidao> _gs;
        protected List<FuncAptidao> _hs;
        private List<double> _gmax;
        private List<double> _hmax;
        private FuncValidarRestricao _validarRestricao;
        private List<IndividuoBin> populacao;
        private int _popValida = 0;
        private double _popMax = 0;
        private double _popMin = 0;

        protected RotinaAlgo(FuncAptidao aptidao, FuncRepopRestricao restricao,
            List<FuncAptidao> gs, List<FuncAptidao> hs, FuncValidarRestricao validarRestricao)
        {
            FuncApt = aptidao;
            FuncRestr = restricao;
            _gs = gs;
            _hs = hs;
            _validarRestricao = validarRestricao;
        }

        protected double FuncaoAptidao(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            double f = FuncApt(atributos);
            if (_validarRestricao == null || _validarRestricao(atributos)) return f;

            List<double> gValues = _gs.Select(gi => Math.Max(0, gi(atributos))).ToList();

            // δ = 1E-4
            List<double> hValues = _hs.Select(hi => Math.Max(0, Math.Abs(hi(atributos)) - 1E-4)).ToList();

            // v(X)
            double v = 1;

            // Gmax
            for (int i = 0; i < _gs.Count; i++)
            {
                if (_gmax.Count < i + 1) _gmax.Add(0);
                if (_gmax[i] < gValues[i]) _gmax[i] = gValues[i];
                v += gValues[i] / _gmax[i];
            }
            for (int i = 0; i < _hs.Count; i++)
            {
                if (_hmax.Count < i + 1) _hmax.Add(0);
                if (_hmax[i] < hValues[i]) _hmax[i] = hValues[i];
                v += hValues[i] / _hmax[i];
            }

            v /= (_gmax.Sum(g => 1 / g) + _hmax.Sum(h => 1 / h));

            double rf = (double)_popValida / _tamanhoPop;

            if (rf == 0) return v;

            double fc = (f - _popMin) / (_popMax - _popMin);

            double d = Math.Sqrt(fc * fc + v * v);

            double p = (1 - rf) * v + rf * fc;

            return d + p;
        }

        public AlgoInfo Rodar(int geracoesMAx, int tamanhoPop, double min, double max, int nAtributos, int precisao, //double erroAceitavel, double minGlobal,
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
            populacao = PopulacaoAleatoria(tamanhoPop, min, max, nAtributos, precisao, lsChains != null);

            InicializarAlgoritmo(populacao);

            // avaliação da população inicial
            foreach (IndividuoBin individuo in populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

            for (int g = 0; g < geracoesMAx || geracoesMAx == 0; g++)
            {
                // n. de individuos validos, se for o caso
                if (_validarRestricao != null)
                {
                    _popValida = populacao.Count(ind => _validarRestricao(ind.Atributos));
                    _popMax = populacao.Max(ind => ind.Aptidao);
                    _popMin = populacao.Min(ind => ind.Aptidao);
                }
                // Limite do nr. de avaliações
                if (_maxAval != 0 && _avaliacoes >= _maxAval) break;

                //if (erroAceitavel > 0 && Math.Abs(_agInfo.MelhorIndividuo.Aptidao - minGlobal) < erroAceitavel) break;

                // avaliará individuos novos
                foreach (IndividuoBin individuo in populacao.Where(p => p.Aptidao == double.MaxValue))
                    individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

                ExecutarAlgoritmo(populacao);

                populacao = populacao.OrderBy(ind => ind.Aptidao).Take(tamanhoPop).ToList();

                BuscaLocal(populacao, max, min, precisao, qtdMutLocal, hillClimbing, lsChains);

                int ultimaRepop = 0;
                int nRepopulacao = 0;
                //criterio de repopulação, evitar mínimos locais devido ao elitismo
                if (usarTabu)
                {
                    if (!CriterioRepopular(_agInfo, ultimaRepop)) continue;
                    // criterio de parada: repopular n vezes
                    if (nRepopulacao >= nMaxRepop) break;
                    nRepopulacao++;

                    individuosTabu.Add(populacao[0].Clone());

                    populacao = PopulacaoAleatoria(tamanhoPop - (tabuNaPop ? individuosTabu.Count : 0), min, max, nAtributos, precisao,
                        lsChains != null, individuosTabu);

                    // adicionando os individuos tabus
                    if (tabuNaPop)
                        populacao.AddRange(individuosTabu);

                    ultimaRepop = g;
                }
                else
                {
                    // criterio de parada
                    if (CriterioDeParada(_agInfo)) break;
                }

                _agInfo.AdicionarInfo(populacao, g, _avaliacoes);

                // se todos os individuos convergirem 
                int gr = populacao.GroupBy(ind => ind.Aptidao).Count();
                if (g > 400 && gr == 1) break;
            }
            return _agInfo;
        }

        // método destinado a inserir algorítmos meméticos
        // considerando que a lista populacao já está ordenada por aptidao
        private void BuscaLocal(List<IndividuoBin> populacao, double max, double min, int precisao, // parametros do algoritmo
            int qtdMutLocal, ParametrosHillClimbing hillClimbing, ParametrosLSChains lsChains) // parametros das buscas locais
        {
            // busca local
            for (int z = 0; z < qtdMutLocal; z++)
            {
                //populacao[z] = MutacaoBinaria.Executar(populacao[z], FuncaoAptidao);
                //populacao[z] = MutacaoBinaria.Executar(populacao[z], FuncaoAptidao);

                // +- 5% -> alta convergencia
                populacao[z] = MutacaoReal.Executar(populacao[z], FuncaoAptidao, (max - min) / 20);
                // +- 0.1% - > exploração
                populacao[z] = MutacaoReal.Executar(populacao[z], FuncaoAptidao, (max - min) / 1000);
                // +- precisão - > explotação
                populacao[z] = MutacaoReal.Executar(populacao[z], FuncaoAptidao, Math.Pow(10, -precisao));
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
        protected abstract void ExecutarAlgoritmo(List<IndividuoBin> populacao);

        private bool CriterioRepopular(AlgoInfo agInfo, int ultimaRepop)
        {
            // n gerações sem melhoras do melhor
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

        private List<IndividuoBin> PopulacaoAleatoria<T>(int nPop, double min, double max, int nAtributos, int precisao,
            List<IndividuoBin> tabu = null) where T : IndividuoBin, new()
        {
            List<T> populacao;

            if (FuncRestr == null)
                populacao = IndividuoBin.GerarPopulacao<T>(nPop, min, max, nAtributos, precisao, tabu, _distTabu);
            else populacao = FuncRestr().Select(atrs => new T { Atributos = atrs }).ToList();

            // avaliação da população inicial
            foreach (T individuo in populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

            List<IndividuoBin> populacaoRetorno = new List<IndividuoBin>();
            foreach (T ind in populacao)
                populacaoRetorno.Add(ind);
            populacao.Clear();

            return populacaoRetorno;
        }

        private List<IndividuoBin> PopulacaoAleatoria(int nPop, double min, double max, int nAtributos, int precisao,
            bool isLSChains, List<IndividuoBin> tabu = null)
        {
            if (isLSChains)
                return PopulacaoAleatoria<IndividuoLSChains>(nPop, min, max, nAtributos, precisao, tabu);
            else return PopulacaoAleatoria<IndividuoBin>(nPop, min, max, nAtributos, precisao, tabu);
        }
    }
}
