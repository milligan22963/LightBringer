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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using SharedInterfaces;
using System.Xml;
using System.ComponentModel;

namespace Transform
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Transform"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Transform;assembly=Transform"
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
    ///     <MyNamespace:Transform/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_TransformImage", Type = typeof(Image))]
    [TemplatePart(Name = "PART_TransformName", Type = typeof(TextBlock))]
    public class Transform : Control, ITransform, INotifyPropertyChanged
    {
        private Image m_transformIcon;
        private TextBlock m_transformName;
        private const string m_transformElementName = "Transform";
        private const string m_transformTypeAttribute = "Type";
        private Point m_startDragPoint = new Point();
        private bool m_dragStarted = false;
        protected ITransform m_linkedTransform;

        static Transform()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Transform), new FrameworkPropertyMetadata(typeof(Transform)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get the image
            m_transformIcon = GetTemplateChild("PART_TransformImage") as Image;

            if (m_transformIcon != null)
            {
                Assembly thisAssembly = System.Reflection.Assembly.GetCallingAssembly();// GetExecutingAssembly();
                if (thisAssembly != null)
                {
                    string resourceName = string.Format("{0}.Images.{1}", thisAssembly.GetName().Name, "Icon.png");
                    Stream iconStream = thisAssembly.GetManifestResourceStream(resourceName);

                    if (iconStream != null)
                    {
                        PngBitmapDecoder bmpDecoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                        if (bmpDecoder.Frames.Count > 0)
                        {
                            m_transformIcon.Source = bmpDecoder.Frames[0]; // typically the first frame
                            m_transformIcon.Stretch = Stretch.Fill;
                        }
                        iconStream.Close();
                    }
                }
                m_transformIcon.Height = Height;
                m_transformIcon.Width = Width;
            }

            m_transformName = GetTemplateChild("PART_TransformName") as TextBlock;
            if (m_transformName != null)
            {
                if (Height > 16)
                {
                    if (m_transformName.Height != double.NaN)
                    {
                        Height += m_transformName.Height;
                    }

                    // if we have text then we may want to widen this a little
                    if (m_transformName.Width != double.NaN)
                    {
                        Width += m_transformName.Width;
                    }
                    else if (Width < 32)
                    {
                        Width = 32;
                    }
                }
                else
                {
                    m_transformName.Visibility = System.Windows.Visibility.Hidden;
                }
            }

            // Setup drag/drop
            MouseLeftButtonUp += Transform_MouseLeftButtonUp;
            PreviewMouseLeftButtonDown += Transform_PreviewMouseLeftButtonDown;
            PreviewMouseMove += Transform_PreviewMouseMove;
        }

        void Transform_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragStarted == true)
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = m_startDragPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject("Transform", GetType().ToString());
                    DragDrop.DoDragDrop(sender as Transform, dragData, DragDropEffects.Move);
                    m_dragStarted = false;
                }
            }
        }

        void Transform_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_dragStarted = true;
            m_startDragPoint = e.GetPosition(null);
        }

        void Transform_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_dragStarted = false;

            // will need to ensure we are on the animation panel
            ConfigureTransform();
        }

        static public string ElementName
        {
            get
            {
                return m_transformElementName;
            }
        }

        static public string TransformType(XmlReader reader)
        {
            string transformType = reader.GetAttribute(m_transformTypeAttribute);

            return transformType;
        }

        protected void SetIcon(Image iconImage)
        {
            m_transformIcon.Source = iconImage.Source; // set source
        }

        protected Image GetIcon()
        {
            return m_transformIcon;
        }

        protected void SetName(string name)
        {
            if (m_transformName != null)
            {
                m_transformName.Text = name;
            }
        }

        #region ITransform

        public virtual SharedInterfaces.TransformCategory Category()
        {
            return SharedInterfaces.TransformCategory.eIgnore; // default to ignore
        }

        public virtual SharedInterfaces.TransformOrigin Origin()
        {
            return SharedInterfaces.TransformOrigin.eSystem;
        }

        public virtual void PreRenderFrames(int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
        }

        public virtual List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            return endingPixels;
        }

        public virtual void LinkTransform(ITransform linkedTransform)
        {
            // When persisting we need to relink upon restoration
            m_linkedTransform = linkedTransform;
        }

        public virtual int ComputeFrameCount(double frameRate)
        {
            return 0;
        }

        public virtual void SetIconSize(SharedInterfaces.TransformIconSize size)
        {
            switch (size)
            {
                case TransformIconSize.eSmall:
                    {
                        Width = 16;
                        Height = 16;
                    }
                    break;
                case TransformIconSize.eMedium:
                    {
                        Width = 32;
                        Height = 32;
                    }
                    break;
                case TransformIconSize.eLarge:
                    {
                        Width = 64;
                        Height = 64;
                    }
                    break;
                case TransformIconSize.eXLarge:
                    {
                        Width = 128;
                        Height = 128;
                    }
                    break;
            }
        }

        public virtual void ConfigureTransform()
        {
            // The base doesn't have any configuration
        }

        #endregion // ITransform

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region PERSISTENCE

        protected virtual void StartLoad(XmlReader reader)
        {
        }

        protected virtual void FinishLoad(XmlReader reader)
        {
        }

        protected virtual void StartSave(XmlWriter writer)
        {
            writer.WriteStartElement(m_transformElementName);
            writer.WriteAttributeString(m_transformTypeAttribute, GetType().ToString());
        }

        protected virtual void FinishSave(XmlWriter writer)
        {
            writer.WriteEndElement();
        }

        #region IPersistence
        public virtual void Load(XmlReader reader)
        {
            StartLoad(reader);
            FinishLoad(reader);
        }

        public virtual void Save(XmlWriter writer)
        {
            StartSave(writer);
            FinishSave(writer);
        }

        public virtual void Load(BinaryReader reader)
        {
        }

        public virtual void Save(BinaryWriter writer)
        {
        }
        #endregion
        #endregion // PERSISTENCE
    }
}
