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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Platformer.UserControls
{
    /// <summary>
    /// Interaction logic for Leaderboard_UserControl.xaml
    /// </summary>
    public partial class Leaderboard_UserControl : UserControl
    {
        private int _levelNum = 1;

        public Leaderboard_UserControl()
        {
            InitializeComponent();
            SetTable();
        }

        private async void SetTable()
        {
            TcpClient tcpClient = new TcpClient();
            var table = new List<(string Name, TimeSpan Time)>();
            try
            {
                await tcpClient.ConnectAsync("217.71.129.139", 5668);
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
            }
            catch (SocketException)
            {
                for (int i = 0; i < 5; i++) table.Add(("User", new TimeSpan()));
            }

            FirstName.Text = table[0].Name;
            FirstTime.Text = $"{table[0].Time.Minutes:00}:{table[0].Time.Seconds:00}";

            SecondName.Text = table[1].Name;
            SecondTime.Text = $"{table[1].Time.Minutes:00}:{table[1].Time.Seconds:00}";

            ThirdName.Text = table[2].Name;
            ThirdTime.Text = $"{table[2].Time.Minutes:00}:{table[2].Time.Seconds:00}";

            FourthName.Text = table[3].Name;
            FourthTime.Text = $"{table[3].Time.Minutes:00}:{table[3].Time.Seconds:00}";

            FifthName.Text = table[4].Name;
            FifthTime.Text = $"{table[4].Time.Minutes:00}:{table[4].Time.Seconds:00}";
        }

        private void BackButtonCallback(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Content = new MainMenu_UserControl();
        }

        private void Level1ButtonCallback(object sender, RoutedEventArgs e)
        {
            _levelNum = 1;
            SetTable();
        }

        private void Level2ButtonCallback(object sender, RoutedEventArgs e)
        {
            _levelNum = 2;
            SetTable();
        }
    }
}
