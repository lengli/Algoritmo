using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public delegate double FuncAptidao(List<double> atributos);
    public delegate List<List<double>> FuncRepopRestricao();
    public delegate bool FuncValidarRestricao(List<double> atributos);

    public interface IFunctions
    {
        FuncAptidao FuncApt { get; set; }
        FuncRepopRestricao FuncRestr { get; set; }
    }
}
