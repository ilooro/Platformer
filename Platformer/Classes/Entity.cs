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
        // TODO: Some variables for animation or create a new interface for animated objects

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
