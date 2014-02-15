using System;
using System.Collections.Generic;
using System.Linq;
using AlgoResult;

namespace DECore
{
    public class OperadoresDE
    {
        public static List<IndividuoBin> SelecaoAleatoria(int nIndividuos, List<IndividuoBin> populacao, HashSet<int> filtroIndividuos)
        {
            if (nIndividuos >= populacao.Count) return populacao;

            HashSet<int> indiceSelecionados;
            if (filtroIndividuos != null) indiceSelecionados = filtroIndividuos;
            else indiceSelecionados = new HashSet<int>();

            nIndividuos += indiceSelecionados.Count;

            List<IndividuoBin> indSelecionados = new List<IndividuoBin>(nIndividuos);
            int indice = new Random(AlgoCore.AlgoUtil.GetSeed()).Next(0, populacao.Count - 1);
            while (indiceSelecionados.Count < nIndividuos)
            {
                // selecionar um índice não escolhido
                while (indiceSelecionados.Contains(indice))
                    indice = new Random(AlgoCore.AlgoUtil.GetSeed()).Next(0, populacao.Count - 1);

                indiceSelecionados.Add(indice);
                indSelecionados.Add(populacao[indice]);
            }

            return indSelecionados;
        }

        // melhor na primeira posição da lista
        public static List<IndividuoBin> SelecaoBest(List<IndividuoBin> populacao)
        {
            List<IndividuoBin> ind = SelecaoAleatoria(2, populacao, new HashSet<int> { 0 });
            ind.Add(populacao.First());
            return ind;
        }
    }
}
