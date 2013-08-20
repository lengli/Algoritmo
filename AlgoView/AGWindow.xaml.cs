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


namespace AlgoView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AGWindow : Window
    {
        public AGWindow()
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
            Func.SelecionarFuncao(out funcao, out min, out max, out nGeracoes, FuncaoCombo.Text);

            double pm;
            double pc;
            int nPopMutLocal;
            int dimensao;
            bool elitismo = Elitismo.IsChecked.Value;
            bool usarTabu = UsarTabu.IsChecked.Value;
            bool tabuNaPop = TabuNaPop.IsChecked.Value;
            int precisao;
            int nPop;
            int maxRepop;
            int critParada;
            double deltaMedApt;
            double distTabu;
            int maxAval;
            ParametrosHillClimbing hillClimbing = null;
            ParametrosLSChains lsChains = null;

            if (!double.TryParse(ProbMutacao.Text.Replace(".", ","), out pm)) return;
            if (!double.TryParse(ProbCrossover.Text.Replace(".", ","), out pc)) return;
            if (!double.TryParse(DeltaMedApt.Text.Replace(".", ","), out deltaMedApt)) return;
            if (!double.TryParse(DistTabu.Text.Replace(".", ","), out distTabu)) return;
            if (!int.TryParse(QtdMutLocal.Text, out nPopMutLocal)) return;
            if (!int.TryParse(NPop.Text, out nPop)) return;
            if (!int.TryParse(Precisao.Text, out precisao)) return;
            if (!int.TryParse(MaxRepop.Text, out maxRepop)) return;
            if (!int.TryParse(CritParada.Text, out critParada)) return;
            if (!int.TryParse(MaxAval.Text, out maxAval)) return;
            if (!int.TryParse(Dimensao.Text, out dimensao)) return;

            if (HillClimbing.IsChecked.Value)
            {
                double aceleracao;
                double epsilon;
                int step;

                if (double.TryParse(AceleHill.Text.Replace(".", ","), out aceleracao) &&
                    double.TryParse(EpsilonHill.Text.Replace(".", ","), out epsilon) && int.TryParse(StepHill.Text, out step))
                    hillClimbing = new ParametrosHillClimbing(aceleracao, epsilon, dimensao, step);
            }

            if (LSChains.IsChecked.Value)
            {
                double aceleracao;
                int nIteracoes;

                if (double.TryParse(AceleLSChains.Text.Replace(".", ","), out aceleracao) && int.TryParse(NIterLSChains.Text, out nIteracoes))
                    lsChains = new ParametrosLSChains(aceleracao, nIteracoes);
            }


            List<AlgoInfo> infos = new List<AlgoInfo>();

            for (int i = 0; i < Convert.ToInt32(NVezes.Text); i++)
            {
                AlgoInfo agInfo = new AGClassico(funcao).Rodar(
                   nPop, min, max, precisao, dimensao, nGeracoes, pc, pm, nPopMutLocal, elitismo, maxRepop, usarTabu, tabuNaPop, critParada,
                   hillClimbing, lsChains, maxAval, deltaMedApt, distTabu);
                infos.Add(agInfo);
            }

            //NGerMedio.Text = agInfo.Informacoes.Last().Geracao.ToString("0.00###");
            //MediaMelhores.Text = agInfo.Informacoes.Average(info => info.MelhorAptidao).ToString("0.00###");
            //STDMelhores.Text = Std(agInfo.Informacoes.Select(info => info.MelhorAptidao).ToList()).ToString("0.00###");
            //MelhorEntre30.Text = agInfo.Informacoes.Take(30).Min(info => info.MelhorAptidao).ToString("0.00###");
            //GerDoMelhor.Text = agInfo.GerDoMelhor.ToString();
            //MelhorAptidão.Text = agInfo.MelhorIndividuo.Aptidao.ToString("0.00###");
            //NAval.Text = agInfo.Informacoes.First(info => info.Geracao == agInfo.GerDoMelhor).Avaliacoes.ToString();
            NGerMedio.Text = infos.Average(x => x.GerDoMelhor).ToString("0.00000000");
            MediaMelhores.Text = infos.Average(x => x.MelhorIndividuo.Aptidao).ToString("0.00000000");
            STDMelhores.Text = Std(infos.Select(info => info.MelhorIndividuo.Aptidao).ToList()).ToString("0.00000000");
            //MelhorEntre30.Text = agInfo.Informacoes.Take(30).Min(info => info.MelhorAptidao).ToString("0.0000");
            // média do n. de aval.
            NAval.Text = infos.Average(x => x.Informacoes.First(info => info.Geracao == x.GerDoMelhor).Avaliacoes).ToString("0.00000000");

            int rodadaDoMelhor = 0;
            int gerDoMelhor = 0;
            double melhorAptidao = double.MaxValue;
            double melhor = infos.Min(info => info.MelhorIndividuo.Aptidao);
            for (; rodadaDoMelhor < infos.Count; rodadaDoMelhor++)
            {
                if (infos[rodadaDoMelhor].MelhorIndividuo.Aptidao <= melhor)
                {
                    melhorAptidao = infos[rodadaDoMelhor].MelhorIndividuo.Aptidao;
                    gerDoMelhor = infos[rodadaDoMelhor].Informacoes.Last().Geracao;
                    break;
                }
            }

            GerDoMelhor.Text = gerDoMelhor.ToString();
            MelhorAptidão.Text = melhorAptidao.ToString("0.00000000");

            //List<Point> medias = new List<Point>();
            //List<Point> melhores = new List<Point>();
            //List<Point> avaliacoes = new List<Point>();

            //int inc = Convert.ToInt32(Math.Ceiling(agInfo.Informacoes.Count / 700.0));

            //for (int i = 0; i < agInfo.Informacoes.Count; i += inc)
            //{
            //    medias.Add(new Point { X = i, Y = agInfo.Informacoes[i].Media });
            //    melhores.Add(new Point { X = i, Y = agInfo.Informacoes[i].MelhorAptidao });
            //    avaliacoes.Add(new Point { X = i, Y = agInfo.Informacoes[i].Avaliacoes });
            //}

            //SerieMedia.ItemsSource = medias;
            //SerieMelhor.ItemsSource = melhores;
            //SerieAvaliacoes.ItemsSource = avaliacoes;

            TimeSpan deltaTempo = DateTime.Now - inicio;
            Tempo.Text = deltaTempo.TotalSeconds.ToString();

        }

        private double Std(List<double> els)
        {
            double average = els.Average();
            double sumOfSquaresOfDifferences = els.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / els.Count);
        }
    }
}
