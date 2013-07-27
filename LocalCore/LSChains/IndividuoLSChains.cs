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
            List<Numero> novosAtributos = new List<Numero>();
            foreach (Numero num in Atributos)
                novosAtributos.Add(new Numero(Precisao) { ValorReal = num.ValorReal });

            return new IndividuoLSChains { ID = ID, Aptidao = Aptidao, Atributos = novosAtributos, DirecaoBusca = DirecaoBusca };
        }
    }
}
