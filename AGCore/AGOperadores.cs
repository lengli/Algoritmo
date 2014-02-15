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
            List<IndividuoBin> popIntermediaria = new List<IndividuoBin>(individuos.Count);

            for (int i = 0; i < individuos.Count; i++)
            {
                // aleatorio, entre 0 e 1
                double prob = new Random(AlgoCore.AlgoUtil.GetSeed()).NextDouble();

                if (prob <= probsAcum[i])
                {
                    // selecao do individuo
                    popIntermediaria.Add(individuos[i].Clone());
                }
            }

            return popIntermediaria;
        }

        // pc => prob. de cross over, [0,1]
        public static void Crossover(List<IndividuoBin> pop, double pc, CrossType crossType)
        {
            List<IndividuoBin> filhos = new List<IndividuoBin>();
            Random rand = new Random(AlgoCore.AlgoUtil.GetSeed());

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

                        switch (crossType)
                        {
                            case CrossType.Aritmetico:
                                filhos.Add(CruzamentoReal(ind1, ind2));
                                break;
                            case CrossType.Heuristico:
                                filhos.Add(CruzamentoHeuristico(ind1, ind2));
                                break;
                        }

                        //filhos.Add(CruzamentoReal(ind1, ind2));
                        //filhos.Add(CruzamentoHeuristico(ind1, ind2));

                        break;
                    }
                }

            } while (controle.Sum() != 0); // até todos serem escolhidos
            pop.InsertRange(0, filhos);
        }

        public static void MutacaoReal(List<IndividuoBin> pop, double pm, double rangeMut)
        {
            List<IndividuoBin> popIntermediaria = new List<IndividuoBin>();
            for (int i = 0; i < pop.Count; i++)
            {
                IndividuoBin ind = pop[i].Clone();
                bool mutated = false;
                for (int j = 0; j < ind.Atributos.Count; j++)
                {
                    if (new Random(AlgoCore.AlgoUtil.GetSeed()).NextDouble() < pm)
                    {
                        //http://www.geatbx.com/docu/algindex-04.html
                        double signal = new Random(AlgoCore.AlgoUtil.GetSeed()).NextDouble() > 0.5 ? -1 : 1;
                        double mutVal = new Random(AlgoCore.AlgoUtil.GetSeed()).NextDouble() * rangeMut * 
                            (IndividuoBin.Maximo(j) - IndividuoBin.Minimo(j)) * Math.Pow(2, new Random(AlgoCore.AlgoUtil.GetSeed()).NextDouble() * (-20));

                        double novoVal = ind.Atributos[j] + signal * mutVal;
                        if (novoVal <= IndividuoBin.Maximo(j) && novoVal >= IndividuoBin.Minimo(j))
                        {
                            ind.Atributos[j] = novoVal;
                            mutated = true;
                        }
                        /*

                        double novoVal = (IndividuoBin.Maximo - IndividuoBin.Minimo) * rand.NextDouble() + IndividuoBin.Minimo;
                        ind.Atributos[j] = novoVal;
                        mutated = true;
                        */
                    }
                }
                if (mutated)
                {
                    ind.Aptidao = double.MaxValue;
                    popIntermediaria.Add(ind);
                }
            }

            pop.InsertRange(0, popIntermediaria);
        }
        /*

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
        */
        public static IndividuoBin CruzamentoReal(IndividuoBin ind1, IndividuoBin ind2)
        {/*
            List<double> cromo1 = ind1.Atributos;
            List<double> cromo2 = ind2.Atributos;

            int nCromo = cromo1.Count;
            int ptoCorte = new Random(DateTime.Now.Millisecond).Next(1, nCromo);

            List<double> novo1 = cromo1.Take(ptoCorte).Concat(cromo2.Skip(ptoCorte)).ToList();
            List<double> novo2 = cromo2.Take(ptoCorte).Concat(cromo1.Skip(ptoCorte)).ToList();

            ind1.Atributos = novo1;
            ind2.Atributos = novo2;
          * */

            //http://www.geatbx.com/docu/algindex-03.html

            IndividuoBin filho = new IndividuoBin();

            for (int i = 0; i < ind1.Atributos.Count; i++)
            {
                Random rand = new Random(AlgoCore.AlgoUtil.GetSeed());

                double a = rand.NextDouble() * 1.5 - .25;
                double val = a * ind1.Atributos[i] + (1 - a) * ind2.Atributos[i];
                if (val > IndividuoBin.Maximo(i)) val = IndividuoBin.Maximo(i);
                else if (val < IndividuoBin.Minimo(i)) val = IndividuoBin.Minimo(i);
                filho.Atributos.Add(val);
            }

            return filho;
        }

        public static IndividuoBin CruzamentoHeuristico(IndividuoBin ind1, IndividuoBin ind2)
        {
            int ptoCorte = new Random(AlgoCore.AlgoUtil.GetSeed()).Next(1, ind1.Atributos.Count);

            IndividuoBin indMelhor = ind1.Aptidao > ind2.Aptidao ? ind1.Clone() : ind2.Clone();
            IndividuoBin indPior = ind1.Aptidao >= ind2.Aptidao ? ind2.Clone() : ind1.Clone();

            ind2 = indMelhor;

            List<double> atrb1 = new List<double>();
            for (int i = 0; i < indMelhor.Atributos.Count; i++)
            {
                double vMelhor = indMelhor.Atributos[i];
                double valor = vMelhor + ptoCorte * (vMelhor - indPior.Atributos[i]);
                if (valor > IndividuoBin.Maximo(i)) valor = IndividuoBin.Maximo(i);
                else if (valor < IndividuoBin.Minimo(i)) valor = IndividuoBin.Minimo(i);
                atrb1.Add(valor);
            }

            return new IndividuoBin() { Atributos = atrb1 };
        }

        public static int SelecaoParaReproducao(List<int> controle, int nPop)
        {
            Random rand = new Random(AlgoCore.AlgoUtil.GetSeed());
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
    }
}
