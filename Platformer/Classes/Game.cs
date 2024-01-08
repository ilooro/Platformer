using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Platformer.Classes.Entity;
using static System.Net.Mime.MediaTypeNames;

namespace Platformer.Classes {
    internal class Game(Engine gameEngine) {
        #region Attributes
        //game engine
        public readonly Engine engine = gameEngine;
        //current level index
        public uint currentLevel = 0;
        #region SceneContent
        //player
        public Player hero = new();
        //enemies
        public Dictionary<Rectangle, Enemy> Enemies = [];
        //ground boundaries
        public List<Rect> Bounds = [];
        //healthbars sprites (babe please, store this in corresponding entity)
        public Dictionary<Entity, ProgressBar> HealthBars = [];

        public Stopwatch GameStopwatch = new();

        public Point[] FlingSpawnPosition;
        public Point[] GroundSpawnPosition;

        // Spawn
        private int _enemyIndex = 0;
        private int _flyPositionIndex = 0;
        private int _groundPositionIndex = 0;

        private int _spawnСooldown = 0; 
        private int _enemesSpawnСount = 2;
        private int _spawnDelta = 135; // Start spawn delay
        public int SpawnDelta // Delay between spawn in ticks
        { 
            get
            {
                return _spawnDelta;
            }
            set
            {
                _spawnDelta = value;
            }
        } 

