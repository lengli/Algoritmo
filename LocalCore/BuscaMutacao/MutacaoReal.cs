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
        public static IndividuoBin Executar(IndividuoBin ind, FuncAptidao funcAptidao, double mutReal,
            FuncValidarFronteira _validarFronteira)
        {
            IndividuoBin tempInd = ind.Clone();
            double aptidaoInicial = ind.Aptidao;

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i];
                double novoValor = tempInd.Atributos[i] - mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo ||
                    (_validarFronteira != null && !_validarFronteira(novoValor, i))) continue;

                tempInd.Atributos[i] = novoValor;
                double aptidaoNova = funcAptidao(tempInd.Atributos);

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i] = valorAntigo;
                }
            }

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i];
                double novoValor = tempInd.Atributos[i] + mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo ||
                    (_validarFronteira != null && !_validarFronteira(novoValor, i))) continue;

                tempInd.Atributos[i] = novoValor;
                var list = tempInd.Atributos.Select(n => n).ToList();
                double aptidaoNova = funcAptidao(list);

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i] = valorAntigo;
                }
            }
            return ind;
        }
    }
}
