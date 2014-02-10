﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<SelecaoDE, double> _crm = new Dictionary<SelecaoDE, double>();

        // para _lp gerações guarda o CR bem sucedidos de cada tipo de DE
        private Dictionary<SelecaoDE, List<double>> _crSucessos = new Dictionary<SelecaoDE, List<double>>();
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
            for (int k = 0; k < _nEstrategias; k++) _crm.Add(selecoes[k], 0.5);
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            Random ran = new Random(DateTime.Now.Millisecond);

            #region 3.1
            List<double> pk = new List<double>();
            if (g > _lp)
            {
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
                    for (int k = 0; k < _nEstrategias; k++)
                        _crm[_selecoes[k]] = _crSucessos[_selecoes[k]].Median();
                }
                List<double> crs = new List<double>();
                for (int k = 0; k < _nEstrategias; k++)
                {
                    double cr = -1;
                    while (cr < 0 || cr > 1)
                        cr = ran.RandomNormal(_crm[_selecoes[k]], 0.1);
                    crs.Add(cr);
                }
                if (!individuo.ParamExtras.ContainsKey(keyCR)) individuo.ParamExtras.Add(keyCR, crs);
                else individuo.ParamExtras[keyCR] = crs;

            }

            #endregion

            #region 3.3 / 3.5

            for (int i = 0; i < populacao.Count; i++)
            {
                IndividuoBin ind = populacao[i];
                DEUtil.ExecutarMutacao(i, populacao, (SelecaoDE)ind.ParamExtras[keyEstrategia],
                   (double)ind.ParamExtras[keyF], _nAtributos, (double)ind.ParamExtras[keyCR], _min, _max,
                   _validarFronteira, FuncaoAptidao, NoSucesso);
            }

            #endregion
        }

        private void NoSucesso(bool sucesso, double cr, SelecaoDE estrategia)
        {
            if (!sucesso)
            {
                List<int> frs = _estrFracassos[estrategia];
                if (frs.Count < _lp) frs.Add(1);
                else frs[frs.Count - 1]++;
                return;
            }
            
            List<int> scs = _estrSucessos[estrategia];
            if (scs.Count < _lp) scs.Add(1);
            else scs[scs.Count - 1]++;

            List<double> crs = _crSucessos[estrategia];
            if (crs.Count < _lp) crs.Add(cr);
            else crs[crs.Count - 1] = cr;
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo) { return false; }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao) { }
    }
}
