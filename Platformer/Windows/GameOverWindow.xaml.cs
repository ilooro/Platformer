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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Platformer.Windows
{
    /// <summary>
    /// Interaction logic for GameOverWindow.xaml
    /// </summary>
    public partial class GameOverWindow : Window
    {
        private bool _newRecord = false;
        private TimeSpan _time = new TimeSpan();
        private uint _levelNum = 1;
        public GameOverWindow(TimeSpan time, uint levelNum)
        {
            InitializeComponent();

            _time = time;
            _levelNum = levelNum;

            Time.Text = $"Time: {time.Minutes:00}:{time.Seconds:00}";

            var table = new List<(string Name, TimeSpan Time)>();
            for (int i = 0; i < 5; i++) table.Add(("User", new TimeSpan())); // Delete this and get data from server with levelNum param

            _newRecord = time > table[4].Time;

            if (!_newRecord) // Checking new record
            {
                NewRecordTitle.Visibility = Visibility.Collapsed;
                NewRecordText.Visibility = Visibility.Collapsed;
                Username.Visibility = Visibility.Collapsed;
                Time.Margin = new Thickness(0, 160, 0, 0);
                ExitButton.Margin = new Thickness(0, 160, 0, 0);
            }
        }

        private void ExitButtonCallback(object sender, RoutedEventArgs e)
        {
            if (Username.Text.Any(Char.IsWhiteSpace))
            {
                Username.BorderBrush = Brushes.Red;
                return;
            }

            if (!_newRecord || !string.IsNullOrEmpty(Username.Text) && !string.IsNullOrWhiteSpace(Username.Text))
            {
                // Send information to server (_levelNum, Username.Text, _time)

                Owner.Content = new LevelSelector_UserControl();
                Close();
            }
        }
    }
}
