using Platformer.Classes;
using System.Reflection;
using System.Security.RightsManagement;
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

namespace Platformer {
    public partial class MainWindow : Window {
        #region Attributes
        //game itself
        private readonly Game platformer;
        #endregion
        #region Methods
        private void CustomRender(object? sender, EventArgs e) {
            Rect playerHitBox = new(Canvas.GetLeft(playerSprite), Canvas.GetTop(playerSprite), playerSprite.Width, playerSprite.Height);
            var boxes = new List<Rect>();
            IEnumerable<Rectangle> rectangles = Canvas.Children.OfType<Rectangle>();
            foreach (var rect in rectangles)
                if (rect != playerSprite)
                    boxes.Add(new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height));

            platformer.hero.Update(ref playerHitBox, boxes);

            Canvas.SetTop(playerSprite, Canvas.GetTop(playerSprite) + platformer.hero.CurrentVerOffset);
            Canvas.SetLeft(playerSprite, Canvas.GetLeft(playerSprite) + platformer.hero.CurrentHorOffset);
        }

        //callbacks
        private void CanvasKeyDownCallback(object? sender, KeyEventArgs e) {
            //basic application options
            if (e.Key == Key.Escape) {
                Environment.Exit(0);
                return;
            } //quit application

            //basic game inputs
            if (e.Key == Key.R) {
                platformer.LoadLevel(this, platformer.currentLevel);
            } //restart current level

            //player movement
            if (e.Key == Key.D)
                platformer.hero.MoveRight();
            else if (e.Key == Key.A)
                platformer.hero.MoveLeft();
            else if (e.Key == Key.W)
                platformer.hero.Jump();
        }
        private void CanvasKeyUpCallback(object? sender, KeyEventArgs e)
        {
            //control inputs
            platformer.hero.StopMoving();
        }

        //constructors
        public MainWindow() {
            //application components initialization
            InitializeComponent();
            Canvas.Focus();

            //game engine initialization
            platformer = new(new(CustomRender), new(7, 7, 15));

            //load first game level (main menu load should be here as level 0)
            platformer.LoadLevel(this, 0);
        } //default constructor
        #endregion
    }
}