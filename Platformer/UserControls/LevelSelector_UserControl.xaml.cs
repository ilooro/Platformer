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

namespace Platformer.UserControls
{
    public partial class LevelSelector_UserControl : UserControl
    {
        public LevelSelector_UserControl()
        {
            InitializeComponent();
        }

        private void Level1ButtonCallback(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Content = new Game_UserControl();
        }
        private void Level2ButtonCallback(object sender, RoutedEventArgs e)
        {
            
        }

        private void BackButtonCallback(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Content = new MainMenu_UserControl();
        }
    }
}
