using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using AlgoResult;
using Functions;
using LocalCore.HillClimbing;
using LocalCore.LSChains;
using LocalCore.BuscaMutacao;

namespace AGCore
{
    public class AGClassico : IFunctions
    {
        public AGClassico(FuncAptidao aptidao)
        {
            FuncApt = aptidao;
        }

        #region IFunctions

        public FuncAptidao FuncApt { get; set; }

        #endregion

        private double FuncaoAptidao(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            double f= FuncApt(atributos);

            if (f < -13000)
            {
            }

            return f;
        }

        private int _critNMelhores = 0;
        private int _avaliacoes = 0;
        private int _maxAval = 0;
        private double _deltaMedBest = 0;
        private double _tabuDist = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nPop"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="precisao"></param>
        /// <param name="nAtributos"></param>
        /// <param name="nMaxGeracoes"></param>
        /// <param name="probCrossover"></param>
        /// <param name="probMutacao"></param>
        /// <param name="qtdMutLocal"></param>
        /// <param name="elitismo">Parametro para gerar pop. aleatória a cada geração</param>
        /// <param name="maxRepop"></param>
        /// <param name="usarTabu"></param>
        /// <returns></returns>
        public AlgoInfo Rodar(int nPop, double min, double max, int precisao, int nAtributos, int nMaxGeracoes,
            double probCrossover, double probMutacao, int qtdMutLocal, bool elitismo, int maxRepop = 0, bool usarTabu = false,
            bool tabuNaPop = false, int critNMelhores = 5, ParametrosHillClimbing hillClimbing = null,
            ParametrosLSChains lsChains = null, int maxAvaliacoes = 0, double deltaMedApt = 0.001, double tabuDist = 0.001)
        {
            _maxAval = maxAvaliacoes;
            _tabuDist = tabuDist;
            _deltaMedBest = deltaMedApt;
            _critNMelhores = critNMelhores;
            int geracao = 0;
            int gerAleatoria = 0;
            AlgoInfo agInfo = new AlgoInfo();
            int nRepopulacao = 0;

            List<IndividuoBin> populacao = PopulacaoAleatoria(nPop, min, max, nAtributos, precisao, lsChains != null);
            agInfo.AdicionarInfo(populacao, geracao, _avaliacoes);

            geracao++;

            List<IndividuoBin> individuosTabu = new List<IndividuoBin>();

            DateTime inicio = DateTime.Now;
            for (; geracao < nMaxGeracoes; geracao++)
            {
                if (_maxAval != 0 && _avaliacoes >= _maxAval) break;

                List<double> probAcum =
                    AGOperadores.MetodoDaRoleta(populacao.Select(ind => ind.Aptidao).ToList());
                List<IndividuoBin> popInterm =
                    AGOperadores.SelecaoPopulacaoIntermediaria(probAcum, populacao);
                AGOperadores.Crossover(popInterm, probCrossover);
                AGOperadores.Mutacao(popInterm, probMutacao);

                foreach (IndividuoBin individuo in popInterm)
                    // não avalia individuos previamente avaliados
                    if (individuo.Aptidao >= double.MaxValue)
                        individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

                //populacao = popInterm;
                populacao.AddRange(popInterm);
                populacao = populacao.OrderBy(ind => ind.Aptidao).Take(nPop).ToList();

                BuscaLocal(populacao, max, min, precisao, qtdMutLocal, hillClimbing, lsChains);

                agInfo.AdicionarInfo(populacao, geracao, _avaliacoes);

                //criterio de repopulação, evitar mínimos locais devido ao elitismo
                if (usarTabu && elitismo)
                {
                    if (!Repopular(agInfo, gerAleatoria)) continue;
                    // criterio de parada: repopular n vezes
                    if (nRepopulacao >= maxRepop) break;
                    nRepopulacao++;

                    individuosTabu.Add(populacao[0].Clone());

                    populacao = PopulacaoAleatoria(nPop - (tabuNaPop ? individuosTabu.Count : 0), min, max, nAtributos, precisao,
                        lsChains != null, individuosTabu);

                    // adicionando os individuos tabus
                    if (tabuNaPop)
                        populacao.AddRange(individuosTabu);

                    gerAleatoria = geracao;
                }
                else
                {
                    // criterio de parada
                    if (Repopular(agInfo)) break;
                    if (!elitismo)
                        populacao = PopulacaoAleatoria(nPop, min, max, nAtributos, precisao, lsChains != null);
                }
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

        // para AG sem elitismo/sem tabu esse é o critério de parada
        private bool Repopular(AlgoInfo agInfo, int gerAleatoria = 0)
        {
            // media muito próxima do melhor
            AlgoInfoGeracao info = agInfo.Informacoes.Last();
            if (info.Media - info.MelhorAptidao <= _deltaMedBest) return true;

            // n gerações sem melhoras do melhor
            double melhorAptidao = agInfo.Informacoes.Last().MelhorAptidao;
            int contarMelhor = 1;
            for (int j = agInfo.Informacoes.Count - 2; j >= gerAleatoria; j--)
            {
                if (agInfo.Informacoes[j].MelhorAptidao <= melhorAptidao) contarMelhor++;
                else break;
            }

            if (contarMelhor >= _critNMelhores) return true;
            return false;
        }

        private List<IndividuoBin> PopulacaoAleatoria<T>(int nPop, double min, double max, int nAtributos, int precisao,
            List<IndividuoBin> tabu = null) where T : IndividuoBin, new()
        {
            List<T> populacao = IndividuoBin.GerarPopulacao<T>(nPop, min, max, nAtributos, precisao, tabu, _tabuDist);

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
