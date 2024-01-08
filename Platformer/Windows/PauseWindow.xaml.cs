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
    /// Interaction logic for PauseWindow.xaml
    /// </summary>
    public partial class PauseWindow : Window
    {
        public EventHandler? StartTimer { get; set; } = null; // Continue game
        public PauseWindow()
        {
            InitializeComponent();
        }

        public void ContinueButtonCallback(object sender, RoutedEventArgs e)
        {
            Closed += StartTimer;
            Close();
        }

        public void ExitButtonCallback(object sender, RoutedEventArgs e)
        {
            Owner.Content = new LevelSelector_UserControl();
            Close();
        }
    }
}
