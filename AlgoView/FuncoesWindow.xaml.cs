﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Func = Functions.Functions;
using Functions;

namespace AlgoView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FuncoesWindow
    {
        public FuncoesWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FuncAptidao funcao;
            ListAptidao gs;
            ListAptidao hs;
            FuncValidarRestricao validar ;
            FuncValidarFronteira validarFronteira;
            FuncRepopRestricao restricao;
            Bound min, max;
            int nGeracoes;
            double minGlobal;
            double erro;
            int dim = 1;

            if (FuncaoCombo.Text.Contains("h5"))
            {
                List<PointDouble> aval = new List<PointDouble>();
                for (double xh = -0.55; xh < 0.55; xh += 5E-4)
                {
                    double yh = xh - .25 - Math.Asin(-1 * Math.Sin(xh - 0.25) - 1.2948);
                    if (double.IsNaN(yh)) continue;
                    aval.Add(new PointDouble { X = xh, Y = yh });
                }

                Grafico.ItemsSource = aval;
                return;
            }

            if (string.IsNullOrEmpty(FuncaoCombo.Text)) return;
            Func.SelecionarFuncao(out funcao, out restricao, out gs, out hs, out validar, out validarFronteira, 
                out min, out max, out nGeracoes, out minGlobal, out erro, FuncaoCombo.Text, ref dim);

            List<PointDouble> avaliacoes = new List<PointDouble>();

            double x = min(0);
            while (x <= max(0))
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
