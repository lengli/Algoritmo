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
        private Random rand = new Random(DateTime.Now.Millisecond);
        private SelecaoDE _tipoSelecao;
        private double _probCross;
        private double _fatorF;

        public RotinaDE(FuncAptidao aptidao, SelecaoDE tipoSelecao, double probCross, double fatorF)
            : base(aptidao)
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
                        Numero atributo = new Numero(IndividuoBin.Precisao);
                        //ui,j,G+1 = xr3,j,G + F(xr1,j,G − xr2,j,G)
                        atributo.ValorReal = selecao[2].Valor(j) - _fatorF * (selecao[0].Valor(j) - selecao[1].Valor(j));

                        // tratamento de restrição
                        if (atributo.ValorReal >= _min && atributo.ValorReal <= _max)
                        {
                            individuoTemp.Atributos.Add(atributo);
                            continue;
                        }
                    }

                    individuoTemp.Atributos.Add(new Numero(IndividuoBin.Precisao) { ValorReal = populacao[i].Valor(j) });
                }

                individuoTemp.Aptidao = FuncaoAptidao(individuoTemp.Atributos.Select(n => n.ValorReal).ToList());
                if (individuoTemp.Aptidao < populacao[i].Aptidao)
                    populacao[i] = individuoTemp;
            }
        }
    }
}