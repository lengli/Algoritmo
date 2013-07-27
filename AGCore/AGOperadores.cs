using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgoResult;
using Functions;

namespace AGCore
{
    public class AGOperadores
    {
        /// <summary>
        /// Retorna probabilidade acumulada para cada individuo
        /// </summary>
        public static List<double> MetodoDaRoleta(List<double> avaliacoes)
        {
            double somaAvaliacoes = avaliacoes.Sum();

            List<double> probAcum = new List<double>(avaliacoes.Count);
            double probAcumTemp = 0;

            foreach (double aval in avaliacoes)
            {
                // armazenando a probabilidade acumulada relativa a cada individuo
                probAcumTemp += aval / somaAvaliacoes;
                probAcum.Add(probAcumTemp);
            }

            return probAcum;
        }

        public static List<IndividuoBin> SelecaoPopulacaoIntermediaria(List<double> probsAcum, List<IndividuoBin> individuos)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<IndividuoBin> popIntermediaria = new List<IndividuoBin>(individuos.Count);

            for (int i = 0; i < individuos.Count; i++)
            {
                // aleatorio, entre 0 e 1
                double prob = rand.NextDouble();

                for (int j = 0; j < probsAcum.Count; j++)
                {
                    if (prob <= probsAcum[j])
                    {
                        // selecao do individuo
                        popIntermediaria.Add(individuos[j].Clone());
                        // encerrando o "for"
                        break;
                    }
                }
            }

            return popIntermediaria;
        }

        // pc => prob. de cross over, [0,1]
        public static void Crossover(List<IndividuoBin> pop, double pc)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            // iniciando lista de indicação de cruzamento
            List<int> controle = new List<int>(pop.Count);
            int nPop = pop.Count;

            for (int i = 0; i < nPop; i++)
                controle.Add(1);

            do
            {
                IndividuoBin ind1 = null;
                while (controle.Sum() >= 2)
                {
                    int indice = SelecaoParaReproducao(controle, nPop);
                    double probCross = rand.NextDouble();
                    if (probCross < pc)
                    {
                        ind1 = pop[indice];
                        break;
                    }
                }
                if (ind1 == null) break;

                while (controle.Sum() >= 1)
                {
                    int indice = SelecaoParaReproducao(controle, nPop);
                    double probCross = rand.NextDouble();
                    if (probCross < pc)
                    {
                        IndividuoBin ind2 = pop[indice];
                        CruzamentoUmPonto(ind1, ind2);
                        break;
                    }
                }

            } while (controle.Sum() != 0); // até todos serem escolhidos
        }

        public static void Mutacao(List<IndividuoBin> pop, double pm)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < pop.Count; i++)
            {
                IndividuoBin ind = pop[i];
                List<bool> cromo = ind.Cromossomo;

                for (int j = 0; j < cromo.Count; j++)
                {
                    if (rand.NextDouble() < pm)
                    {
                        cromo[j] = cromo[j] ? false : true;

                        if (ind.Atributos.Any(a => a.ValorReal > IndividuoBin.Maximo || a.ValorReal < IndividuoBin.Minimo))
                            cromo[j] = cromo[j] ? false : true;
                    }
                }
            }
        }

        public static void CruzamentoUmPonto(IndividuoBin ind1, IndividuoBin ind2)
        {
            List<bool> cromo1 = ind1.Cromossomo;
            List<bool> cromo2 = ind2.Cromossomo;

            int nCromo = cromo1.Count;
            int ptoCorte = new Random(DateTime.Now.Millisecond).Next(1, nCromo);

            List<bool> novo1 = cromo1.Take(ptoCorte).Concat(cromo2.Skip(ptoCorte)).ToList();
            List<bool> novo2 = cromo2.Take(ptoCorte).Concat(cromo1.Skip(ptoCorte)).ToList();

            ind1.AtualizarCromo(novo1);
            ind2.AtualizarCromo(novo2);
        }

        public static int SelecaoParaReproducao(List<int> controle, int nPop)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            int j = rand.Next(0, controle.Sum());
            int k = -1;
            for (int i = 0; i < nPop; i++)
            {
                if (controle[i] == 0) continue;
                k++;
                if (k != j) continue;
                controle[i] = 0;
                return i;
            }
            return -1;
        }

        public static IndividuoBin MutacaoReal(IndividuoBin ind, FuncAptidao funcAptidao, double mutReal)
        {
            IndividuoBin tempInd = ind.Clone();
            List<bool> cromo = tempInd.Cromossomo;
            double aptidaoInicial = ind.Aptidao;

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i].ValorReal;
                double novoValor = tempInd.Atributos[i].ValorReal - mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo) continue;

                tempInd.Atributos[i].ValorReal = novoValor;
                double aptidaoNova = funcAptidao(tempInd.Atributos.Select(n => n.ValorReal).ToList());

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i].ValorReal = valorAntigo;
                }
            }

            for (int i = 0; i < tempInd.Atributos.Count; i++)
            {
                double valorAntigo = tempInd.Atributos[i].ValorReal;
                double novoValor = tempInd.Atributos[i].ValorReal + mutReal;

                if (novoValor < IndividuoBin.Minimo || novoValor > IndividuoBin.Maximo) continue;

                tempInd.Atributos[i].ValorReal = novoValor;
                var list = tempInd.Atributos.Select(n => n.ValorReal).ToList();
                double aptidaoNova = funcAptidao(list);

                if (aptidaoNova < aptidaoInicial)
                {
                    ind = tempInd.Clone();
                    ind.Aptidao = aptidaoNova;
                    aptidaoInicial = aptidaoNova;
                }
                else
                {
                    tempInd.Atributos[i].ValorReal = valorAntigo;
                }
            }
            return ind;
        }

        public static IndividuoBin MutacaoLocal(IndividuoBin ind, FuncAptidao funcAptidao)
        {
            IndividuoBin tempInd = ind.Clone();
            List<bool> cromo = tempInd.Cromossomo;
            double aptidaoInicial = ind.Aptidao;

            for (int i = 0; i < cromo.Count; i++)
            {
                cromo[i] = cromo[i] ? false : true;
                tempInd.Cromossomo = cromo;

                if (!tempInd.Atributos.Any(a => a.ValorReal < IndividuoBin.Minimo || a.ValorReal > IndividuoBin.Maximo))
                {
                    double aptidaoNova = funcAptidao(tempInd.Atributos.Select(n => n.ValorReal).ToList());

                    if (aptidaoNova < aptidaoInicial)
                    {
                        ind = tempInd.Clone();
                        ind.Aptidao = aptidaoNova;
                        aptidaoInicial = aptidaoNova;
                    }
                    continue;
                }

                tempInd = ind.Clone();
                cromo = tempInd.Cromossomo;
            }

            return ind;
        }
    }
}
