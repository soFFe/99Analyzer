using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NinetyNineLibrary;
using NinetyNineLibrary.Entities;
using NinetyNineLibrary.EntityModels;

namespace NinetyNineAnalyzer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            btnAnalyze.IsEnabled = false;
            ErrorHandling.Clear();

            var tiSuccess = false;
            var miSuccess = false;
            try
            {
                var team = await TeamModel.GetInstanceAsync(txtURL.Text);
                tiSuccess = team.ParseInfo();

                // Get all MatchURLs of our team for the divisions they played
                var divisonUrls = new List<string>();
                foreach (var season in team.Seasons)
                {
                    var div = season.Value;
                    divisonUrls.Add(div.Url);
                }
            }
            catch(Exception x)
            {
                ErrorHandling.Log(x.Message);
            }

            if (!tiSuccess)
                MessageBox.Show(ErrorHandling.JoinedString, AnalyzerConstants.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            btnAnalyze.IsEnabled = true;
        }
    }
}
