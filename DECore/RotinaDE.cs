using System;
using System.Collections.Generic;
using AlgoResult;
using Functions;
using System.Linq;
using AlgoCore;

namespace DECore
{
    public enum SelecaoDE
    {
        Rand1Bin,
        Rand2Bin,
        Best1Bin,
        RandToBest1Bin,
        RandToBest2Bin,
        CurrentToRand1Bin
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
                List<IndividuoBin> selecao;
                switch (_tipoSelecao)
                {
                    case SelecaoDE.Rand1Bin: selecao = OperadoresDE.SelecaoAleatoria(3, populacao); break;
                    case SelecaoDE.Best1Bin: selecao = OperadoresDE.SelecaoBest(populacao); break;
                    case SelecaoDE.Rand2Bin: selecao = OperadoresDE.SelecaoAleatoria(5, populacao); break;
                    case SelecaoDE.RandToBest2Bin:
                        selecao = OperadoresDE.SelecaoAleatoria(4, populacao);
                        selecao.Insert(0, populacao[i]);
                        selecao.Insert(0, populacao.First());
                        selecao.Insert(0, populacao[i]);
                        break;
                    case SelecaoDE.CurrentToRand1Bin:
                        selecao = OperadoresDE.SelecaoAleatoria(3, populacao);
                        selecao.Insert(0, populacao[i]);
                        selecao.Insert(2, populacao[i]);
                        break;
                    default: return;
                }

                IndividuoBin individuoTemp = new IndividuoBin();

                int jRand = rand.Next(0, _nAtributos - 1);
                for (int j = 0; j < _nAtributos; j++)
                {
                    if (rand.NextDouble() < _probCross || j == jRand)
                    {
                        //ui,j,G+1 = xr3,j,G + F(xr1,j,G − xr2,j,G)
                        double atributo = selecao[0].Atributos[j] - _fatorFUsado * (selecao[1].Atributos[j] - selecao[2].Atributos[j]);

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