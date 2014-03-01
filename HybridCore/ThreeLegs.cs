using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoCore;
using AGCore;
using Functions;
using DECore;
using PSOCore;

namespace HybridCore
{
    public class ThreeLegs : RotinaAlgo
    {
        List<RotinaAlgo> _rotinas = new List<RotinaAlgo>();
        public ThreeLegs(FuncAptidao apt, FuncRepopRestricao repop, ListAptidao gs, ListAptidao hs,
            FuncValidarRestricao valRestr, FuncValidarFronteira front)
            : base(apt, repop, gs, hs, valRestr, front)
        {
            _rotinas.Add(new AGClassico(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, 0.5, 0.8, 0.5, 0, 0, CrossType.Aritmetico, front));
            //_rotinas.Add(new AGClassico(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, 0.3, 0.9, 0.9, 0, 0, CrossType.Aritmetico, front));

            _rotinas.Add(new RotinaDE(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, SelecaoDE.RandToBest1Bin, 0.7, 0.5, front));
            _rotinas.Add(new RotinaJADE(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, 0.1, 0.05, false, front));
            _rotinas.Add(new RotinaSaDE(FuncaoAptidaoVirtual, repop, gs, hs, valRestr,
                new List<SelecaoDE> { SelecaoDE.RandToBest2Bin, SelecaoDE.Rand2Bin, SelecaoDE.Best1Bin },
                20, front));

            _rotinas.Add(new RotinaPSO(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, 0.3, 1.5, 1.5, true, true, 0, false, 0, front));
            //_rotinas.Add(new RotinaPSO(FuncaoAptidaoVirtual, repop, gs, hs, valRestr, 0.5, 0.7, 1.5, true, true, 0, false, 0, front));

        }

        protected override bool CriterioDeParada(AlgoResult.AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<AlgoResult.IndividuoBin> populacao)
        {
            foreach (RotinaAlgo algo in _rotinas)
            {
                algo.Rodar(-1, 0, _min, _max, _nAtributos, 0, _maxAval, false, 0, 0, 0, 0, false);
                if (algo is RotinaPSO)
                    (algo as RotinaPSO).GerarVelocidadesIniciais(_nAtributos, populacao);
            }
        }

        public override void ExecutarAlgoritmo(List<AlgoResult.IndividuoBin> populacao)
        {
            double rand = new Random(AlgoUtil.GetSeed()).NextDouble();
            int c = _rotinas.Count;
            for (int i = 0; i < c; i++)
            {
                if (rand > (i + 1) / (double)c) continue;
                _rotinas[i].ExecutarAlgoritmo(populacao);
                break;
            }
        }
    }
}
