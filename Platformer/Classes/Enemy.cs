using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Platformer.Classes
{
    internal class Enemy(double speed, int jumpSpeed, int jumpForce, int heatPoint, int attackPower, 
        Rect hitBox = new(), bool isFlying = false) : MovableEntity(speed, jumpSpeed, jumpForce, hitBox, heatPoint, attackPower, true)
    {
        public void Update(List<Rect> boxes, Rect target)
        {
            TargetingTransform(boxes, target, isFlying);
        }
    }
}
