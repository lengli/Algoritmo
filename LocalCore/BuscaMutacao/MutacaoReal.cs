using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace LocalCore.BuscaMutacao
{
    public class MutacaoReal
    {
        public static IndividuoBin Executar(IndividuoBin ind, FuncAptidao funcAptidao, double mutReal)
        {
            IndividuoBin tempInd = ind.Clone();
            List<bool> cromo = tempInd.Cromossomo;
            double aptidaoInicial = ind.Aptidao;

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i].ValorReal;
                double novoValor = tempInd.Atributos[i].ValorReal - mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo) continue;

                tempInd.Atributos[i].ValorReal = novoValor;
                double aptidaoNova = funcAptidao(tempInd.Atributos.Select(n => n.ValorReal).ToList());

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i].ValorReal = valorAntigo;
                }
            }

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i].ValorReal;
                double novoValor = tempInd.Atributos[i].ValorReal + mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo) continue;

                tempInd.Atributos[i].ValorReal = novoValor;
                var list = tempInd.Atributos.Select(n => n.ValorReal).ToList();
                double aptidaoNova = funcAptidao(list);

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i].ValorReal = valorAntigo;
                }
            }
            return ind;
        }
    }
}
