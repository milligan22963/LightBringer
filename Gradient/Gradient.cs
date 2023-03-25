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
using System.IO;
using System.Xml;

namespace Gradient
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gradient"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gradient;assembly=Gradient"
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
    public class Gradient : Transform.Transform
    {
        private const string m_gradentAttribute = "Gradient";

        private Dictionary<int, List<int>> m_stripDeltaR;
        private Dictionary<int, List<int>> m_stripDeltaG;
        private Dictionary<int, List<int>> m_stripDeltaB;
        private Dictionary<int, List<int>> m_stripDeltaA;

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double),
            typeof(Gradient),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        bool m_includeAlpha = false;

        static Gradient()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Gradient), new FrameworkPropertyMetadata(typeof(Gradient)));
        }

        public Gradient()
        {
            m_stripDeltaR = new Dictionary<int, List<int>>();
            m_stripDeltaG = new Dictionary<int, List<int>>();
            m_stripDeltaB = new Dictionary<int, List<int>>();
            m_stripDeltaA = new Dictionary<int, List<int>>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ToolTipService.SetToolTip(this, m_gradentAttribute);

            SetName(m_gradentAttribute);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            LinearGradientBrush brush = new LinearGradientBrush(Colors.White, Colors.Black, Angle);
            Pen pen = new Pen(brush, 1.0);

            Image currentIcon = GetIcon();

            int offsetX = (int)Math.Floor(((this.ActualWidth - (double)currentIcon.Width) / 2.0));

            drawingContext.DrawRectangle(brush, pen, new Rect(offsetX, 2, currentIcon.Width, currentIcon.Height));
        }

        #region properties

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

        #endregion // properties

        #region ITransform

        public override SharedInterfaces.TransformCategory Category()
        {
            return SharedInterfaces.TransformCategory.eColor;
        }

        public override SharedInterfaces.TransformOrigin Origin()
        {
            return SharedInterfaces.TransformOrigin.eSystem;
        }

        public override void PreRenderFrames(int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            if (m_stripDeltaR.Keys.Contains<int>(stripId) == false)
            {
                m_stripDeltaR.Add(stripId, new List<int>());
                m_stripDeltaG.Add(stripId, new List<int>());
                m_stripDeltaB.Add(stripId, new List<int>());
                m_stripDeltaA.Add(stripId, new List<int>());
            }

            for (int index = 0; index < startingPixels.Count; index++)
            {
                m_stripDeltaR[stripId].Add(endingPixels[index].R - startingPixels[index].R);
                m_stripDeltaG[stripId].Add(endingPixels[index].G - startingPixels[index].G);
                m_stripDeltaB[stripId].Add(endingPixels[index].B - startingPixels[index].B);
                m_stripDeltaA[stripId].Add(endingPixels[index].A - startingPixels[index].A);
            }
        }

        // we return a new color set because, the old color set might be used with another transform which might change the actual pixels
        public override List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            double percentChange = (double)currentFrame / (double)totalFrames;
            List<Color> transformPixels = new List<Color>();

            List<int> deltaR = m_stripDeltaR[stripId];
            List<int> deltaG = m_stripDeltaG[stripId];
            List<int> deltaB = m_stripDeltaB[stripId];
            List<int> deltaA = m_stripDeltaA[stripId];

            for (int index = 0; index < startingPixels.Count; index++)
            {
                Color gradientColor = new Color();

                gradientColor.R = (byte)(startingPixels[index].R + (byte)((double)deltaR[index] * percentChange));
                gradientColor.G = (byte)(startingPixels[index].G + (byte)((double)deltaG[index] * percentChange));
                gradientColor.B = (byte)(startingPixels[index].B + (byte)((double)deltaB[index] * percentChange));
                if (m_includeAlpha == true)
                {
                    gradientColor.A = (byte)(startingPixels[index].A + (byte)((double)deltaA[index] * percentChange));
                }
                else
                {
                    gradientColor.A = startingPixels[index].A;
                }
                transformPixels.Add(gradientColor);
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
/*            ConfigSettings settings = new ConfigSettings();
            bool? dialogResults = false;

            settings.SelectedValue = DelayTime;

            dialogResults = settings.ShowDialog();

            if (dialogResults == true)
            {
                DelayTime = settings.SelectedValue;
            }*/
        }
        #endregion // ITransform

        #region PERSISTENCE

        #region IPersistence
        public override void Load(XmlReader reader)
        {
            StartLoad(reader);

//            DelayTime = Convert.ToDouble(reader.GetAttribute(m_delayAttributeName));

            FinishLoad(reader);
        }

        public override void Save(XmlWriter writer)
        {
            StartSave(writer);

            // add in our specific attribute(s)
//            writer.WriteAttributeString(m_delayAttributeName, DelayTime.ToString());

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
