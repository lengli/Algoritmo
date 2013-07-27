using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalCore.HillClimbing
{
    public class ParametrosHillClimbing
    {
        public int NIndividuos = 1;
        public double Aceleracao;
        public double Epsilon;
        public List<double> StepAtributos;

        public ParametrosHillClimbing(double aceleracao, double epsilon, int nAtributos, double stepAtributos = 1)
        {
            this.StepAtributos = new List<double>(nAtributos);
            for (int i = 0; i < nAtributos; i++) this.StepAtributos.Add(stepAtributos);
            Aceleracao = aceleracao;
            Epsilon = epsilon;
        }

        public ParametrosHillClimbing(double aceleracao, double epsilon, List<double> stepAtributos)
        {
            StepAtributos = stepAtributos;
            Aceleracao = aceleracao;
            Epsilon = epsilon;
        }
    }
}
