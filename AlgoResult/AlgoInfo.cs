using System.Collections.Generic;
using System.Linq;

namespace AlgoResult
{
    public class AlgoInfo
    {
        public AlgoInfo()
        {
            Informacoes = new List<AlgoInfoGeracao>();
        }

        public List<AlgoInfoGeracao> Informacoes { get; set; }
        public IndividuoBin MelhorIndividuo { get; set; }
        public int GerDoMelhor { get; set; }
        public int AvalParaMelhor { get; set; }

        public void AdicionarInfo(List<IndividuoBin> populacao, int geracao, int avaliacoes)
        {
            if (populacao.Count == 0) return;

            // pega o individuo com o valor aptidao menor possível
            IndividuoBin melhorGer = populacao.OrderBy(ind => ind.Aptidao).First();
            AlgoInfoGeracao infoGer = new AlgoInfoGeracao
                {
                    Geracao = geracao,
                    Media = populacao.Sum(ind => ind.Aptidao) / populacao.Count,
                    MelhorAptidao = melhorGer.Aptidao,
                    Avaliacoes = avaliacoes
                };

            double[] atrs = new double[melhorGer.Atributos.Count];
            melhorGer.Atributos.CopyTo(atrs);
            infoGer.MelhorCromo = atrs.ToList();

            Informacoes.Add(infoGer);

            // salva o melhor de todos os tempos
            if (MelhorIndividuo == null || MelhorIndividuo.Aptidao > melhorGer.Aptidao)
            {
                MelhorIndividuo = melhorGer.Clone();
                GerDoMelhor = geracao;
                AvalParaMelhor = avaliacoes;
            }

        }
    }
}
