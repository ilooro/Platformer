using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Numerics;
using System.Windows.Controls;

namespace Platformer.Classes
{
    internal class Hero
    {
        private int _speed;
        private int _jumpSpeed;
        private int _jumpForce;

        public int CurrentHorOffset { get; private set; } = 0;
        public int CurrentVerOffset { get; private set; } = 0;

        private int _currentJumpForce = 0;
        private bool _isJumping = false;

        private int _inputHorOffset = 0;
        private Rect _prevHeroBox = new Rect(0, 0, 0, 0);

        public Hero(int speed, int jumpSpeed, int jumpForce) 
        {
            _speed = speed;
            _jumpSpeed = jumpSpeed;
            _jumpForce = jumpForce;
        }

        public void Update(ref Rect heroBox, List<Rect> boxes)
        {
            if (_isJumping && _currentJumpForce <= 0)
                _isJumping = false;
            if (_isJumping)
            {
                CurrentVerOffset = -_jumpSpeed;
                _currentJumpForce--;
            }
            else
            {
                CurrentVerOffset = _jumpSpeed;
            }

            CurrentHorOffset = _inputHorOffset;
            foreach (Rect box in boxes)
            {
                if (heroBox.IntersectsWith(box))
                {
                    if (_prevHeroBox.Bottom < box.Top && heroBox.Bottom > box.Top)
                    {
                        CurrentVerOffset = (int)(box.Top - heroBox.Bottom);
                    }
                    if (!_isJumping && heroBox.Bottom == box.Top)
                    {
                        CurrentVerOffset = 0;
                    }

                    if (_prevHeroBox.Right <= box.Left)
                    {
                        if (heroBox.Right > box.Left)
                        {
                            CurrentHorOffset = (int)(box.Left - heroBox.Right);
                        }
                        else if (heroBox.Right == box.Left && CurrentHorOffset > 0)
                        {
                            CurrentHorOffset = 0;
                        }
                    }
                    else if (_prevHeroBox.Left >= box.Right)
                    {
                        if (heroBox.Left < box.Right)
                        {
                            CurrentHorOffset = (int)(box.Right - heroBox.Left);
                        }
                        else if (heroBox.Left == box.Right && CurrentHorOffset < 0)
                        {
                            CurrentHorOffset = 0;
                        }
                    }
                }
            }
            _prevHeroBox = heroBox;
        }

        public void MoveLeft()
        {
            _inputHorOffset = -_speed;
        }

        public void MoveRight()
        {
            _inputHorOffset = _speed;
            //_inputHorOffset = _rightWall ? 0 : _speed;
        }

        public void StopMoving()
        {
            _inputHorOffset = 0;
        }

        public void Jump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _currentJumpForce = _jumpForce;
            }
        }
    }
}
