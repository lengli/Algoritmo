using System;
using System.Collections.Generic;
using AlgoResult;
using Functions;
using System.Linq;
using AlgoCore;

namespace DECore
{
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

        //double _fatorFUsado;
        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            //if (_fatorF > 0)
            //    _fatorFUsado = _fatorF / (Math.Pow(20, (double)_avaliacoes / _maxAval));
            //_fatorFUsado = _fatorF;

            for (int i = 0; i < populacao.Count; i++)
                DEUtil.ExecutarMutacao(i, populacao, _tipoSelecao, _fatorF, _nAtributos, _probCross, _min, _max, _validarFronteira, FuncaoAptidao);
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