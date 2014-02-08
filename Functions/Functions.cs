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
            return typeof(Functions).GetMethods().Where(m => m.GetCustomAttributes(false).
                Any(at => at is FunctionAtt)).Select(x => x.Name).OrderBy(x => x).ToList();
        }

        #region F CEC 2013
        // CEC 2013 - http://www.ntu.edu.sg/home/EPNSugan/index_files/CEC2013/CEC2013.htm
        // Mr: Rotation Matrix
        // Os: Shift Point

        /// <summary>
        /// f 01 - Unimodal; Separable
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -1400)]
        public static double F01_sphere(List<double> x) /* Sphere */
        {
            List<double> z = x.shiftfunc();
            return z.Sum(c => c * c) - 1400;
        }

        /// <summary>
        /// f 02 - Unimodal; Non-separable; Quadratic ill-conditioned; Smooth local irregularities
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -1300)]
        public static double F02_ellips(List<double> x) /* Ellipsoidal */
        {
            List<double> y = x.shiftfunc().rotatefunc(0).oszfunc();
            int nx = x.Count;
            double ev = 0;
            for (int i = 0; i < nx; i++)
                ev += Math.Pow(10.0, 6.0 * i / (nx - 1)) * y[i] * y[i];
            return ev - 1300;
        }

        /// <summary>
        /// f 03 - Unimodal; Non-separable; Smooth but narrow ridge
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -1200)]
        public static double F03_bent_cigar(List<double> x) /* Bent_Cigar */
        {
            double beta = 0.5;
            List<double> z = x.shiftfunc().rotatefunc(0).asyfunc(beta).rotatefunc(1);
            double f = z[0] * z[0];

            for (int i = 1; i < x.Count; i++)
                f += Math.Pow(10.0, 6.0) * z[i] * z[i];
            return f - 1200;
        }

        /// <summary>
        /// f 04 - Unimodal; Non-separable; Asymetrical; Smooth local irregularities; One sensitive direction
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -1100)]
        public static double F04_discus(List<double> x) /* Discus */
        {
            List<double> y = x.shiftfunc().rotatefunc(0).oszfunc();

            double f = Math.Pow(10.0, 6.0) * y[0] * y[0];
            for (int i = 1; i < x.Count; i++)
                f += y[i] * y[i];
            return f - 1100;
        }

        /// <summary>
        /// f 05 - Unimodal; Separable; Sensitivities of the Zi-variables are different
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -1000)]
        public static double F05_dif_powers(List<double> x) /* Different Powers */
        {
            List<double> z = x.shiftfunc();
            double f = 0.0;
            int nx = x.Count;
            for (int i = 0; i < nx; i++)
                f += Math.Pow(Math.Abs(z[i]), 2 + 4 * i / (nx - 1));

            return Math.Pow(f, 0.5) - 1000;
        }

        // Basic modal functions

        /// <summary>
        /// f 06 - Multi-modal; Non-separable; Very narrow valley from local optimum to global optimum
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -900)]
        public static double F06_rosenbrock(List<double> x) /* Rotated Rosenbrock's */
        {
            double tmp1, tmp2;
            List<double> z = x.shiftfunc().
                Select(yi => yi * 2.048 / 100.0).//shrink to the orginal search range
                ToList().rotatefunc(0).
                Select(zi => zi + 1).ToList();//shift to orgin

            double f = 0.0;

            for (int i = 0; i < x.Count - 1; i++)
            {
                tmp1 = z[i] * z[i] - z[i + 1];
                tmp2 = z[i] - 1.0;
                f += 100.0 * tmp1 * tmp1 + tmp2 * tmp2;
            }
            return f;
        }

        /// <summary>
        /// f 07 - Multi-modal; Non-separable; Asymetrical; Local optima's number is huge
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -800)]
        public static double F07_schaffer_F7(List<double> x) /* Schwefel's 1.2  */
        {
            double tmp;
            List<double> y = x.shiftfunc().rotatefunc(0).asyfunc(0.5).rotatefunc(1).diagonalfunc(10);
            int nx = x.Count;
            List<double> z = new List<double>();

            for (int i = 0; i < nx - 1; i++)
                z.Add(Math.Pow(y[i] * y[i] + y[i + 1] * y[i + 1], 0.5));

            double f = 0.0;

            for (int i = 0; i < nx - 1; i++)
            {
                tmp = Math.Sin(50.0 * Math.Pow(z[i], 0.2));
                f += Math.Pow(z[i], 0.5) + Math.Pow(z[i], 0.5) * tmp * tmp;
            }

            return f * f / (nx - 1) / (nx - 1) - 800;

        }

        /// <summary>
        /// f 08 - Multi-modal; Non-separable; Asymetrical
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -700)]
        public static double F08_ackley(List<double> x) /* Ackley's  */
        {
            int nx = x.Count;
            List<double> y = x.shiftfunc().rotatefunc(0).
                asyfunc(0.5).rotatefunc(1).diagonalfunc(10);

            double sum1 = 0.0, sum2 = 0.0;
            for (int i = 0; i < nx; i++)
            {
                sum1 += y[i] * y[i];
                sum2 += Math.Cos(2.0 * Math.PI * y[i]);
            }

            sum1 = -0.2 * Math.Sqrt(sum1 / nx);
            sum2 /= nx;

            return -20.0 * Math.Exp(sum1) - Math.Exp(sum2) + 20.0 + Math.E - 700;
        }

        /// <summary>
        /// f 09 - Multi-modal; Non-separable; Asymetrical; Continuous but differentiable only ona a set of points
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -600)]
        public static double F09_weierstrass(List<double> x) /* Weierstrass's  */
        {
            int j, k_max = 20, nx = x.Count;
            double sum = 0, sum2 = 0, a = 0.5, b = 3;

            x = x.shiftfunc().
                Select(yi => yi * 0.5 / 100). //shrink to the orginal search range
                ToList().rotatefunc(0).asyfunc(0.5).rotatefunc(1).diagonalfunc(10);

            double f = 0.0;
            for (int i = 0; i < nx; i++)
            {
                for (j = 0; j <= k_max; j++)
                {
                    sum += Math.Pow(a, j) * Math.Cos(2.0 * Math.PI * Math.Pow(b, j) * (x[i] + 0.5));
                    sum2 += Math.Pow(a, j) * Math.Cos(2.0 * Math.PI * Math.Pow(b, j) * 0.5);
                }
                f += sum;
            }
            return f - nx * sum2 - 600;
        }

        /// <summary>
        /// f 10 - Multi-modal; Rotated; Non-separable;
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -500)]
        public static double F10_griewank(List<double> x) /* Griewank's  */
        {
            int nx = x.Count;
            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] *= 600.0 / 100.0;

            List<double> z = x.rotatefunc(0).diagonalfunc(100);

            double s = 0, p = 1;
            for (int i = 0; i < nx; i++)
            {
                s += z[i] * z[i];
                p *= Math.Cos(z[i] / Math.Sqrt(1.0 + i));
            }

            return 1.0 + s / 4000.0 - p - 500;
        }

        /// <summary>
        /// f 11 - Multi-modal; Separable; Asymetrical;  Local optima's number is huge
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -400)]
        public static double F11_rastrigin(List<double> x) /* Rastrigin's  */
        {
            double alpha = 10.0, beta = 0.2;
            int nx = x.Count;
            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] = x[i] * 5.12 / 100;

            x = x.oszfunc().asyfunc(beta).diagonalfunc(alpha);

            double f = 0;
            for (int i = 0; i < nx; i++)
                f += (x[i] * x[i] - 10.0 * Math.Cos(2.0 * Math.PI * x[i]) + 10.0);
            return f - 400;
        }

        /// <summary>
        /// f 12 - Multi-modal; Non-separable; Asymetrical;  Local optima's number is huge
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -300)]
        public static double F12_rotated_rastrigin(List<double> x) /* Rotated Rastrigin's  */
        {
            double alpha = 10.0, beta = 0.2;
            int nx = x.Count;
            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] = x[i] * 5.12 / 100;

            x = x.rotatefunc(0).oszfunc().asyfunc(beta).
                rotatefunc(1).diagonalfunc(alpha).rotatefunc(0);

            double f = 0;
            for (int i = 0; i < nx; i++)
                f += (x[i] * x[i] - 10.0 * Math.Cos(2.0 * Math.PI * x[i]) + 10.0);
            return f - 300;
        }

        /// <summary>
        /// f 13 - Multi-modal; Rotated; Non-separable; Asymetrical;  Local optima's number is huge; Non-continuous
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -200)]
        public static double F13_step_rastrigin(List<double> x) /* Noncontinuous Rastrigin's  */
        {
            double alpha = 10.0, beta = 0.2;
            int nx = x.Count;
            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] *= 5.12 / 100;

            x = x.rotatefunc(0);

            for (int i = 0; i < nx; i++) // make step
                if (Math.Abs(x[i]) > 0.5)
                    x[i] = Math.Floor(2 * x[i] + 0.5) / 2;

            x = x.oszfunc().asyfunc(beta).rotatefunc(1).diagonalfunc(alpha).rotatefunc(0);

            double f = 0.0;

            for (int i = 0; i < nx; i++)
                f += (x[i] * x[i] - 10.0 * Math.Cos(2.0 * Math.PI * x[i]) + 10.0);

            return f - 200;
        }

        /// <summary>
        /// f 14 - Multi-modal; Non-separable; Asymetrical;  Local optima's number is huge; Second better optimum is far from global
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, -100)]
        public static double F14_nonrotated_schwefel(List<double> x) { return schwefel(x, false) - 100; }

        /// <summary>
        /// f 15 - Multi-modal; Rotated; Non-separable; Asymetrical;  Local optima's number is huge; Second better optimum is far from global
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 100)]
        public static double F15_rotated_schwefel(List<double> x) { return schwefel(x, true) + 100; }

        private static double schwefel(List<double> x, bool r_flag) /* Schwefel's  */
        {
            double tmp;
            int nx = x.Count;
            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] *= 1000 / 100;
            if (r_flag) x = x.rotatefunc(0);
            x = x.diagonalfunc(10);

            for (int i = 0; i < nx; i++)
                x[i] += 4.209687462275036e+002;

            double f = 0;

            for (int i = 0; i < nx; i++)
            {
                if (x[i] > 500)
                {
                    f -= (500.0 - x[i] % 500) * Math.Sin(Math.Pow(500.0 - x[i] % 500, 0.5));
                    tmp = (x[i] - 500.0) / 100;
                    f += tmp * tmp / nx;
                }
                else if (x[i] < -500)
                {
                    f -= (-500.0 + (Math.Abs(x[i]) % 500)) * Math.Sin(Math.Pow(500.0 - (Math.Abs(x[i]) % 500), 0.5));
                    tmp = (x[i] + 500.0) / 100;
                    f += tmp * tmp / nx;
                }
                else
                    f -= x[i] * Math.Sin(Math.Pow(Math.Abs(x[i]), 0.5));
            }
            return 4.189828872724338e+002 * nx + f;
        }

        /// <summary>
        /// f 16 - Multi-modal; Non-separable; Asymetrical; Continuous everywhere; Differentiable nowhere
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 200)]
        public static double F16_katsuura(List<double> x) /* Katsuura  */
        {
            int nx = x.Count;
            double temp, tmp1, tmp2,
                tmp3 = pow(1.0 * nx, 1.2);

            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] *= 5.0 / 100.0;

            List<double> y = x.rotatefunc(0).diagonalfunc(100).rotatefunc(1);

            double f = 1.0;
            for (int i = 0; i < nx; i++)
            {
                temp = 0.0;
                for (int j = 1; j <= 32; j++)
                {
                    tmp1 = pow(2.0, j);
                    tmp2 = tmp1 * y[i];
                    temp += fabs(tmp2 - floor(tmp2 + 0.5)) / tmp1;
                }
                f *= pow(1.0 + (i + 1) * temp, 10.0 / tmp3);
            }

            tmp1 = 10.0 / nx / nx;
            return f * tmp1 - tmp1 + 200;
        }

        /// <summary>
        /// f 17 - Multi-modal; Non-separable; Asymetrical; Continuous everywhere; Differentiable nowhere
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 300)]
        public static double F17_nonrotated_bi_rastrigin(List<double> x) { return bi_rastrigin(x, false) + 300; }

        /// <summary>
        /// f 18 - Multi-modal; Non-separable; Asymetrical; Continuous everywhere; Differentiable nowhere
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 400)]
        public static double F18_rotated_bi_rastrigin(List<double> x) { return bi_rastrigin(x, true) + 400; }

        /********************************************************************************************************/

        // Verificar essa função!!

        /*********************************************************************************************************/

        private static double bi_rastrigin(List<double> x, bool r_flag) /* Lunacek Bi_rastrigin Function */
        {
            double mu0 = 2.5, d = 1.0, tmp, tmp1, tmp2;
            List<double> tmpx = new List<double>();
            int nx = x.Count;
            double s = 1.0 - 1.0 / (2.0 * pow(nx + 20.0, 0.5) - 8.2);
            double mu1 = -pow((mu0 * mu0 - d) / s, 0.5);

            x = x.shiftfunc();

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] *= 10.0 / 100.0;

            for (int i = 0; i < nx; i++)
            {
                tmpx.Add(2 * x[i]);
                if (Os[i] < 0) tmpx[i] *= -1;
            }

            for (int i = 0; i < nx; i++)
            {
                x[i] = tmpx[i];
                tmpx[i] += mu0;
            }

            if (r_flag) x = x.rotatefunc(0);
            x = x.diagonalfunc(100);
            if (r_flag) x = x.rotatefunc(1);

            tmp1 = 0.0; tmp2 = 0.0;

            for (int i = 0; i < nx; i++)
            {
                tmp = tmpx[i] - mu0;
                tmp1 += tmp * tmp;
                tmp = tmpx[i] - mu1;
                tmp2 += tmp * tmp;
            }

            tmp2 *= s;
            tmp2 += d * nx;
            tmp = 0;
            double f;

            for (int i = 0; i < nx; i++)
                tmp += cos(2.0 * PI * x[i]);

            if (tmp1 < tmp2) f = tmp1;
            else f = tmp2;

            f += 10.0 * (nx - tmp);
            return f;
        }

        /// <summary>
        /// f 19 - Multi-modal; Non-separable;
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 500)]
        public static double F19_grie_rosen(List<double> x) /* Griewank-Rosenbrock  */
        {
            double temp, tmp1, tmp2;
            x = x.shiftfunc();
            int nx = x.Count;

            for (int i = 0; i < nx; i++)//shrink to the orginal search range
                x[i] = x[i] * 5 / 100;

            x = x.rotatefunc(0);

            for (int i = 0; i < nx; i++)//shift to orgin
                x[i] = x[i] + 1;

            double f = 0.0;

            for (int i = 0; i < nx - 1; i++)
            {
                tmp1 = x[i] * x[i] - x[i + 1];
                tmp2 = x[i] - 1.0;
                temp = 100.0 * tmp1 * tmp1 + tmp2 * tmp2;
                f += (temp * temp) / 4000.0 - cos(temp) + 1.0;
            }

            tmp1 = x[nx - 1] * x[nx - 1] - x[0];
            tmp2 = x[nx - 1] - 1.0;
            temp = 100.0 * tmp1 * tmp1 + tmp2 * tmp2;
            f += (temp * temp) / 4000.0 - cos(temp) + 1.0;
            return f + 500;
        }

        /// <summary>
        /// f 20 - Multi-modal; Non-separable; Asymetrical;
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 600)]
        public static double F20_escaffer6(List<double> z) /* Expanded Scaffer°Øs F6  */
        {
            double temp1, temp2;
            int nx = z.Count;
            z = z.shiftfunc().rotatefunc(0).asyfunc(0.5)
                .rotatefunc(1);

            double f = 0.0;

            for (int i = 0; i < nx - 1; i++)
            {
                temp1 = sin(sqrt(z[i] * z[i] + z[i + 1] * z[i + 1]));
                temp1 = temp1 * temp1;
                temp2 = 1.0 + 0.001 * (z[i] * z[i] + z[i + 1] * z[i + 1]);
                f += 0.5 + (temp1 - 0.5) / (temp2 * temp2);
            }

            temp1 = sin(sqrt(z[nx - 1] * z[nx - 1] + z[0] * z[0]));
            temp1 = temp1 * temp1;
            temp2 = 1.0 + 0.001 * (z[nx - 1] * z[nx - 1] + z[0] * z[0]);
            f += 0.5 + (temp1 - 0.5) / (temp2 * temp2);
            return f + 600;
        }

        // Composition functions

        /// <summary>
        /// f 21 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 700)]
        public static double F21_cf01(List<double> x) /* Composition Function 1 */
        {
            List<double> fit = new List<double>(5),
                delta = new List<double> { 10, 20, 30, 40, 50 },
                bias = new List<double> { 0, 100, 200, 300, 400 };

            fit.Add(F06_rosenbrock(x));
            fit[0] = 10000 * fit[0] / 1e+4;

            fit.Add(F05_dif_powers(x));
            fit[1] = 10000 * fit[1] / 1e+10;

            fit.Add(F03_bent_cigar(x));
            fit[2] = 10000 * fit[2] / 1e+30;

            fit.Add(F04_discus(x));
            fit[3] = 10000 * fit[3] / 1e+10;

            fit.Add(F01_sphere(x));
            fit[4] = 10000 * fit[4] / 1e+5;

            return cf_cal(x, delta, bias, fit) + 700;
        }

        /// <summary>
        /// f 22 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 800)]
        public static double F22_cf02(List<double> x) /* Composition Function 2 */
        {
            List<double> fit = new List<double>(3);
            List<double> delta = new List<double> { 20, 20, 20 };
            List<double> bias = new List<double> { 0, 100, 200 };

            for (int i = 0; i < 3; i++)
                fit.Add(schwefel(x, false));

            return cf_cal(x, delta, bias, fit) + 800;
        }

        /// <summary>
        /// f 23 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 900)]
        public static double F23_cf03(List<double> x) /* Composition Function 3 */
        {
            List<double> fit = new List<double>();
            List<double> delta = new List<double> { 20, 20, 20 };
            List<double> bias = new List<double> { 0, 100, 200 };

            for (int i = 0; i < 3; i++)
                fit.Add(F15_rotated_schwefel(x));

            return cf_cal(x, delta, bias, fit) + 900;
        }

        /// <summary>
        /// f 24 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 1000)]
        public static double F24_cf04(List<double> x) /* Composition Function 4 */
        {
            return cf0405(x, new List<double> { 20, 20, 20 }) + 1000;
        }

        /// <summary>
        /// f 25 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 1100)]
        public static double F25_cf05(List<double> x) /* Composition Function 5 */
        {
            return cf0405(x, new List<double> { 10, 30, 50 }) + 1100;
        }

        private static double cf0405(List<double> x, List<double> delta)
        {
            List<double> fit = new List<double>();
            List<double> lambdas = new List<double> { 0.25, 1, 2.5 };
            List<double> bias = new List<double> { 0, 100, 200 };

            fit.Add(F15_rotated_schwefel(x) * lambdas[0]);
            fit.Add(F12_rotated_rastrigin(x) * lambdas[1]);
            fit.Add(F09_weierstrass(x) * lambdas[2]);

            return cf_cal(x, delta, bias, fit);
        }

        /// <summary>
        /// f 26 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 1200)]
        public static double F26_cf06(List<double> x) /* Composition Function 6 */
        {
            List<double> fit = new List<double>();
            List<double> delta = new List<double> { 10, 10, 10, 10, 10 };
            List<double> lambdas = new List<double> { 0.25, 1, 1E-7, 2.5, 10 };
            List<double> bias = new List<double> { 0, 100, 200, 300, 400 };

            fit.Add(F15_rotated_schwefel(x) * lambdas[0]);
            fit.Add(F12_rotated_rastrigin(x) * lambdas[1]);
            fit.Add(F02_ellips(x) * lambdas[2]);
            fit.Add(F09_weierstrass(x) * lambdas[3]);
            fit.Add(F10_griewank(x) * lambdas[4]);

            return cf_cal(x, delta, bias, fit) + 1200;
        }

        /// <summary>
        /// f 27 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 1300)]
        public static double F27_cf07(List<double> x) /* Composition Function 7 */
        {
            List<double> fit = new List<double>();
            List<double> delta = new List<double> { 10, 10, 10, 20, 20 };
            List<double> lambdas = new List<double> { 100, 10, 2.5, 25, 0.1 };
            List<double> bias = new List<double> { 0, 100, 200, 300, 400 };

            fit.Add(F10_griewank(x) * lambdas[0]);
            fit.Add(F12_rotated_rastrigin(x) * lambdas[1]);
            fit.Add(F15_rotated_schwefel(x) * lambdas[2]);
            fit.Add(F09_weierstrass(x) * lambdas[3]);
            fit.Add(F01_sphere(x) * lambdas[4]);

            return cf_cal(x, delta, bias, fit) + 1300;
        }

        /// <summary>
        /// f 28 - Multi-modal; Non-separable; Asymetrical; Different proerties around local optima
        /// </summary>
        [FunctionAtt(1E-8, -100, 100, 1500, 1400)]
        public static double F28_cf08(List<double> x) /* Composition Function 8 */
        {
            List<double> fit = new List<double>();
            List<double> delta = new List<double> { 10, 20, 30, 40, 50 };
            List<double> lambdas = new List<double> { 2.5, 2.5E-3, 2.5, 5E-4, 0.1 };
            List<double> bias = new List<double> { 0, 100, 200, 300, 400 };

            fit.Add(F19_grie_rosen(x) * lambdas[0]);
            fit.Add(F07_schaffer_F7(x) * lambdas[1]);
            fit.Add(F15_rotated_schwefel(x) * lambdas[2]);
            fit.Add(F20_escaffer6(x) * lambdas[3]);
            fit.Add(F01_sphere(x) * lambdas[4]);

            return cf_cal(x, delta, bias, fit) + 1400;

        }

        // Shift vector
        private static List<double> _os;
        private static List<double> _firstOs;
        private static List<double> Os
        {
            get
            {
                if (_firstOs == null)
                    _firstOs = OsIndex(0, 100);
                return _firstOs;
            }
        }

        private static double OneO(int index)
        {
            return OsIndex(index, 1)[0];
        }

        private static List<double> OsIndex(int index, int dim)
        {
            if (_os == null)
                using (StreamReader sr = new StreamReader("data/shifted_data.txt"))
                {
                    _os = new List<double>();
                    string line = sr.ReadLine();

                    while (line != null)
                    {
                        Regex reg = new Regex("([\\-0-9.e+]+)");

                        Match match = reg.Match(line);
                        while (match.Success)
                        {
                            string val = match.Groups[1].Value.ToUpper().Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                            double vDouble;
                            if (double.TryParse(val, out vDouble)) _os.Add(vDouble);
                            match = match.NextMatch();
                        }
                        line = sr.ReadLine();
                    }

                }
            return _os.Skip(index).Take(dim).ToList();
        }

        // Rotation matrix
        private static List<List<double>> _mr = new List<List<double>>();
        private static List<double> Mr(int dim, int index)
        {
            while (_mr.Count <= index) _mr.Add(new List<double>());
            if (_mr[index].Count == dim * dim) return _mr[index];

            using (StreamReader sr = new StreamReader("data/M_D" + dim + ".txt"))
            {
                int shift = index * dim;
                for (int i = 0 + shift; i < dim + shift; i++)
                {
                    string line = sr.ReadLine();
                    if (line == null) break;
                    Regex reg = new Regex("([\\-0-9.e+]+)");

                    Match match = reg.Match(line);
                    while (match.Success)
                    {
                        string val = match.Groups[1].Value.ToUpper().Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                        double vDouble;
                        if (double.TryParse(val, out vDouble)) _mr[index].Add(vDouble);
                        match = match.NextMatch();
                    }
                }
            }
            return _mr[index];
        }

        // aux functions

        private static List<double> diagonalfunc(this List<double> x, double factor)
        {
            int nx = x.Count;
            for (int i = 0; i < nx; i++)
                x[i] *= Math.Pow(factor, 1.0 * i / (nx - 1) / 2.0);
            return x;
        }

        private static List<double> shiftfunc(this List<double> x)
        {
            List<double> xshift = new List<double>();
            for (int i = 0; i < x.Count; i++)
                xshift.Add(x[i] - Os[i]);
            return xshift;
        }

        private static List<double> rotatefunc(this List<double> x, int index)
        {
            List<double> xrot = new List<double>();
            int nx = x.Count;
            for (int i = 0; i < nx; i++)
            {
                xrot.Add(0);
                for (int j = 0; j < nx; j++)
                    xrot[i] += x[j] * Mr(nx, index)[i * nx + j];
            }

            return xrot;
        }

        private static List<double> oszfunc(this List<double> x)
        {
            List<double> xosz = new List<double>();
            int nx = x.Count;

            for (int i = 0; i < nx; i++)
            {
                if (i == 0 || i == nx - 1)
                {
                    double c1, c2,
                        xx = 0;
                    int sx; //signal
                    if (x[i] != 0)
                        xx = Math.Log(Math.Abs(x[i]));

                    if (x[i] > 0)
                    {
                        c1 = 10;
                        c2 = 7.9;
                    }
                    else
                    {
                        c1 = 5.5;
                        c2 = 3.1;
                    }

                    if (x[i] > 0)
                        sx = 1;
                    else if (x[i] == 0)
                        sx = 0;
                    else
                        sx = -1;

                    xosz.Add(sx * Math.Exp(xx + 0.049 * (Math.Sin(c1 * xx) + Math.Sin(c2 * xx))));
                }
                else
                    xosz.Add(x[i]);
            }
            return xosz;
        }

        private static List<double> asyfunc(this List<double> x, double beta)
        {
            int nx = x.Count;
            List<double> xasy = new List<double>();
            for (int i = 0; i < nx; i++)
            {
                if (x[i] > 0)
                    xasy.Add(Math.Pow(x[i], 1.0 + beta * i / (nx - 1) * Math.Pow(x[i], 0.5)));
                else xasy.Add(x[i]);
            }
            return xasy;
        }

        private static double cf_cal(List<double> x, List<double> delta, List<double> bias, List<double> fit)
        {
            int i, j, nx = x.Count, cf_num = fit.Count;
            List<double> w = new List<double>();
            double w_max = 0, w_sum = 0;

            for (i = 0; i < cf_num; i++)
            {
                fit[i] += bias[i];
                w.Add(0);

                for (j = 0; j < nx; j++)
                    w[i] += pow(x[j] - OneO(i * nx + j), 2.0);

                if (w[i] != 0)
                    w[i] = pow(1.0 / w[i], 0.5) * exp(-w[i] / 2.0 / nx / pow(delta[i], 2.0));
                else
                    w[i] = INF;

                if (w[i] > w_max)
                    w_max = w[i];
            }

            for (i = 0; i < cf_num; i++)
                w_sum = w_sum + w[i];

            if (w_max == 0)
            {
                for (i = 0; i < cf_num; i++)
                    w[i] = 1;
                w_sum = cf_num;
            }

            double f = 0.0;
            for (i = 0; i < cf_num; i++)
                f += w[i] / w_sum * fit[i];
            return f;
        }

        private static double INF = 1.0e99;

        #endregion

        #region c++ -> c#

        private static double floor(double x) { return Math.Floor(x); }
        private static double fabs(double x) { return Math.Abs(x); }
        private static double exp(double x) { return Math.Exp(x); }
        private static double sin(double x) { return Math.Sin(x); }
        private static double cos(double x) { return Math.Cos(x); }
        private static double pow(double x, double y) { return Math.Pow(x, y); }
        private static double sqrt(double x) { return Math.Sqrt(x); }

        private static double PI = Math.PI;

        #endregion

        #region F CEC 2005

        [FunctionAtt(1E-6, -100, 100, 1500, 0)]
        public static double F_2005_01(IList<double> chromo)
        {
            return chromo.Sum(c => c * c);
        }

        [FunctionAtt(1E-6, -10, 10, 2000, 0)]
        public static double F_2005_02(IList<double> chromo)
        {
            double product = 1;
            foreach (double c in chromo)
                product *= Math.Abs(c);

            return chromo.Sum(c => Math.Abs(c)) + product;
        }

        [FunctionAtt(1E-4, -100, 100, 5000, 0)]
        public static double F_2005_03(IList<double> chromo)
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
        public static double F_2005_04(IList<double> chromo)
        {
            return chromo.Select(Math.Abs).OrderByDescending(c => c).First();
        }

        [FunctionAtt(1E-4, -30, 30, 20000, 0)]
        public static double F_2005_05(IList<double> chromo)
        {
            double sum = 0;
            for (int i = 0; i < chromo.Count - 1; i++)
                sum += 100 * Math.Pow(chromo[i + 1] - (chromo[i] * chromo[i]), 2) + Math.Pow(chromo[i] - 1, 2);
            return sum;
        }

        [FunctionAtt(1E-6, -100, 100, 1500, 0)]
        public static double F_2005_06(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Pow(Math.Floor(c + .5), 2));
        }

        private static int F7Seed;
        [FunctionAtt(1E-4, -1.28, 1.28, 3000, 0)]
        public static double F_2005_07(IList<double> chromo)
        {
            double res = 0;
            Random rand = new Random(F7Seed++);
            for (int i = 0; i < chromo.Count; i++)
                res += (i + 1) * Math.Pow(chromo[i], 4);
            res *= rand.NextDouble();
            return res;
        }

        [FunctionAtt(1E-4, -500, 500, 9000, -.125694866181649E05)]
        public static double F_2005_08d30(IList<double> chromo)
        {
            return chromo.Sum(c => Math.Sin(Math.Sqrt(Math.Abs(c))) * (-c));
        }

        [FunctionAtt(1E-4, -500, 500, 9000, -0.209491443636081E+05)]
        public static double F_2005_08d50(IList<double> chromo) { return F_2005_08d30(chromo); }

        [FunctionAtt(1E-6, -5.12, 5.12, 5000, 0)]
        public static double F_2005_09(IList<double> chromo)
        {
            return chromo.Sum(c => c * c - 10 * Math.Cos(2 * Math.PI * c) + 10);
        }

        [FunctionAtt(1E-6, -32, 32, 1500, 0)]
        public static double F_2005_10(IList<double> chromo)
        {
            double sum1 = chromo.Sum(c => c * c);
            double sum2 = chromo.Sum(c => Math.Cos(2 * Math.PI * c));
            int n = chromo.Count;
            return -20 * Math.Exp(-.2 * Math.Sqrt(sum1 / n)) - Math.Exp(sum2 / n) + 20 + Math.E;
        }

        [FunctionAtt(1E-4, -600, 600, 2000, 0)]
        public static double F_2005_11(IList<double> chromo)
        {
            double res = chromo.Sum(c => c * c) / 4000;
            double prod = 1;
            for (int i = 0; i < chromo.Count; i++)
                prod *= Math.Cos(chromo[i] / Math.Sqrt(i + 1));

            return res - prod + 1;
        }

        #region F12

        [FunctionAtt(1E-4, -50, 50, 1500, 0)]
        public static double F_2005_12(IList<double> chromo)
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
        public static double F_2005_13(IList<double> chromo)
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
