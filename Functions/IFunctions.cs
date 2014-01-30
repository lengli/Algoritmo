using System.Collections.Generic;

namespace Functions
{
    public delegate List<FuncAptidao> ListAptidao();
    public delegate double FuncAptidao(List<double> atributos);
    public delegate List<List<double>> FuncRepopRestricao(int nPop);
    public delegate bool FuncValidarRestricao(List<double> atributos);
    public delegate bool FuncValidarFronteira(double atributo, int indice);
    public delegate double Bound(int index);

    public interface IFunctions
    {
        FuncAptidao FuncApt { get; set; }
        FuncRepopRestricao FuncRestr { get; set; }
    }
}
