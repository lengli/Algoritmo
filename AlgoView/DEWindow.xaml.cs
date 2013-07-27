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
using DECore;


namespace AlgoView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DEWindow : Window
    {
        public DEWindow()
        {
            InitializeComponent();
            List<ComboBoxItem> items = new List<ComboBoxItem>();
            items.Add(new ComboBoxItem { Content = "Aleatória", Tag = SelecaoDE.Rand });
            items.Add(new ComboBoxItem { Content = "Melhor", Tag = SelecaoDE.Best });
            TipoSelecao.ItemsSource = items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime inicio = DateTime.Now;

            FuncAptidao funcao = null;
            double min = 0, max = 0;
            int nGeracoes = 0;

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;
            Func.SelecionarFuncao(out funcao, out min, out max, out nGeracoes, FuncaoCombo.Text);

            double fatorF;
            double pc;
            int maxAval;
            int gerMax;
            int precisao;
            int nPop;

            if (!double.TryParse(ProbCrossover.Text.Replace(".", ","), out pc)) return;
            if (!double.TryParse(FatorF.Text.Replace(".", ","), out fatorF)) return;
            if (!int.TryParse(MaxAval.Text, out maxAval)) return;
            if (!int.TryParse(NPop.Text, out nPop)) return;
            if (!int.TryParse(Precisao.Text, out precisao)) return;
            if (!int.TryParse(GerMax.Text, out gerMax)) return;
            if (TipoSelecao.SelectedItem == null) return;
            SelecaoDE tipoSelecao = (SelecaoDE)((ComboBoxItem)TipoSelecao.SelectedItem).Tag;

            AlgoInfo agInfo = new RotinaDE(funcao).Rodar(gerMax, nPop, min, max, 30, precisao, pc, fatorF, tipoSelecao, maxAval);

            NGerMedio.Text = agInfo.Informacoes.Last().Geracao.ToString("0.00###");
            MediaMelhores.Text = agInfo.Informacoes.Average(info => info.MelhorAptidao).ToString("0.00###");
            STDMelhores.Text = Std(agInfo.Informacoes.Select(info => info.MelhorAptidao).ToList()).ToString("0.00###");
            MelhorEntre30.Text = agInfo.Informacoes.Take(30).Min(info => info.MelhorAptidao).ToString("0.00###");

            GerDoMelhor.Text = agInfo.GerDoMelhor.ToString();
            MelhorAptidão.Text = agInfo.MelhorIndividuo.Aptidao.ToString("0.00###");

            List<Point> medias = new List<Point>();
            List<Point> melhores = new List<Point>();
            List<Point> avaliacoes = new List<Point>();

            int inc = Convert.ToInt32(Math.Ceiling(agInfo.Informacoes.Count / 300.0));

            for (int i = 0; i < agInfo.Informacoes.Count; i += inc)
            {
                medias.Add(new Point { X = i, Y = agInfo.Informacoes[i].Media });
                melhores.Add(new Point { X = i, Y = agInfo.Informacoes[i].MelhorAptidao });
                avaliacoes.Add(new Point { X = i, Y = agInfo.Informacoes[i].Avaliacoes });
            }

            SerieMedia.ItemsSource = medias;
            SerieMelhor.ItemsSource = melhores;
            SerieAvaliacoes.ItemsSource = avaliacoes;

            TimeSpan deltaTempo = DateTime.Now - inicio;
            Tempo.Text = deltaTempo.TotalSeconds.ToString();

            NAval.Text = agInfo.Informacoes.First(info => info.Geracao == agInfo.GerDoMelhor).Avaliacoes.ToString();
        }

        private double Std(List<double> els)
        {
            double average = els.Average();
            double sumOfSquaresOfDifferences = els.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / els.Count);
        }
    }
}
