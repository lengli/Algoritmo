using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public delegate double FuncAptidao(List<double> atributos);
    public delegate List<List<double>> FuncRepopRestricao(int nPop);
    public delegate bool FuncValidarRestricao(List<double> atributos);
    public delegate bool FuncValidarFronteira(double atributo, int indice);

    public interface IFunctions
    {
        FuncAptidao FuncApt { get; set; }
        FuncRepopRestricao FuncRestr { get; set; }
    }
}
