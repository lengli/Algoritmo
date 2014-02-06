using System.Windows;

namespace AlgoView
{
    public partial class MainWindow
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
