using Platformer.Classes;
using System;
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

            // Update player
            platformer.hero.Update(platformer.Bounds);
            Canvas.SetLeft(player, platformer.hero.X);
            Canvas.SetTop(player, platformer.hero.Y);

            // Update enemies
            foreach (var (enemyDrawRect, enemy) in platformer.Enemies)
            {
                enemy.Update(platformer.Bounds, platformer.hero.HitBox);
                Canvas.SetLeft(enemyDrawRect, enemy.X);
                Canvas.SetTop(enemyDrawRect, enemy.Y);

                // Attack the hero if the enemy is close to him
                if (platformer.hero.HitBox.IntersectsWith(enemy.HitBox))
                {
                    enemy.Attack(platformer.hero);
                    if (platformer.hero.HitPoints <= 0)
                    {
                        // TODO: Game over
                        platformer.engine.Timer.Stop();
                        MessageBox.Show("You died");
                    }
                }

            }

            // Update healthbars
            foreach (var (entity, healthBar) in platformer.HealthBars)
            {
                healthBar.Value = entity.HitPoints;
                Canvas.SetLeft(healthBar, entity.HitBox.X - (healthBar.Width - entity.HitBox.Width) / 2);
                Canvas.SetTop(healthBar, entity.HitBox.Y - healthBar.Height - 3);
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
            
            //attack
            if (e.Key == Key.X)
            {
                List<Rectangle> forRemoval = [];
                foreach (var (enemyDrawRect, enemy) in platformer.Enemies)
                {
                    if (platformer.hero.HitBox.IntersectsWith(enemy.HitBox))
                    {
                        platformer.hero.Attack(enemy);
                        if (enemy.HitPoints <= 0)
                            forRemoval.Add(enemyDrawRect);
                    }
                }

                foreach (var enemyDrawRect in forRemoval)
                {
                    var enemy = platformer.Enemies[enemyDrawRect];

                    //remove healthbar
                    Canvas.Children.Remove(platformer.HealthBars[enemy]);
                    platformer.HealthBars.Remove(enemy);

                    //remove enemy
                    Canvas.Children.Remove(enemyDrawRect);
                    platformer.Enemies.Remove(enemyDrawRect);
                }
            }

        }
        private void CanvasKeyUpCallback(object? sender, KeyEventArgs e) {
            //control inputs
            if (e.Key == Key.D)
                platformer.hero.StopRight();
            else if (e.Key == Key.A)
                platformer.hero.StopLeft();
        }
        /*
        private void MediaEndedCallback(object? sender, RoutedEventArgs e) {
            backgroundGIF.Position = TimeSpan.FromSeconds(1);
        } //for GIF background animation
        */

        //constructors
        public MainWindow() {
            //application initialization
            InitializeComponent();

            //since we are using pixel art sprites
            SnapsToDevicePixels = true;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            //components of the window initialization
            Canvas.Focus();

            //game engine initialization
            var player = new Player(speed: 10, jumpSpeed: 10, jumpForce: 10, heatPoint: 10, attackPower: 1, attackSpeed: 1);
            platformer = new(new(CustomRender), player);

            //load first game level (main menu load should be here as level 0)
            platformer.LoadLevel(this, 0);
        } //default constructor
        #endregion
    }
}