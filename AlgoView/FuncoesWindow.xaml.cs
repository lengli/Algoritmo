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
    public partial class FuncoesWindow : Window
    {
        public FuncoesWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FuncAptidao funcao = null;
            double min = 0, max = 0;
            int nGeracoes = 0;

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;
            Func.SelecionarFuncao(out funcao, out min, out max, out nGeracoes, FuncaoCombo.Text);

            List<PointDouble> avaliacoes = new List<PointDouble>();

            double x = min;
            while (x <= max)
            {
                List<double> chr = new List<double>(30);
                for (int i = 0; i < 30; i++) chr.Add(x);

                avaliacoes.Add(new PointDouble { X = x, Y = funcao(chr) });
                
                x += 0.25;
            }

            Grafico.ItemsSource = avaliacoes;
        }

        private double Std(List<double> els)
        {
            double average = els.Average();
            double sumOfSquaresOfDifferences = els.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / els.Count);
        }
    }
}
