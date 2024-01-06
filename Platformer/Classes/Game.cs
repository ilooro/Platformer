using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Platformer.Classes {
    internal class Game(Engine gameEngine, Player gameHero) {
        #region Attributes
        //game engine
        public readonly Engine engine = gameEngine;

        //player
        public readonly Player hero = gameHero;

        //enemies
        public Dictionary<Rectangle, Enemy> Enemies = [];

        //current level index
        public uint currentLevel = 0;

        //other entities
        //private Entity Core;
        //private List<Enemy> monsters;
        #endregion
        #region Methods
        //get tiling texture
        private Rectangle GenerateGroundBlock(uint tileSize, Vector2 size, uint tileIndex, uint rotate = 0, bool invert = false, Canvas? canvas = null, Vector2? placement = null) {
            Rectangle groundBlock = new();
            {
                groundBlock.Name = "ground";
                size *= tileSize;
                groundBlock.Width = size.X;
                groundBlock.Height = size.Y;

                BitmapImage tileTexture = new BitmapImage(new(@"Textures/stonetiles.png", UriKind.Relative));
                Vector2 tileLayout = new((uint)tileTexture.Width / tileSize, (uint)tileTexture.Height / tileSize);
                Vector2 tilingFactor = new(1.0f / (uint)tileLayout.X, 1.0f / (uint)tileLayout.Y);
                ImageBrush tileImageBrush = new();
                {
                    tileImageBrush.ImageSource = tileTexture;
                    tileImageBrush.Viewbox = new Rect(tilingFactor.X * (tileIndex % (uint)tileLayout.X), tilingFactor.Y * (tileIndex / (uint)tileLayout.X), tilingFactor.X, tilingFactor.Y);
                    tileImageBrush.ViewportUnits = BrushMappingMode.Absolute;
                    tileImageBrush.Viewport = new Rect(0.0f, 0.0f, tileSize, tileSize);
                    tileImageBrush.AlignmentX = AlignmentX.Left;
                    tileImageBrush.AlignmentY = AlignmentY.Top;
                    tileImageBrush.TileMode = TileMode.Tile;

                    TransformGroup transformation = new();
                    transformation.Children.Add(new ScaleTransform(invert ? -1.0 : 1.0, 1.0));
                    transformation.Children.Add(new RotateTransform(rotate * 90.0, 0.5, 0.5));

                    tileImageBrush.RelativeTransform = transformation;
                } //tiling brush generation

                groundBlock.Fill = tileImageBrush;
                groundBlock.Stroke = new SolidColorBrush(Colors.Red); //temporary
            } //ground block generation
            {
                if (canvas != null) {
                    if (placement != null) {
                        Canvas.SetLeft(groundBlock, ((Vector2)placement).X * tileSize);
                        Canvas.SetTop(groundBlock, ((Vector2)placement).Y * tileSize);
                    }
                    else {
                        Canvas.SetLeft(groundBlock, 0.0f);
                        Canvas.SetTop(groundBlock, 0.0f);
                    }
                    Canvas.SetZIndex(groundBlock, 1);
                    canvas.Children.Add(groundBlock);
                }
            } //ground block drawing
            return groundBlock;
        }

        public void GenerateEnemy(MainWindow window, string name, Color color, Enemy enemy)
        {
            Rectangle eDrawRect = new()
            {
                Name = name,
                Height = enemy.HitBox.Height,
                Width = enemy.HitBox.Width,
                Fill = new SolidColorBrush(color)
            };
            Canvas.SetLeft(eDrawRect, enemy.HitBox.X);
            Canvas.SetTop(eDrawRect, enemy.HitBox.Y);
            window.Canvas.Children.Add(eDrawRect);
            Enemies.Add(eDrawRect, enemy);
        }

        //level managing
        public void LoadLevel(MainWindow window, uint LevelIndex = 0) {
            window.Canvas.Children.Clear(); //clear canvas
            //load level map
            switch (LevelIndex) {
                default:
                case 0: {
                        {
                            //tiles with texture index of 0
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, window.Canvas, new(15, 1));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, window.Canvas, new(13, 2));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, window.Canvas, new(17, 10));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, window.Canvas, new(14, 11));

                            //tiles with texture index of 1
                            GenerateGroundBlock(32, new(8, 1), 1, 0, false, window.Canvas, new( 6, 12));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, window.Canvas, new( 3, 11));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, window.Canvas, new(15, 11));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, window.Canvas, new( 0, 10));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, window.Canvas, new(18, 10));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, window.Canvas, new( 4,  3));
                            GenerateGroundBlock(32, new(1, 1), 1, 0, false, window.Canvas, new(14,  2));

                            //tiles with texture index of 2
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, window.Canvas, new(3,  2));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, window.Canvas, new(6,  3));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, window.Canvas, new(2, 10));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, window.Canvas, new(5, 11));
                            
                            //texture index of 3 left unused

                            //tiles with texture index of 4
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, window.Canvas, new(2, 2));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, window.Canvas, new(3, 3));
                            GenerateGroundBlock(32, new(1, 1), 4, 1, false, window.Canvas, new(4, 4));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, window.Canvas, new(2, 11));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, window.Canvas, new(5, 12));

                            //tiles with texture index of 5
                            GenerateGroundBlock(32, new(1, 1), 5, 0, false, window.Canvas, new(16, 0));

                            //tiles with texture index of 6
                            GenerateGroundBlock(32, new( 2, 5), 6, 0, false, window.Canvas, new( 0,  0));
                            GenerateGroundBlock(32, new( 1, 2), 6, 0, false, window.Canvas, new( 2,  3));
                            GenerateGroundBlock(32, new( 1, 1), 6, 0, false, window.Canvas, new( 3,  4));
                            GenerateGroundBlock(32, new( 2, 4), 6, 0, false, window.Canvas, new( 0, 11));
                            GenerateGroundBlock(32, new( 3, 3), 6, 0, false, window.Canvas, new( 2, 12));
                            GenerateGroundBlock(32, new(10, 2), 6, 0, false, window.Canvas, new( 5, 13));
                            GenerateGroundBlock(32, new( 3, 3), 6, 0, false, window.Canvas, new(15, 12));
                            GenerateGroundBlock(32, new( 2, 4), 6, 0, false, window.Canvas, new(18, 11));
                            GenerateGroundBlock(32, new( 3, 5), 6, 0, false, window.Canvas, new(17,  0));
                            GenerateGroundBlock(32, new( 1, 2), 6, 0, false, window.Canvas, new(16,  2));
                            GenerateGroundBlock(32, new( 1, 1), 6, 0, false, window.Canvas, new(15,  3));

                            //tiles with texture index of 7
                            GenerateGroundBlock(32, new(1, 2), 7, 0, false, window.Canvas, new(2, 0));

                            //texture index of 8 left unused

                            //tiles with texture index of 9
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, window.Canvas, new(16,  1));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, window.Canvas, new(15,  2));
                            GenerateGroundBlock(32, new(1, 1), 9, 3, false, window.Canvas, new(14,  3));
                            GenerateGroundBlock(32, new(1, 1), 9, 3, false, window.Canvas, new(16,  4));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, window.Canvas, new(17, 11));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, window.Canvas, new(14, 12));

                            //tiles with texture index of 10
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, window.Canvas, new(13, 3));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, window.Canvas, new(14, 4));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, window.Canvas, new(16, 5));

                            //tiles with texture index of 11                 
                            GenerateGroundBlock(32, new(4, 1), 11, 0, false, window.Canvas, new( 0, 5));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, window.Canvas, new( 5, 4));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, window.Canvas, new(15, 4));
                            GenerateGroundBlock(32, new(3, 1), 11, 0, false, window.Canvas, new(17, 5));

                            //tiles with texture index of 12
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, window.Canvas, new(6, 4));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, window.Canvas, new(4, 5));

                            //texture index of 13 and 14 left unused
                        } //load level landscape

                        //create hero sprite and place it on canvas
                        Rectangle Hero = new();
                        Hero.Name = "playerSprite";
                        Hero.Height = 64;
                        Hero.Width = 32;
                        Hero.Fill = new SolidColorBrush(Colors.Lime);
                        Point startPosition = new(640 / 2, 480 / 2);
                        Canvas.SetLeft(Hero, startPosition.X);
                        Canvas.SetTop(Hero, startPosition.Y);
                        window.Canvas.Children.Add(Hero);
                        
                        hero.HitBox = new Rect(startPosition.X, startPosition.Y, Hero.Width, Hero.Height);

                        // TODO: I don't know, just... just fix it?
                        GenerateEnemy(window, "enemy1Sprite", Colors.Red,
                            new(speed: 8, jumpSpeed: 10, jumpForce: 10, heatPoint: 10, attackPower: 1, hitBox: new Rect(450, 240, 30, 40)));
                        GenerateEnemy(window, "enemy2Sprite", Colors.Cyan,
                            new(speed: 5, jumpSpeed: 0, jumpForce: 0, heatPoint: 10, attackPower: 1, hitBox: new Rect(338, 210, 35, 20), isFlying: true));
                        break;
                    }
            }
            //start timer
            engine.Timer.Start();
        }
        #endregion
    }
}
