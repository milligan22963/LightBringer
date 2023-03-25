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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightBringer.Visuals
{
    public delegate void AnimationStopped(object sender, EventArgs e);
    public delegate void AnimationStarted(object sender, EventArgs e);
    public delegate void AnimationPause(object sender, EventArgs e);
    public delegate void AnimationProgress(object sender, AnimationEvent e);
    public delegate void AnimationResumed(object sender, EventArgs e);

    /// <summary>
    /// Interaction logic for AnimationPanel.xaml
    /// </summary>
    public partial class AnimationPanel : UserControl
    {
        #region DATA_REGION
        public AnimationStopped AnimationStoppedHandler;
        public AnimationStarted AnimationStartedHandler;
        public AnimationProgress AnimationProgressHandler;
        public AnimationPause AnimationPausedHandler;
        public AnimationResumed AnimationResumedHandler;
        AnimationView m_view;
        bool m_isPlaying;
        List<Visuals.AnimationFrame> m_frames;
        public FrameSelectedDelagate FrameSelected;

        #endregion // DATA_REGION

        public AnimationPanel()
        {
            InitializeComponent();

            m_frames = new List<AnimationFrame>();
            FrameRate = 29.9997;

            m_view = new AnimationView();
            m_view.AnimationFinished += AnimationStopped;
            m_view.Closing += OnClosing;

            m_view.Panel = this;
        }

        #region PROPERTIES
        public UIElementCollection Children
        {
            get
            {
                return AnimationContainer.Children;
            }
        }

        public double FrameRate
        {
            get;
            set;
        }

        #endregion // PROPERTIES

        public void AddFrame(AnimationFrame frame)
        {
            Children.Add(frame); // this can contain both frames and stack panels
            m_frames.Add(frame); // only frames go here

            frame.FrameSelected += OnFrameSelected;
            frame.TransformAdded += OnTransformAdded;
        }

        public int FrameCount
        {
            get
            {
                return m_frames.Count;
            }
        }

        /// <summary>
        /// When doing animation and changing views, we will want to keep the frames but will
        /// remove the transform stacks and add them again after as needed
        /// </summary>
        /// <param name="clearAll">Clears all children, frames and stacks</param>
        public void ClearData(bool clearAll)
        {
            // zero based
            // Walk backwards so we don't invalidate the index we are using
            // We do this regardless of clear all so that our transforms
            // are cleaned up and the child transforms are disassociated from the parent
            for (int index = Children.Count - 1; index >= 0; index--)
            {
                Transform.TransformStack transformStack = Children[index] as Transform.TransformStack;

                if (transformStack != null)
                {
                    transformStack.Children.Clear();
                    Children.Remove(transformStack);
                }
            }

            if (clearAll == true)
            {
                m_frames.Clear();
                Children.Clear();
            }
        }

        public AnimationFrame GetFrame(int frameId)
        {
            AnimationFrame returnFrame = null;

            if (frameId < m_frames.Count)
            {
                returnFrame = m_frames[frameId];
            }
            return returnFrame;
        }

        public void AddTransformPanel(AnimationFrame frame, Transform.TransformStack transformStack)
        {
            int index = Children.IndexOf(frame);

            if (index != -1)
            {
                // Add it after the given frame
                Children.Insert(index + 1, transformStack);
            }
        }

        #region ANIMATION_COMMANDS
        
        #region ANIMATION_CALLBACKS

        private void IssueAnimationStarted()
        {
            if (AnimationStartedHandler != null)
            {
                AnimationStartedHandler(this, new EventArgs());
            }
        }

        private void IssueAnimationPaused()
        {
            if (AnimationPausedHandler != null)
            {
                AnimationPausedHandler(this, new EventArgs());
            }
        }

        private void IssueAnimationProgress(int currentFrame, double percentageDone)
        {
            if (AnimationProgressHandler != null)
            {
                AnimationEvent e = new AnimationEvent(currentFrame);
                
                e.FramePercentage = percentageDone;

                AnimationProgressHandler(this, e);
            }
        }

        private void IssueAnimationStopped()
        {
            if (AnimationStoppedHandler != null)
            {
                AnimationStoppedHandler(this, new EventArgs());
            }
        }

        private void IssueAnimationResumed()
        {
            if (AnimationResumedHandler != null)
            {
                AnimationResumedHandler(this, new EventArgs());
            }
        }
        #endregion

        public void Home()
        {
            if (m_view != null)
            {
                m_view.Home();
            }
        }

        public void Forward()
        {
            m_view.Forward(5);
        }

        public void Rewind()
        {
            m_view.Rewind(5);
        }

        public void End()
        {
            if (m_view != null)
            {
                m_view.End();
            }
        }

        // Need a callback to indicate the show finishing
        public void Play(int startFrame, int endFrame)
        {
            if (m_isPlaying == false)
            {
                if (startFrame < 0)
                {
                    startFrame = 0;
                }

                if (endFrame >= m_frames.Count)
                {
                    endFrame = m_frames.Count - 1;
                }
                else if (endFrame < startFrame)
                {
                    endFrame = m_frames.Count - 1;
                }

                // Instruct the view to start showing from frame x to frame y
                m_isPlaying = true;

                if (m_view == null)
                {
                    m_view = new AnimationView();
                    m_view.AnimationFinished += AnimationStopped;
                    m_view.Closing += OnClosing;

                    m_view.Panel = this;
                }
                m_view.Show();
                m_view.Topmost = true;
                Visuals.AnimationFrame frame = m_frames[0] as Visuals.AnimationFrame;

                m_view.StartAnimating(startFrame, endFrame, FrameRate);

                IssueAnimationStarted();
            }
            else if (m_view.IsPaused == true)
            {
                m_view.RestartAnimating();
                IssueAnimationResumed();
            }
        }

        public void Pause()
        {
            if (m_isPlaying == true)
            {
                m_view.PauseAnimating();
                IssueAnimationPaused();
            }
        }

        public void AnimationStopped()
        {
            if (m_isPlaying == true)
            {
                IssueAnimationStopped();
                m_isPlaying = false;
            }
        }

        public void Stop()
        {
            if (m_isPlaying == true)
            {
                m_view.StopAnimating();
                IssueAnimationStopped();
                m_isPlaying = false;
            }
        }

        /*
         * ShowFrame - shows the given frame with the given offset on the given drawingContext
         * 
         * param
         *  frameId         - the frame to be shown
         *  offset          - how far into the frame we should go i.e. if there is a transform, then this will be framex.y
         *  drawingContext  - the context to draw the frame onto
         */
        public void ShowFrame(int frameId, int offset, int totalSteps, AnimationFrame animationFrame)
        {
            if (frameId < m_frames.Count)
            {
                Visuals.AnimationFrame frame = m_frames[frameId] as Visuals.AnimationFrame;

                // Display this frame on the drawing context
                animationFrame.AssociatedData = frame.AssociatedData;
                animationFrame.Offset = offset;
                animationFrame.Total = totalSteps;
                int nextFrame = frameId + 1;

                if (nextFrame < m_frames.Count)
                {
                    Visuals.AnimationFrame nextFrameVisual = m_frames[nextFrame] as Visuals.AnimationFrame;

                    animationFrame.NextFrame = nextFrameVisual.AssociatedData;
                }
                else
                {
                    animationFrame.NextFrame = null;
                }

                animationFrame.TransformStack = GetTransformStack(frame);
                animationFrame.InvalidateVisual();

                // Need to update offset to be the amount into the frame based on the transform
                IssueAnimationProgress(frameId, (offset / totalSteps) * 100.0);
            }
        }

        public int StepsForFrame(int frameId)
        {
            int stepsForFrame = 1;

            if (frameId < m_frames.Count)
            {
                Visuals.AnimationFrame frame = m_frames[frameId] as Visuals.AnimationFrame;
                Transform.TransformStack stack = GetTransformStack(frame);

                if (stack != null)
                {
                    stepsForFrame = stack.ComputeFrameCount(FrameRate);
                }
            }
            return stepsForFrame;
        }

        #endregion // ANIMATION_COMMANDS

        // Called when the view window closes
        private void OnClosing(object sender, EventArgs e)
        {
            Stop();
            m_view = null;
        }

        public void Shutdown()
        {
            if (m_view != null)
            {
                m_view.Close();
            }
        }

        private void OnFrameSelected(int frameId)
        {
            if (FrameSelected != null)
            {
                FrameSelected(frameId);
            }
        }

        private Transform.TransformStack GetTransformStack(AnimationFrame frame)
        {
            Transform.TransformStack stack = null;

            int index = Children.IndexOf(frame);
            if (index != -1)
            {
                if (Children.Count > index + 1)
                {
                    stack = Children[index + 1] as Transform.TransformStack;
                }
            }
            return stack;
        }

        private void OnTransformAdded(AnimationFrame frame, SharedInterfaces.ITransform transform)
        {
            Transform.TransformStack transformStack = GetTransformStack(frame);

            // If it is still null then we are at the end of the animation panel or the
            // next one was a frame opposed to transform stack
            if (transformStack == null)
            {
                int index = Children.IndexOf(frame);
                if (index != -1)
                {
                    // Add it after the given frame
                    transformStack = new Transform.TransformStack();
                    Children.Insert(index + 1, transformStack);
                }
                else
                {
                    return; // getting out - bad things are happening
                }
            }
            transformStack.Children.Add(transform as UIElement);
        }
    }
}