        //other entities
        //private Entity Core;
        #endregion
        #endregion
        #region Methods
        //get tiling texture
        public Rectangle GenerateGroundBlock(uint tileSize, Vector2 sizeInTiles, uint tileIndex, uint rotate = 0, bool invert = false, Canvas? canvas = null, Vector2? placement = null) {
            Rectangle groundBlock = new();
            {
                groundBlock.Name = "ground";
                sizeInTiles *= tileSize;
                groundBlock.Width = sizeInTiles.X;
                groundBlock.Height = sizeInTiles.Y;

                BitmapImage tileTexture = new BitmapImage(new Uri("pack://application:,,,/Textures/stone_tiles.png"));
                Vector2 tileLayout = new((uint)tileTexture.Width / tileSize, (uint)tileTexture.Height / tileSize);
                Vector2 tilingFactor = new(1.0f / (uint)tileLayout.X, 1.0f / (uint)tileLayout.Y);
                ImageBrush tileImageBrush = new();
                {
                    tileImageBrush.ImageSource = tileTexture;
                    tileImageBrush.Viewbox = new Rect(tilingFactor.X * (tileIndex % (uint)tileLayout.X), tilingFactor.Y * (tileIndex / (uint)tileLayout.X), tilingFactor.X, tilingFactor.Y);
                    if (rotate % 2 == 0)
                        tileImageBrush.Viewport = new Rect(0.0f, 0.0f, tileSize / sizeInTiles.X, tileSize / sizeInTiles.Y);
                    else
                        tileImageBrush.Viewport = new Rect(0.0f, 0.0f, tileSize / sizeInTiles.Y, tileSize / sizeInTiles.X);
                    tileImageBrush.AlignmentX = AlignmentX.Left;
                    tileImageBrush.AlignmentY = AlignmentY.Top;
                    tileImageBrush.TileMode = TileMode.Tile;
                    tileImageBrush.Stretch = Stretch.None;

                    TransformGroup transformation = new();
                    transformation.Children.Add(new ScaleTransform(invert ? -1.0 : 1.0, 1.0));
                    transformation.Children.Add(new RotateTransform(rotate * 90.0, 0.5, 0.5));

                    tileImageBrush.RelativeTransform = transformation;
                } //tiling brush generation

                groundBlock.Fill = tileImageBrush;

                //tile seams workaround fix
                //groundBlock.Stroke = new SolidColorBrush(Colors.Red); //temporary (to see seams clearly)
                groundBlock.Stroke = new SolidColorBrush(Color.FromRgb(20, 12, 28));
                groundBlock.StrokeThickness = 1.1f;
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

            // Save to list for performance
            Bounds.Add(new Rect(Canvas.GetLeft(groundBlock), Canvas.GetTop(groundBlock), groundBlock.Width, groundBlock.Height));

            return groundBlock;
        }

        public void GenerateHero(Canvas canvas, Player Hero, Point startPosition) {            
            //sprite (rectangle) generation
            Rectangle heroSprite = new();
            heroSprite.Name = "playerSprite";
            heroSprite.Height = 111;
            heroSprite.Width = 160;
            Hero.sprite = heroSprite;
            Hero.HitBox = new Rect(startPosition.X, startPosition.Y, 20, 40);
            Hero.hitBoxOffset = new(71, 65);
            Canvas.SetLeft(heroSprite, startPosition.X - ((Point)Hero.hitBoxOffset).X);
            Canvas.SetTop(heroSprite, startPosition.Y - ((Point)Hero.hitBoxOffset).Y);

            //animation configuration
            Hero.currState = AnimationState.Idle;
            Hero.currFrame = 0;
            Hero.invertFrame = false;
            var configs = new List<Tuple<AnimationState, StateInfo>> {
                Tuple.Create(AnimationState.Idle,   new StateInfo(0, 8)),
                Tuple.Create(AnimationState.Walk,   new StateInfo(1, 8)),
                Tuple.Create(AnimationState.Jump,   new StateInfo(2, 2)),
                Tuple.Create(AnimationState.Fall,   new StateInfo(3, 2)),
                Tuple.Create(AnimationState.Attack, new StateInfo(4, 4)),
                Tuple.Create(AnimationState.Damage, new StateInfo(7, 4)),
                Tuple.Create(AnimationState.Death,  new StateInfo(8, 6))
            };
            Hero.ConfigureAnimation(statesConfig: configs, animationSpeed: 3);
            Hero.spritesheet.Height = 9;

            hero = Hero; //hero assignment

            canvas.Children.Add(hero.sprite);
            AddHealthBar(canvas, hero, 50, 8, Brushes.Green);
            hero.Animate(hero.sprite);
        }

        public void GenerateEnemy(Canvas canvas, Enemy enemy, Point startPosition, bool addHealthBar = true)
        {
            //skeleton
            if (enemy.SpritePath == "Sprites/skeleton.png")
            {
                //sprite (rectangle) generation
                Rectangle enemySprite = new();
                enemySprite.Name = "enemySprite";
                enemySprite.Height = 150;
                enemySprite.Width = 150;
                enemy.sprite = enemySprite;
                enemy.HitBox = new Rect(startPosition.X, startPosition.Y, 25, 50);
                enemy.hitBoxOffset = new(66, 51);
                Canvas.SetLeft(enemySprite, startPosition.X - ((Point)enemy.hitBoxOffset).X);
                Canvas.SetTop(enemySprite, startPosition.Y - ((Point)enemy.hitBoxOffset).Y);

                //animation configuration
                enemy.currState = AnimationState.Walk;
                enemy.currFrame = 0;
                enemy.invertFrame = false;
                var configs = new List<Tuple<AnimationState, StateInfo>> {
                    Tuple.Create(AnimationState.Idle,   new StateInfo(0, 4)),
                    Tuple.Create(AnimationState.Walk,   new StateInfo(1, 4)),
                    Tuple.Create(AnimationState.Attack, new StateInfo(2, 8)),
                    Tuple.Create(AnimationState.Damage, new StateInfo(4, 4)),
                    Tuple.Create(AnimationState.Death,  new StateInfo(5, 4))
                };
                enemy.ConfigureAnimation(statesConfig: configs, animationSpeed: 3);
                enemy.spritesheet.Height = 6;
            }
            //mushroom
            if (enemy.SpritePath == "Sprites/mushroom.png")
            {
                //sprite (rectangle) generation
                Rectangle enemySprite = new();
                enemySprite.Name = "enemySprite";
                enemySprite.Height = 150;
                enemySprite.Width = 150;
                enemy.sprite = enemySprite;
                enemy.HitBox = new Rect(startPosition.X, startPosition.Y, 20, 35);
                enemy.hitBoxOffset = new(65, 66);
                Canvas.SetLeft(enemySprite, startPosition.X - ((Point)enemy.hitBoxOffset).X);
                Canvas.SetTop(enemySprite, startPosition.Y - ((Point)enemy.hitBoxOffset).Y);

                //animation configuration
                enemy.currState = AnimationState.Walk;
                enemy.currFrame = 0;
                enemy.invertFrame = false;
                var configs = new List<Tuple<AnimationState, StateInfo>> {
                    Tuple.Create(AnimationState.Idle,   new StateInfo(0, 4)),
                    Tuple.Create(AnimationState.Walk,   new StateInfo(1, 8)),
                    Tuple.Create(AnimationState.Attack, new StateInfo(2, 8)),
                    Tuple.Create(AnimationState.Damage, new StateInfo(4, 4)),
                    Tuple.Create(AnimationState.Death,  new StateInfo(5, 4))
                };
                enemy.ConfigureAnimation(statesConfig: configs, animationSpeed: 3);
            }
            //goblin
            if (enemy.SpritePath == "Sprites/goblin.png")
            {
                //sprite (rectangle) generation
                Rectangle enemySprite = new();
                enemySprite.Name = "enemySprite";
                enemySprite.Height = 150;
                enemySprite.Width = 150;
                enemy.sprite = enemySprite;
                enemy.HitBox = new Rect(startPosition.X, startPosition.Y, 25, 35);
                enemy.hitBoxOffset = new(62, 66);
                Canvas.SetLeft(enemySprite, startPosition.X - ((Point)enemy.hitBoxOffset).X);
                Canvas.SetTop(enemySprite, startPosition.Y - ((Point)enemy.hitBoxOffset).Y);

                //animation configuration
                enemy.currState = AnimationState.Walk;
                enemy.currFrame = 0;
                enemy.invertFrame = false;
                var configs = new List<Tuple<AnimationState, StateInfo>> {
                    Tuple.Create(AnimationState.Idle,   new StateInfo(0, 4)),
                    Tuple.Create(AnimationState.Walk,   new StateInfo(1, 8)),
                    Tuple.Create(AnimationState.Attack, new StateInfo(2, 8)),
                    Tuple.Create(AnimationState.Damage, new StateInfo(4, 4)),
                    Tuple.Create(AnimationState.Death,  new StateInfo(5, 4))
                };
                enemy.ConfigureAnimation(statesConfig: configs, animationSpeed: 3);
            }
            //flying eye
            if (enemy.SpritePath == "Sprites/flying_eye.png")
            {
                //sprite (rectangle) generation
                Rectangle enemySprite = new();
                enemySprite.Name = "enemySprite";
                enemySprite.Height = 150;
                enemySprite.Width = 150;
                enemy.sprite = enemySprite;
                enemy.HitBox = new Rect(startPosition.X, startPosition.Y, 20, 20);
                enemy.hitBoxOffset = new(70, 67);
                Canvas.SetLeft(enemySprite, startPosition.X - ((Point)enemy.hitBoxOffset).X);
                Canvas.SetTop(enemySprite, startPosition.Y - ((Point)enemy.hitBoxOffset).Y);

                //animation configuration
                enemy.currState = AnimationState.Walk;
                enemy.currFrame = 0;
                enemy.invertFrame = false;
                var configs = new List<Tuple<AnimationState, StateInfo>> {
                    Tuple.Create(AnimationState.Walk,   new StateInfo(0, 8)),
                    Tuple.Create(AnimationState.Attack, new StateInfo(1, 8)),
                    Tuple.Create(AnimationState.Damage, new StateInfo(2, 4)),
                    Tuple.Create(AnimationState.Death,  new StateInfo(3, 4))
                };
                enemy.ConfigureAnimation(statesConfig: configs, animationSpeed: 3);
            }

            if (enemy.sprite != null)
                Enemies.Add(enemy.sprite, enemy); //enemy assignment

            canvas.Children.Add(enemy.sprite);

            if (addHealthBar)
                AddHealthBar(canvas, enemy, 50, 5, Brushes.Red);

            enemy.Animate(enemy.sprite);
        }

        public void GenerateEnemies(Canvas canvas)
        {
            if (_spawnСooldown >= SpawnDelta)
            {
                _spawnСooldown = 0;
                for (int i = 0; i < _enemesSpawnСount; i++)
                {
                    switch (_enemyIndex)
                    {
                        default:
                        case 0:
                            {
                                GenerateEnemy(canvas,
                                    new Enemy(speed: 2,
                                              jumpSpeed: 7,
                                              jumpForce: 7,
                                              heatPoint: 10,
                                              attackPower: 1,
                                              attackSpeed: 30,
                                              spritePath: "Sprites/skeleton.png",
                                              isAnimated_: true,
                                              isFlying: false),
                                    GroundSpawnPosition[_groundPositionIndex]);
                                _groundPositionIndex = (_groundPositionIndex + 1) % GroundSpawnPosition.Length;
                                break;
                            }
                        case 1:
                            {
                                GenerateEnemy(canvas,
                                    new Enemy(speed: 3,
                                              jumpSpeed: 7,
                                              jumpForce: 7,
                                              heatPoint: 3,
                                              attackPower: 1,
                                              attackSpeed: 50,
                                              spritePath: "Sprites/mushroom.png",
                                              isAnimated_: true,
                                              isFlying: false),
                                    GroundSpawnPosition[_groundPositionIndex]);
                                _groundPositionIndex = (_groundPositionIndex + 1) % GroundSpawnPosition.Length;
                                break;
                            }
                        case 2:
                            {
                                GenerateEnemy(canvas,
                                    new Enemy(speed: 4,
                                              jumpSpeed: 7,
                                              jumpForce: 7,
                                              heatPoint: 5,
                                              attackPower: 1,
                                              attackSpeed: 30,
                                              spritePath: "Sprites/goblin.png",
                                              isAnimated_: true,
                                              isFlying: false),
                                    GroundSpawnPosition[_groundPositionIndex]);
                                _groundPositionIndex = (_groundPositionIndex + 1) % GroundSpawnPosition.Length;
                                break;
                            }
                        case 3:
                            {
                                GenerateEnemy(canvas,
                                    new Enemy(speed: 4,
                                              jumpSpeed: 7,
                                              jumpForce: 7,
                                              heatPoint: 5,
                                              attackPower: 1,
                                              attackSpeed: 20,
                                              spritePath: "Sprites/flying_eye.png",
                                              isAnimated_: true,
                                              isFlying: true),
                                    FlingSpawnPosition[_flyPositionIndex]);
                                _flyPositionIndex = (_flyPositionIndex + 1) % FlingSpawnPosition.Length;
                                break;
                            }
                    }
                    _enemyIndex = (_enemyIndex + 1) % 4;
                }
            }
            _spawnСooldown += 1;
        }


        public void AddHealthBar(Canvas canvas, Entity entity, int width, int height, Brush brush)
        {
            ProgressBar healthBar = new()
            {
                Width = width,
                Height = height,
                Minimum = 0,
                Maximum = entity.MaxHitPoints,
                Foreground = brush
            };
            Canvas.SetLeft(healthBar, entity.HitBox.X);
            Canvas.SetTop(healthBar, entity.HitBox.Y);
            canvas.Children.Add(healthBar);
            HealthBars.Add(entity, healthBar);
        }

        //level managing
        public void LoadLevel(Canvas canvas, uint LevelIndex = 0) {
            {
                Enemies = [];
                Bounds = [];
                HealthBars = [];
            } //clear level
            //canvas.Children.Clear(); //clear canvas
            GameStopwatch.Restart();
            GameStopwatch.Start();
            //load level map
            switch (LevelIndex) {
                default:
                case 0: {
                        {
                            //tiles with texture index of 0
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(22, 1));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(20, 2));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(24, 10));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(21, 11));

                            //tiles with texture index of 1
                            GenerateGroundBlock(32, new(15, 1), 1, 0, false, canvas, new(6, 12));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(3, 11));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(22, 11));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(0, 10));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(25, 10));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(4, 3));
                            GenerateGroundBlock(32, new(1, 1), 1, 0, false, canvas, new(21, 2));

                            //tiles with texture index of 2
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(3, 2));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(6, 3));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(2, 10));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(5, 11));

                            //texture index of 3 left unused

                            //tiles with texture index of 4
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(2, 2));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(3, 3));
                            GenerateGroundBlock(32, new(1, 1), 4, 1, false, canvas, new(4, 4));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(2, 11));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(5, 12));

                            //tiles with texture index of 5
                            GenerateGroundBlock(32, new(1, 1), 5, 0, false, canvas, new(23, 0));

                            //tiles with texture index of 6
                            GenerateGroundBlock(32, new(2, 5), 6, 0, false, canvas, new(0, 0));
                            GenerateGroundBlock(32, new(1, 2), 6, 0, false, canvas, new(2, 3));
                            GenerateGroundBlock(32, new(1, 1), 6, 0, false, canvas, new(3, 4));
                            GenerateGroundBlock(32, new(2, 4), 6, 0, false, canvas, new(0, 11));
                            GenerateGroundBlock(32, new(3, 3), 6, 0, false, canvas, new(2, 12));
                            GenerateGroundBlock(32, new(17, 2), 6, 0, false, canvas, new(5, 13));
                            GenerateGroundBlock(32, new(3, 3), 6, 0, false, canvas, new(22, 12));
                            GenerateGroundBlock(32, new(2, 4), 6, 0, false, canvas, new(25, 11));
                            GenerateGroundBlock(32, new(3, 5), 6, 0, false, canvas, new(24, 0));
                            GenerateGroundBlock(32, new(1, 2), 6, 0, false, canvas, new(23, 2));
                            GenerateGroundBlock(32, new(1, 1), 6, 0, false, canvas, new(22, 3));
                            GenerateGroundBlock(32, new(1, 1), 6, 0, false, canvas, new(25, 3));

                            //tiles with texture index of 7
                            GenerateGroundBlock(32, new(1, 2), 7, 0, false, canvas, new(2, 0));

                            //texture index of 8 left unused

                            //tiles with texture index of 9
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(23, 1));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(22, 2));
                            GenerateGroundBlock(32, new(1, 1), 9, 3, false, canvas, new(21, 3));
                            GenerateGroundBlock(32, new(1, 1), 9, 3, false, canvas, new(23, 4));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(24, 11));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(21, 12));

                            //tiles with texture index of 10
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(20, 3));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(21, 4));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(23, 5));

                            //tiles with texture index of 11                 
                            GenerateGroundBlock(32, new(4, 1), 11, 0, false, canvas, new(0, 5));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(5, 4));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(22, 4));
                            GenerateGroundBlock(32, new(3, 1), 11, 0, false, canvas, new(24, 5));

                            //tiles with texture index of 12
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(6, 4));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(4, 5));
                            
                            // Screen walls
                            GenerateGroundBlock(32, new(1, 16), 0, 0, false, canvas, new(-1, 0));
                            GenerateGroundBlock(32, new(1, 16), 0, 0, false, canvas, new(27, 0));

                            //texture indecies of 13 and 14 left unused
                        } //load level landscape

                        {
                            GenerateHero(canvas,
                            new Player(speed: 5,
                                       jumpSpeed: 7,
                                       jumpForce: 7,
                                       heatPoint: 10,
                                       attackPower: 1,
                                       attackSpeed: 5,
                                       spritePath: "Sprites/medieval_king.png",
                                       isAnimated_: true),
                            new(400, 480 / 2));
                        } //load hero

                        GroundSpawnPosition = [new(0, 240), new(830, 240)];
                        FlingSpawnPosition = [new(0, 240), new(300, 100), new(830, 240), new(500, 100)];

                        break;
                    }
                case 1: {
                        {
                            //used different technique here, drawn contour first, and just then filled - much faster
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(0, 3));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(1, 3));
                            GenerateGroundBlock(32, new(1, 1), 4, 1, false, canvas, new(1, 2));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(2, 2));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(3, 2));
                            GenerateGroundBlock(32, new(1, 1), 4, 1, false, canvas, new(3, 1));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(4, 1));
                            GenerateGroundBlock(32, new(1, 1), 4, 1, false, canvas, new(4, 0));
                            GenerateGroundBlock(32, new(2, 1), 11, 0, false, canvas, new(5, 0));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(7, 0));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(5, 1));
                            GenerateGroundBlock(32, new(1, 1), 13, 0, false, canvas, new(5, 2));
                            GenerateGroundBlock(32, new(1, 1), 13, 1, false, canvas, new(5, 3));
                            GenerateGroundBlock(32, new(1, 1), 3, 1, false, canvas, new(6, 3));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(4, 4));
                            GenerateGroundBlock(32, new(1, 1), 8, 0, false, canvas, new(4, 5));
                            GenerateGroundBlock(32, new(1, 1), 13, 0, false, canvas, new(4, 6));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(7, 4));
                            GenerateGroundBlock(32, new(1, 1), 8, 0, false, canvas, new(7, 5));
                            GenerateGroundBlock(32, new(1, 1), 13, 0, false, canvas, new(7, 6));
                            GenerateGroundBlock(32, new(1, 1), 13, 1, false, canvas, new(5, 6));
                            GenerateGroundBlock(32, new(1, 1), 3, 1, false, canvas, new(6, 6));
                            GenerateGroundBlock(32, new(1, 3), 6, 0, false, canvas, new(0, 0));
                            GenerateGroundBlock(32, new(2, 2), 6, 0, false, canvas, new(1, 0));
                            GenerateGroundBlock(32, new(2, 1), 6, 0, false, canvas, new(3, 0));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(20, 0));
                            GenerateGroundBlock(32, new(2, 1), 11, 0, false, canvas, new(21, 0));
                            GenerateGroundBlock(32, new(1, 1), 4, 2, false, canvas, new(23, 0));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(23, 1));
                            GenerateGroundBlock(32, new(1, 1), 4, 2, false, canvas, new(24, 1));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(24, 2));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(25, 2));
                            GenerateGroundBlock(32, new(1, 1), 4, 2, false, canvas, new(26, 2));
                            GenerateGroundBlock(32, new(1, 1), 10, 0, false, canvas, new(26, 3));
                            GenerateGroundBlock(32, new(1, 1), 11, 0, false, canvas, new(27, 3));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(22, 1));
                            GenerateGroundBlock(32, new(1, 1), 13, 0, false, canvas, new(22, 2));
                            GenerateGroundBlock(32, new(1, 1), 13, 1, false, canvas, new(21, 3));
                            GenerateGroundBlock(32, new(1, 1), 3, 1, false, canvas, new(22, 3));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(20, 4));
                            GenerateGroundBlock(32, new(1, 1), 13, 0, false, canvas, new(20, 5));
                            GenerateGroundBlock(32, new(1, 1), 3, 0, false, canvas, new(23, 4));
                            GenerateGroundBlock(32, new(1, 1), 8, 0, false, canvas, new(23, 5));
                            GenerateGroundBlock(32, new(1, 1), 12, 0, false, canvas, new(23, 6));
                            GenerateGroundBlock(32, new(1, 1), 13, 1, false, canvas, new(22, 6));
                            GenerateGroundBlock(32, new(2, 1), 6, 0, false, canvas, new(23, 0));
                            GenerateGroundBlock(32, new(2, 2), 6, 0, false, canvas, new(25, 0));
                            GenerateGroundBlock(32, new(1, 3), 6, 0, false, canvas, new(27, 0));
                            GenerateGroundBlock(32, new(6, 1), 1, 0, false, canvas, new(-2, 13));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(4, 13));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(4, 12));
                            GenerateGroundBlock(32, new(3, 1), 1, 0, false, canvas, new(5, 12));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(8, 12));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(8, 11));
                            GenerateGroundBlock(32, new(5, 1), 1, 0, false, canvas, new(9, 11));
                            GenerateGroundBlock(32, new(1, 1), 9, 0, false, canvas, new(14, 11));
                            GenerateGroundBlock(32, new(1, 1), 0, 0, false, canvas, new(14, 10));
                            GenerateGroundBlock(32, new(3, 1), 1, 0, false, canvas, new(15, 10));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(18, 10));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(18, 11));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(19, 11));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(19, 12));
                            GenerateGroundBlock(32, new(2, 1), 1, 0, false, canvas, new(20, 12));
                            GenerateGroundBlock(32, new(1, 1), 2, 0, false, canvas, new(21, 12));
                            GenerateGroundBlock(32, new(1, 1), 4, 0, false, canvas, new(21, 13));
                            GenerateGroundBlock(32, new(7, 1), 1, 0, false, canvas, new(22, 13));
                            GenerateGroundBlock(32, new(5, 1), 6, 0, false, canvas, new(0, 14));
                            GenerateGroundBlock(32, new(4, 2), 6, 0, false, canvas, new(5, 13));
                            GenerateGroundBlock(32, new(4, 2), 6, 0, false, canvas, new(5, 13));
                            GenerateGroundBlock(32, new(6, 3), 6, 0, false, canvas, new(9, 12));
                            GenerateGroundBlock(32, new(3, 4), 6, 0, false, canvas, new(15, 11));
                            GenerateGroundBlock(32, new(1, 3), 6, 0, false, canvas, new(18, 12));
                            GenerateGroundBlock(32, new(3, 2), 6, 0, false, canvas, new(19, 13));
                            GenerateGroundBlock(32, new(6, 1), 6, 0, false, canvas, new(22, 14));
                        } //load level landscape

                        {
                            GenerateHero(canvas,
                            new Player(speed: 5,
                                       jumpSpeed: 7,
                                       jumpForce: 7,
                                       heatPoint: 10,
                                       attackPower: 1,
                                       attackSpeed: 5,
                                       spritePath: "Sprites/medieval_king.png",
                                       isAnimated_: true),
                            new(400, 480 / 2));

                            GenerateEnemy(canvas,
                                new Enemy(speed: 4,
                                          jumpSpeed: 7,
                                          jumpForce: 7,
                                          heatPoint: 5,
                                          attackPower: 1,
                                          attackSpeed: 20,
                                          spritePath: "Sprites/flying_eye.png",
                                          isAnimated_: true,
                                          isFlying: true),
                                new(180, 160),
                                false);

                            GroundSpawnPosition = [new(-32, 300), new(870, 300)];
                            FlingSpawnPosition = [new(-32, 300), new(300, -32), new(870, 300), new(500, -32)];
                        } //load hero and enemies

                        //add spawn positions here
                        break;
                    }
            }
            //start timer
            engine.Timer.Start();
        }
        #endregion
    }
}
