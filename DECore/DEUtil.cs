using System;
using System.Collections.Generic;
using System.Linq;
using AlgoResult;
using Functions;

namespace DECore
{
    internal static class DEUtil
    {
        //ui,j,G+1 = x1,j,G + F(x2,j,G − x3,j,G) + F(x4,j,G − x5,j,G) + ...
        internal static double CalculoAtributo(List<IndividuoBin> selecao, int atr, List<double> fs)
        {
            double atributo = selecao[0].Atributos[atr];
            for (int i = 1; i < selecao.Count; i++)
            {
                int sinal = i % 2 == 1 ? 1 : -1;
                int j = (i - 1 - ((i - 1) % 2)) / 2;
                double f = fs.Count <= j ? fs.Last() : fs[j];
                atributo += (selecao[i].Atributos[atr] * f) * sinal;
            }
            return atributo;
        }

        internal static void ExecutarMutacao(int atualInd, List<IndividuoBin> populacao, SelecaoDE _tipoSelecao, double _fatorF,
            int _nAtributos, double _probCross, Bound _min, Bound _max, FuncValidarFronteira valFront, FuncAptidao aptidao,
            Action<bool, double, double, SelecaoDE, IndividuoBin> noSucesso = null, Dictionary<string, object> extraParams = null)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<IndividuoBin> selecao;
            List<double> fs;
            HashSet<int> filtros;

            switch (_tipoSelecao)
            {
                case SelecaoDE.Rand1Bin:
                    selecao = OperadoresDE.SelecaoAleatoria(3, populacao, new HashSet<int> { atualInd });
                    fs = new List<double> { _fatorF };
                    break;
                case SelecaoDE.Best1Bin: selecao = OperadoresDE.SelecaoBest(populacao);
                    fs = new List<double> { _fatorF }; break;
                case SelecaoDE.Rand2Bin: selecao = OperadoresDE.SelecaoAleatoria(5, populacao, new HashSet<int> { atualInd });
                    fs = new List<double> { _fatorF }; break;
                case SelecaoDE.RandToBest1Bin:
                    filtros = new HashSet<int> { atualInd };
                    filtros.Add(0);
                    selecao = OperadoresDE.SelecaoAleatoria(2, populacao, filtros);
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(0, populacao.First());
                    selecao.Insert(0, populacao[atualInd]);
                    fs = new List<double> { _fatorF };
                    break;
                case SelecaoDE.RandToBest2Bin:
                    filtros = new HashSet<int> { atualInd };
                    filtros.Add(0);
                    selecao = OperadoresDE.SelecaoAleatoria(4, populacao, filtros);
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(0, populacao.First());
                    selecao.Insert(0, populacao[atualInd]);
                    fs = new List<double> { _fatorF };
                    break;
                case SelecaoDE.CurrentToRand1Bin:
                    selecao = OperadoresDE.SelecaoAleatoria(3, populacao, new HashSet<int> { atualInd });
                    selecao.Insert(0, populacao[atualInd]);
                    selecao.Insert(2, populacao[atualInd]);
                    fs = new List<double> { rand.NextDouble(), _fatorF };
                    break;
                case SelecaoDE.CurrentToPBest1BinArchive:
                    if (extraParams == null || !extraParams.ContainsKey("pBest") || !extraParams.ContainsKey("archive"))
                        throw new Exception("Extraparam precisa ser definido corretamente para pBest");
                    double pBest = (double)extraParams["pBest"];
                    if (pBest > 1) pBest = 1;
                    int nIndex = (int)Math.Round(pBest * (populacao.Count - 1) * rand.NextDouble());
                    List<IndividuoBin> arquivo = (List<IndividuoBin>)extraParams["archive"];
                    int arIndex = rand.Next(0, populacao.Count - 1 + arquivo.Count - 1);

                    filtros = new HashSet<int> { atualInd };
                    filtros.Add(nIndex);
                    if (arIndex < populacao.Count - 1)
                        filtros.Add(arIndex);

                    selecao = OperadoresDE.SelecaoAleatoria(1, populacao, filtros);
                    selecao.Insert(0, populacao[atualInd]);

                    selecao.Insert(1, populacao[nIndex]);
                    selecao.Insert(2, populacao[atualInd]);
                    selecao.Add(arIndex <= populacao.Count - 1 ? populacao[arIndex] : arquivo[arIndex - populacao.Count - 1]);
                    fs = new List<double> { _fatorF };
                    break;
                default: return;
            }

            IndividuoBin individuoTemp = new IndividuoBin();

            int jRand = rand.Next(0, _nAtributos - 1);
            for (int j = 0; j < _nAtributos; j++)
            {
                if (rand.NextDouble() < _probCross || j == jRand)
                {
                    double atributo = CalculoAtributo(selecao, j, fs);

                    // tratamento de restrição
                    if (atributo >= _min(j) && atributo <= _max(j)
                        && (valFront == null || valFront(atributo, j)))
                    {
                        individuoTemp.Atributos.Add(atributo);
                        continue;
                    }
                }

                individuoTemp.Atributos.Add(populacao[atualInd].Atributos[j]);
            }

            individuoTemp.Aptidao = aptidao(individuoTemp.Atributos);
            bool sucesso = individuoTemp.Aptidao < populacao[atualInd].Aptidao;
            if (sucesso)
                populacao[atualInd] = individuoTemp;
            if (noSucesso != null) noSucesso(sucesso, _probCross, _fatorF, _tipoSelecao, populacao[atualInd]);
        }
    }
}
