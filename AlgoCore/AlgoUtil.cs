using System;
using System.Collections.Generic;
using System.Linq;
using AlgoResult;

namespace AlgoCore
{
    public static class AlgoUtil
    {
        public static double DistEuclidiana(this IndividuoBin ind1, IndividuoBin ind2)
        {
            double dist = 0;

            for (int i = 0; i < ind1.Atributos.Count; i++)
                dist += Math.Pow(ind1.Atributos[i] - ind2.Atributos[i], 2);

            return Math.Sqrt(dist);
        }

        //http://stackoverflow.com/questions/218060/random-gaussian-variables
        /// <summary>
        /// Selecionar um numero randomicamente, em uma distribuição normal
        /// </summary>
        public static double RandomNormal(this Random rand, double mean, double stdDev)
        {
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }

        //http://en.wikipedia.org/wiki/Cauchy_distribution
        /// <summary>
        /// Selecionar um numero randomicamente, em uma distribuição de Cauchy
        /// </summary>
        public static double RandomCauchy(this Random rand, double mean, double stdDev)
        {
            double p = rand.NextDouble();
            return mean + stdDev * (Math.Tan(Math.PI * (p - .5)));
        }

        public static double Median(this List<double> vector)
        {
            int count = vector.Count;
            if (count == 0) return 0.5;

            List<double> ordered = vector.OrderBy(x => x).ToList();
            if (count % 2 == 0)
            {
                int lIndex = count / 2;
                return (ordered[lIndex - 1] + ordered[lIndex]) / 2;
            }
            int index = (count - 1) / 2;
            return ordered[index];
        }

        public static double Mean(this List<double> numbers)
        {
            return numbers.Sum() / numbers.Count;
        }

        public static double LehmerMean(this List<double> numbers)
        {
            return numbers.Sum(x => x * x) / numbers.Sum();
        }

        private static int seed = int.MinValue;
        public static int GetSeed()
        {
            if (seed == int.MaxValue) seed = int.MinValue;
            else seed++;
            return seed;
        }
    }
}
