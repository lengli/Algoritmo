using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoCore;
using Functions;
using AlgoResult;

namespace DECore
{
    public class RotinaRandomDE : RotinaAlgo
    {
        public RotinaRandomDE(FuncAptidao ap, FuncRepopRestricao rp, ListAptidao gs, ListAptidao hs, FuncValidarRestricao vl, FuncValidarFronteira fr)
            : base(ap, rp, gs, hs, vl, fr) { }

        protected override bool CriterioDeParada(AlgoResult.AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<AlgoResult.IndividuoBin> populacao) { }

        public override void ExecutarAlgoritmo(List<AlgoResult.IndividuoBin> populacao)
        {
            for (int i = 0; i < populacao.Count; i++)
            {
                IndividuoBin individuo = null;
                if (new Random(AlgoUtil.GetSeed()).NextDouble() > 0.0)
                    individuo = IndividuoBin.GerarPopulacao<IndividuoBin>(1, _min, _max,
                        _nAtributos, IndividuoBin.Precisao, null, 0).FirstOrDefault();

                DEUtil.ExecutarMutacao(i, populacao, (SelecaoDE)new Random(AlgoUtil.GetSeed()).Next(0, 5),
                    new Random(AlgoUtil.GetSeed()).NextDouble() * 0.9 + 0.1,
                    _nAtributos, new Random(AlgoUtil.GetSeed()).NextDouble() * 0.6 + 0.3,
                    _min, _max, _validarFronteira, FuncaoAptidao, null, null,
                    //new Random(AlgoUtil.GetSeed()).Next(0, 100));
                    0);
            }
        }
    }
}
