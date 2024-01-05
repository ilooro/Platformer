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
    internal class Player(int speed, int jumpSpeed, int jumpForce)
    {
        #region Attributes
        /*
        //movement improvement
        private Vector2 movementSpeed;    //{ speed along 0x axis, speed along 0y axis }
        private Vector2 movementSpeedMax; //{ maximum achievable speed along 0x axis, maximum achievable speed along 0y axis }
        private Vector2 movementAccel;    //{ accel along 0x axis, accel along 0y axis }
        private bool isGrounded;          //flag for whether the player is grounded
        */

        private readonly int _speed     = speed;
        private readonly int _jumpSpeed = jumpSpeed;
        private readonly int _jumpForce = jumpForce;

        public int CurrentHorOffset { get; private set; } = 0;
        public int CurrentVerOffset { get; private set; } = 0;

        private int _currentJumpForce = 0;
        private bool _isJumping = false;

        private int _inputHorOffset = 0;
        private Rect _prevHeroBox = new(0, 0, 0, 0);
        #endregion
        #region Methods
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
        #endregion
    }
}
