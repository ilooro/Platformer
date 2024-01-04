using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Platformer.Classes {
    internal class Game(Engine gameEngine, Player gameHero) {
        #region Attributes
        //game engine
        public readonly Engine engine = gameEngine;

        //player
        public readonly Player hero = gameHero;

        //current level index
        public uint currentLevel = 0;

        //other entities
        //private Entity Core;
        //private List<Enemy> monsters;
        #endregion
        #region Methods
        //level managing
        public void LoadLevel(MainWindow window, uint LevelIndex = 0) {
            //todo:
            //window.Canvas.Children.Clear();
            switch (LevelIndex) {
                default:
                case 0: {
                        //set hero initial position on canvas
                        Canvas.SetLeft(window.playerSprite, 190);
                        Canvas.SetTop(window.playerSprite, 288);

                        //start timer
                        engine.Timer.Start();
                        break;
                    }
            }
        }
        #endregion
    }
}
