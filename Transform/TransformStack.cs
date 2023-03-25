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
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:TransformStack/>
    ///
    /// </summary>
    public class TransformStack : StackPanel, SharedInterfaces.ITransform, SharedInterfaces.IPersistence
    {
        private const string m_TransformsElementName = "Transforms";

        static TransformStack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransformStack), new FrameworkPropertyMetadata(typeof(TransformStack)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            VerticalAlignment = System.Windows.VerticalAlignment.Center;
        }

        #region IPERSISTENCE
        public void Load(XmlReader reader)
        {
            Children.Clear(); // out with the old, in with the new

            SharedInterfaces.TransformFactory factory = SharedInterfaces.TransformFactory.GetInstance();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    // If it is a transform, need to know what type before we expand it
                    if (reader.Name == Transform.ElementName)
                    {
                        string transformType = Transform.TransformType(reader);

                        SharedInterfaces.ITransform transform = factory.GetTransform(transformType);

                        transform.Load(reader);

                        Children.Add(transform as UIElement);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_TransformsElementName)
                    {
                        break;
                    }
                }
            }
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteStartElement(m_TransformsElementName);
            // Dump each of our children that are transforms

            foreach (Control child in Children)
            {
                SharedInterfaces.IPersistence persistentChild = child as SharedInterfaces.IPersistence;

                if (persistentChild != null)
                {
                    persistentChild.Save(writer);
                }
            }
            writer.WriteEndElement();
        }

        public void Load(BinaryReader reader)
        {
        }

        public void Save(BinaryWriter writer)
        {
        }
        #endregion // IPERSISTENCE

        public int Transforms
        {
            get
            {
                return Children.Count;
            }
        }

        #region ITRANSFORM
        public SharedInterfaces.TransformCategory Category()
        {
            return SharedInterfaces.TransformCategory.eIgnore;
        }

        public SharedInterfaces.TransformOrigin Origin()
        {
            return SharedInterfaces.TransformOrigin.eSystem;
        }

        public void LinkTransform(SharedInterfaces.ITransform linkedTransform)
        {
            // cant really do much here
        }

        private void PreRenderFrames(int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            foreach (Transform transform in Children)
            {
                transform.PreRenderFrames(totalFrames, stripId, startingPixels, endingPixels);
            }
        }

        public List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            List<Color> renderedPixels = endingPixels;

            if (currentFrame == 0)
            {
                PreRenderFrames(totalFrames, stripId, startingPixels, endingPixels);
            }

            // process each time based frame
            foreach (Transform transform in Children)
            {
                if (transform.Category() == SharedInterfaces.TransformCategory.eTime)
                {
                    renderedPixels = transform.RenderFrames(currentFrame, totalFrames, stripId, startingPixels, endingPixels);
                }
            }

            // We need to "render" the time based ones first then the non-time ones
            foreach (Transform transform in Children)
            {
                if (transform.Category() != SharedInterfaces.TransformCategory.eTime)
                {
                    renderedPixels = transform.RenderFrames(currentFrame, totalFrames, stripId, renderedPixels, endingPixels);
                }
            }

            return renderedPixels;
        }

        public int ComputeFrameCount(double frameRate)
        {
            int frameCount = 0;

            foreach (SharedInterfaces.ITransform transform in Children)
            {
                frameCount = Math.Max(frameCount, transform.ComputeFrameCount(frameRate));
            }

            return frameCount;
        }

        public void SetIconSize(SharedInterfaces.TransformIconSize size)
        {
            // do nothing
        }

        /// <summary>
        /// Used to configure this transform
        /// </summary>
        public void ConfigureTransform()
        {
            // do nothing
        }

        #endregion // ITRANSFORM

    }
}
