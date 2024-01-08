using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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

namespace Platformer.UserControls
{
    public partial class MainMenu_UserControl : UserControl
    {
        public MainMenu_UserControl()
        {
            InitializeComponent();
        }

        private void PlayButtonCallback(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Content = new LevelSelector_UserControl();
        }

        private void LeadertableButtonCallback(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Content = new Leaderboard_UserControl();
        }

        private void ExitButtonCallback(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
