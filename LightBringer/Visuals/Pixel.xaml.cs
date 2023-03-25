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
using System.Windows.Media.Media3D;

namespace LightBringer.Visuals
{
    public delegate void PixelEventHandler(object sender, PixelEventArgs e);

    /// <summary>
    /// Interaction logic for Pixell.xaml
    /// </summary>
    public partial class Pixel : UserControl
    {
        public event PixelEventHandler Selected = null;

        private Color m_CurrentColor;
        private HelixToolkit.Wpf.SphereVisual3D m_pixelSphere;
        private double m_defaultWidth;
        private double m_defaultHeight;
        bool m_isSelected = false;
        bool m_selectionStarted = false;

        public Pixel()
        {
            InitializeComponent();

            AssociatedView = null;
            Color = Colors.Black;

            Border pixelBorder = PixelCanvas.Parent as Border;

            m_defaultHeight = pixelBorder.Height;
            m_defaultWidth = pixelBorder.Width;
            MaximumZoom = 500; // default max in %
        }

        public Color Color
        {
            get
            {
                return m_CurrentColor;
            }
            set
            {
                m_CurrentColor = value;
                PixelCanvas.Fill = new SolidColorBrush(m_CurrentColor);

                if (m_pixelSphere != null)
                {
                    m_pixelSphere.Fill = PixelCanvas.Fill;
                }

                if (AssociatedView != null)
                {
                    AssociatedView.SetColor(value);
                }
            }
        }

        public ViewModel.PixelViewModel AssociatedView
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public double MaximumZoom
        {
            get;
            set;
        }

        public double ZoomIn()
        {
            Border pixelBorder = PixelCanvas.Parent as Border;
            double newScale = ((pixelBorder.Height * 2) / m_defaultHeight) * 100.0;
            double scaleAmount = Math.Min(newScale, MaximumZoom) / 100.0;

            if (pixelBorder.Height != 0)
            {
                pixelBorder.Height = m_defaultHeight * scaleAmount;
                pixelBorder.Width = m_defaultWidth * scaleAmount;
            }
            else
            {
                pixelBorder.Height = 1;
                pixelBorder.Width = 1;
            }
            return (pixelBorder.Height /  m_defaultHeight) * 100.0;
        }

        public double ZoomOut()
        {
            Border pixelBorder = PixelCanvas.Parent as Border;
            double newScale = ((pixelBorder.Height / 2) / m_defaultHeight) * 100.0;
            double scaleAmount = Math.Min(newScale, MaximumZoom) / 100.0;

            pixelBorder.Height = m_defaultHeight * scaleAmount;
            pixelBorder.Width = m_defaultWidth * scaleAmount;

            return (pixelBorder.Height / m_defaultHeight) * 100.0;
        }

        public void Zoom(double amount)
        {
            Border pixelBorder = PixelCanvas.Parent as Border;

            double scaledAmount = Math.Min(MaximumZoom, amount) / 100.00;

            pixelBorder.Height = m_defaultHeight * scaledAmount;
            pixelBorder.Width = m_defaultWidth * scaledAmount;
        }

        private void PixelCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_selectionStarted == true)
            {
                OnSelected(new PixelEventArgs(this));
                m_selectionStarted = false;
            }
        }

        private void PixelCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_selectionStarted = true;
        }

        public bool SelectionTest(Rect selectionArea, Vector parentOffset)
        {
            bool isSelected = false;

            // Get our current location
            Vector pixelOffset = VisualTreeHelper.GetOffset(this) + parentOffset;

            if (pixelOffset.X >= selectionArea.Left && pixelOffset.X <= selectionArea.Right)
            {
                if (pixelOffset.Y >= selectionArea.Top && pixelOffset.Y <= selectionArea.Bottom)
                {
                    IsSelected = true;

                    AssociatedView.IsSelected = true;
                }
            }
            return isSelected;
        }

        protected virtual void OnSelected(PixelEventArgs e)
        {
            if (Selected != null)
            {
                Selected(this, e);
            }
        }

        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                Border pixelBorder = PixelCanvas.Parent as Border;
                pixelBorder.BorderThickness = new Thickness(1.0);

                if (value == true)
                {
                    pixelBorder.BorderBrush = Brushes.White;
                }
                else
                {
                    pixelBorder.BorderBrush = Brushes.Transparent;
                }

                m_isSelected = value;
                AssociatedView.IsSelected = m_isSelected;
            }
        }

        #region THREE_D_MODEL
        public ModelVisual3D Generate3DView(double radius)
        {
            m_pixelSphere = new HelixToolkit.Wpf.SphereVisual3D();

            m_pixelSphere.Radius = radius;
            if (AssociatedView != null)
            {
                m_pixelSphere.Fill = AssociatedView.CurrentColorAsBrush;
            }
            else
            {
                m_pixelSphere.Fill = Brushes.Black;
            }

            return m_pixelSphere;
        }
        #endregion // THREE_D_MODEL
    }
}
