using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platformer.Classes
{
    internal class Entity
    {
        //basic attributes
        public int HitPoint { get; set; }
        public int AttackPower { get; set; }
        String? SpritePath { get; set; }

        #region Animation
        readonly bool isAnimated = false;
        
        //animation states
        public enum AnimationState {   Idle,   Walk,  Jump,        Fall,
                                     Attack, Damage, Death, TotalStates };
        private class AnimInfo(int posInSheet = -1, uint spritesCount = 0) {
            public readonly  int posInSheet   = posInSheet;
            public readonly uint spritesCount = spritesCount;
        };
        //List<AnimInfo> Animation;
        #endregion
        //btw, i think, that interface for movable entity
        //could be here as well with isMovable flag._.

        public Entity(int heatPoint, int attackPower)
        {
            HitPoint = heatPoint;
            AttackPower = attackPower;
        }
    }
}
