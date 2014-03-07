using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlgoCore;
using Functions;
using AlgoResult;

namespace DECore
{
    public class RotinaRandomDE : RotinaAlgo
    {
        private double _cr, _f, _margem;
        private int _ger;

        public RotinaRandomDE(double cr, double f, double margem, int ger,
            FuncAptidao ap, FuncRepopRestricao rp, ListAptidao gs, ListAptidao hs, FuncValidarRestricao vl, FuncValidarFronteira fr)
            : base(ap, rp, gs, hs, vl, fr)
        {
            _cr = cr;
            _f = f;
            _margem = margem;
            _ger = ger;
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao) { }

        private int _gerFalhas;
        private double _melhorAptAnterior = double.MaxValue;
        public override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            if (_ger != 0 && _gerFalhas >= _ger)
            {
                IndividuoBin individuo = IndividuoBin.GerarPopulacao<IndividuoBin>(1, _min, _max,
                    _nAtributos, IndividuoBin.Precisao).FirstOrDefault();

                _gerFalhas = 0;
                _melhorAptAnterior = double.MaxValue;

                List<IndividuoBin> popAux = populacao.Concat(new List<IndividuoBin> { individuo }).ToList();

                DEUtil.ExecutarMutacao(0, popAux,
                    (SelecaoDE)new Random(AlgoUtil.GetSeed()).Next(0, 5),
                    _f == 0 ? new Random(AlgoUtil.GetSeed()).NextDouble() * 0.9 + 0.1 : _f,
                    _nAtributos,
                    _cr == 0 ? new Random(AlgoUtil.GetSeed()).NextDouble() * 0.6 + 0.3 : _cr,
                    _min, _max, _validarFronteira, FuncaoAptidao, null, null, _margem);
            }
            else
                for (int i = 0; i < populacao.Count; i++)
                {
                    DEUtil.ExecutarMutacao(i, populacao,
                        (SelecaoDE)new Random(AlgoUtil.GetSeed()).Next(0, 5),
                        _f == 0 ? new Random(AlgoUtil.GetSeed()).NextDouble() * 0.9 + 0.1 : _f,
                        _nAtributos,
                        _cr == 0 ? new Random(AlgoUtil.GetSeed()).NextDouble() * 0.6 + 0.3 : _cr,
                        _min, _max, _validarFronteira, FuncaoAptidao, null, null, _margem);
                }

            if (_ger == 0) return;

            double melhorApt = populacao.Min(x => x.Aptidao);
            
            if (melhorApt < _melhorAptAnterior)
            {
                _melhorAptAnterior = melhorApt;
                _gerFalhas = 0;
            }
            else _gerFalhas++;
        }
    }
}
