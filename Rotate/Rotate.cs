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
using System.Xml;
using System.IO;
using System.Reflection;

namespace Rotate
{
    public enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Rotate"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Rotate;assembly=Rotate"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class Rotate : Transform.Transform
    {
        private const string m_rotateAttributeName = "Rotate";
        private const string m_rotateDirectionAttribute = "Direction";
        private const string m_rotateCountAttribute = "Count";

        private RotateDirection m_rotateDirection;
        private double m_rotateCount;
        private Image m_clockWiseIcon;
        private Image m_counterClockWiseIcon;

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double),
            typeof(Rotate),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        static Rotate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Rotate), new FrameworkPropertyMetadata(typeof(Rotate)));
        }

        public Rotate()
        {
            Direction = RotateDirection.Clockwise;
            Count = 1;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_clockWiseIcon = GetIcon();

            // This only needs to be overridden if the current class needs to pull additional items out of the template i.e.
            // if there happens to be an icon set opposed to a single icon which needs to be named Icon.png by default
            Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (thisAssembly != null)
            {
                string resourceName = string.Format("{0}.Images.{1}", thisAssembly.GetName().Name, "IconCCW.png");
                Stream iconStream = thisAssembly.GetManifestResourceStream(resourceName);

                if (iconStream != null)
                {
                    PngBitmapDecoder bmpDecoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    if (bmpDecoder.Frames.Count > 0)
                    {
                        m_counterClockWiseIcon = new Image();
                        m_counterClockWiseIcon.Source = bmpDecoder.Frames[0]; // typically the first frame
                        m_counterClockWiseIcon.Stretch = Stretch.Fill;
                    }
                    iconStream.Close();
                }
            }
            ToolTipService.SetToolTip(this, m_rotateAttributeName);

            SetName(m_rotateAttributeName);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            RotateTransform rotation = new RotateTransform(Angle);

            if (Direction == RotateDirection.CounterClockwise)
            {
                rotation.Angle = 0 - Angle;
            }
            rotation.CenterX = GetIcon().RenderSize.Width / 2;
            rotation.CenterY = GetIcon().RenderSize.Height / 2;

            GetIcon().RenderTransform = rotation;
            base.OnRender(drawingContext);
        }

        #region PROPERTIES
        public double Angle
        {
            get
            {
                return (double)GetValue(AngleProperty);
            }
            set
            {
                SetValue(AngleProperty, value);
            }
        }

        public RotateDirection Direction
        {
            get
            {
                return m_rotateDirection;
            }
            set
            {
                m_rotateDirection = value;
                if (m_rotateDirection == RotateDirection.Clockwise)
                {
                    if (m_clockWiseIcon != null)
                    {
                        SetIcon(m_clockWiseIcon);
                    }
                }
                else
                {
                    if (m_counterClockWiseIcon != null)
                    {
                        SetIcon(m_counterClockWiseIcon);
                    }
                }
                OnPropertyChanged("Direction");
            }
        }

        public double Count
        {
            get
            {
                return m_rotateCount;
            }
            set
            {
                m_rotateCount = value;
                OnPropertyChanged("Count");
            }
        }
        #endregion
        #region ITransform

        public override SharedInterfaces.TransformCategory Category()
        {
            return SharedInterfaces.TransformCategory.eMovement;
        }

        public override SharedInterfaces.TransformOrigin Origin()
        {
            return SharedInterfaces.TransformOrigin.eSystem;
        }

        public override void PreRenderFrames(int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            // do nothing
        }

        // we return a new set of pixels due to the pixels being modified/changed by other renderers
        public override List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            int stripLength = startingPixels.Count;

            // See how "far" we are into the animation
            double framePosition = (double)currentFrame / (double)totalFrames;

            // Determine the number of pixels to rotate based on number of rotations, number of pixels in the strip, and current frame position
            int pixelMovement = (int)Math.Floor(Count * (double)stripLength * framePosition) % stripLength;  // We do not need to move around more then once although the movement amount might be X times

            List<Color> transformPixels = new List<Color>();

            transformPixels.InsertRange(0, startingPixels);

            if (pixelMovement > 0)
            {
                uint stripOffset = (uint)pixelMovement;

                // If clockwise then shift down the strip
                // otherwise shift backup
                for (int colorIndex = 0; colorIndex < stripLength; colorIndex++)
                {
                    Color newColor = new Color();
                    newColor.R = startingPixels[colorIndex].R;
                    newColor.G = startingPixels[colorIndex].G;
                    newColor.B = startingPixels[colorIndex].B;
                    newColor.A = startingPixels[colorIndex].A;

                    transformPixels[(int)stripOffset] = newColor;
                    if (Direction == RotateDirection.Clockwise)
                    {
                        stripOffset++;
                    }
                    else
                    {
                        stripOffset--;
                    }
                    stripOffset %= (uint)pixelMovement; // roll over
                }
            }
            return transformPixels;
        }

        public override int ComputeFrameCount(double frameRate)
        {
            int numFrames = 1;

            if (m_linkedTransform != null)
            {
                numFrames = m_linkedTransform.ComputeFrameCount(frameRate);
            }

            return numFrames;
        }

        public override void ConfigureTransform()
        {
            ConfigSettings settings = new ConfigSettings();
            bool? dialogResults = false;

            settings.Direction = Direction;
            settings.Count = Count;

            dialogResults = settings.ShowDialog();

            if (dialogResults == true)
            {
                Direction = settings.Direction;
                Count = settings.Count;
            }
        }
        #endregion // ITransform

        #region PERSISTENCE

        #region IPersistence
        public override void Load(XmlReader reader)
        {
            StartLoad(reader);

            try
            {
                Direction = (RotateDirection)Enum.Parse(typeof(RotateDirection), reader.GetAttribute(m_rotateAttributeName));
            }
            catch (ArgumentException)
            {
                Direction = RotateDirection.Clockwise;
            }
            Count = Convert.ToDouble(reader.GetAttribute(m_rotateCountAttribute));
            FinishLoad(reader);
        }

        public override void Save(XmlWriter writer)
        {
            StartSave(writer);

            // add in our specific attribute(s)
            writer.WriteAttributeString(m_rotateAttributeName, Direction.ToString());
            writer.WriteAttributeString(m_rotateCountAttribute, Count.ToString());
            FinishSave(writer);
        }

        public override void Load(BinaryReader reader)
        {
        }

        public override void Save(BinaryWriter writer)
        {
        }
        #endregion
        #endregion // PERSISTENCE
    }
}
