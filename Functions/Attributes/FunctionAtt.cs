using System;

namespace Functions.Attributes
{
    public class FunctionAtt : Attribute
    {
        public double Error;
        public Bound MinBound;
        public Bound MaxBound;
        public int MaxGen;
        public double MinGlobal;

        public FunctionAtt() { }

        public FunctionAtt(double error, double minBound, double maxBound, int maxGen, double minGlobal)
        {
            Error = error;
            MinBound = StBound(minBound);
            MaxBound = StBound(maxBound);
            MaxGen = maxGen;
            MinGlobal = minGlobal;
        }

        protected static Bound StBound(double b) { return ind => b; }
    }
}
