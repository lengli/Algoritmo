using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalCore.LSChains
{
    public class ParametrosLSChains
    {
        public double Aceleracao;
        public int NIteracoes;

        public ParametrosLSChains(double aceleracao, int nIteracoes)
        {
            Aceleracao = aceleracao;
            NIteracoes = nIteracoes;
        }
    }
}
