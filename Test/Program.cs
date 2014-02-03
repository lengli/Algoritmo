using System;
using System.Collections.Generic;
using Functions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestMinGlobalFunctions();

            Console.ReadLine();
        }

        static void TestMinGlobalFunctions()
        {
            ShowResult(Functions.Functions.F_2005_01, "F1", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_02, "F2", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_03, "F3", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_04, "F4", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_05, "F5", new List<double> { 1, 1 });
            ShowResult(Functions.Functions.F_2005_06, "F6", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_07, "F7", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_08d30, "F8", new List<double> { 424.45, 424.45 });
            ShowResult(Functions.Functions.F_2005_09, "F9", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_10, "F10", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_11, "F11", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F_2005_12, "F12", new List<double> { -1, -1 });
            ShowResult(Functions.Functions.F_2005_13, "F13", new List<double> { 1, 1 });
        }

        static void ShowResult(FuncAptidao function, string Title, List<double> atts)
        {
            Console.WriteLine(Title + ": " + (function(atts) * 15));
        }
    }
}
