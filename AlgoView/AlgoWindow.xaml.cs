using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Func = Functions.Functions;
using AGCore;
using AlgoResult;
using Functions;
using LocalCore.HillClimbing;
using LocalCore.LSChains;
using AlgoCore;
using DECore;
using AlgoView.Helpers;


namespace AlgoView
{
    public partial class AlgoWindow
    {
        private double _minGlobal;
        private double _erroAceitavel;
        private ListAptidao _gs;
        private ListAptidao _hs;
        private FuncValidarRestricao _validar;
        private FuncValidarFronteira _validarFronteira;

        public AlgoWindow()
        {
            InitializeComponent();
            FuncaoCombo.ItemsSource = Functions.Functions.Funcoes();

            // Tipo de seleção DE
            IEnumerable<SelecaoDE> values = Enum.GetValues(typeof(SelecaoDE)).Cast<SelecaoDE>();
            TipoSelecao.ItemsSource = values;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime inicio = DateTime.Now;

            FuncAptidao funcao;
            FuncRepopRestricao restricao;
            Bound min, max;
            int nGeracoes;

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;

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
            double distTabu = DistTabu.Text.ToDouble();
            int gerSem;
            if (!int.TryParse(GerSMelhor.Text, out gerSem)) return;
            double margemComp = MargemComp.Text.ToDouble();

            // busca por mutação
            int nPopMutLocal;
            if (!int.TryParse(QtdMutLocal.Text, out nPopMutLocal)) return;

            // Hill Climbing
            ParametrosHillClimbing hillClimbing = null;
            if (HillClimbing.IsChecked.Value)
            {
                double aceleracao = AceleHill.Text.ToDouble();
                double epsilon = EpsilonHill.Text.ToDouble();
                int step;

                if (aceleracao != 0 && epsilon > 0 && int.TryParse(StepHill.Text, out step) && step > 0)
                    hillClimbing = new ParametrosHillClimbing(aceleracao, epsilon, 30, step);
            }

            // LS Chains
            ParametrosLSChains lsChains = null;
            if (LSChains.IsChecked.Value)
            {
                double aceleracao = AceleLSChains.Text.ToDouble();
                int nIteracoes;

                if (int.TryParse(NIterLSChains.Text, out nIteracoes))
                    lsChains = new ParametrosLSChains(aceleracao, nIteracoes);
            }

            #endregion

            Func.SelecionarFuncao(out funcao, out restricao, out _gs, out _hs, out _validar, out _validarFronteira, out min, out max, out nGeracoes, out _minGlobal, out _erroAceitavel, FuncaoCombo.Text, ref dimensao);

            List<AlgoInfo> infos = new List<AlgoInfo>();

            for (int i = 0; i < nVezes; i++)
            {
                RotinaAlgo algo = null;
                if (AlgoCombo.Text == "PSO")
                    algo = RotinaPSO(funcao, restricao);
                if (AlgoCombo.Text == "AG")
                    algo = RotinaAG(funcao, restricao);
                if (AlgoCombo.Text == "DE")
                    algo = RotinaDE(funcao, restricao);
                if (algo == null) return;

                algo.MinGlogal = _minGlobal;
                algo.ErroAceitavel = _erroAceitavel;
                infos.Add(algo.Rodar(maxGer, nPop, min, max, dimensao, precisao, maxAval, usarTabu, distTabu, maxRepop, gerSem, margemComp, tabuNaPop, hillClimbing, lsChains, nPopMutLocal));
            }

            if (infos.Count == 1)
                ExibirUmaRodada(infos[0]);
            else
                ExibirNRodadas(infos);

            TimeSpan deltaTempo = DateTime.Now - inicio;
            Tempo.Text = deltaTempo.TotalSeconds.ToString();

            if (FuncaoCombo.Text == "G5")
            {
                EvolGraph.AddSequencialPoints(
                    infos[0].Informacoes.Select(info =>
                        new PointDouble { X = info.MelhorCromo[0], Y = info.MelhorCromo[1] }).ToList());

                List<double> xs0 =
                    infos[0].Informacoes.Select(info => info.MelhorCromo[0]).ToList();
                List<double> xs1 =
                    infos[0].Informacoes.Select(info => info.MelhorCromo[1]).ToList();
                List<double> xs2 =
                    infos[0].Informacoes.Select(info => info.MelhorCromo[2]).ToList();
                List<double> xs3 =
                    infos[0].Informacoes.Select(info => info.MelhorCromo[3]).ToList();

                Range0.Text = string.Format("{0:0.####} - {1:0.####}", xs0.Min(), xs0.Max());
                Range1.Text = string.Format("{0:0.####} - {1:0.####}", xs1.Min(), xs1.Max());

                // range não muda
                Range2.Text = string.Format("{0:0.####} - {1:0.####}", xs2.Min(), xs2.Max());
                Range3.Text = string.Format("{0:0.####} - {1:0.####}", xs3.Min(), xs3.Max());

                EvolGraph.AddValidArea(_validar, xs2.Min(), xs3.Min());
            }
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
            double txSucesso = (double)infos.Count(info => Math.Abs(info.MelhorIndividuo.Aptidao - _minGlobal) <= _erroAceitavel) / infos.Count();

            Melhor_Aval.Text = string.Format("{0} ({1})", melhorRodada.MelhorIndividuo.Aptidao, melhorRodada.AvalParaMelhor);
            Pior_Aval.Text = string.Format("{0} ({1})", piorRodada.MelhorIndividuo.Aptidao, piorRodada.AvalParaMelhor);
            MediaMelhor_MediaAval.Text = string.Format("{0} ({1})", mediaMelhores, mediaAval);
            TxSucesso.Text = string.Format("{0:0.00%}", txSucesso);
            StdMelhor_StdAval.Text = string.Format("{0} ({1})", Std(infos.Select(info => info.MelhorIndividuo.Aptidao).ToList()),
                Std(infos.Select(info => (double)info.AvalParaMelhor).ToList()));
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

        private RotinaAlgo RotinaPSO(FuncAptidao funcao, FuncRepopRestricao restricao)
        {
            double fatorPond = FatorPond.Text.ToDouble();
            double fi1 = Fi1.Text.ToDouble();
            double fi2 = Fi2.Text.ToDouble();
            bool usarRand1 = Rand1.IsChecked.Value;
            bool usarRand2 = Rand2.IsChecked.Value;
            double coefKConstr = KConstr.Text.ToDouble();
            bool usarCoefConstr = coefKConstr != 0;
            int nVizinhos;
            if (!int.TryParse(NVizinhos.Text, out nVizinhos) || nVizinhos < 0) return null;

            return new PSOCore.RotinaPSO(funcao, restricao, _gs, _hs, _validar, fatorPond, fi1, fi2, usarRand1, usarRand2, coefKConstr, usarCoefConstr, nVizinhos, _validarFronteira);
        }

        private RotinaAlgo RotinaAG(FuncAptidao funcao, FuncRepopRestricao FuncRestr)
        {
            double probCrossover = ProbCrossover.Text.ToDouble();
            double probMutacao = ProbMutacao.Text.ToDouble();
            double rangeMutacao = RangeMutacao.Text.ToDouble();
            int critParada;
            if (!int.TryParse(CritParada.Text, out critParada)) return null;
            double deltaMedApt = DeltaMedApt.Text.ToDouble();
            CrossType crossType;
            ComboBoxItem item = CrossType.SelectedItem as ComboBoxItem;
            if (item == null || !Enum.TryParse(item.Tag.ToString(), out crossType)) return null;

            return new AGClassico(funcao, FuncRestr, _gs, _hs, _validar, probCrossover, probMutacao, rangeMutacao / 100, deltaMedApt, critParada, crossType, _validarFronteira);
        }

        private RotinaAlgo RotinaDE(FuncAptidao funcao, FuncRepopRestricao restricao)
        {
            double probCrossDE = ProbCrossDE.Text.ToDouble();
            double fatorF = FatorF.Text.ToDouble();
            SelecaoDE selecao;
            object tipoDE = TipoSelecao.SelectedValue;
            if (tipoDE == null || !Enum.TryParse(tipoDE.ToString(), out selecao)) return null;
            return new RotinaDE(funcao, restricao, _gs, _hs, _validar, selecao, probCrossDE, fatorF, _validarFronteira);
        }

        #endregion

    }
}
