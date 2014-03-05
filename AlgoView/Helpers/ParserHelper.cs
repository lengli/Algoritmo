namespace AlgoView.Helpers
{
    public static class ParserHelper
    {
        public static double ToDouble(this object raw)
        {
            string rStr = raw.ToString().Replace(',', System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
            rStr = rStr.Replace('.', System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
            double result;
            return double.TryParse(rStr, out result) ? result : 0;
        }
    }
}
