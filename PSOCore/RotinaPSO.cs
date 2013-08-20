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

        public RotinaPSO(FuncAptidao apt, double fatorPond, double fi1, double fi2,
            bool usarRand1, bool usarRand2, double coefKConstr, bool usarCoefConstr, int nVizinhos)
            : base(apt)
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
                IndividuoBin pi = populacao.OrderBy(p => p.Aptidao).First();

                var vizinhos = populacao.OrderBy(ind => ind.DistEuclidiana(individuo)).Take(_nVizinhos);
                IndividuoBin pg = vizinhos.OrderBy(v => v.Aptidao).First();

                List<double> velocidade = ((List<double>)individuo.ParamExtras[propVelocidade]);
                for (int i = 0; i < _nAtributos; i++)
                {
                    velocidade[i] = _fatorPond * velocidade[i];
                    velocidade[i] += _fi1 * (pi.Valor(i) - individuo.Valor(i)) * (usarRand1 ? rand.NextDouble() : 1);
                    velocidade[i] += _fi2 * (pg.Valor(i) - individuo.Valor(i)) * (usarRand2 ? rand.NextDouble() : 1);
                    velocidade[i] *= CoefConstr();
                    double novoValor = individuo.Valor(i) + velocidade[i];
                    if (novoValor <= IndividuoBin.Minimo) novoValor = IndividuoBin.Minimo;
                    else if (novoValor >= IndividuoBin.Maximo) novoValor = IndividuoBin.Maximo;
                    individuo.Atributos[i].ValorReal = novoValor;
                }
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());
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
