﻿using System;
using System.Collections.Generic;
using System.Linq;
using Functions;

namespace AlgoResult
{
    public class IndividuoBin
    {
        #region static

        private static int seed = int.MinValue;
        public static int GetSeed()
        {
            if (seed == int.MaxValue) seed = int.MinValue;
            else seed++;
            return seed;
        }

        private static int NextID;
        public static int GetNextID { get { return NextID++; } }

        public static int Precisao { get; set; }
        public static Bound Minimo { get; set; }
        public static Bound Maximo { get; set; }

        public static List<T> GerarPopulacao<T>(int nPop, Bound min, Bound max, int nAtributos,
            int precisao, List<IndividuoBin> tabu = null, double tabuDist = 0)
            where T : IndividuoBin, new()
        {
            if (tabuDist == 0) tabuDist = (max(0) - min(0)) / 1000;

            Maximo = max;
            Minimo = min;
            Precisao = precisao;

            List<T> pop = new List<T>(nPop);

            for (int i = 0; i < nPop; i++)
            {
                T ind = new T();
                for (int j = 0; j < nAtributos; j++)
                {
                    double next = new Random(GetSeed()).NextDouble();
                    next *= (max(j) - min(j));
                    next += min(j);
                    ind.Atributos.Add(next);
                }

                if (tabu != null)
                {
                    if (tabu.Any(tInd => tInd.Proximo(tabuDist, ind)))
                    {
                        i--;
                        continue;
                    }
                }

                pop.Add(ind);
            }

            return pop;
        }

        #endregion

        public IndividuoBin()
        {
            ID = NextID;
            Aptidao = double.MaxValue;
            Atributos = new List<double>();
            ParamExtras = new Dictionary<string, object>();
        }

        public bool Proximo(double distancia, IndividuoBin ind2)
        {
            // distancia euclidiana
            double sqrSum = 0;
            for (int i = 0; i < Atributos.Count; i++)
                sqrSum += Math.Pow(Atributos[i] - ind2.Atributos[i], 2);

            sqrSum = Math.Sqrt(sqrSum);

            return sqrSum <= distancia;
        }

        public int ID { get; protected set; }
        public double Aptidao { get; set; }
        private List<double> _atributos;
        public List<double> Atributos
        {
            get
            {
                return _atributos;
            }
            set
            {
                // reconstroi o individuo
                ID = NextID;
                Aptidao = double.MaxValue;
                _atributos = value;
            }
        }
        public Dictionary<string, object> ParamExtras { get; set; }

        public IndividuoBin Clone()
        {
            List<double> novosAtributos = new List<double>();
            foreach (double num in Atributos)
                novosAtributos.Add(num);

            IndividuoBin individuo = new IndividuoBin { ID = ID, Atributos = novosAtributos, ParamExtras = ParamExtras };
            individuo.Aptidao = Aptidao;
            return individuo;
        }
        /*
        private List<bool> _cromossomo = null;
        public List<bool> Cromossomo
        {
            get
            {
                if (_cromossomo != null)
                    return _cromossomo;

                _cromossomo = new List<bool>();

                // encadeia os números como cadeia de bits
                foreach (double at in Atributos)
                    _cromossomo.AddRange(at.GetBits(Maximo, Minimo));

                return _cromossomo;
            }
            set
            {
                // reconstroi o individuo
                ID = NextID;
                Aptidao = double.MaxValue;

                int nBits = Numero.NBits(Maximo - Minimo, Precisao);

                for (int i = 0; i < Atributos.Count; i++)
                {
                    // dividindo cromossomo em atributos
                    List<bool> cromoParcial = value.Skip(i * nBits).Take(nBits).ToList();
                    Atributos[i].SetBits(cromoParcial, Minimo, Maximo);
                }
                _cromossomo = null;
            }
        }
        public double Valor(int indiceAtributo)
        {
            return Atributos[indiceAtributo].ValorReal;
        }

        public void AtualizarCromo(List<bool> cromo)
        {
            List<bool> tempCromo = cromo;
            Cromossomo = cromo;
            // limitação da função
            if (Atributos.Any(a => a.ValorReal > IndividuoBin.Maximo || a.ValorReal < IndividuoBin.Minimo))
                Cromossomo = tempCromo;
        }
        */
    }
}
