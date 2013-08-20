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
using AlgoCore;

namespace AGCore
{
    public enum CrossType { Bin = 0, Aritmetico = 1, Heuristico = 2 }

    public class AGClassico : RotinaAlgo
    {
        public AGClassico(FuncAptidao funcao, double probCrossover, double probMutacao, double deltaMedApt, int critParada, CrossType crossType)
            : base(funcao)
        {
            this.probCrossover = probCrossover;
            this.probMutacao = probMutacao;
            this.deltaMedApt = deltaMedApt;
            this.critParada = critParada;
            this.crossType = crossType;
            erroPrecisao = Math.Pow(10, -IndividuoBin.Precisao);
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            List<double> probAcum = AGOperadores.MetodoDaRoleta(populacao.Select(ind => ind.Aptidao).ToList());
            List<IndividuoBin> popInterm = AGOperadores.SelecaoPopulacaoIntermediaria(probAcum, populacao);
            AGOperadores.Crossover(popInterm, probCrossover, crossType);
            AGOperadores.Mutacao(popInterm, probMutacao);

            foreach (IndividuoBin individuo in popInterm)
                // não avalia individuos previamente avaliados
                if (individuo.Aptidao >= double.MaxValue)
                    individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

            populacao.AddRange(popInterm);
        }

        #region private

        private double probCrossover;
        private double probMutacao;
        private double deltaMedApt;
        private int critParada;
        private double erroPrecisao;
        private CrossType crossType;

        #endregion

        protected override bool CriterioDeParada(AlgoInfo agInfo)
        {
            if (agInfo.MelhorIndividuo == null) return false;
            // Verificar quantas gerações o melhor individuo nao melhora a aptidao
            if (critParada != 0)
            {
                double melhorApt = agInfo.MelhorIndividuo.Aptidao;

                int nMelhores = 0;
                for (int i = agInfo.Informacoes.Count - 1; i >= 0; i--)
                {
                    if (agInfo.Informacoes[i].Geracao == agInfo.GerDoMelhor) continue;
                    if (Math.Abs(agInfo.Informacoes[i].MelhorAptidao - melhorApt) > erroPrecisao) break;
                    nMelhores++;
                }
                if (nMelhores >= critParada) return true;
            }

            if (Math.Abs(agInfo.MelhorIndividuo.Aptidao - agInfo.Informacoes[agInfo.GerDoMelhor].Media) < deltaMedApt)
                return true;
            return false;
        }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao)
        {
            return;
        }
    }
}
