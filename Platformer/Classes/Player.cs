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
    internal class Player(double speed, double jumpSpeed, int jumpForce, Rect hitBox, int heatPoint, int attackPower) : MovableEntity(speed, jumpSpeed, jumpForce, hitBox, heatPoint, attackPower)
    {
        public void Update(List<Rect> boxes)
        {
            Transform(boxes);
        }
    }
}
