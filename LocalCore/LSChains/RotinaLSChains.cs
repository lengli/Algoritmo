using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace LocalCore.LSChains
{
    public class RotinaLSChains : IFunctions
    {
        public RotinaLSChains(FuncAptidao aptidao)
        {
            FuncApt = aptidao;
        }

        public IndividuoLSChains Rodar(IndividuoLSChains individuo, ParametrosLSChains parametros)
        {
            int nAtributos = individuo.Atributos.Count;

            if (individuo.DirecaoBusca == null)
            {
                individuo.DirecaoBusca = new List<double>(nAtributos);
                for (int i = 0; i < nAtributos; i++)
                    individuo.DirecaoBusca.Add(0);
            }

            List<double> candidatos = new List<double>(4);
            candidatos.Add(-parametros.Aceleracao);
            candidatos.Add(-1 / parametros.Aceleracao);
            candidatos.Add(1 / parametros.Aceleracao);
            candidatos.Add(parametros.Aceleracao);

            IndividuoLSChains novoIndiv = individuo.Clone();
            double aptAnterior;
            for (int it = 0; it < parametros.NIteracoes; it++)
            {
                aptAnterior = novoIndiv.Aptidao;

                for (int i = 0; i < nAtributos; i++)
                {
                    int melhor = -1;
                    double melhorApt = novoIndiv.Aptidao;

                    for (int j = 0; j < 4; j++)
                    {
                        // direcao negativa
                        if ((j == 0 || j == 1) && novoIndiv.DirecaoBusca[i] == 1) continue;
                        // direcao positiva
                        if ((j == 2 || j == 3) && novoIndiv.DirecaoBusca[i] == -1) continue;

                        // atributos para serem comparados
                        List<double> atts = novoIndiv.Atributos.Select(at => at.ValorReal).ToList();

                        atts[i] += candidatos[j];
                        double apt = FuncApt(atts);

                        if (apt < melhorApt)
                        {
                            melhorApt = apt;
                            melhor = j;
                        }
                    }

                    // escolha do melhor fator entre os "candidatos"
                    if (melhor != -1)
                    {
                        novoIndiv.Atributos[i].ValorReal = novoIndiv.Atributos[i].ValorReal + candidatos[melhor];
                        novoIndiv.Aptidao = melhorApt;

                        // construindo o vetor direcao
                        if (novoIndiv.DirecaoBusca[i] == 0)
                        {
                            // direcao negativa
                            if (melhor == 0 || melhor == 1) novoIndiv.DirecaoBusca[i] = -1;
                            // direcao positiva
                            else if (melhor == 2 || melhor == 3) novoIndiv.DirecaoBusca[i] = 1;
                        }
                    }
                }
            }
            return novoIndiv.Aptidao < individuo.Aptidao ? novoIndiv : null;
        }

        public FuncAptidao FuncApt { get; set; }
    }
}
