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
    internal class Player(double speed = 5, double jumpSpeed = 7, int jumpForce = 7, int heatPoint = 10, int attackPower = 1, int attackSpeed = 3, Rect hitBox=new(), String? spritePath = null, bool isAnimated_ = false) 
        : MovableEntity(speed, jumpSpeed, jumpForce, hitBox, heatPoint, attackPower, attackSpeed, false, spritePath, isAnimated_)
    {
        public void Update(List<Rect> boxes)
        {
            Transform(boxes);
        }
    }
}
