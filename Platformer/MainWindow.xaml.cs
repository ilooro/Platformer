using Platformer.Classes;
using System.Numerics;
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
using static System.Net.Mime.MediaTypeNames;

namespace Platformer {
    public partial class MainWindow : Window {
        #region Attributes
        //game itself
        private readonly Game platformer;
        #endregion
        #region Methods
        private Rectangle? GetRectangleByName(string rectName) {
            IEnumerable<Rectangle> rectangles = Canvas.Children.OfType<Rectangle>();
            foreach (var rect in rectangles)
                if (rect.Name == rectName) 
                    return rect;
            return null;
        }

        private void CustomRender(object? sender, EventArgs e) {
            Rectangle? player = GetRectangleByName("playerSprite");
            if (player == null) return;

            Rect playerHitBox = new(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            var boxes = new List<Rect>();
            IEnumerable<Rectangle> rectangles = Canvas.Children.OfType<Rectangle>();
            foreach (var rect in rectangles)
                if (rect.Name.StartsWith("bound"))
                    boxes.Add(new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height));

            // Update player
            platformer.hero.Update(boxes);
            Canvas.SetLeft(player, platformer.hero.X);
            Canvas.SetTop(player, platformer.hero.Y);

            // Update enemies
            foreach (var enemy in platformer.Enemies)
            {
                enemy.Value.Update(boxes, platformer.hero.HitBox);
                Canvas.SetLeft(enemy.Key, enemy.Value.X);
                Canvas.SetTop(enemy.Key, enemy.Value.Y);
            }

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
            if (e.Key == Key.D)
                platformer.hero.StopRight();
            else if (e.Key == Key.A)
                platformer.hero.StopLeft();
        }

        //constructors
        public MainWindow() {
            //application initialization
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor); //since we are using pixel art sprites

            //components of the window initialization
            Canvas.Focus();

            //game engine initialization
            var playerHitBox = new Rect(Canvas.GetLeft(playerSprite), Canvas.GetTop(playerSprite), playerSprite.Width, playerSprite.Height);
            var player = new Player(speed: 10, jumpSpeed: 10, jumpForce: 10, hitBox: playerHitBox, heatPoint: 10, attackPower: 1);

            var enemy1HitBox = new Rect(Canvas.GetLeft(enemy1Sprite), Canvas.GetTop(enemy1Sprite), enemy1Sprite.Width, enemy1Sprite.Height);
            var enemy2HitBox = new Rect(Canvas.GetLeft(enemy2Sprite), Canvas.GetTop(enemy2Sprite), enemy2Sprite.Width, enemy2Sprite.Height);
            var enemies = new Dictionary<Rectangle, Enemy>() {
                { enemy1Sprite, new(speed: 8, jumpSpeed: 10, jumpForce: 10, hitBox: enemy1HitBox,
                                    heatPoint: 10, attackPower: 1) },
                { enemy2Sprite, new(speed: 5, jumpSpeed: 0, jumpForce: 0, hitBox: enemy2HitBox,
                                    heatPoint: 10, attackPower: 1, isFlying: true) }
            };

            platformer = new(new(CustomRender), player, enemies);

            //load first game level (main menu load should be here as level 0)
            platformer.LoadLevel(this, 0);
        } //default constructor
        #endregion
    }
}