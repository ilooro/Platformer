using Platformer.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        }

        private async void ExitButtonCallback(object sender, RoutedEventArgs e)
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

                TcpClient tcpClient = new TcpClient();
                try
                {
                    await tcpClient.ConnectAsync("217.71.129.139", 5668);
                    var stream = tcpClient.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes($"{_levelNum - 1} {Username.Text} {_time}\n");
                    await stream.WriteAsync(data);

                    tcpClient.Close();
                }
                catch (SocketException) { }
            }
        }

        async private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TcpClient tcpClient = new TcpClient();
            var table = new List<(string Name, TimeSpan Time)>();
            try
            {
                await tcpClient.ConnectAsync("217.71.129.139", 5668);
                //await tcpClient.ConnectAsync("127.0.0.1", 8888);
                var stream = tcpClient.GetStream();

                await stream.WriteAsync(Encoding.UTF8.GetBytes("GET\n"));

                var response = new List<byte>();
                int bytesRead = 10;

                while ((bytesRead = stream.ReadByte()) != '\n')
                {
                    response.Add((byte)bytesRead);
                }

                string[] split = Encoding.UTF8.GetString(response.ToArray()).Split('/')[_levelNum - 1].Split(';');
                string[] split2;

                for (int i = 0; i < 5; i++)
                {
                    split2 = split[i].Split(' ');
                    if (split2[0] == "")
                        split2[0] = "User";
                    table.Add((split2[0], TimeSpan.Parse(split2[1])));
                }
                tcpClient.Close();
                //_newRecord = _time > table[4].Time;
            }
            catch (SocketException)
            {
                for (int i = 0; i < 5; i++) table.Add(("User", new TimeSpan()));
                //_newRecord = false;
            }
            _newRecord = _time > table[4].Time;


            if (!_newRecord) // Checking new record
            {
                NewRecordTitle.Visibility = Visibility.Collapsed;
                NewRecordText.Visibility = Visibility.Collapsed;
                Username.Visibility = Visibility.Collapsed;
                Time.Margin = new Thickness(0, 160, 0, 0);
                ExitButton.Margin = new Thickness(0, 160, 0, 0);
            }
        }
    }
}
