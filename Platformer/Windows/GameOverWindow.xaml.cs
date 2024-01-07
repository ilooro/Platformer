using Platformer.UserControls;
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
using System.Windows.Shapes;

namespace Platformer.Windows
{
    /// <summary>
    /// Interaction logic for GameOverWindow.xaml
    /// </summary>
    public partial class GameOverWindow : Window
    {
        public TimeSpan _time;
        public GameOverWindow(TimeSpan time)
        {
            InitializeComponent();
            _time = time;
            Time.Text = $"Time: {_time.Minutes:00}:{_time.Seconds:00}";
        }

        private void ExitButtonCallback(object sender, RoutedEventArgs e)
        {
            Owner.Content = new LevelSelector_UserControl();
            Close();
        }
    }
}
