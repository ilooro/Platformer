using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Platformer.Classes
{
    internal class MovableEntity(double speed, double jumpSpeed, int jumpForce, Rect hitBox,
        int heatPoint, int attackPower, int attackSpeed, bool autoJump = false, String? spritePath = null, bool isAnimated_ = false) : Entity(heatPoint, attackPower, attackSpeed, hitBox, spritePath, isAnimated_)
    {
        public double Speed { get; set; } = speed;
        public double JumpSpeed { get; set; } = jumpSpeed;
        public int JumpForce { get; set; } = jumpForce;
        public double Gravity { get; set; } = 12;

        private double _inputHorOffset = 0;
        private int _currentJumpForce = 0;
        private bool _isJumping = false;
        private bool _onFloor = false;

        public double X { get; private set; } = hitBox.X;
        public double Y { get; private set; } = hitBox.Y;

        public Point GetCenter()
        {
            return new(HitBox.X + HitBox.Width / 2, HitBox.Y + HitBox.Height / 2);
        }

        protected void TargetingTransform(List<Rect> boxes, Rect target, bool isFlying = false)
        {
            if (isFlying)
            {
                if (/*HitBox.Right < target.Left || HitBox.Left > target.Right*/
                    !HitBox.IntersectsWith(target))
                {
                    var direction = new Point(target.X + target.Width / 2,
                        target.Y + target.Height / 2) - GetCenter();
                    double len = direction.Length;
                    double horOffset = Speed * direction.X / len;
                    double verOffset = Speed * direction.Y / len;

                    invertFrame = (horOffset < 0);

                    Transform(boxes, horOffset, verOffset);
                }
                else
                    return;
            }
            else
            {
                if (HitBox.Right <= target.Left) {
                    currState = AnimationState.Walk;
                    invertFrame = false;
                    MoveRight();
                }
                else if (HitBox.Left >= target.Right) {
                    currState = AnimationState.Walk;
                    invertFrame = true;
                    MoveLeft();
                }
                else
                    StopMove();
                Transform(boxes);
            }
        }

        protected void Transform(List<Rect> boxes, double? inHorOffset = null, double? inVerOffset = null)
        {
            double horOffset = inHorOffset ?? _inputHorOffset;
            double verOffset = inVerOffset ?? Gravity;

            if (_isJumping && _currentJumpForce <= 0)
                _isJumping = false;
            if (_isJumping)
            {
                verOffset = -JumpSpeed;
                _currentJumpForce--;
            }

            _onFloor = false;

            double prevX = HitBox.X;
            double prevY = HitBox.Y;

            HitBox.X += horOffset;
            HitBox.Y += verOffset;

            foreach (Rect box in boxes)
            {
                if (HitBox.IntersectsWith(box))
                {
                    // Top colision
                    if (prevY > box.Bottom && HitBox.Top < box.Bottom)
                    {
                        HitBox.Y = box.Bottom;
                        _isJumping = false;
                        _currentJumpForce = 0;
                    }

                    // Bottom colision
                    if (prevY + HitBox.Height < box.Top && HitBox.Bottom > box.Top)
                        HitBox.Y = box.Top - HitBox.Height;
                    else if (!_isJumping && prevY + HitBox.Height == box.Top)
                    {
                        _onFloor = true;
                        HitBox.Y = prevY;
                    }

                    // Right colision
                    if (HitBox.Bottom - box.Top >= 1 && prevX + HitBox.Width < box.Left && HitBox.Right > box.Left)
                        HitBox.X = box.Left - HitBox.Width;
                    else if (prevX + HitBox.Width == box.Left && horOffset > 0)
                    {
                        if (autoJump)
                        {
                            HitBox.X = prevX;
                            Jump();
                        }
                        if (HitBox.Bottom - box.Top >= 1)
                            HitBox.X = prevX;
                    }

                    // Left colision
                    if (HitBox.Bottom - box.Top >= 1 && prevX > box.Right && HitBox.Left < box.Right)
                        HitBox.X = box.Right;
                    else if (prevX == box.Right && horOffset < 0)
                    {
                        if (autoJump)
                        {
                            HitBox.X = prevX;
                            Jump();
                        }
                        if (HitBox.Bottom - box.Top >= 1)
                            HitBox.X = prevX;
                    }
                }
            }

            X = HitBox.X;
            Y = HitBox.Y;
        }
        public void MoveLeft() { _inputHorOffset = -Speed; }
        public void MoveRight() { _inputHorOffset = Speed; }

        public void StopLeft()
        {
            if (_inputHorOffset < 0)
                _inputHorOffset = 0;
        }

        public void StopRight()
        {
            if (_inputHorOffset > 0)
                _inputHorOffset = 0;
        }

        public void StopMove() { _inputHorOffset = 0; }

        public void Jump()
        {
            if (!_isJumping && _onFloor)
            {
                _isJumping = true;
                _currentJumpForce = JumpForce;
            }
        }
    }
}
