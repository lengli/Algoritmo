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


namespace AlgoView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PerformanceWindow : Window
    {
        public PerformanceWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime inicio = DateTime.Now;

            FuncAptidao funcao = null;
            double min = 0, max = 0;
            int nGeracoes = 0;
            double pm = 0.04;
            double pc = 0.8;
            int nPopMutLocal = 1;
            bool elitismo = true;

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;

            switch (FuncaoCombo.Text)
            {
                case "F1": { funcao = Func.F1; min = -100; max = 100; nGeracoes = 1500; break; }
                case "F2": { funcao = Func.F2; min = -10; max = 10; nGeracoes = 2000; break; }
                case "F3": { funcao = Func.F3; min = -100; max = 100; nGeracoes = 5000; break; }
                case "F4": { funcao = Func.F4; min = -100; max = 100; nGeracoes = 5000; break; }
                case "F5": { funcao = Func.F5; min = -30; max = 30; nGeracoes = 20000; break; }
                case "F6": { funcao = Func.F6; min = -100; max = 100; nGeracoes = 1500; break; }
                case "F7": { funcao = Func.F7; min = -1.28; max = 1.28; nGeracoes = 3000; break; }
                case "F8": { funcao = Func.F8; min = -500; max = 500; nGeracoes = 9000; break; }
                case "F9": { funcao = Func.F9; min = -5.12; max = 5.12; nGeracoes = 5000; break; }
                case "F10": { funcao = Func.F10; min = -32; max = 32; nGeracoes = 1500; break; }
                case "F11": { funcao = Func.F11; min = -600; max = 600; nGeracoes = 2000; break; }
                case "F12": { funcao = Func.F12; min = -50; max = 50; nGeracoes = 1500; break; }
                case "F13": { funcao = Func.F13; min = -50; max = 50; nGeracoes = 1500; break; }
            }
            
            // Thread 1
            AutoResetEvent event1 = new AutoResetEvent(false);
            List<AlgoInfo> infos1 = new List<AlgoInfo>();
            new Thread(() =>
            {
                AGClassico ag = new AGClassico(funcao);
                for (int i = 0; i < 17; i++)
                {
                    infos1.Add(ag.Rodar(100, min, max, 2, 30, nGeracoes, pc, pm, nPopMutLocal, elitismo));
                }
                event1.Set();
            }).Start();

            // Thread 2
            AutoResetEvent event2 = new AutoResetEvent(false);
            List<AlgoInfo> infos2 = new List<AlgoInfo>();
            new Thread(() =>
            {
                AGClassico ag = new AGClassico(funcao);
                for (int i = 0; i < 17; i++)
                {
                    infos2.Add(ag.Rodar(100, min, max, 2, 30, nGeracoes, pc, pm, nPopMutLocal, elitismo));
                }
                event2.Set();
            }).Start();
            
            AGClassico agPricipal = new AGClassico(funcao);
            List<AlgoInfo> infos = new List<AlgoInfo>();

            for (int i = 0; i < 16; i++)
            {
                infos.Add(agPricipal.Rodar(100, min, max, 2, 30, nGeracoes, pc, pm, nPopMutLocal, elitismo, 5));
            }

            event1.WaitOne();
            event2.WaitOne();

            infos.AddRange(infos1);
            infos.AddRange(infos2);

            NGerMedio.Text = infos.Average(info => info.Informacoes.Last().Geracao).ToString("0.00###");
            MediaMelhores.Text = infos.Average(info => info.MelhorIndividuo.Aptidao).ToString("0.00###");
            STDMelhores.Text = Std(infos.Select(info => info.MelhorIndividuo.Aptidao).ToList()).ToString("0.00###");
            MelhorEntre30.Text = infos.Take(30).Min(info => info.MelhorIndividuo.Aptidao).ToString("0.00###");

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

            RodadaDoMelhor.Text = rodadaDoMelhor.ToString();
            GerDoMelhor.Text = gerDoMelhor.ToString();
            MelhorAptidão.Text = melhorAptidao.ToString("0.00###");

            List<Point> medias = new List<Point>();
            for (int i = 0; i < infos.Count; i++)
                medias.Add(new Point { X = i, Y = infos[i].Informacoes.Last().Media });
            SerieMedia.ItemsSource = medias;


            List<Point> melhores = new List<Point>();
            for (int i = 0; i < infos.Count; i++)
                melhores.Add(new Point { X = i, Y = infos[i].MelhorIndividuo.Aptidao });
            SerieMelhor.ItemsSource = melhores;



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
