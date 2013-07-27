using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGCore
{
    public class IndividuoBin
    {
        public IndividuoBin()
        {
            ID = NextID;
            Aptidao = double.MaxValue;
            Atributos = new List<Numero>();
        }

        public bool Proximo(double distancia, IndividuoBin ind2)
        {
            double sqrSum = 0;
            for (int i = 0; i < Atributos.Count; i++)
            {
                sqrSum += Math.Pow(Atributos[i].ValorReal - ind2.Atributos[i].ValorReal, 2);
            }

            sqrSum = Math.Sqrt(sqrSum);

            return sqrSum <= distancia;
        }

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

        public int ID { get; private set; }
        public double Aptidao { get; set; }
        public List<Numero> Atributos { get; set; }

        public IndividuoBin Clone()
        {
            List<Numero> novosAtributos = new List<Numero>();
            foreach (Numero num in Atributos)
                novosAtributos.Add(new Numero(Precisao) { ValorReal = num.ValorReal });

            return new IndividuoBin { ID = ID, Aptidao = Aptidao, Atributos = novosAtributos };
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
    }
}
