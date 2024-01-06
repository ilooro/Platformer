using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.IO;
using System.Windows.Shapes;

namespace Platformer.Classes
{
    internal class Player(double speed, double jumpSpeed, int jumpForce, int heatPoint, int attackPower, int attackSpeed, Rect hitBox=new()) 
        : MovableEntity(speed, jumpSpeed, jumpForce, hitBox, heatPoint, attackPower, attackSpeed)
    {
        public void Update(List<Rect> boxes)
        {
            Transform(boxes);
        }
    }
}
