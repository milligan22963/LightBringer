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

namespace Fade
{
    public enum FadeDirection
    {
        DarkToLight,
        LightToDark
    }

    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Fade"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Fade;assembly=Fade"
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
    public class Fade : Transform.Transform
    {
        private const string m_fadeAttributeName = "Fade";
        private const string m_fadeDirectionAttribute = "Direction";
        private const string m_fadePercentageAttribute = "Percentage";

        private FadeDirection m_fadeDirection;
        private double m_fadePercentage;

        static Fade()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Fade), new FrameworkPropertyMetadata(typeof(Fade)));
        }

        public Fade()
        {
            Direction = FadeDirection.LightToDark;
            Percentage = 50.0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // This only needs to be overridden if the current class needs to pull additional items out of the template i.e.
            // if there happens to be an icon set opposed to a single icon which needs to be named Icon.png by default

            ToolTipService.SetToolTip(this, m_fadeAttributeName);

            SetName(m_fadeAttributeName);
        }

        public FadeDirection Direction
        {
            get
            {
                return m_fadeDirection;
            }
            set
            {
                m_fadeDirection = value;
                OnPropertyChanged("Direction");
            }
        }

        public double Percentage
        {
            get
            {
                return m_fadePercentage;
            }
            set
            {
                m_fadePercentage = value;
                OnPropertyChanged("Percentage");
            }
        }

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
            // do nothing
        }

        // we return a new set of pixels due to the pixels being modified/changed by other renderers
        public override List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels)
        {
            double fadePercentage = (double)currentFrame / (double)totalFrames * (Percentage / 100.0);

            List<Color> transformPixels = new List<Color>();

            for (int colorIndex = 0; colorIndex < startingPixels.Count; colorIndex++)
            {
                int targetAlpha = 0;
                if (Direction == FadeDirection.DarkToLight)
                {
                    targetAlpha = (int)startingPixels[colorIndex].A - (int)Math.Floor((fadePercentage * startingPixels[colorIndex].A));
                }
                else
                {
                    targetAlpha = (int)Math.Floor((fadePercentage * startingPixels[colorIndex].A));
                }

                Color newColor = new Color();
                newColor.R = startingPixels[colorIndex].R;
                newColor.G = startingPixels[colorIndex].G;
                newColor.B = startingPixels[colorIndex].B;
                newColor.A = (byte)targetAlpha;

                transformPixels.Add(newColor);
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
            settings.Percentage = Percentage;

            dialogResults = settings.ShowDialog();

            if (dialogResults == true)
            {
                Percentage = settings.Percentage;
                Direction = settings.Direction;
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
                Direction = (FadeDirection)Enum.Parse(typeof(FadeDirection), reader.GetAttribute(m_fadeDirectionAttribute));
            }
            catch (ArgumentException)
            {
                Direction = FadeDirection.DarkToLight;
            }
            Percentage = Convert.ToDouble(reader.GetAttribute(m_fadePercentageAttribute));
            
            FinishLoad(reader);
        }

        public override void Save(XmlWriter writer)
        {
            StartSave(writer);

            // add in our specific attribute(s)
            writer.WriteAttributeString(m_fadeDirectionAttribute, Direction.ToString());
            writer.WriteAttributeString(m_fadePercentageAttribute, Percentage.ToString());

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
