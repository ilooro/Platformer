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
        // TODO: Some variables for animation or create a new interface for animated objects
        
        public int HitPoint { get; set; }
        public int AttackPower { get; set; }

        public Entity(int heatPoint, int attackPower)
        {
            HitPoint = heatPoint;
            AttackPower = attackPower;
        }
    }
}
