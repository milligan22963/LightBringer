using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LightBringer.Visuals
{
    public delegate void AnimationViewFinished();

    /// <summary>
    /// Interaction logic for AnimationView.xaml
    /// </summary>
    public partial class AnimationView : Window
    {
        #region DATA
        private DispatcherTimer m_animationTimer;// = new DispatcherTimer();
        int m_startFrame;
        int m_currentFrame;
        int m_currentStep;
        int m_totalSteps;
        int m_endFrame;
        bool m_isAnimating;
        public AnimationViewFinished AnimationFinished;
        #endregion // DATA

        public AnimationView()
        {
            InitializeComponent();
            
            m_animationTimer = new DispatcherTimer();

            m_animationTimer.Tick += OnAnimationTimer_Tick;
            m_isAnimating = false;
            Panel = null;
        }

        void OnAnimationTimer_Tick(object sender, EventArgs e)
        {
            // Our animation timer has gone off, assuming we are currently animating
            // we will move to the next frame and stop if we just finished the last frame
            if (m_currentFrame < m_endFrame)
            {
                // Keep pausedif paused
                if (IsPaused == false)
                {
                    m_currentStep++;
                    if (m_currentStep >= m_totalSteps)
                    {
                        m_currentStep = 0;
                        m_currentFrame++;
                        m_totalSteps = Math.Max(1, Panel.StepsForFrame(m_currentFrame));
                    }
                    Draw(); // force the next draw
                }
            }
            else
            {
                IsAnimating = false;
            }
        }

        #region PROPERTIES
        public bool IsAnimating
        {
            get
            {
                return m_isAnimating;
            }
            private set
            {
                if (m_isAnimating != value)
                {
                    if (value == true)
                    {
                        m_animationTimer.Start();
                    }
                    else // we are done, stop the timer etc
                    {
                        m_animationTimer.Stop();
                        if (AnimationFinished != null)
                        {
                            AnimationFinished();
                        }
                    }
                    m_isAnimating = value;
                }
            }
        }

        public bool IsPaused
        {
            get;
            set;
        }

        public AnimationPanel Panel
        {
            get;
            set;
        }
        #endregion // PROPERTIES

        public void Home()
        {
            m_currentFrame = m_startFrame;
            m_currentStep = 0;
        }

        public void Forward(int numFrames)
        {
            if (m_currentFrame + numFrames < m_endFrame)
            {
                m_currentFrame += numFrames;
            }
            else
            {
                m_currentFrame = m_endFrame;
            }
            m_currentStep = 0;
        }

        public void Rewind(int numFrames)
        {
            if (m_currentFrame - numFrames > m_startFrame)
            {
                m_currentFrame -= numFrames;   
            }
            else
            {
                m_currentFrame = 0;
            }
            m_currentStep = 0;
        }

        public void End()
        {
            m_currentFrame = m_endFrame;
            m_currentStep = m_totalSteps;
        }

        public void StartAnimating(int startFrame, int endFrame, double frameRate)
        {
            IsPaused = false;
            if (Panel != null)
            {
                m_startFrame = startFrame;
                m_endFrame = endFrame;
                m_currentFrame = m_startFrame;

                // start my timer
                m_animationTimer.Interval = TimeSpan.FromMilliseconds((1.0f / frameRate) * 1000);

                m_totalSteps = Math.Max(1, Panel.StepsForFrame(m_currentFrame)); // want at least one
                m_currentStep = 0;

                // I could call draw however it will check to see if we are currently animating
                // I want to do this prior to setting the animation flag as that starts my timer
                // and I want to ensure that at all frame rates, I have displayed the first frame
                Panel.ShowFrame(m_currentFrame, m_currentStep, m_totalSteps, AnimationTarget);

                // And now put us in animation mode and start our timer
                IsAnimating = true;
            }
        }

        public void RestartAnimating()
        {
            if (IsAnimating == true)
            {
                if (IsPaused == true)
                {
                    IsPaused = false;
                }
            }
        }

        public void PauseAnimating()
        {
            IsPaused = true;
        }

        public void StopAnimating()
        {
            IsPaused = false;
            IsAnimating = false;
        }

        protected void Draw()
        {
            if (IsAnimating == true)
            {
                Panel.ShowFrame(m_currentFrame, m_currentStep, m_totalSteps, AnimationTarget);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }
}
