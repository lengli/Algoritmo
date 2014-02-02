using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Functions.Attributes;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Functions
{
    public static class Functions
    {
        public static void SelecionarFuncao(out FuncAptidao funcao, out FuncRepopRestricao restricao,
            out ListAptidao gs, out ListAptidao hs,
            out FuncValidarRestricao validar, out FuncValidarFronteira validarFronteira,
            out Bound min, out Bound max, out int nGeracoes, out double minGlobal, out double erro,
            string selecao, ref int d)
        {
            validarFronteira = null;
            restricao = null;
            gs = null;
            hs = null;
            validar = null;
            funcao = null;
            min = null;
            max = null;
            nGeracoes = 0;
            minGlobal = 0;
            erro = 0;

            MethodInfo method = typeof(Functions).GetMethod(selecao);
            object att = method.GetCustomAttributes(false).FirstOrDefault();
            if (att == null) return;

            Delegate fDelegate = Delegate.CreateDelegate(typeof(FuncAptidao), typeof(Functions).GetMethod(selecao));
            funcao = (FuncAptidao)fDelegate;

            FunctionAtt fAtt = att as FunctionAtt;
            if (fAtt != null)
            {
                min = fAtt.MinBound;
                max = fAtt.MaxBound;
                nGeracoes = fAtt.MaxGen;
                minGlobal = fAtt.MinGlobal;
                erro = fAtt.Error;
            }

            FuncRestrAttr fRestrAtt = att as FuncRestrAttr;
            if (fRestrAtt != null)
            {
                d = fRestrAtt.Dimension;
                gs = fRestrAtt.Gs;
                hs = fRestrAtt.Hs;
                validar = fRestrAtt.Validate;
                validarFronteira = fRestrAtt.ValidateBounds;
                restricao = fRestrAtt.RepopBounds;
            }
        }

        public static List<string> Funcoes()
        {
            return typeof(Functions).GetMethods().Where(m => m.GetCustomAttributes(false).Any(at => at is FunctionAtt || at is FuncRestrAttr)).Select(x => x.Name).ToList();
        }

        #region F CEC 2013
        // CEC 2013 - http://www.ntu.edu.sg/home/EPNSugan/index_files/CEC2013/CEC2013.htm
        // Mr: Rotation Matrix
        // Os: Shift Point

        [FunctionAtt(1E-8, -100, 100, 1500, -1400)]
        public static double sphere_func(List<double> x) /* Sphere */
        {
            List<double> z = x.shiftfunc();
            return z.Sum(c => c * c) - 1400;
        }

        private static List<double> _os = null;
        private static List<double> Os
        {
            get
            {
                if (_os == null)
                    using (StreamReader sr = new StreamReader("data/shifted_data.txt"))
                    {
                        _os = new List<double>();
                        string firstLine = sr.ReadLine();

                        Regex reg = new Regex("([\\-0-9.e+]+)");

                        Match match = reg.Match(firstLine);
                        while (match.Success)
                        {
                            string val = match.Groups[1].Value.Replace('.', CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0]); ;
                            double vDouble;
                            if (double.TryParse(val, out vDouble)) _os.Add(vDouble);
                            match = match.NextMatch();
                        }
                    }
                return _os;
            }
        }

        private static List<double> _mr = new List<double>();
        private static List<double> Mr(int dim)
        {
            if (_mr.Count == dim) return _mr;

            using (StreamReader sr = new StreamReader("data/M_D" + dim + ".txt"))
            {
                _mr = new List<double>();

                for (int i = 0; i < dim; i++)
                {
                    string line = sr.ReadLine();

                    Regex reg = new Regex("([\\-0-9.e+]+)");

                    Match match = reg.Match(line);
                    while (match.Success)
                    {
                        string val = match.Groups[1].Value;
                        double vDouble;
                        if (double.TryParse(val, out vDouble)) _mr.Add(vDouble);
                        match = match.NextMatch();
                    }
                }
            }
            return _mr;
        }

        public static List<double> shiftfunc(this List<double> x)
        {
            List<double> xshift = new List<double>();
            for (int i = 0; i < x.Count; i++)
                xshift.Add(x[i] - Os[i]);
            return xshift;
        }

        public static List<double> rotatefunc(this List<double> x)
        {
            List<double> xrot = new List<double>();
            int nx = xrot.Count;
            for (int i = 0; i < nx; i++)
            {
                xrot.Add(0);
                for (int j = 0; j < nx; j++)
                    xrot[i] = xrot[i] + x[j] * Mr(nx)[i * nx + j];

            }

            return xrot;
        }

        #endregion

        #region F CEC 2005

        [FunctionAtt(1E-6, -100, 100, 1500, 0)]
        public static double F1(IList<double> chromo)
        {
            return chromo.Sum(c => c * c);
        }

        [FunctionAtt(1E-6, -10, 10, 2000, 0)]
        public static double F2(IList<double> chromo)
        {
            double product = 1;
            foreach (double c in chromo)
                product *= Math.Abs(c);

            return chromo.Sum(c => Math.Abs(c)) + product;
        }

        [FunctionAtt(1E-4, -100, 100, 5000, 0)]
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

        [FunctionAtt(1E-4, -100, 100, 5000, 0)]
        public static double F4(IList<double> chromo)
        {
            return chromo.Select(Math.Abs).OrderByDescending(c => c).First();
        }

        [FunctionAtt(1E-4, -30, 30, 20000, 0)]
        public static double F5(IList<double> chromo)
        {
            double sum = 0;
            for (int i = 0; i < chromo.Count - 1; i++)
            {
                sum += 100 * Math.Pow(chromo[i + 1] - (chromo[i] * chromo[i]), 2) + Math.Pow(chromo[i] - 1, 2);
            }
            return sum;
        }

        [FunctionAtt(1E-6, -100, 100, 1500, 0)]
        public static double F6(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Pow(Math.Floor(c + .5), 2));
        }

        private static int F7Seed;
        [FunctionAtt(1E-4, -1.28, 1.28, 3000, 0)]
        public static double F7(IList<double> chromo)
        {
            double res = 0;
            Random rand = new Random(F7Seed++);
            for (int i = 0; i < chromo.Count; i++)
                res += (i + 1) * Math.Pow(chromo[i], 4);
            res *= rand.NextDouble();
            return res;
        }

        [FunctionAtt(1E-4, -500, 500, 9000, -.125694866181649E05)]
        public static double F8d30(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Sin(Math.Sqrt(Math.Abs(c))) * (-c));
        }

        [FunctionAtt(1E-4, -500, 500, 9000, -0.209491443636081E+05)]
        public static double F8d50(IList<double> chromo) { return F8d30(chromo); }

        [FunctionAtt(1E-6, -5.12, 5.12, 5000, 0)]
        public static double F9(IList<double> chromo)
        {
            return chromo.Sum(c => c * c - 10 * Math.Cos(2 * Math.PI * c) + 10);
        }

        [FunctionAtt(1E-6, -32, 32, 1500, 0)]
        public static double F10(IList<double> chromo)
        {
            double sum1 = chromo.Sum(c => c * c);
            double sum2 = chromo.Sum(c => Math.Cos(2 * Math.PI * c));
            int n = chromo.Count;
            return -20 * Math.Exp(-.2 * Math.Sqrt(sum1 / n)) - Math.Exp(sum2 / n) + 20 + Math.E;
        }

        [FunctionAtt(1E-4, -600, 600, 2000, 0)]
        public static double F11(IList<double> chromo)
        {
            double res = chromo.Sum(c => c * c) / 4000;
            double prod = 1;
            for (int i = 0; i < chromo.Count; i++)
                prod *= Math.Cos(chromo[i] / Math.Sqrt(i + 1));

            return res - prod + 1;
        }

        #region F12

        [FunctionAtt(1E-4, -50, 50, 1500, 0)]
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
            if (x > a) return k * Math.Pow(x - a, m);
            if (x < -a) return k * Math.Pow(-x - a, m);
            return 0;
        }

        #region F13

        [FunctionAtt(1E-4, -50, 50, 1500, 0)]
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

        #endregion

        #region G

        private const double Epsilon = 0.0001;

        #region G1

        [FuncRestrAttr(1E-4, 0, 100, 1500, -15, 13, "G1_gs", null, "G1_Valid", "G1_Bounds", "G1_Repop_Bounds")]
        public static double G1(IList<double> x)
        {
            return 5 * x.Take(4).Sum() -
                5 * x.Take(4).Sum(xi => xi * xi) -
                x.Skip(4).Sum();
        }

        public static List<List<double>> G1_Repop_Bounds(int npop)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<List<double>> pop = new List<List<double>>();

            while (pop.Count != npop)
            {
                List<double> ind = new List<double>();

                for (int i = 0; i < 13; i++)
                    ind.Add(0);

                for (int i = 0; i < 2000; i++)
                {
                    if (pop.Count == npop) break;

                    #region [7] [8] [11]

                    ind[7] = rand.NextDouble();
                    ind[8] = rand.NextDouble();
                    ind[11] = rand.NextDouble() * 100;
                    if (-2 * ind[7] - ind[8] + ind[11] > 0) continue;

                    #endregion

                    #region [0] [2] [9]

                    ind[0] = rand.NextDouble();
                    ind[2] = rand.NextDouble();
                    ind[9] = rand.NextDouble() * 100;
                    if (2 * ind[0] + 2 * ind[2] + ind[9] + ind[11] - 10 > 0) continue;

                    #endregion

                    #region [1] [10]

                    ind[1] = rand.NextDouble();
                    ind[10] = rand.NextDouble() * 100;
                    if (2 * ind[1] + 2 * ind[2] + ind[10] + ind[11] - 10 > 0) continue;

                    #endregion

                    #region

                    if (2 * ind[0] + 2 * ind[1] + ind[9] + ind[10] - 10 > 0) continue;

                    #endregion

                    #region [3] [4]

                    ind[3] = rand.NextDouble();
                    ind[4] = rand.NextDouble();
                    if (-2 * ind[3] - ind[4] + ind[9] > 0) continue;

                    #endregion

                    #region [5] [6]

                    ind[5] = rand.NextDouble();
                    ind[6] = rand.NextDouble();
                    if (-2 * ind[5] - ind[6] + ind[10] > 0) continue;

                    #endregion

                    ind[12] = rand.NextDouble();

                    pop.Add(ind);
                    break;
                }
            }

            return pop;
        }

        public static bool G1_Bounds(double parametro, int indice)
        {
            if (indice == 12 || indice < 9)
                return parametro >= 0 && parametro <= 1;
            return parametro >= 0 && parametro <= 100;
        }

        public static bool G1_Valid(IList<double> x)
        {
            // g1
            return (2 * x[0] + 2 * x[1] + x[9] + x[10] - 10 <= 0) &&
                //g2
                (2 * x[0] + 2 * x[2] + x[9] + x[11] - 10 <= 0) &&
                //g3
                (2 * x[1] + 2 * x[2] + x[10] + x[11] - 10 <= 0) &&
                //g7
                (-2 * x[3] - x[4] + x[9] <= 0) &&
                //g8
                (-2 * x[5] - x[6] + x[10] <= 0) &&
                //g9
                (-2 * x[7] - x[8] + x[11] <= 0) &&
                x.All(xi => xi >= 0) &&
                x.Take(9).All(xi => xi <= 1) &&
                x.Skip(9).Take(3).All(xi => xi <= 100) &&
                x[12] <= 1;
        }

        public static List<FuncAptidao> G1_gs()
        {
            return new List<FuncAptidao> { G1_g1, G1_g2, G1_g3, G1_g7, G1_g8, G1_g9 };
        }

        public static double G1_g1(List<double> x)
        {
            return 2 * x[0] + 2 * x[1] + x[9] + x[10] - 10;
        }
        public static double G1_g2(List<double> x)
        {
            return 2 * x[0] + 2 * x[2] + x[9] + x[11] - 10;
        }
        public static double G1_g3(List<double> x)
        {
            return 2 * x[1] + 2 * x[2] + x[10] + x[11] - 10;
        }
        public static double G1_g7(List<double> x)
        {
            return -2 * x[3] - x[4] + x[9];
        }
        public static double G1_g8(List<double> x)
        {
            return -2 * x[5] - x[6] + x[10];
        }
        public static double G1_g9(List<double> x)
        {
            return -2 * x[7] - x[8] + x[11];
        }

        #endregion

        #region G5

        [FuncRestrAttr(1E-4, "MinG5", "MaxG5", 1500, -15, 4, "G5_gs", "G5_hs", "G5_Valid", "G5_Bounds", "G5_Repop_Bounds")]
        public static double G5(IList<double> chromo)
        {
            double res = 3 * chromo[0] + 0.000001 * Math.Pow(chromo[0], 3) + 2 * chromo[1] +
                (0.000002 / 3) * Math.Pow(chromo[1], 3);

            return res;
        }

        public static List<List<double>> G5_Repop_Bounds(int npop)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<List<double>> pop = new List<List<double>>();

            while (pop.Count != npop)
            {
                List<double> ind = new List<double>();

                ind.Add(0);
                ind.Add(0);
                ind.Add(0);
                ind.Add(0);

                for (int i = 0; i < 2000; i++)
                {
                    if (pop.Count == npop) break;

                    ind[3] = rand.NextDouble() * 1.1 - 0.55;
                    if (pop.Any(p => Math.Abs(p[3] - ind[3]) <= 1E-4))
                        continue;

                    ind[2] = X2Fromh5(ind[3]);
                    if (!G5_Bounds(ind[2], 2))
                        continue;

                    if ((-ind[3] + ind[2] - 0.55 > 0) ||
                        (-ind[2] + ind[3] - 0.55 > 0)) continue;

                    ind[1] = 894.8 + 1000 * Math.Sin(ind[2] - .25) + 1000 * Math.Sin(ind[2] - ind[3] - .25);
                    ind[0] = 894.8 + 1000 * Math.Sin(-ind[2] - .25) + 1000 * Math.Sin(-ind[3] - .25);

                    if (!G5_Bounds(ind[0], 0) || !G5_Bounds(ind[1], 1))
                        continue;

                    pop.Add(ind);
                    break;
                }
            }

            return pop;
        }

        public static bool G5_Bounds(double parametro, int indice)
        {
            if (indice <= 1)
                return parametro >= 0 && parametro <= 1200;
            return parametro >= -.55 && parametro <= .55;
        }

        public static double MinG5(int indice)
        {
            if (indice <= 1) return 0;
            return -.55;
        }

        public static double MaxG5(int indice)
        {
            if (indice <= 1) return 1200;
            return .55;
        }

        public static bool G5_Valid(IList<double> x)
        {
            // g1
            return (-x[3] + x[2] - 0.55 <= 0) &&
                //g2
                (-x[2] + x[3] - 0.55 <= 0) &&
                //h3
                Math.Abs
                (1000 * Math.Sin(-x[2] - 0.25) + 1000 * Math.Sin(-x[3] - 0.25) + 894.8 - x[0]) <= 1E-4 &&
                //h4
                Math.Abs
                (1000 * Math.Sin(x[2] - 0.25) + 1000 * Math.Sin(x[2] - x[3] - 0.25) + 894.8 - x[1]) <= 1E-4 &&
                //h5
                Math.Abs
                (1000 * Math.Sin(x[3] - 0.25) + 1000 * Math.Sin(x[3] - x[2] - 0.25) + 1294.8) <= 1E-4 &&
                x[0] >= 0 && x[1] >= 0 && x[2] >= -0.55 && x[3] >= -0.55 &&
                x[0] <= 1200 && x[1] <= 1200 && x[2] <= 0.55 && x[3] <= 0.55;
        }

        public static List<FuncAptidao> G5_gs()
        {
            return new List<FuncAptidao> { G5_g1, G5_g2 };
        }

        public static double G5_g1(List<double> x)
        {
            return -x[3] + x[2] - 0.55;
        }
        public static double G5_g2(List<double> x)
        {
            return -x[2] + x[3] - 0.55;
        }

        public static List<FuncAptidao> G5_hs()
        {
            return new List<FuncAptidao> { G5_h3, G5_h4, G5_h5 };
        }

        public static double G5_h3(List<double> x)
        {
            return 1000 * Math.Sin(-x[2] - 0.25) + 1000 * Math.Sin(-x[3] - 0.25) + 894.8 - x[0];
        }
        public static double G5_h4(List<double> x)
        {
            return 1000 * Math.Sin(x[2] - 0.25) + 1000 * Math.Sin(x[2] - x[3] - 0.25) + 894.8 - x[1];
        }
        public static double G5_h5(List<double> x)
        {
            return 1000 * Math.Sin(x[3] - 0.25) + 1000 * Math.Sin(x[3] - x[2] - 0.25) + 1294.8;
        }

        // retorna x[2] usando x[3] e h5
        public static double X2Fromh5(double x3)
        {
            return x3 - .25 - Math.Asin(-1 * Math.Sin(x3 - 0.25) - 1.2948);
        }

        #endregion

        #endregion
    }
}
