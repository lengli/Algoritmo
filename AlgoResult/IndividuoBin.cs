using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoResult
{
    public class IndividuoBin
    {
        #region static

        private static int NextID;

        public static int GetNextID
        {
            get
            {
                return NextID++;
            }

        }

        public static int Precisao { get; set; }
        public static double Minimo { get; set; }
        public static double Maximo { get; set; }

        public static Random rand = new Random(DateTime.Now.Millisecond);
        public static List<T> GerarPopulacao<T>(int nPop, double min, double max, int nAtributos,
            int precisao, List<IndividuoBin> tabu = null, double tabuDist = 0)
            where T : IndividuoBin, new()
        {
            if (tabuDist == 0) tabuDist = (max - min) / 1000;

            IndividuoBin.Maximo = max;
            IndividuoBin.Minimo = min;
            IndividuoBin.Precisao = precisao;

            List<T> pop = new List<T>(nPop);

            for (int i = 0; i < nPop; i++)
            {
                T ind = new T();
                for (int j = 0; j < nAtributos; j++)
                {
                    double next = rand.NextDouble();
                    next *= (max - min);
                    next += min;
                    ind.Atributos.Add(new Numero(precisao) { ValorReal = next });
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
            Atributos = new List<Numero>();
            ParamExtras = new Dictionary<string, object>();
        }

        public bool Proximo(double distancia, IndividuoBin ind2)
        {
            // distancia euclidiana
            double sqrSum = 0;
            for (int i = 0; i < Atributos.Count; i++)
            {
                sqrSum += Math.Pow(Atributos[i].ValorReal - ind2.Atributos[i].ValorReal, 2);
            }

            sqrSum = Math.Sqrt(sqrSum);

            return sqrSum <= distancia;
        }

        public int ID { get; protected set; }
        public double Aptidao { get; set; }
        private List<Numero> _atributos;
        public List<Numero> Atributos
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
            List<Numero> novosAtributos = new List<Numero>();
            foreach (Numero num in Atributos)
                novosAtributos.Add(new Numero(Precisao) { ValorReal = num.ValorReal });

            return new IndividuoBin { ID = ID, Aptidao = Aptidao, Atributos = novosAtributos, ParamExtras = ParamExtras };
        }

        private List<bool> _cromossomo = null;
        public List<bool> Cromossomo
        {
            get
            {
                if (_cromossomo != null)
                    return _cromossomo;

                _cromossomo = new List<bool>();

                // encadeia os números como cadeia de bits
                foreach (Numero at in Atributos)
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
    }
}
