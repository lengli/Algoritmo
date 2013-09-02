using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Func = Functions.Functions;
using AGCore;
using System.Threading;
using AlgoResult;
using Functions;
using LocalCore.HillClimbing;
using LocalCore.LSChains;
using AlgoCore;
using DECore;


namespace AlgoView
{
    public partial class AlgoWindow : Window
    {
        private double _minGlobal;
        private double _erroAceitavel;
        public AlgoWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime inicio = DateTime.Now;

            FuncAptidao funcao = null;
            double min = 0, max = 0;
            int nGeracoes = 0;

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;
            Func.SelecionarFuncao(out funcao, out min, out max, out nGeracoes, out _minGlobal, out _erroAceitavel, FuncaoCombo.Text);

            #region parametro genéricos

            // básico
            int precisao;
            if (!int.TryParse(Precisao.Text, out precisao)) return;
            int nPop;
            if (!int.TryParse(NPop.Text, out nPop)) return;
            int maxGer;
            if (!int.TryParse(MaxGer.Text, out maxGer)) return;
            int maxAval;
            if (!int.TryParse(MaxAval.Text, out maxAval)) return;
            int nVezes;
            if (!int.TryParse(NVezes.Text, out nVezes)) return;
            int dimensao;
            if (!int.TryParse(Dim.Text, out dimensao)) return;

            // tabu
            bool usarTabu = UsarTabu.IsChecked.Value;
            int maxRepop;
            if (!int.TryParse(MaxRepop.Text, out maxRepop)) return;
            bool tabuNaPop = TabuNaPop.IsChecked.Value;
            double distTabu;
            if (!double.TryParse(DistTabu.Text.Replace(".", ","), out distTabu)) return;
            int gerSem;
            if (!int.TryParse(GerSMelhor.Text, out gerSem)) return;
            double margemComp;
            if (!double.TryParse(MargemComp.Text.Replace(".", ","), out margemComp)) return;

            // busca por mutação
            int nPopMutLocal;
            if (!int.TryParse(QtdMutLocal.Text, out nPopMutLocal)) return;

            // Hill Climbing
            ParametrosHillClimbing hillClimbing = null;
            if (HillClimbing.IsChecked.Value)
            {
                double aceleracao;
                double epsilon;
                int step;

                if (double.TryParse(AceleHill.Text.Replace(".", ","), out aceleracao) &&
                    double.TryParse(EpsilonHill.Text.Replace(".", ","), out epsilon) && int.TryParse(StepHill.Text, out step))
                    hillClimbing = new ParametrosHillClimbing(aceleracao, epsilon, 30, step);
            }

            // LS Chains
            ParametrosLSChains lsChains = null;
            if (LSChains.IsChecked.Value)
            {
                double aceleracao;
                int nIteracoes;

                if (double.TryParse(AceleLSChains.Text.Replace(".", ","), out aceleracao) && int.TryParse(NIterLSChains.Text, out nIteracoes))
                    lsChains = new ParametrosLSChains(aceleracao, nIteracoes);
            }

            #endregion

            List<AlgoInfo> infos = new List<AlgoInfo>();

            for (int i = 0; i < nVezes; i++)
            {
                RotinaAlgo algo = null;
                if (AlgoCombo.Text == "PSO")
                    algo = RotinaPSO(funcao);
                if (AlgoCombo.Text == "AG")
                    algo = RotinaAG(funcao);
                if (AlgoCombo.Text == "DE")
                    algo = RotinaDE(funcao);
                if (algo == null) return;

                infos.Add(algo.Rodar(maxGer, nPop, min, max, dimensao, precisao, maxAval, usarTabu, distTabu, maxRepop, gerSem, margemComp, tabuNaPop, hillClimbing, lsChains, nPopMutLocal));
            }

            if (infos.Count == 1)
                ExibirUmaRodada(infos[0]);
            else
                ExibirNRodadas(infos);

            TimeSpan deltaTempo = DateTime.Now - inicio;
            Tempo.Text = deltaTempo.TotalSeconds.ToString();
        }

