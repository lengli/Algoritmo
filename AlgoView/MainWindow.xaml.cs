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
using System.Windows.Shapes;

namespace AlgoView
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FuncoesClick(object sender, RoutedEventArgs e)
        {
            new FuncoesWindow().Show();
        }

        private void PSOClick(object sender, RoutedEventArgs e)
        {
            new AlgoWindow().Show();
        }
    }
}
