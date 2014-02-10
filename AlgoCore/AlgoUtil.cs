using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
