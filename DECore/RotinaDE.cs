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

        //ui,j,G+1 = x1,j,G + F(x2,j,G − x3,j,G) + F(x4,j,G − x5,j,G) + ...
        private static double CalculoAtributo(List<IndividuoBin> selecao, int atr, double _fatorF)
        {
            double atributo = selecao[0].Atributos[atr];

            for (int i = 1; i < selecao.Count; i++)
            {
                int sinal = i % 2 == 1 ? 1 : -1;
                atributo += selecao[i].Atributos[atr] + _fatorF;
                atributo *= sinal;
            }
            return atributo;
        }

        internal static void ExecutarMutacao(int atualInd, List<IndividuoBin> populacao, SelecaoDE _tipoSelecao, double _fatorF,
            int _nAtributos, double _probCross, Bound _min, Bound _max, FuncValidarFronteira valFront, FuncAptidao aptidao)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<IndividuoBin> selecao;
            switch (_tipoSelecao)
            {
                case SelecaoDE.Rand1Bin: selecao = OperadoresDE.SelecaoAleatoria(3, populacao); break;
                case SelecaoDE.Best1Bin: selecao = OperadoresDE.SelecaoBest(populacao); break;
                case SelecaoDE.Rand2Bin: selecao = OperadoresDE.SelecaoAleatoria(5, populacao); break;
                case SelecaoDE.RandToBest1Bin:
                    selecao = OperadoresDE.SelecaoAleatoria(2, populacao);
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(0, populacao.First());
                    selecao.Insert(0, populacao[atualInd]);
                    break;
                case SelecaoDE.RandToBest2Bin:
                    selecao = OperadoresDE.SelecaoAleatoria(4, populacao);
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(0, populacao.First());
                    selecao.Insert(0, populacao[atualInd]);
                    break;
                case SelecaoDE.CurrentToRand1Bin:
                    selecao = OperadoresDE.SelecaoAleatoria(3, populacao);
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(2, populacao[atualInd]);
                    break;
                default: return;
            }

            IndividuoBin individuoTemp = new IndividuoBin();

            int jRand = rand.Next(0, _nAtributos - 1);
            for (int j = 0; j < _nAtributos; j++)
            {
                if (rand.NextDouble() < _probCross || j == jRand)
                {
                    double atributo = CalculoAtributo(selecao, j, _fatorF);

                    // tratamento de restrição
                    if (atributo >= _min(j) && atributo <= _max(j)
                        && (valFront == null || valFront(atributo, j)))
                    {
                        individuoTemp.Atributos.Add(atributo);
                        continue;
                    }
                }

                individuoTemp.Atributos.Add(populacao[atualInd].Atributos[j]);
            }

            individuoTemp.Aptidao = aptidao(individuoTemp.Atributos);
            if (individuoTemp.Aptidao < populacao[atualInd].Aptidao)
                populacao[atualInd] = individuoTemp;
        }

        //double _fatorFUsado;
        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            //if (_fatorF > 0)
            //    _fatorFUsado = _fatorF / (Math.Pow(20, (double)_avaliacoes / _maxAval));
            //_fatorFUsado = _fatorF;

            for (int i = 0; i < populacao.Count; i++)
                ExecutarMutacao(i, populacao, _tipoSelecao, _fatorF, _nAtributos, _probCross, _min, _max, _validarFronteira, FuncaoAptidao);
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