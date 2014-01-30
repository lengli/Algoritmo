using System;
using System.Collections.Generic;
using System.Linq;
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
            ListAptidao gs, ListAptidao hs, FuncValidarRestricao validar,
            double fatorPond, double fi1, double fi2,
            bool usarRand1, bool usarRand2, double coefKConstr,
            bool usarCoefConstr, int nVizinhos, FuncValidarFronteira valFront)
            : base(apt, restricao, gs, hs, validar, valFront)
        {
            _fatorPond = fatorPond;
            _fi1 = fi1;
            _fi2 = fi2;
            this.usarRand1 = usarRand1;
            this.usarRand2 = usarRand2;
            _coefKConstr = coefKConstr;
            _usarCoefConstr = usarCoefConstr;
            _nVizinhos = nVizinhos;
            _fatorPondUsado = _fatorPond;
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
            _fatorPondUsado = _fatorPond - (0.5 * _fatorPond * _avaliacoes / _maxAval);
            for (int j = populacao.Count - 1; j >= 0; j--)
            {
                IndividuoBin individuo = populacao[j];
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
                    double vel = velocidade[i];
                    vel = _fatorPondUsado * vel;
                    vel += _fi1 * (pi[i] - individuo.Atributos[i]) * (usarRand1 ? rand.NextDouble() : 1);
                    vel += _fi2 * (pg.Atributos[i] - individuo.Atributos[i]) * (usarRand2 ? rand.NextDouble() : 1);

                    //double u = (1 - Math.Abs(pg.Atributos[i] - individuo.Atributos[i])) / (IndividuoBin.Maximo - IndividuoBin.Minimo);
                    //vel += _fi2 * u * u * (usarRand2 ? rand.NextDouble() : 1);
                    //vel *= CoefConstr();

                    // velocidade limitada entre o mínimo e o máximo
                    if (vel <= IndividuoBin.Minimo(i)) vel = IndividuoBin.Minimo(i);
                    else if (vel >= IndividuoBin.Maximo(i)) vel = IndividuoBin.Maximo(i);
                    velocidade[i] = vel;

                    double novoValor = individuo.Atributos[i] + vel;
                    if (novoValor <= IndividuoBin.Minimo(i)) novoValor = IndividuoBin.Minimo(i);
                    else if (novoValor >= IndividuoBin.Maximo(i)) novoValor = IndividuoBin.Maximo(i);
                    individuo.Atributos[i] = novoValor;
                }
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos);

                if (individuo.Aptidao < piApt)
                {
                    individuo.ParamExtras[MelhorApt] = individuo.Aptidao;
                    for (int i = 0; i < individuo.Atributos.Count; i++)
                        ((List<double>)individuo.ParamExtras[MelhorCromo])[i] = individuo.Atributos[i];
                }
            }
        }

        #region privates

        private double _fatorPond = 1;
        private double _fatorPondUsado = 1;
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
                    //((List<double>)individuo.ParamExtras[propVelocidade]).Add(rand.NextDouble()* (_max - _min)+ _min);
                    ((List<double>)individuo.ParamExtras[propVelocidade]).Add(0);
                    //((List<double>)individuo.ParamExtras[propVelocidade]).Add((IndividuoBin.Maximo - IndividuoBin.Minimo) * .1);
                }
            }
        }

        #endregion
    }
}
