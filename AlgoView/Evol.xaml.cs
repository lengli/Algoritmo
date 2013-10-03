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
using Functions;

namespace AlgoView
{
    /// <summary>
    /// Interaction logic for Evol.xaml
    /// </summary>
    public partial class Evol : UserControl
    {
        public Evol()
        {
            InitializeComponent();
        }

        // margin inside canvas
        private double minPx = 30;
        private double maxPx = 330;
        private double minXVal;
        private double maxXVal;
        private double minYVal;
        private double maxYVal;
        private double ratioX;
        private double ratioY;

        public void AddSequencialPoints(List<PointDouble> points)
        {
            CanvasGraph.Children.Clear();
            minXVal = points.Min(p => p.X);
            maxXVal = points.Max(p => p.X);
            minYVal = points.Min(p => p.Y);
            maxYVal = points.Max(p => p.Y);

            MinXTB.Text = string.Format("{0:0.######}", minXVal);
            MinYTB.Text = string.Format("{0:0.######}", minYVal);
            MaxXTB.Text = string.Format("{0:0.######}", maxXVal);
            MaxYTB.Text = string.Format("{0:0.######}", maxYVal);

            if (maxXVal == minXVal || maxYVal == minYVal) return;

            ratioX = (maxPx - minPx) / (maxXVal - minXVal);
            ratioY = (maxPx - minPx) / (maxYVal - minYVal);
            for (int i = 0; i < points.Count - 1; i++)
            {
                double x1Px = (points[i].X - minXVal) * ratioX + minPx;
                double y1Px = (points[i].Y - minYVal) * ratioY + minPx;
                double x2Px = (points[i + 1].X - minXVal) * ratioX + minPx;
                double y2Px = (points[i + 1].Y - minYVal) * ratioY + minPx;

                CanvasGraph.Children.Add(new Line
                {
                    X1 = x1Px,
                    Y1 = y1Px,
                    X2 = x2Px,
                    Y2 = y2Px,
                    Stroke = new SolidColorBrush(Colors.Blue),
                    StrokeThickness = 1
                });

                CanvasGraph.Children.Add(new Ellipse
                {
                    Margin = new Thickness(x1Px - 5, y1Px - 5, 0, 0),
                    Width = 10,
                    Height = 10,
                    Stroke = new SolidColorBrush(
                        i == 0 ? Colors.Blue : Colors.Black)
                });

                if (i == points.Count - 2)
                    CanvasGraph.Children.Add(new Ellipse
                    {
                        Margin = new Thickness(x2Px - 5, y2Px - 5, 0, 0),
                        Width = 10,
                        Height = 10,
                        Stroke = new SolidColorBrush(Colors.Red)
                    });
            }
        }

        public void AddValidArea(FuncValidarRestricao restricao, double x2, double x3)
        {
            return;
            ValidArea.Children.Clear();
            // parametros fixos
            double xPerPx = (maxXVal - minXVal) / (maxPx - minPx);
            double yPerPx = (maxYVal - minYVal) / (maxPx - minPx);

            if (double.IsInfinity(xPerPx) || double.IsInfinity(yPerPx) ||
                double.IsNaN(xPerPx) || double.IsNaN(yPerPx)) return;

            SolidColorBrush green = new SolidColorBrush(Color.FromRgb(200, 255, 200));
            green.Freeze();
            SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 70, 20));
            red.Freeze();
            int r = 1;
            for (int ix = 0; ix < (maxPx - minPx); ix += r)
            {
                double x0 = xPerPx * ix + minXVal;
                for (int iy = 0; iy < (maxPx - minPx); iy += r)
                {
                    double x1 = yPerPx * iy + minYVal;
                    bool valido = restricao(new List<double> { x0, x1, x2, x3 });
                    ValidArea.Children.Add(new Rectangle
                    {
                        Margin = new Thickness(ix, iy, 0, 0),
                        Width = r,
                        Height = r,
                        Stroke = valido ? green : red
                    });
                }
            }
        }
    }
}
