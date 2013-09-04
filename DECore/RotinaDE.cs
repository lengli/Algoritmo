using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;
using AlgoCore;

namespace DECore
{
    public enum SelecaoDE
    {
        None = 0, Rand = 1, Best = 2
    }

    public class RotinaDE : RotinaAlgo
    {
        private SelecaoDE _tipoSelecao;
        private double _probCross;
        private double _fatorF;

        public RotinaDE(FuncAptidao aptidao, FuncRepopRestricao FuncRestr,
            List<FuncAptidao> gs, List<FuncAptidao> hs, FuncValidarRestricao validar,
            SelecaoDE tipoSelecao, double probCross, double fatorF)
            : base(aptidao, FuncRestr, gs, hs, validar)
        {
            _tipoSelecao = tipoSelecao;
            _probCross = probCross;
            _fatorF = fatorF;
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            for (int i = 0; i < populacao.Count; i++)
            {
                // selecionando 3 individuos aleatoriamente
                List<IndividuoBin> selecao;

                switch (_tipoSelecao)
                {
                    case SelecaoDE.Rand: selecao = OperadoresDE.SelecaoAleatoria(3, populacao); break;
                    case SelecaoDE.Best: selecao = OperadoresDE.SelecaoBest(populacao); break;
                    default: return;
                }

                IndividuoBin individuoTemp = new IndividuoBin();

                int jRand = rand.Next(0, _nAtributos - 1);
                for (int j = 0; j < _nAtributos; j++)
                {
                    if (rand.NextDouble() < _probCross || j == jRand)
                    {
                        //ui,j,G+1 = xr3,j,G + F(xr1,j,G − xr2,j,G)
                        double atributo = selecao[2].Atributos[j] - _fatorF * (selecao[0].Atributos[j] - selecao[1].Atributos[j]);

                        // tratamento de restrição
                        if (atributo >= _min && atributo <= _max)
                        {
                            individuoTemp.Atributos.Add(atributo);
                            continue;
                        }
                    }

                    individuoTemp.Atributos.Add(populacao[i].Atributos[j]);
                }

                individuoTemp.Aptidao = FuncaoAptidao(individuoTemp.Atributos);
                if (individuoTemp.Aptidao < populacao[i].Aptidao)
                    populacao[i] = individuoTemp;
            }
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo)
        {
            return false;
        }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao)
        {
        }
    }
}