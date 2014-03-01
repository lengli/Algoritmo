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
        public static IndividuoBin Executar(IndividuoBin ind,
            FuncAptidao funcAptidao, FuncAptidao funcAptidaoVirtual, double mutReal,
            FuncValidarRestricao _validarFronteira, int i)
        {
            IndividuoBin tempInd = ind.Clone();
            double aptidaoInicial = ind.Aptidao;

            //double max = IndividuoBin.Maximo(i);
            //double min = IndividuoBin.Minimo(i);

            double valorAntigo = tempInd.Atributos[i];
            double novoValor = tempInd.Atributos[i] + mutReal; //+ (max - min) * mutReal + min;

            if (novoValor < IndividuoBin.Minimo(i) || novoValor > IndividuoBin.Maximo(i)
                /*|| (_validarFronteira != null && !_validarFronteira(novoValor, i))*/)
                return ind;

            tempInd.Atributos[i] = novoValor;
            tempInd.Aptidao = funcAptidao(tempInd.Atributos);
            return tempInd;

            double aptidaoNova;

            // passar para fora para utilizar a apt virtual para as fcs Gs
            if (_validarFronteira != null && !_validarFronteira(tempInd.Atributos) && funcAptidaoVirtual != null)
                aptidaoNova = funcAptidaoVirtual(tempInd.Atributos);
            else
                aptidaoNova = funcAptidao(tempInd.Atributos);

            if (aptidaoNova < aptidaoInicial)
            {
                if (_validarFronteira == null || _validarFronteira(tempInd.Atributos))
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                }
                aptidaoInicial = aptidaoNova;
            }
            else
            {
                tempInd.Atributos[i] = valorAntigo;
            }
            return ind;
        }
    }
}
