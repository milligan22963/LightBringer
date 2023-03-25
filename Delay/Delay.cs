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
using System.ComponentModel;
using System.Reflection;

namespace Delay
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Delay"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Delay;assembly=Delay"
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
    public class Delay : Transform.Transform
    {
        private static readonly int IconCount = 10;

        private const string m_delayAttributeName = "Delay";
        private double m_delayTime;
        private static Image[] ms_transformImages = new Image[IconCount];

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(int),
            typeof(Delay),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        static Delay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Delay), new FrameworkPropertyMetadata(typeof(Delay)));
        }

        public Delay()
        {
            DelayTime = 2.0; // in seconds
        }

        /*
         * SetBinding(ToolTipProperty, new Binding
                            {
                                Source = this,
                                Path = new PropertyPath("ControlStatus"),
                                StringFormat = "Status: {0}"
                            });
         */
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ms_transformImages[0] = new Image(); // get the first default icon

            ms_transformImages[0].Source = GetIcon().Source;
            ms_transformImages[0].Stretch = Stretch.Fill;

            // This only needs to be overridden if the current class needs to pull additional items out of the template i.e.
            // if there happens to be an icon set opposed to a single icon which needs to be named Icon.png by default
            Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (thisAssembly != null)
            {
                for (int index = 1; index < IconCount; index++)
                {
                    // Now grab the other 9
                    string resourceName = string.Format("{0}.Images.{1}{2}.png", thisAssembly.GetName().Name, "Icon", index);
                    Stream iconStream = thisAssembly.GetManifestResourceStream(resourceName);

                    if (iconStream != null)
                    {
                        PngBitmapDecoder bmpDecoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                        if (bmpDecoder.Frames.Count > 0)
                        {
                            ms_transformImages[index] = new Image();
                            ms_transformImages[index].Source = bmpDecoder.Frames[0]; // typically the first frame
                            ms_transformImages[index].Stretch = Stretch.Fill;
                        }
                        iconStream.Close();
                    }
                }
            }
            ToolTipService.SetToolTip(this, m_delayAttributeName);

            SetName(m_delayAttributeName);
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            int imageIndex = Icon;

            if (imageIndex > 0 && imageIndex < IconCount)
            {
                SetIcon(ms_transformImages[imageIndex]);
            }
            else
            {
                SetIcon(ms_transformImages[0]);
            }
            base.OnRender(drawingContext);
        }

        #region PROPERTIES
        public int Icon
        {
            get
            {
                return (int)GetValue(IconProperty);
            }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public double DelayTime
        {
            get
            {
                return m_delayTime;
            }
            set
            {
                m_delayTime = value;
                OnPropertyChanged("DelayTime");
            }
        }
        #endregion // PROPERTIES

        #region ITransform

        public override SharedInterfaces.TransformCategory Category()
        {
            return SharedInterfaces.TransformCategory.eTime;
        }

        public override SharedInterfaces.TransformOrigin Origin()
        {
            return SharedInterfaces.TransformOrigin.eSystem;
        }

        public override void PreRenderFrames(int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            // nothing to be done here
        }

        // we return a new color set because, the old color set might be used with another transform which might change the actual pixels
        public override List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            // We need to generate a number of frames based on the starting frame passed in
            // Since this is a delay, we just need to duplicate it up to the delay amount
            // based on the frames per second value

            // I assume that each starting and ending pixels will be equivalent i.e. the same amount each
            List<Color> transformPixels = new List<Color>();

            transformPixels.InsertRange(0, startingPixels);

            return transformPixels;
        }

        public override void LinkTransform(SharedInterfaces.ITransform linkedTransform)
        {
            // When persisting we need to relink upon restoration
        }

        public override int ComputeFrameCount(double frameRate)
        {
            int numFrames = 0;

            numFrames = (int)Math.Ceiling(frameRate * DelayTime);

            return numFrames;
        }

        public override void ConfigureTransform()
        {
            ConfigSettings settings = new ConfigSettings();
            bool? dialogResults = false;

            settings.SelectedValue = DelayTime;

            dialogResults = settings.ShowDialog();

            if (dialogResults == true)
            {
                DelayTime = settings.SelectedValue;
            }
        }
        #endregion // ITransform

        #region PERSISTENCE

        #region IPersistence
        public override void Load(XmlReader reader)
        {
            StartLoad(reader);

            DelayTime = Convert.ToDouble(reader.GetAttribute(m_delayAttributeName));
            
            FinishLoad(reader);
        }

        public override void Save(XmlWriter writer)
        {
            StartSave(writer);

            // add in our specific attribute(s)
            writer.WriteAttributeString(m_delayAttributeName, DelayTime.ToString());

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
