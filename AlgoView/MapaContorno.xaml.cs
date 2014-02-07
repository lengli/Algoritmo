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
using System.Windows.Shapes;

namespace AlgoView
{
    public partial class MapaContorno
    {
        public MapaContorno()
        {
            InitializeComponent();
            FuncaoCombo.ItemsSource = Functions.Functions.Funcoes();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // variaveis do usuário
            int precisao;
            if (!int.TryParse(PontosTb.Text, out precisao)) return;
            string val = FuncaoCombo.SelectedValue.ToString();

            Functions.FuncAptidao aptidao;
            Functions.FuncRepopRestricao restr;
            Functions.ListAptidao gs;
            Functions.ListAptidao hs;
            Functions.FuncValidarRestricao validar;
            Functions.FuncValidarFronteira validarFronteira;
            Functions.Bound min;
            Functions.Bound max;
            int nGeracoes;
            double minGlobal;
            double erro;
            int d = 2;
            Functions.Functions.SelecionarFuncao(out aptidao, out restr, out gs, out hs, out validar,
                out validarFronteira, out min, out max, out nGeracoes, out minGlobal, out erro, val, ref d);

            // duas dimensões
            List<List<double>> tabelaResultado = new List<List<double>>();
            double min0 = min(0), min1 = min(1), max0 = max(0), max1 = max(1);
            double rangeI = max0 - min0, rangeJ = max1 - min1;
            double interI = rangeI / precisao, interJ = rangeJ / precisao;

            // cálculo da aptidão nos pontos a serem mapeados
            double menorApt = double.MaxValue, maiorApt = double.MinValue;
            for (int i = 0; i < precisao; i++)
            {
                tabelaResultado[i] = new List<double>();
                double atrI = min0 + interI * i;
                for (int j = 0; j < precisao; j++)
                {
                    double atrJ = min1 + interJ * j;
                    double res = aptidao(new List<double> { atrI, atrJ });
                    tabelaResultado[i][j] = res;
                    if (res < menorApt) menorApt = res;
                    if (res > maiorApt) maiorApt = res;
                }
            }

            double rangeRes = maiorApt - menorApt;

            // menor => azul
            // maior => vermelho

            List<List<Color>> tabelaCores = new List<List<Color>>();

            for (int i = 0; i < precisao; i++)
            {
                tabelaCores[i] = new List<Color>();
                for (int j = 0; j < precisao; j++)
                {
                    byte blue = (byte)Math.Round(rangeRes / 255);
                    byte red = 255;
                    red -= blue;
                    tabelaCores[i][j] = Color.FromRgb(red, 0, blue);
                }
            }
            Desenhar(tabelaCores);
        }

        private double minPx = 30;
        private double maxPx = 330;
        private void Desenhar(List<List<Color>> tabelaCores)
        {
            CanvasGraph.Children.Clear();
            int nT = tabelaCores.Count;
            double delta = maxPx - minPx;
            double stepX = delta / nT;

            for (int i = 0; i < nT; i++)
            {
                int iT = tabelaCores[i].Count;
                double stepY = delta / iT;
                double x = minPx + i * stepX;

                for (int j = 0; j < iT; j++)
                {
                    double y = minPx + j * stepY;
                    CanvasGraph.Children.Add(new Ellipse
                    {
                        Margin = new Thickness(x, y, 0, 0),
                        Width = 1,
                        Height = 1,
                        Stroke = new SolidColorBrush(tabelaCores[i][j])
                    });
                }
            }
        }
    }
}
