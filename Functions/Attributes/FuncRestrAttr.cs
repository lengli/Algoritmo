using System;

namespace Functions.Attributes
{
    public class FuncRestrAttr : FunctionAtt
    {
        public int Dimension;
        public ListAptidao Gs, Hs;
        public FuncValidarRestricao Validate;
        public FuncValidarFronteira ValidateBounds;
        public FuncRepopRestricao RepopBounds;

        public FuncRestrAttr(double error, object minBound, object maxBound, int maxGen, double minGlobal,
            int dimension, string gs, string hs, string validate, string validateBounds, string repopBounds)
        {
            Error = error;
            MaxGen = maxGen;
            MinGlobal = minGlobal;
            Dimension = dimension;

            if (minBound != null)
            {
                double minDouble;
                if (double.TryParse(minBound.ToString(), out minDouble)) MinBound = StBound(minDouble);
                else
                    MinBound = (Bound)Delegate.CreateDelegate(typeof(Bound), typeof(Functions).GetMethod(minBound.ToString()));
            }

            if (maxBound != null)
            {
                double maxDouble;
                if (double.TryParse(maxBound.ToString(), out maxDouble)) MaxBound = StBound(maxDouble);
                else
                    MaxBound = (Bound)Delegate.CreateDelegate(typeof(Bound), typeof(Functions).GetMethod(maxBound.ToString()));
            }

            if (!string.IsNullOrEmpty(gs))
                Gs = (ListAptidao)Delegate.CreateDelegate(typeof(ListAptidao), typeof(Functions).GetMethod(gs));
            if (!string.IsNullOrEmpty(hs))
                Hs = (ListAptidao)Delegate.CreateDelegate(typeof(ListAptidao), typeof(Functions).GetMethod(hs));
            if (!string.IsNullOrEmpty(validate))
                Validate = (FuncValidarRestricao)Delegate.CreateDelegate(typeof(FuncValidarRestricao), typeof(Functions).GetMethod(validate));
            if (!string.IsNullOrEmpty(validateBounds))
                ValidateBounds = (FuncValidarFronteira)Delegate.CreateDelegate(typeof(FuncValidarFronteira), typeof(Functions).GetMethod(validateBounds));
            if (!string.IsNullOrEmpty(repopBounds))
                RepopBounds = (FuncRepopRestricao)Delegate.CreateDelegate(typeof(FuncRepopRestricao), typeof(Functions).GetMethod(repopBounds));
        }
    }
}