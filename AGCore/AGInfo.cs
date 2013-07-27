using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGCore
{
    public class AGInfo
    {
        public AGInfo()
        {
            Informacoes = new List<AGInfoGeracao>();
        }

        public List<AGInfoGeracao> Informacoes { get; set; }
        public IndividuoBin MelhorIndividuo { get; set; }
        public int GerDoMelhor { get; set; }
    }
}
