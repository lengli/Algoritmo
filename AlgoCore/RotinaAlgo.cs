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

        protected RotinaAlgo(FuncAptidao aptidao)
        {
            FuncApt = aptidao;
        }

        protected double FuncaoAptidao(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            return FuncApt(atributos);
        }

        public AlgoInfo Rodar(int geracoesMAx, int tamanhoPop, double min, double max, int nAtributos, int precisao,
            int maxAvaliacoes, bool usarTabu, double distTabu, int nMaxRepop, int gerSemMelhorar, double margemComp, bool tabuNaPop, ParametrosHillClimbing hillClimbing = null,
            ParametrosLSChains lsChains = null, int qtdMutLocal = 0)
        {
            IndividuoBin.Precisao = precisao;
            _maxAval = maxAvaliacoes;
            _geracoesMAx = geracoesMAx;
            _tamanhoPop = tamanhoPop;
            _min = min;
            _max = max;
            _nAtributos = nAtributos;
            _distTabu = distTabu;
            _margemComp = margemComp;
            _gerSemMelhorar = gerSemMelhorar;

            AlgoInfo agInfo = new AlgoInfo();
            List<IndividuoBin> populacao = PopulacaoAleatoria(tamanhoPop, min, max, nAtributos, precisao, lsChains != null);

            InicializarAlgoritmo(populacao);

            // avaliação da população inicial
            foreach (IndividuoBin individuo in populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

            for (int g = 0; g < geracoesMAx || geracoesMAx == 0; g++)
            {
                if (_maxAval != 0 && _avaliacoes >= _maxAval) break;

                // avaliará individuos novos
                foreach (IndividuoBin individuo in populacao.Where(p => p.Aptidao == double.MaxValue))
                    individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

                ExecutarAlgoritmo(populacao);

                populacao = populacao.OrderBy(ind => ind.Aptidao).Take(tamanhoPop).ToList();

                BuscaLocal(populacao, max, min, precisao, qtdMutLocal, hillClimbing, lsChains);

                int ultimaRepop = 0;
                int nRepopulacao = 0;
                //criterio de repopulação, evitar mínimos locais devido ao elitismo
                if (usarTabu)
                {
                    if (!CriterioRepopular(agInfo, ultimaRepop)) continue;
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
                    if (CriterioDeParada(agInfo)) break;
                }

                agInfo.AdicionarInfo(populacao, g, _avaliacoes);
            }
            return agInfo;
        }

        // método destinado a inserir algorítmos meméticos
        // considerando que a lista populacao já está ordenada por aptidao
        private void BuscaLocal(List<IndividuoBin> populacao, double max, double min, int precisao, // parametros do algoritmo
            int qtdMutLocal, ParametrosHillClimbing hillClimbing, ParametrosLSChains lsChains) // parametros das buscas locais
        {
            // busca local
            for (int z = 0; z < qtdMutLocal; z++)
            {
                populacao[z] = MutacaoBinaria.Executar(populacao[z], FuncaoAptidao);
                populacao[z] = MutacaoBinaria.Executar(populacao[z], FuncaoAptidao);

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
            List<T> populacao = IndividuoBin.GerarPopulacao<T>(nPop, min, max, nAtributos, precisao, tabu, _distTabu);

            // avaliação da população inicial
            foreach (T individuo in populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

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
