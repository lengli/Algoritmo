using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace AlgoView
{
    public partial class MapaContorno
    {
        private List<double> _xs;
        private List<double> _ys;

        public MapaContorno()
        {
            InitializeComponent();
            FuncaoCombo.ItemsSource = Functions.Functions.Funcoes();
        }

        public MapaContorno(string fc, List<double> xs, List<double> ys)
            : this()
        {
            FuncaoCombo.Text = fc;
            FuncaoCombo.IsEnabled = false;
            PontosTb.Text = "300";
            PontosTb.IsEnabled = false;
            GerarButton.Visibility = Visibility.Collapsed;
            _xs = xs;
            _ys = ys;
            Button_Click(null, null);
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
            // 0 => y; 1 => x
            double min0 = min(0) / 2, min1 = min(1) / 2, max0 = max(0) / 2, max1 = max(1) / 2;
            //double min0 = -30, max0 =-10, min1 = 0, max1 = 20;
            double rangeI = max0 - min0, rangeJ = max1 - min1;
            double interI = rangeI / precisao, interJ = rangeJ / precisao;
            MaxYTB.Text = string.Format("{0:#,0.00}", max0);
            MaxXTB.Text = string.Format("{0:#,0.00}", max1);
            MinYTB.Text = string.Format("{0:#,0.00}", min0);
            MinXTB.Text = string.Format("{0:#,0.00}", min1);
            int iMenor = 0;
            int jMenor = 0;

            // cálculo da aptidão nos pontos a serem mapeados
            double menorApt = double.MaxValue, maiorApt = double.MinValue;
            for (int i = 0; i < precisao; i++)
            {
                tabelaResultado.Add(new List<double>());
                double atrI = min0 + interI * i;
                for (int j = 0; j < precisao; j++)
                {
                    double atrJ = min1 + interJ * j;
                    double res = aptidao(new List<double> { atrI, atrJ });
                    tabelaResultado[i].Add(res);
                    if (res < menorApt)
                    {
                        iMenor = i;
                        jMenor = j;
                        menorApt = res;
                    }
                    if (res > maiorApt) maiorApt = res;
                }
            }

            double rangeRes = maiorApt - menorApt;

            // menor => azul
            // maior => vermelho

            List<List<Color>> tabelaCores = new List<List<Color>>();

            for (int i = 0; i < precisao; i++)
            {
                tabelaCores.Add(new List<Color>());
                for (int j = 0; j < precisao; j++)
                    tabelaCores[i].Add(CalculoCor(tabelaResultado[i][j] - menorApt, rangeRes));
            }
            Desenhar(tabelaCores, iMenor, jMenor);

            if (_xs != null && _ys != null)
                DesenharPontos(min0, max0, min1, max1);
        }

        private int RangeCor = 255 * 4 + 100;
        private Color CalculoCor(double apt, double range)
        {
            double cor = (RangeCor * apt) / range;
            if (cor >= RangeCor - 255)
            {
                double nC = Math.Round(RangeCor - cor);
                return Color.FromRgb(255, (byte)nC, 0);
            }
            if (cor >= RangeCor - 2 * 255)
            {
                double nC = 255 - Math.Round(RangeCor - cor - 255);
                return Color.FromRgb((byte)nC, 255, 0);
            }
            if (cor >= RangeCor - 3 * 255)
            {
                double nC = Math.Round(RangeCor - cor - 2 * 255);
                return Color.FromRgb(0, 255, (byte)nC);
            }
            if (cor >= RangeCor - 4 * 255)
            {
                double nC = 255 - Math.Round(RangeCor - cor - 3 * 255);
                return Color.FromRgb(0, (byte)nC, 255);
            }
            else
            {
                double nC = 154 + Math.Round(cor);
                return Color.FromRgb(0, 0, (byte)nC);
                //return Colors.White;
            }
        }

        private double minPx = 30;
        private double maxPx = 330;
        private void Desenhar(List<List<Color>> tabelaCores, int iMenor, int jMenor)
        {
            CanvasGraph.Children.Clear();
            int nT = tabelaCores.Count;
            double delta = maxPx - minPx;
            double stepX = delta / nT;
            double xMenor = 0, yMenor = 0;

            for (int i = 0; i < nT; i++)
            {
                int iT = tabelaCores[i].Count;
                double stepY = delta / iT;
                double x = minPx + i * stepX;

                for (int j = 0; j < iT; j++)
                {
                    double y = minPx + j * stepY;
                    if (i == iMenor && j == jMenor)
                    {
                        xMenor = x;
                        yMenor = y;
                    }
                    else
                        CanvasGraph.Children.Add(new Rectangle
                        {
                            Margin = new Thickness(x, y, 0, 0),
                            Width = 300 / nT,
                            Height = 300 / nT,
                            Stroke = new SolidColorBrush(tabelaCores[i][j]),
                            Fill = new SolidColorBrush(tabelaCores[i][j])
                        });
                }
            }
            CanvasGraph.Children.Add(new Ellipse
            {
                Margin = new Thickness(xMenor - 5, yMenor - 5, 0, 0),
                Width = 10,
                Height = 10,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.White)
            });
        }

        private void DesenharPontos(double min0, double max0, double min1, double max1)
        {
            double range0 = max0 - min0;
            double range1 = max1 - min1;

            for (int i = 0; i < _xs.Count; i++)
            {
                double x = minPx + (maxPx - minPx) * (_xs[i] - min0) / range0;
                double y = minPx + (maxPx - minPx) * (_ys[i] - min1) / range1;
                CanvasGraph.Children.Add(new Ellipse
                {
                    Margin = new Thickness(x - 3, y - 3, 0, 0),
                    Width = 6,
                    Height = 6,
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = new SolidColorBrush(Colors.White)
                });
                /*
                if (i % 50 == 0 || i == _xs.Count - 1)
                    CanvasGraph.Children.Add(new TextBlock
                    {
                        Margin = new Thickness(x - 3, y - 3 + 10, 0, 0),
                        Text = (_xs.Count - i).ToString()
                    });*/
            }
        }
    }
}
