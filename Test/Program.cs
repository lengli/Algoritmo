using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGCore;
using System.Collections;
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
            ShowResult(Functions.Functions.F1, "F1", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F2, "F2", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F3, "F3", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F4, "F4", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F5, "F5", new List<double> { 1, 1 });
            ShowResult(Functions.Functions.F6, "F6", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F7, "F7", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F8, "F8", new List<double> { 424.45, 424.45});
            ShowResult(Functions.Functions.F9, "F9", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F10, "F10", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F11, "F11", new List<double> { 0, 0 });
            ShowResult(Functions.Functions.F12, "F12", new List<double> { -1, -1 });
            ShowResult(Functions.Functions.F13, "F13", new List<double> { 1, 1 });
        }

        static void ShowResult(FuncAptidao function, string Title, List<double> atts)
        {
            Console.WriteLine(Title + ": " + (function(atts) * 15));
        }
    }
}
