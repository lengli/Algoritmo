using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;

namespace LocalCore.LSChains
{
    public class IndividuoLSChains : IndividuoBin
    {
        public List<double> DirecaoBusca;

        public new IndividuoLSChains Clone()
        {
            List<double> novosAtributos = new List<double>();
            foreach (double num in Atributos)
                novosAtributos.Add(num);

            return new IndividuoLSChains { ID = ID, Aptidao = Aptidao, Atributos = novosAtributos, DirecaoBusca = DirecaoBusca };
        }
    }
}
