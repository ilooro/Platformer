using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Platformer.Classes {
    internal class Engine {
        #region Attributes
        //engine run time
        public DispatcherTimer Timer { get; private set; } //timer variable
        public float DeltaTime { get; private set; }       //time difference between timer ticks (used for rate-independent rendering)

        //engine rendering cycle
        public EventHandler? Render { get; private set; }  //render function which invokes by timer with each tick
        #endregion
        #region Methods
        //setters
        public void SetDeltaTime(float timeDiff) {
            DeltaTime = timeDiff;
            Timer.Interval = TimeSpan.FromSeconds(DeltaTime);
        } //deltaTime setter
        public void SetRender(EventHandler? renderFunc) {
            Render = renderFunc;
            if (renderFunc != null) Timer.Tick += Render;
        } //render function setter

        //constructors
        public Engine(float timeDiff = 1.0f / 60) {
            //timer initialization
            Timer = new();

            //deltaTime initialization (default value corresponds to 60FPS rate)
            SetDeltaTime(timeDiff);
        } //default constructor
        public Engine(EventHandler? renderFunc, float timeDiff = 1.0f / 60) {
            //timer initialization
            Timer = new();

            //deltaTime initialization (default value corresponds to 60FPS rate)
            SetDeltaTime(timeDiff);

            //render function initialization
            SetRender(renderFunc);
        } //constructor with render function initialization
        #endregion
    }
}
