using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Platformer.Classes
{
    internal class Entity(int maxHeatPoint, int attackPower, int attackSpeed, Rect hitBox)
    {
<<<<<<< HEAD
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
=======
        // TODO: Some variables for animation or create a new interface for animated objects
>>>>>>> 86cf06c3b80ff3140a6631ecca45ced5cbb6fad7

        public Rect HitBox = hitBox;

        public int MaxHitPoints { get; set; } = maxHeatPoint;
        public int HitPoints { get; set; } = maxHeatPoint;
        public int AttackPower { get; set; } = attackPower;
        public int AttackSpeed { get; set; } = attackSpeed; // in timer ticks between attack
        private double _currentAttackState = 0;

        public void Attack(Entity entity)
        {
            _currentAttackState++;
            if (_currentAttackState >= AttackSpeed)
            {
                entity.HitPoints -= AttackPower;
                _currentAttackState = 0;
            }
        }
    }
}
