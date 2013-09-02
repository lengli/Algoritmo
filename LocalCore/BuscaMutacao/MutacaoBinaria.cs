using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace LocalCore.BuscaMutacao
{
    /*
    public class MutacaoBinaria
    {
        public static IndividuoBin Executar(IndividuoBin ind, FuncAptidao funcAptidao)
        {
            IndividuoBin tempInd = ind.Clone();
            List<bool> cromo = tempInd.Cromossomo;
            double aptidaoInicial = ind.Aptidao;

            for (int i = 0; i < cromo.Count; i++)
            {
                cromo[i] = cromo[i] ? false : true;
                tempInd.Cromossomo = cromo;

                if (!tempInd.Atributos.Any(a => a.ValorReal < IndividuoBin.Minimo || a > IndividuoBin.Maximo))
                {
                    double aptidaoNova = funcAptidao(tempInd.Atributos);

                    if (aptidaoNova < aptidaoInicial)
                    {
                        ind = tempInd.Clone();
                        ind.Aptidao = aptidaoNova;
                        aptidaoInicial = aptidaoNova;
                    }
                    continue;
                }

                tempInd = ind.Clone();
                cromo = tempInd.Cromossomo;
            }

            return ind;
        }
    }
    */
}