        private double Std(List<double> els)
        {
            double average = els.Average();
            double sumOfSquaresOfDifferences = els.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / els.Count);
        }

        private void AlgoComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
            if (item == null) return;

            AGPanel.Visibility = Visibility.Collapsed;
            DEPanel.Visibility = Visibility.Collapsed;
            PsoPanel.Visibility = Visibility.Collapsed;

            switch (item.Content.ToString())
            {
                case "AG":
                    AGPanel.Visibility = Visibility.Visible;
                    break;
                case "DE":
                    DEPanel.Visibility = Visibility.Visible;
                    break;
                case "PSO":
                    PsoPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ExibirNRodadas(List<AlgoInfo> infos)
        {
            AlgoInfo melhorRodada = infos.OrderBy(info => info.MelhorIndividuo.Aptidao).First();
            AlgoInfo piorRodada = infos.OrderByDescending(info => info.MelhorIndividuo.Aptidao).First();
            double mediaMelhores = infos.Average(info => info.MelhorIndividuo.Aptidao);
            double mediaAval = infos.Average(info => info.AvalParaMelhor);
            double txSucesso = infos.Count(info => Math.Abs(info.MelhorIndividuo.Aptidao - _minGlobal) <= _erroAceitavel) / infos.Count();

            Melhor_Aval.Text = string.Format("{0} ({1})", melhorRodada.MelhorIndividuo.Aptidao, melhorRodada.AvalParaMelhor);
            Pior_Aval.Text = string.Format("{0} ({1})", piorRodada.MelhorIndividuo.Aptidao, piorRodada.AvalParaMelhor);
            MediaMelhor_MediaAval.Text = string.Format("{0} ({1})", mediaMelhores, mediaAval);
            TxSucesso.Text = string.Format("{0:0.00%}", txSucesso);
        }

        private void ExibirUmaRodada(AlgoInfo agInfo)
        {
            NGerMedio.Text = agInfo.Informacoes.Last().Geracao.ToString();
            MediaMelhores.Text = agInfo.Informacoes.Average(info => info.MelhorAptidao).ToString();
            STDMelhores.Text = Std(agInfo.Informacoes.Select(info => info.MelhorAptidao).ToList()).ToString();
            MelhorEntre30.Text = agInfo.Informacoes.Take(30).Min(info => info.MelhorAptidao).ToString();

            GerDoMelhor.Text = agInfo.GerDoMelhor.ToString();
            MelhorAptidão.Text = agInfo.MelhorIndividuo.Aptidao.ToString();

            List<Point> medias = new List<Point>();
            List<Point> melhores = new List<Point>();
            List<Point> avaliacoes = new List<Point>();

            int inc = Convert.ToInt32(Math.Ceiling(agInfo.Informacoes.Count / 500.0));

            for (int i = 0; i < agInfo.Informacoes.Count; i += inc)
            {
                if (agInfo.Informacoes[i].Media <= int.MaxValue)
                    medias.Add(new Point { X = i, Y = agInfo.Informacoes[i].Media });
                if (agInfo.Informacoes[i].MelhorAptidao <= int.MaxValue)
                    melhores.Add(new Point { X = i, Y = agInfo.Informacoes[i].MelhorAptidao });
                avaliacoes.Add(new Point { X = i, Y = agInfo.Informacoes[i].Avaliacoes });
            }

            SerieMedia.ItemsSource = medias;
            SerieMelhor.ItemsSource = melhores;
            SerieAvaliacoes.ItemsSource = avaliacoes;

            NAval.Text = agInfo.AvalParaMelhor.ToString();
        }

        #region Construir objeto algoritmo

        private RotinaAlgo RotinaPSO(FuncAptidao funcao)
        {
            double fatorPond;
            if (!double.TryParse(FatorPond.Text.Replace(".", ","), out fatorPond)) return null;
            double fi1;
            if (!double.TryParse(Fi1.Text.Replace(".", ","), out fi1)) return null;
            double fi2;
            if (!double.TryParse(Fi2.Text.Replace(".", ","), out fi2)) return null;
            bool usarRand1 = Rand1.IsChecked.Value;
            bool usarRand2 = Rand2.IsChecked.Value;
            double coefKConstr;
            double.TryParse(KConstr.Text, out coefKConstr);
            bool usarCoefConstr = coefKConstr != 0;
            int nVizinhos;
            if (!int.TryParse(NVizinhos.Text, out nVizinhos)) return null;

            return new PSOCore.RotinaPSO(funcao, fatorPond, fi1, fi2, usarRand1, usarRand2, coefKConstr, usarCoefConstr, nVizinhos);
        }

        private RotinaAlgo RotinaAG(FuncAptidao funcao)
        {
            double probCrossover;
            if (!double.TryParse(ProbCrossover.Text.Replace(".", ","), out probCrossover)) return null;
            double probMutacao;
            if (!double.TryParse(ProbMutacao.Text.Replace(".", ","), out probMutacao)) return null;
            double rangeMutacao;
            if (!double.TryParse(RangeMutacao.Text.Replace(".", ","), out rangeMutacao)) return null;
            int critParada;
            if (!int.TryParse(CritParada.Text, out critParada)) return null;
            double deltaMedApt;
            if (!double.TryParse(DeltaMedApt.Text.Replace(".", ","), out deltaMedApt)) return null;
            CrossType crossType;
            ComboBoxItem item = CrossType.SelectedItem as ComboBoxItem;
            if (item == null || !Enum.TryParse(item.Tag.ToString(), out crossType)) return null;

            return new AGClassico(funcao, probCrossover, probMutacao, rangeMutacao / 100, deltaMedApt, critParada, crossType);
        }

        private RotinaAlgo RotinaDE(FuncAptidao funcao)
        {
            double probCrossDE;
            if (!double.TryParse(ProbCrossDE.Text.Replace(".", ","), out probCrossDE)) return null;
            double fatorF;
            if (!double.TryParse(FatorF.Text.Replace(".", ","), out fatorF)) return null;
            SelecaoDE selecao;
            ComboBoxItem item = TipoSelecao.SelectedItem as ComboBoxItem;
            if (item == null || !Enum.TryParse(item.Content.ToString(), out selecao)) return null;
            return new RotinaDE(funcao, selecao, probCrossDE, fatorF);
        }

        #endregion

    }
}
