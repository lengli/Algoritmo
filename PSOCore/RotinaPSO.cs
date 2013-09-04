using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoCore;
using AlgoResult;
using Functions;


namespace PSOCore
{
    public class RotinaPSO : RotinaAlgo
    {
        private const string MelhorApt = "MelhorApt";
        private const string MelhorCromo = "MelhorCromo";

        public RotinaPSO(FuncAptidao apt, FuncRepopRestricao restricao,
            List<FuncAptidao> gs, List<FuncAptidao> hs, FuncValidarRestricao validar,
            double fatorPond, double fi1, double fi2,
            bool usarRand1, bool usarRand2, double coefKConstr, bool usarCoefConstr, int nVizinhos)
            : base(apt, restricao, gs, hs, validar)
        {
            _fatorPond = fatorPond;
            _fi1 = fi1;
            _fi2 = fi2;
            this.usarRand1 = usarRand1;
            this.usarRand2 = usarRand2;
            _coefKConstr = coefKConstr;
            _usarCoefConstr = usarCoefConstr;
            _nVizinhos = nVizinhos;
        }

        protected override bool CriterioDeParada(AlgoInfo agInfo)
        {
            return false;
        }

        protected override void InicializarAlgoritmo(List<IndividuoBin> populacao)
        {
            GerarVelocidadesIniciais(_nAtributos, populacao);
        }

        protected override void ExecutarAlgoritmo(List<IndividuoBin> populacao)
        {
            foreach (IndividuoBin individuo in populacao)
            {
                List<double> pi;
                double piApt;
                if (!individuo.ParamExtras.ContainsKey(MelhorApt))
                {
                    pi = individuo.Atributos.Select(n => n).ToList();
                    piApt = individuo.Aptidao;
                    individuo.ParamExtras.Add(MelhorApt, piApt);
                    individuo.ParamExtras.Add(MelhorCromo, pi);
                }
                else
                {
                    piApt = (double)individuo.ParamExtras[MelhorApt];
                    pi = (List<double>)individuo.ParamExtras[MelhorCromo];
                }

                IndividuoBin pg;

                if (_nVizinhos > 0)
                {
                    var vizinhos = populacao.OrderBy(ind => ind.DistEuclidiana(individuo)).Take(_nVizinhos);
                    pg = vizinhos.OrderBy(v => v.Aptidao).First();
                }
                else pg = populacao.OrderBy(ind => ind.Aptidao).FirstOrDefault();

                List<double> velocidade = ((List<double>)individuo.ParamExtras[propVelocidade]);
                for (int i = 0; i < _nAtributos; i++)
                {
                    velocidade[i] = _fatorPond * velocidade[i];
                    velocidade[i] += _fi1 * (pi[i] - individuo.Atributos[i]) * (usarRand1 ? rand.NextDouble() : 1);
                    velocidade[i] += _fi2 * (pg.Atributos[i] - individuo.Atributos[i]) * (usarRand2 ? rand.NextDouble() : 1);
                    velocidade[i] *= CoefConstr();
                    double novoValor = individuo.Atributos[i] + velocidade[i];
                    if (novoValor <= IndividuoBin.Minimo) novoValor = IndividuoBin.Minimo;
                    else if (novoValor >= IndividuoBin.Maximo) novoValor = IndividuoBin.Maximo;
                    individuo.Atributos[i] = novoValor;
                }
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

                if (individuo.Aptidao < piApt)
                {
                    individuo.ParamExtras[MelhorApt] = piApt;
                    individuo.ParamExtras[MelhorCromo] = pi;
                }
            }
        }

        #region privates

        private double _fatorPond = 1;
        private double _fi1;
        private double _fi2;
        private bool usarRand1;
        private bool usarRand2;
        private double _coefKConstr;
        private bool _usarCoefConstr;
        private int _nVizinhos;

        private const string propVelocidade = "Velocidade";

        private double CoefConstr()
        {
            if (_coefKConstr <= 0) return 1;
            double fi = _fi1 + _fi2;
            if (fi <= 4) return Math.Sqrt(_coefKConstr);

            return (2 * _coefKConstr) / (fi - 2 + Math.Sqrt(fi * fi - (4 * fi)));
        }

        private void GerarVelocidadesIniciais(int nAtributos, List<IndividuoBin> populacao)
        {
            foreach (IndividuoBin individuo in populacao)
            {
                if (!individuo.ParamExtras.ContainsKey(propVelocidade))
                    individuo.ParamExtras.Add(propVelocidade, new List<double>());

                for (int i = 0; i < nAtributos; i++)
                {
                    //double r = rand.NextDouble();
                    //r *= (_max - _min);
                    //r += _min;

                    ((List<double>)individuo.ParamExtras[propVelocidade]).Add(0);
                }
            }
        }

        #endregion
    }
}
