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
using System.Globalization;

namespace LightBringer.Visuals
{
    public delegate void FrameSelectedDelagate(int frameId);
    public delegate void TransformAddedDelagate(AnimationFrame frame, SharedInterfaces.ITransform transform);

    /// <summary>
    /// Interaction logic for Frame.xaml
    /// </summary>
    public partial class AnimationFrame : UserControl
    {
        public FrameSelectedDelagate FrameSelected;
        public TransformAddedDelagate TransformAdded;

        List<Color> [] m_firstColors; // this will be the array of the colors for the starting frame
        List<Color>[] m_lastColors; // this will be the array of the colors for the next frame

        public AnimationFrame()
        {
            InitializeComponent();

            AssociatedData = new DataModel.Frame();
            NextFrame = null;
            FrameId = 1;
            Offset = 0;
            m_firstColors = null;
            m_lastColors = null;
        }

        public DataModel.Frame AssociatedData
        {
            get;
            set;
        }

        public DataModel.Frame NextFrame
        {
            get;
            set;
        }

        public int FrameId
        {
            get;
            set;
        }

        public int Offset
        {
            get;
            set;
        }

        public int Total
        {
            get;
            set;
        }

        public Transform.TransformStack TransformStack
        {
            get;
            set;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            int maxPixels = 1;

            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            // First time through create our color arrays
            if (m_lastColors == null)
            {
                if (AssociatedData.Strips.Count > 0)
                {
                    m_firstColors = new List<Color>[AssociatedData.Strips.Count];
                    m_lastColors = new List<Color>[AssociatedData.Strips.Count];
                    for (int index = 0; index < AssociatedData.Strips.Count; index++)
                    {
                        m_firstColors[index] = new List<Color>();
                        m_lastColors[index] = new List<Color>();
                    }
                }
            }

            // Draw the strips
            foreach (DataModel.Strip strip in AssociatedData.Strips)
            {
                maxPixels = Math.Max(maxPixels, strip.Pixels.Count);
            }

            double borderWidth = FrameBorder.BorderThickness.Left + FrameBorder.BorderThickness.Right + FrameBorder.Margin.Left + FrameBorder.Margin.Right;
            double borderHeight = FrameBorder.BorderThickness.Top + FrameBorder.BorderThickness.Bottom + FrameBorder.Margin.Top + FrameBorder.Margin.Bottom;
            double widthDelta = (RenderSize.Width - borderWidth) / (maxPixels * 2);
            double heightDelta = (RenderSize.Height - borderHeight) / (Math.Max(AssociatedData.Strips.Count, 1) * 2);

            double pixelDiameter = Math.Min(widthDelta, heightDelta) - 0.2;

            Point centerPoint = new Point();

            centerPoint.Y = heightDelta + FrameBorder.BorderThickness.Top + FrameBorder.Margin.Top;

            int stripId = 0;

            foreach (DataModel.Strip strip in AssociatedData.Strips)
            {
                centerPoint.X = widthDelta + FrameBorder.BorderThickness.Left + FrameBorder.Margin.Left;

                // First one or no transforms
                // The m_firstColors array will be this frames starting color for each pixel
                if ((Offset == 0) && (TransformStack != null))
                {
                    m_firstColors[stripId].Clear();
                    m_lastColors[stripId].Clear();

                    foreach (DataModel.Pixel pixel in strip.Pixels)
                    {
                        m_firstColors[stripId].Add(pixel.PixelColor);
                        m_lastColors[stripId].Add(pixel.PixelColor);
                    }

                    // If we are working with a transform and there is another frame after this one
                    if ((TransformStack != null) && (NextFrame != null))
                    {
                        DataModel.Strip nextStrip = NextFrame.Strips[stripId];
                        int index = 0;
                        foreach (DataModel.Pixel pixel in nextStrip.Pixels)
                        {
                            m_lastColors[stripId][index++] =  pixel.PixelColor;
                        }
                    }
                }

                if (TransformStack != null)
                {
                    List<Color> renderedPixels = TransformStack.RenderFrames(Offset, Total, stripId, m_firstColors[stripId], m_lastColors[stripId]);
                    foreach (Color pixel in renderedPixels)
                    {
                        drawingContext.DrawEllipse(new System.Windows.Media.SolidColorBrush(pixel), null, centerPoint, pixelDiameter, pixelDiameter);
                        centerPoint.X += (widthDelta * 2);
                    }
                }
                else
                {
                    foreach (DataModel.Pixel pixel in strip.Pixels)
                    {
                        drawingContext.DrawEllipse(new System.Windows.Media.SolidColorBrush(pixel.PixelColor), null, centerPoint, pixelDiameter, pixelDiameter);
                        centerPoint.X += (widthDelta * 2);
                    }
                }

                centerPoint.Y += (heightDelta * 2);

                stripId++;
            }

            // Add in the id
            if (AssociatedData.Transforms > 0)
            {
                FormattedText formattedText = new FormattedText(
                        FrameId.ToString() + "." + Offset.ToString(),
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        16,
                        Brushes.Black);
                 drawingContext.DrawText(formattedText, new Point(15, 15));
            }
        }

        private void FrameBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Notify listeners that the frame was selected or at least I feel that is the case
            if (FrameSelected != null)
            {
                FrameSelected(FrameId);
            }
        }

        private void AnimationFrameTransform_Drop(object sender, DragEventArgs e)
        {
            // We should be receiving a transform to add to the timeline
            if (e.Data.GetDataPresent("Transform"))
            {
                string transformType = e.Data.GetData("Transform") as string;

                SharedInterfaces.TransformFactory factory = SharedInterfaces.TransformFactory.GetInstance();

                SharedInterfaces.ITransform transform = factory.GetTransform(transformType);

                if (transform != null)
                {
                    transform.SetIconSize(SharedInterfaces.TransformIconSize.eSmall);
                    AssociatedData.AddTransform(transform);
                    OnTransformAdded(transform);
                }
            }
        }

        private void OnTransformAdded(SharedInterfaces.ITransform transform)
        {
            if (TransformAdded != null)
            {
                TransformAdded(this, transform);
            }
        }
    }
}
