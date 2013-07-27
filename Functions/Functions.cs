using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public static class Functions
    {
        public static void SelecionarFuncao(out FuncAptidao funcao, out double min, out double max, out int nGeracoes,
            string selecao)
        {
            switch (selecao)
            {
                default: { funcao = F1; min = -100; max = 100; nGeracoes = 1500; break; }
                case "F2": { funcao = F2; min = -10; max = 10; nGeracoes = 2000; break; }
                case "F3": { funcao = F3; min = -100; max = 100; nGeracoes = 5000; break; }
                case "F4": { funcao = F4; min = -100; max = 100; nGeracoes = 5000; break; }
                case "F5": { funcao = F5; min = -30; max = 30; nGeracoes = 20000; break; }
                case "F6": { funcao = F6; min = -100; max = 100; nGeracoes = 1500; break; }
                case "F7": { funcao = F7; min = -1.28; max = 1.28; nGeracoes = 3000; break; }
                case "F8": { funcao = F8; min = -500; max = 500; nGeracoes = 9000; break; }
                case "F9": { funcao = F9; min = -5.12; max = 5.12; nGeracoes = 5000; break; }
                case "F10": { funcao = F10; min = -32; max = 32; nGeracoes = 1500; break; }
                case "F11": { funcao = F11; min = -600; max = 600; nGeracoes = 2000; break; }
                case "F12": { funcao = F12; min = -50; max = 50; nGeracoes = 1500; break; }
                case "F13": { funcao = F13; min = -50; max = 50; nGeracoes = 1500; break; }
            }
        }

        // [-100,100]
        public static double F1(IList<double> chromo)
        {
            return chromo.Sum(c => c * c);
        }

        // [-10,10]
        public static double F2(IList<double> chromo)
        {
            double product = 1;
            foreach (double c in chromo)
                product *= Math.Abs(c);

            return chromo.Sum(c => Math.Abs(c)) + product;
        }

        // [-100,100]
        public static double F3(IList<double> chromo)
        {
            double sum = 0;
            for (int i = 0; i < chromo.Count; i++)
            {
                for (int j = 0; j <= i; j++)
                    sum += chromo[j] * chromo[j];
            }
            return sum;
        }

        // [-100,100]
        public static double F4(IList<double> chromo)
        {
            return chromo.Select(c => Math.Abs(c)).OrderByDescending(c => c).First();
        }


        // [-30,30]
        public static double F5(IList<double> chromo)
        {
            double sum = 0;
            for (int i = 0; i < chromo.Count - 1; i++)
            {
                sum += 100 * Math.Pow(chromo[i + 1] - (chromo[i] * chromo[i]), 2) + Math.Pow(chromo[i] - 1, 2);
            }
            return sum;
        }

        // [-100,100]
        public static double F6(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Pow(Math.Floor(c + .5), 2));
        }

        // [-1.28,1.28]
        public static double F7(IList<double> chromo)
        {
            double res = 0;
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < chromo.Count; i++)
                res += (i + 1) * Math.Pow(chromo[i], 4);
            return res + rand.NextDouble();
        }

        // [-500,500]
        public static double F8(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Sin(Math.Sqrt(Math.Abs(c))) * (-c));
        }

        // [-5.12,5.12]
        public static double F9(IList<double> chromo)
        {
            return chromo.Sum(c => c * c - 10 * Math.Cos(2 * Math.PI * c) + 10);
        }

        // [-32,32]
        public static double F10(IList<double> chromo)
        {
            double sum1 = chromo.Sum(c => c * c);
            double sum2 = chromo.Sum(c => Math.Cos(2 * Math.PI * c));
            int n = chromo.Count;
            return -20 * Math.Exp(-.2 * Math.Sqrt(sum1 / n)) - Math.Exp(sum2 / n) + 20 + Math.E;
        }

        // [-600,600]
        public static double F11(IList<double> chromo)
        {
            double res = chromo.Sum(c => c * c) / 4000;
            double prod = 1;
            for (int i = 0; i < chromo.Count; i++)
                prod *= Math.Cos(chromo[i] / Math.Sqrt(i + 1));

            return res - prod + 1;
        }

        #region F12

        // [-50,50]
        public static double F12(IList<double> chromo)
        {
            return (Math.PI / chromo.Count) * (Parte1(chromo[0]) + Parte2(chromo) +
                Parte3(chromo.Last())) + Parte4(chromo);
        }

        private static double Parte1(double x)
        {
            return 10 * Math.Pow(Math.Sin(Math.PI * Y(x)), 2);
        }

        private static double Parte2(IList<double> x)
        {
            double soma = 0;
            for (int i = 0; i < x.Count - 1; i++)
            {
                soma += (Math.Pow(Y(x[i]) - 1, 2)) * (1 + Parte1(x[i + 1]));
            }
            return soma;
        }

        private static double Parte3(double x)
        {
            return Math.Pow(Y(x) - 1, 2);
        }

        private static double Parte4(IList<double> x)
        {
            double soma = 0;
            for (int i = 0; i < x.Count; i++)
            {
                soma += U(x[i], 10, 100, 4);
            }
            return soma;
        }

        private static double Y(double x)
        {
            return 1 + ((x + 1) / 4);
        }

        #endregion

        private static double U(double x, double a, double k, double m)
        {
            if (x > a)
                return k * Math.Pow(x - a, m);
            if (x < -a)
                return k * Math.Pow(-x - a, m);
            return 0;
        }

        #region F13

        // [-50,50]
        public static double F13(IList<double> chromo)
        {
            return 0.1 * (F13_1(chromo.First()) + F13_2(chromo) + F13_3(chromo.Last()) + F13_4(chromo));
        }

        private static double F13_1(double x)
        {
            return Math.Pow(Math.Sin(3 * Math.PI * x), 2);
        }

        private static double F13_2(IList<double> x)
        {
            double soma = 0;
            for (int i = 0; i < x.Count - 1; i++)
                soma += Math.Pow(x[i] - 1, 2) * (1 + F13_1(x[i + 1]));
            return soma;
        }

        private static double F13_3(double x)
        {
            return Math.Pow(x - 1, 2) * (1 + Math.Pow(Math.Sin(2 * Math.PI * x), 2));
        }

        private static double F13_4(IList<double> x)
        {
            double soma = 0;
            for (int i = 0; i < x.Count; i++)
                soma += U(x[i], 5, 100, 4);
            return soma;
        }

        #endregion
    }
}
