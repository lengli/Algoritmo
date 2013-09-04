using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace LocalCore.HillClimbing
{
    public class RotinaHillClimbing : IFunctions
    {
        public RotinaHillClimbing(FuncAptidao aptidao)
        {
            FuncApt = aptidao;
        }

        public IndividuoBin Rodar(IndividuoBin individuo, ParametrosHillClimbing parametros)
        {
            int nAtributos = individuo.Atributos.Count;

            List<double> candidatos = new List<double>(4);
            candidatos.Add(-parametros.Aceleracao);
            candidatos.Add(-1 / parametros.Aceleracao);
            candidatos.Add(1 / parametros.Aceleracao);
            candidatos.Add(parametros.Aceleracao);

            IndividuoBin novoIndiv = individuo.Clone();
            double aptAnterior;
            do
            {
                aptAnterior = novoIndiv.Aptidao;

                for (int i = 0; i < nAtributos; i++)
                {
                    int melhor = -1;
                    double melhorApt = novoIndiv.Aptidao;

                    for (int j = 0; j < 3; j++)
                    {
                        // atributos para serem comparados - criando uma cópia da lista
                        List<double> atts = novoIndiv.Atributos.Select(at => at).ToList();

                        atts[i] += parametros.StepAtributos[i] * candidatos[j];
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
                        novoIndiv.Atributos[i] = novoIndiv.Atributos[i] + parametros.StepAtributos[i] * candidatos[melhor];
                        novoIndiv.Aptidao = melhorApt;
                    }
                }
            } while (novoIndiv.Aptidao - aptAnterior > parametros.Epsilon); // criterio de parada da busca local
            return novoIndiv.Aptidao < individuo.Aptidao ? novoIndiv : null;
        }

        public FuncAptidao FuncApt { get; set; }
        public FuncRepopRestricao FuncRestr { get; set; }
    }
}
