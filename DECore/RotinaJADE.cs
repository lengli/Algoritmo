﻿using System;
using System.Collections.Generic;
using AlgoCore;
using AlgoResult;
using Functions;

namespace DECore
{
    public class RotinaJADE : RotinaAlgo
    {
        private double _mCR = 0.5;
        private double _mF = 0.5;
        private List<double> SF = new List<double>();
        private List<double> SCR = new List<double>();
        private List<IndividuoBin> _A = new List<IndividuoBin>();
        private double _c;
        private double _p;
        private bool _arq;

        public RotinaJADE(FuncAptidao aptidao, FuncRepopRestricao FuncRestr,
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            double c, double p, bool arquivamento,
            FuncValidarFronteira valFront)
            : base(aptidao, FuncRestr, gs, hs, validar, valFront)
        {
            _c = c;
            _p = p;
            _arq = arquivamento;
        }

        private void NoSucesso(bool sucesso, double cr, double f, SelecaoDE estrategia, IndividuoBin indAtual)
        {
            if (sucesso)
            {
                SCR.Add(cr);
                SF.Add(f);
                _mCR = (1 - _c) * _mCR + _c * SCR.Median();
                _mF = (1 - _c) * _mF + _c * SF.LehmerMean();
                return;
            }

            if (_arq)
            {
                if (_A.Count >= _tamanhoPop)
                {
                    int r = new Random(AlgoUtil.GetSeed()).Next(0, _A.Count - 1);
                    _A.RemoveAt(r);
                }
                _A.Add(indAtual);
            }

        }

        public override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            double CRi = new Random(AlgoUtil.GetSeed()).RandomNormal(_mCR, 0.1);
            double Fi = new Random(AlgoUtil.GetSeed()).RandomCauchy(_mF, 0.1);
            if (Fi > 1) Fi = 1;
            if (Fi < 0) Fi = 0;

            for (int i = 0; i < populacao.Count; i++)
                DEUtil.ExecutarMutacao(i, populacao, SelecaoDE.CurrentToPBest1BinArchive, Fi, _nAtributos, CRi, _min, _max,
                    _validarFronteira, FuncaoAptidao, NoSucesso,
                    new Dictionary<string, object> { { "pBest", _p }, { "archive", _A } });
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo) { return false; }
        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao) { }
    }
}
