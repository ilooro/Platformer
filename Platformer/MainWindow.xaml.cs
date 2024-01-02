using Platformer.Classes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Platformer
{
    public partial class MainWindow : Window
    {
        DispatcherTimer Timer = new DispatcherTimer();

        Hero GameHero;

        public MainWindow()
        {
            InitializeComponent();

            // Initialization

            Canvas.Focus();
            Timer.Tick += GameEngine;
            Timer.Interval = TimeSpan.FromMilliseconds(10);

            StartGame();
        }

        private void GameEngine(object? sender, EventArgs e)
        {
            Rect playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            var boxes = new List<Rect>();
            IEnumerable<Rectangle> rectangles = Canvas.Children.OfType<Rectangle>();
            foreach (var rect in rectangles)
                if (rect != player)
                    boxes.Add(new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height));

            GameHero.Update(ref playerHitBox, boxes);

            Canvas.SetTop(player, Canvas.GetTop(player) + GameHero.CurrentVerOffset);
            Canvas.SetLeft(player, Canvas.GetLeft(player) + GameHero.CurrentHorOffset);
        }

        private void StartGame()
        {
            Canvas.SetLeft(player, 190);
            Canvas.SetTop(player, 288);

            Timer.Start();
            GameHero = new Hero(7, 7, 15);

        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
                GameHero.MoveRight();
            else if (e.Key == Key.A)
                GameHero.MoveLeft();
            else if (e.Key == Key.W)
                GameHero.Jump();
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            GameHero.StopMoving();
        }
    }
}