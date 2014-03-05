using System;
using System.Collections.Generic;
using System.Linq;
using AlgoCore;
using Functions;
using AlgoResult;

namespace DECore
{
    public class RotinaSaDE : RotinaAlgo
    {
        private int _lp;
        private List<SelecaoDE> _selecoes;
        private int _nEstrategias;
        private Dictionary<SelecaoDE, double> _crm = new Dictionary<SelecaoDE, double>();

        // para _lp gerações guarda o CR bem sucedidos de cada tipo de DE
        private Dictionary<SelecaoDE, List<List<double>>> _crSucessos = new Dictionary<SelecaoDE, List<List<double>>>();
        // para _lp gerações guarda o número de sucessos de cada tipo de DE
        private Dictionary<SelecaoDE, List<int>> _estrSucessos = new Dictionary<SelecaoDE, List<int>>();
        // para _lp gerações guarda o número de fracassos de cada tipo de DE
        private Dictionary<SelecaoDE, List<int>> _estrFracassos = new Dictionary<SelecaoDE, List<int>>();

        public RotinaSaDE(FuncAptidao aptidao, FuncRepopRestricao FuncRestr,
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            List<SelecaoDE> selecoes, int learningPeriod, FuncValidarFronteira valFront)
            : base(aptidao, FuncRestr, gs, hs, validar, valFront)
        {
            _lp = learningPeriod;
            _selecoes = selecoes;
            _nEstrategias = selecoes.Count;

            foreach (SelecaoDE estrategia in _selecoes)
            {
                _estrFracassos.Add(estrategia, new List<int>());
                _estrSucessos.Add(estrategia, new List<int>());
                _crSucessos.Add(estrategia, new List<List<double>>());
                _crm.Add(estrategia, 0.5);
            }
        }

        public override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            List<double> pk = new List<double>();
            if (g > _lp)
            {
                // eq. 14
                List<double> somaTodas = new List<double>();
                for (int k = 0; k < _nEstrategias; k++)
                {
                    const double eps = 0.01;

                    double somaSucesso = _estrSucessos[_selecoes[k]].Sum();
                    double somaFracasso = _estrFracassos[_selecoes[k]].Sum();

                    somaTodas.Add(somaSucesso / (somaSucesso + somaFracasso) + eps);
                }
                double somatorio = somaTodas.Sum();
                for (int i = 0; i < _nEstrategias; i++)
                    pk.Add(somaTodas[i] / somatorio);

            }
            else
                for (int i = 0; i < _nEstrategias; i++)
                    pk.Add(1 / (double)_nEstrategias);

            // probabilidade acumulada
            List<double> probPk = new List<double>();
            double somaPk = 0;
            for (int i = 0; i < pk.Count; i++)
            {
                somaPk += pk[i];
                probPk.Add(somaPk);
            }

            foreach (SelecaoDE estrategia in _selecoes)
            {
                if (g >= _lp)
                {
                    // tamanho fixo
                    _estrFracassos[estrategia].RemoveAt(_lp - 1);
                    _estrSucessos[estrategia].RemoveAt(_lp - 1);
                    _crSucessos[estrategia].RemoveAt(_lp - 1);
                }

                _estrFracassos[estrategia].Add(0);
                _estrSucessos[estrategia].Add(0);
                _crSucessos[estrategia].Add(new List<double>());
            }

            for (int i = 0; i < populacao.Count; i++)
            {
                // seleçao da estratégia para o indivíduo
                double random = new Random(AlgoUtil.GetSeed()).NextDouble();
                int indiceEstrategia = 0;
                for (int j = 0; j < probPk.Count; j++)
                {
                    if (random > probPk[j]) continue;
                    indiceEstrategia = j;
                    break;
                }

                // F
                double fNormal = new Random(AlgoUtil.GetSeed()).RandomNormal(0.5, 0.3);

                // CR
                if (g >= _lp)
                {
                    for (int k = 0; k < _nEstrategias; k++)
                    {
                        List<double> crsTemp = new List<double>();

                        foreach (List<double> listCR in _crSucessos[_selecoes[k]])
                            crsTemp.InsertRange(0, listCR);

                        _crm[_selecoes[k]] = crsTemp.Median();
                    }
                }

                double cr = -1;
                while (cr < 0 || cr > 1)
                    cr = new Random(AlgoUtil.GetSeed()).RandomNormal(_crm[_selecoes[indiceEstrategia]], 0.1);

                DEUtil.ExecutarMutacao(i, populacao, _selecoes[indiceEstrategia],
                   fNormal, _nAtributos, cr, _min, _max, _validarFronteira, FuncaoAptidao, NoSucesso);
            }
        }

        private void NoSucesso(bool sucesso, double cr, double f, SelecaoDE estrategia, IndividuoBin indAtual)
        {
            if (!sucesso)
            {
                List<int> frs = _estrFracassos[estrategia];
                frs[0]++;
                return;
            }

            List<int> scs = _estrSucessos[estrategia];
            scs[0]++;

            List<List<double>> crs = _crSucessos[estrategia];
            crs[0].Add(cr);
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao) { }
    }
}
