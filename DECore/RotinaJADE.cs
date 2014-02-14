using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoCore;
using AlgoResult;
using Functions;

namespace DECore
{
    public class RotinaJADE : RotinaAlgo
    {
        private double _probCross;
        private double _fatorF;

        public RotinaJADE(FuncAptidao aptidao, FuncRepopRestricao FuncRestr,
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            double probCross, double fatorF, FuncValidarFronteira valFront)
            : base(aptidao, FuncRestr, gs, hs, validar, valFront)
        {
            _probCross = probCross;
            _fatorF = fatorF;
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            for (int i = 0; i < populacao.Count; i++)
                DEUtil.ExecutarMutacao(i, populacao, SelecaoDE.CurrentToPBest1BinArchive, _fatorF, _nAtributos, _probCross, _min, _max, _validarFronteira, FuncaoAptidao);
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo) { return false; }
        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao) { }
        /*
01 Begin
02 Set μCR = 0.5; μF = 0.5; A = 0
03 Create a random initial population {xi,0|i = 1, 2, . . . , NP}
04 For g = 1 to G
05 SF = 0; SCR = 0;
06 For i = 1 to NP
07 Generate CRi = randni (μCR, 0.1), Fi = randci (μF, 0.1)
08 Randomly choose xp
best,g as one of the 100p% best vectors
09 Randomly choose xr1,g = xi,g from current population P
10 Randomly choose ˜xr2,g = xr1,g = xi,g from P ∪ A
11 vi,g = xi,g + Fi · (xp
best,g
− xi,g ) + Fi · (xr1,g − ˜xr2,g)
12 Generate jrand = randint(1, D)
13 For j = 1 to D
14 If j = jrand or rand(0, 1)< CRi
15 u j,i,g = v j,i,g
16 Else
17 u j,i,g = x j,i,g
18 End If
19 End For
20 If f (xi,g ) ≤ f (ui,g )
21 xi,g+1 = xi,g
22 Else
23 xi,g+1 = ui,g ; xi,g →A; CRi → SCR, Fi → SF
24 End If
25 End for
26 Randomly remove solutions from A so that |A| ≤ NP
27 μCR = (1 − c) · μCR + c · meanA(SCR)
28 μF = (1 − c) · μF + c · meanL (SF)
29 End for*/
    }
}
