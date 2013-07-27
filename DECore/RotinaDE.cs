using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace DECore
{
    public enum SelecaoDE
    {
        Rand, Best
    }

    public class RotinaDE : IFunctions
    {
        #region IFunctions

        public FuncAptidao FuncApt { get; set; }

        #endregion

        private int _avaliacoes = 0;
        private int _maxAval = 0;
        private Random rand = new Random(DateTime.Now.Millisecond);

        public RotinaDE(FuncAptidao aptidao)
        {
            FuncApt = aptidao;
        }

        private double FuncaoAptidao(List<double> atributos)
        {
            if (_maxAval != 0 && _avaliacoes >= _maxAval) return double.MaxValue;
            _avaliacoes++;
            return FuncApt(atributos);
        }

        public AlgoInfo Rodar(int geracoesMAx, int tamanhoPop, double min, double max, int nAtributos, int precisao,
            double probCross, double fatorF, SelecaoDE tipoSelecao = SelecaoDE.Rand, int maxAvaliacoes = 0)
        {
            _maxAval = maxAvaliacoes;
            AlgoInfo agInfo = new AlgoInfo();
            List<IndividuoBin> populacao = IndividuoBin.GerarPopulacao<IndividuoBin>(tamanhoPop, min, max, nAtributos, precisao);

            // avaliação da população inicial
            foreach (IndividuoBin individuo in populacao)
                individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

            for (int g = 0; g < geracoesMAx; g++)
            {
                if (_maxAval != 0 && _avaliacoes >= _maxAval) break;

                foreach (IndividuoBin individuo in populacao.Where(p => p.Aptidao == double.MaxValue))
                    individuo.Aptidao = FuncaoAptidao(individuo.Atributos.Select(n => n.ValorReal).ToList());

                for (int i = 0; i < populacao.Count; i++)
                {
                    // selecionando 3 individuos aleatoriamente
                    List<IndividuoBin> selecao;

                    switch (tipoSelecao)
                    {
                        default: selecao = OperadoresDE.SelecaoAleatoria(3, populacao); break;
                        case SelecaoDE.Best: selecao = OperadoresDE.SelecaoBest(populacao); break;
                    }

                    IndividuoBin individuoTemp = new IndividuoBin();

                    int jRand = rand.Next(0, nAtributos - 1);
                    for (int j = 0; j < nAtributos; j++)
                    {
                        if (rand.NextDouble() < probCross || j == jRand)
                        {
                            Numero atributo = new Numero(precisao);
                            //ui,j,G+1 = xr3,j,G + F(xr1,j,G − xr2,j,G)
                            atributo.ValorReal = selecao[2].Valor(j) - fatorF * (selecao[0].Valor(j) - selecao[1].Valor(j));

                            // tratamento de restrição
                            if (atributo.ValorReal >= min && atributo.ValorReal <= max)
                            {
                                individuoTemp.Atributos.Add(atributo);
                                continue;
                            }
                        }

                        individuoTemp.Atributos.Add(new Numero(precisao) { ValorReal = populacao[i].Valor(j) });
                    }

                    individuoTemp.Aptidao = FuncaoAptidao(individuoTemp.Atributos.Select(n => n.ValorReal).ToList());
                    if (individuoTemp.Aptidao < populacao[i].Aptidao)
                        populacao[i] = individuoTemp;
                }
                agInfo.AdicionarInfo(populacao, g, _avaliacoes);
            }
            return agInfo;
        }
    }
}