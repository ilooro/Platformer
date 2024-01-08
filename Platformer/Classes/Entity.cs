using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Platformer.Classes
{
    public static class ListExtra
    {
        public static void Resize<T>(this List<T> list, int sz, T c)
        {
            int cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
            {
                if (sz > list.Capacity)
                    list.Capacity = sz;
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }
    }
    internal class Entity(int maxHitPoints, int attackPower, int attackSpeed, Rect hitBox, String? spritePath = null, bool isAnimated_ = false)
    {
        //basic attributes
        public Rect HitBox = hitBox;
        public int MaxHitPoints { get; set; } = maxHitPoints;
        public int HitPoints { get; set; } = maxHitPoints;
        public int AttackPower { get; set; } = attackPower;
        public int AttackSpeed { get; set; } = attackSpeed; // in timer ticks between attack
        private double _currentAttackState = 0;
        public String? SpritePath { get; set; } = spritePath;

        #region SpritesheetAnimation
        //flag, that activates entity animation
        public bool isAnimated = isAnimated_;

        //possible animation states (for convenience)
        public enum AnimationState {   Idle,   Walk,    Jump,  Fall,
                                     Attack, Defence, Damage, Death,
                                     TotalStates };
        //information about one of the many animation states
        public class StateInfo {
            public int  spritesheetIndex = -1;
            public uint framesCount      =  0;
            public StateInfo() {}
            public StateInfo(int spritesheetIndex, uint spritesCount) { 
                this.spritesheetIndex = spritesheetIndex;
                this.framesCount      = spritesCount;
            }
        };

        public Rectangle? sprite = null;
        public Point? hitBoxOffset = null;
        public Rect spritesheet = new Rect(0, 0, 0, 0);        //overall animation information
        public List<StateInfo> animationStates = [];           //overall animation states information
        public uint animationSpeed = 1;                        //animation speed
        public AnimationState prevState = AnimationState.Idle; //previous animation state
        public AnimationState currState = AnimationState.Idle; //current animation state
        public uint currFrame = 0;                             //current animation frame
        public uint ticksCount = 0;
        public bool invertFrame = false;                       //flag for inverting sprite sheet

        public void Animate(Rectangle? spriteSpace) {
            //entity is not animated or has no spritesheet
            if (SpritePath == null || spriteSpace == null || !isAnimated) return;

            if (ticksCount == 0) {
                if (prevState != currState) {
                    prevState = currState;
                    currFrame = 0;
                }

                BitmapImage spriteTexture = new BitmapImage(new Uri("pack://application:,,,/" + SpritePath));
                Vector2 tilingFactor = new(1.0f / (uint)spritesheet.Width, 1.0f / (uint)spritesheet.Height);
                ImageBrush spriteImageBrush = new();
                {
                    spriteImageBrush.ImageSource = spriteTexture;
                    spriteImageBrush.Viewbox = new Rect(tilingFactor.X * currFrame, tilingFactor.Y * animationStates[(int)currState].spritesheetIndex, tilingFactor.X, tilingFactor.Y);
                    TransformGroup transformation = new();
                    transformation.Children.Add(new ScaleTransform(invertFrame ? -1.0 : 1.0, 1.0));
                    transformation.Children.Add(new TranslateTransform(invertFrame ? 1.0 : 0.0, 0.0));
                    spriteImageBrush.RelativeTransform = transformation;
                } //tiling brush generation

                spriteSpace.Fill = spriteImageBrush;

                //proceed with animation
                currFrame = (currFrame + 1) % animationStates[(int)currState].framesCount;
            } //time to animate
            ticksCount = (ticksCount + 1) % animationSpeed;
        }

        #endregion

        public void ConfigureAnimation(List<Tuple<AnimationState, StateInfo>> statesConfig, uint animationSpeed = 1) {
            animationStates.Resize((int)AnimationState.TotalStates);
            uint maximumFrames = 0;
            foreach (var config in statesConfig) {
                animationStates[(int)config.Item1] = config.Item2;
                if (config.Item2.framesCount > maximumFrames)
                    maximumFrames = config.Item2.framesCount;
            }

            spritesheet.Height = statesConfig.Count;
            spritesheet.Width  = maximumFrames;

            this.animationSpeed = animationSpeed;

            currState = statesConfig.First().Item1;
        }
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
