using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoCore;
using Functions;
using AlgoResult;

namespace DECore
{
    public class RotinaSaDE : RotinaAlgo
    {
        private const string keyEstrategia = "Estrategia";
        private const string keyF = "F";
        private const string keyCR = "CR";
        private int _lp;
        private List<SelecaoDE> _selecoes;
        private int _nEstrategias;
        private List<double> _crm = new List<double>();

        // para _lp gerações guarda o CR bem sucedidos de cada tipo de DE
        private List<List<double>> _crSucessos = new List<List<double>>();
        // para _lp gerações guarda o número de sucessos de cada tipo de DE
        private List<List<double>> _estrSucessos = new List<List<double>>();
        // para _lp gerações guarda o número de fracassos de cada tipo de DE
        private List<List<double>> _estrFracassos = new List<List<double>>();

        public RotinaSaDE(FuncAptidao aptidao, FuncRepopRestricao FuncRestr,
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            List<SelecaoDE> selecoes, int learningPeriod, FuncValidarFronteira valFront)
            : base(aptidao, FuncRestr, gs, hs, validar, valFront)
        {
            _lp = learningPeriod;
            _selecoes = selecoes;
            _nEstrategias = selecoes.Count;
            for (int k = 0; k < _nEstrategias; k++) _crm.Add(0.5);
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            Random ran = new Random(DateTime.Now.Millisecond);

            #region 3.1
            List<double> pk = new List<double>();
            if (g > _lp)
            {
                List<double> somaTodas = new List<double>();
                for (int i = 0; i < _nEstrategias; i++)
                {
                    double eps = 0.01;

                    double somaSucesso = _estrSucessos.Sum(x => x[i]);
                    double somaFracasso = _estrFracassos.Sum(x => x[i]);

                    somaTodas.Add(somaSucesso / (somaSucesso + somaFracasso) + eps);
                }
                double somatorio = somaTodas.Sum();
                for (int i = 0; i < _nEstrategias; i++)
                    pk.Add(somaTodas[i] / somatorio);
            }
            else
                for (int i = 0; i < _nEstrategias; i++)
                    pk.Add(1 / _nEstrategias);
            #endregion

            #region 3.2

            // probabilidade acumulada
            List<double> probPk = new List<double>();
            double somaPk = 0;
            for (int i = 0; i < pk.Count; i++)
            {
                somaPk += pk[i];
                probPk.Add(somaPk);
            }

            // seleçao da estratégia para o indivíduo
            foreach (IndividuoBin individuo in populacao)
            {
                double random = ran.NextDouble();
                int indiceEstrategia = 0;
                for (int j = 0; j < probPk.Count; j++)
                {
                    if (random > probPk[j]) continue;
                    indiceEstrategia = j;
                    break;
                }
                if (!individuo.ParamExtras.ContainsKey(keyEstrategia)) individuo.ParamExtras.Add(keyEstrategia, _selecoes[indiceEstrategia]);
                else individuo.ParamExtras[keyEstrategia] = _selecoes[indiceEstrategia];

                // F
                double fNormal = ran.RandomNormal(0.5, 0.3);
                if (!individuo.ParamExtras.ContainsKey(keyF)) individuo.ParamExtras.Add(keyF, fNormal);
                else individuo.ParamExtras[keyF] = fNormal;

                // CR
                if (g >= _lp)
                {
                }
                List<double> crs = new List<double>();
                for (int k = 0; k < _nEstrategias; k++)
                {
                    double cr = -1;
                    while (cr < 0 || cr > 1)
                        cr = ran.RandomNormal(_crm[k], 0.1);
                    crs.Add(cr);
                }
                if (!individuo.ParamExtras.ContainsKey(keyCR)) individuo.ParamExtras.Add(keyCR, crs);
                else individuo.ParamExtras[keyCR] = crs;

            }


            #endregion
        }

        protected override bool CriterioDeParada(AlgoResult.AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<AlgoResult.IndividuoBin> populacao) { }
    }
}
