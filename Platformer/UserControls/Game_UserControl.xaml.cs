using Platformer.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

using static Platformer.Classes.Entity;
using static System.Net.Mime.MediaTypeNames;

namespace Platformer.UserControls
{
    public partial class Game_UserControl : UserControl
    {
        #region Attributes
        //game itself
        private readonly Game platformer;
        #endregion
        #region Methods
        private Rectangle? GetRectangleByName(string rectName)
        {
            IEnumerable<Rectangle> rectangles = Canvas.Children.OfType<Rectangle>();
            foreach (var rect in rectangles)
                if (rect.Name == rectName)
                    return rect;
            return null;
        }

        private void CustomRender(object? sender, EventArgs e)
        {
            TimeSpan time = platformer.GameStopwatch.Elapsed;
            TimerDisplay.Text = $"{time.Minutes:00}:{time.Seconds:00}";
            if (platformer.hero.currState == AnimationState.Death &&
                platformer.hero.prevState == AnimationState.Death &&
                platformer.hero.currFrame == 0)
            {
                platformer.GameStopwatch.Stop();
                platformer.engine.Timer.Stop();

                Windows.GameOverWindow gow = new(platformer.GameStopwatch.Elapsed)
                {
                    Owner = Parent as Window,
                    ShowInTaskbar = false
                };
                gow.ShowDialog();
            }

            if (platformer.hero.currState != AnimationState.Damage ||
                platformer.hero.prevState != AnimationState.Damage ||
                platformer.hero.currFrame != 0)
            {
                // Update player
                platformer.hero.Update(platformer.Bounds);
                if (platformer.hero.hitBoxOffset != null)
                {
                    Canvas.SetLeft(platformer.hero.sprite, platformer.hero.X - ((Point)platformer.hero.hitBoxOffset).X);
                    Canvas.SetTop(platformer.hero.sprite, platformer.hero.Y - ((Point)platformer.hero.hitBoxOffset).Y);
                }
            }
            platformer.hero.Animate(platformer.hero.sprite);

            //Update enemies
            foreach (var (enemyDrawRect, enemy) in platformer.Enemies)
            {
                enemy.Update(platformer.Bounds, platformer.hero.HitBox);
                if (enemy.hitBoxOffset != null)
                {
                    Canvas.SetLeft(enemyDrawRect, enemy.X - ((Point)enemy.hitBoxOffset).X);
                    Canvas.SetTop(enemyDrawRect, enemy.Y - ((Point)enemy.hitBoxOffset).Y);
                }
                // Attack the hero if the enemy is close to him
                if (platformer.hero.HitBox.IntersectsWith(enemy.HitBox))
                {
                    enemy.currState = AnimationState.Attack;
                    enemy.Attack(platformer.hero);
                    if (platformer.hero.HitPoints <= 0)
                        platformer.hero.currState = AnimationState.Death;
                    else
                        platformer.hero.currState = AnimationState.Damage;
                }
                enemy.Animate(enemyDrawRect);
            }

            // Update healthbars
            foreach (var (entity, healthBar) in platformer.HealthBars)
            {
                healthBar.Value = entity.HitPoints;
                Canvas.SetLeft(healthBar, entity.HitBox.X - (healthBar.Width - entity.HitBox.Width) / 2);
                Canvas.SetTop(healthBar, entity.HitBox.Y - healthBar.Height - 20);
            }
        }

        //callbacks
        private void CanvasKeyDownCallback(object? sender, KeyEventArgs e)
        {
            //basic application options
            if (e.Key == Key.Escape)
            {
                platformer.GameStopwatch.Stop();
                platformer.engine.Timer.Stop();

                Windows.PauseWindow pw = new()
                {
                    Owner = Parent as Window,
                    ShowInTaskbar = false
                };
                pw.StartTimer = (object? sender, EventArgs e) => {
                    platformer.GameStopwatch.Start();
                    platformer.engine.Timer.Start();
                };
                pw.ShowDialog();
            } //quit application

            //if player is no longer under control
            if (platformer.hero.currState == AnimationState.Death)
            {
                platformer.hero.StopRight();
                platformer.hero.StopLeft();
                return;
            }

            //player movement
            if (e.Key == Key.D)
            {
                platformer.hero.currState = AnimationState.Walk;
                platformer.hero.invertFrame = false;
                platformer.hero.MoveRight();
            }
            if (e.Key == Key.A)
            {
                platformer.hero.currState = AnimationState.Walk;
                platformer.hero.invertFrame = true;
                platformer.hero.MoveLeft();
            }
            if (e.Key == Key.W)
            {
                platformer.hero.Jump();
            }
            //attack
            if (e.Key == Key.X)
            {
                platformer.hero.currState = AnimationState.Attack;

                //combo attack feature
                if (platformer.hero.currFrame == 0)
                {
                    platformer.hero.animationStates[(int)AnimationState.Attack].spritesheetIndex = (platformer.hero.animationStates[(int)AnimationState.Attack].spritesheetIndex + 1) % 2 + 4;
                }

                int attackSpread = 65;

                Point playerCenter = new(platformer.hero.HitBox.Location.X + platformer.hero.HitBox.Width / 2,
                                         platformer.hero.HitBox.Location.Y + platformer.hero.HitBox.Height / 2);

                List<Rectangle> forRemoval = [];
                foreach (var (enemyDrawRect, enemy) in platformer.Enemies)
                {
                    Point enemyCenter = new(enemy.HitBox.Location.X + enemy.HitBox.Width / 2,
                                            enemy.HitBox.Location.Y + enemy.HitBox.Height / 2);
                    Point distances = new(enemyCenter.X - playerCenter.X, enemyCenter.Y - playerCenter.Y);
                    if (Math.Abs(distances.Y) < platformer.hero.HitBox.Height / 2 + 10 && //10 is a small ~magic~ threshold to sorta fix weird collision behaviour
                        (!platformer.hero.invertFrame && 0 <= distances.X && distances.X < attackSpread ||
                          platformer.hero.invertFrame && 0 <= -distances.X && -distances.X < attackSpread))
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
                
        private void CanvasKeyUpCallback(object? sender, KeyEventArgs e)
        {
            //if player is no longer under control
            if (platformer.hero.currState == AnimationState.Death)
            {
                platformer.hero.StopRight();
                platformer.hero.StopLeft();
                return;
            }

            //control inputs
            if (e.Key == Key.D)
            {
                platformer.hero.currState = AnimationState.Idle;
                platformer.hero.StopRight();
            }
            else if (e.Key == Key.A)
            {
                platformer.hero.currState = AnimationState.Idle;
                platformer.hero.StopLeft();
            }
            else if (e.Key == Key.X)
                platformer.hero.currState = AnimationState.Idle;
        }

        private void MediaEndedCallback(object? sender, RoutedEventArgs e)
        {
            //backgroundGIF.Position = TimeSpan.FromMilliseconds(1);
        } //for GIF background animation

        //constructors
        public Game_UserControl()
        {
            //application initialization
            InitializeComponent();
            //since we are using pixel art sprites
            SnapsToDevicePixels = true;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            Canvas.Focus();

            //game initialization with CustomRender function binded to game engine
            platformer = new(new(CustomRender));

            //load first game level (main menu load should be here as level 0)
            platformer.LoadLevel(Canvas, 0);
        } //default constructor
        #endregion
    }
}
