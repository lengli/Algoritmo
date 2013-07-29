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
                dist += Math.Pow(ind1.Atributos[i].ValorReal - ind2.Atributos[i].ValorReal, 2);

            return Math.Sqrt(dist);
        }
    }
}
