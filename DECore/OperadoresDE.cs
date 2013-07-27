using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;

namespace DECore
{
    public class OperadoresDE
    {
        public static List<IndividuoBin> SelecaoAleatoria(int nIndividuos, List<IndividuoBin> populacao)
        {
            if (nIndividuos >= populacao.Count) return populacao;

            HashSet<int> indiceSelecionados = new HashSet<int>();
            List<IndividuoBin> indSelecionados = new List<IndividuoBin>(nIndividuos);
            Random rand = new Random(DateTime.Now.Millisecond);
            int indice = rand.Next(0, populacao.Count - 1);

            while (indiceSelecionados.Count < nIndividuos)
            {
                // selecionar um índice não escolhido
                while (indiceSelecionados.Contains(indice))
                    indice = rand.Next(0, populacao.Count - 1);

                indiceSelecionados.Add(indice);
                indSelecionados.Add(populacao[indice]);
            }

            return indSelecionados;
        }

        // melhor na última posição da lista
        public static List<IndividuoBin> SelecaoBest(List<IndividuoBin> populacao)
        {
            List<IndividuoBin> ind = SelecaoAleatoria(2, populacao);
            ind.Add(populacao.OrderBy(p => p.Aptidao).First());
            return ind;
        }
    }
}
