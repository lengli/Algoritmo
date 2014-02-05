using System;
using System.Collections.Generic;
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
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            SelecaoDE tipoSelecao, double probCross, double fatorF, FuncValidarFronteira valFront)
            : base(aptidao, FuncRestr, gs, hs, validar, valFront)
        {
            _tipoSelecao = tipoSelecao;
            _probCross = probCross;
            _fatorF = fatorF;
        }

        double _fatorFUsado;
        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            //if (_fatorF > 0)
            //    _fatorFUsado = _fatorF / (Math.Pow(20, (double)_avaliacoes / _maxAval));
            _fatorFUsado = _fatorF;

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
                        double atributo = selecao[2].Atributos[j] - _fatorFUsado * (selecao[0].Atributos[j] - selecao[1].Atributos[j]);

                        // tratamento de restrição
                        if (atributo >= _min(j) && atributo <= _max(j)
                            && (_validarFronteira == null || _validarFronteira(atributo, j)))
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