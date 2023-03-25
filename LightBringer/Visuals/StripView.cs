using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows;

namespace LightBringer.Visuals
{
    public class StripView : Border // needs to be a border
    {
        private MouseWheelEventHandler ZoomEventHandler = null;
        private PixelEventHandler PixelEvents = null;
        private StackPanel m_parentPanel;
        private StackPanel m_pixelPanel;
        private bool m_isSelected = false;

        // Add in code to zoom the children of the pixel containers
//        PixelStack_MouseWheel

        public StripView()
        {
            Initialize();
        }

        public StripView(int numPixels)
        {
            Initialize();

            // Add a number of pixels based on strip size
            for (int pixelId = 0; pixelId < numPixels; pixelId++)
            {
                Pixel pixel = new Pixel();

                pixel.Selected += Handler;
                pixel.Color = Colors.Black;

                Add(pixel);
            }
        }

        private void Initialize()
        {
            m_parentPanel = new StackPanel();
            m_pixelPanel = new StackPanel();

            AllowDrop = true;
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Child = m_parentPanel; // the parent panel is the child of the border
            m_parentPanel.Children.Add(m_pixelPanel); // the parent panel will contain the pixel panel and any decorations for it
            m_parentPanel.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            m_parentPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            m_pixelPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            // Will probably want to configure this via the app
            BorderThickness = new System.Windows.Thickness(2);
            IsSelected = false;

            Margin = new System.Windows.Thickness(5);
        }

        public bool SelectPixels(Rect selectionArea, Vector parentOffset)
        {
            bool selectedPixels = false;

            Vector stripOffset = VisualTreeHelper.GetOffset(this);

            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                if (pixel.SelectionTest(selectionArea, stripOffset) == true)
                {
                    pixel.IsSelected = true;
                    selectedPixels = true;
                }
            }

            return selectedPixels;
        }

        public void DeSelectPixels()
        {
            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                pixel.IsSelected = false;
            }
        }

        public int Id
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                m_isSelected = value;

                SolidColorBrush colorBrush = new SolidColorBrush();
//                if (m_isSelected == true)
//                {
                    colorBrush.Color = Colors.Black;
//                }
//                else
//                {
//                    colorBrush.Color = Colors.AntiqueWhite;
//                }
                BorderBrush = colorBrush;
            }
        }

        public ViewModel.StripViewModel AssociatedView
        {
            get;
            set;
        }

        public PixelEventHandler Handler
        {
            private get
            {
                return PixelEvents;
            }
            set
            {
                if (PixelEvents != null)
                {
                    foreach (Pixel pixel in m_pixelPanel.Children)
                    {
                        pixel.Selected -= PixelEvents;
                    }
                }

                foreach (Pixel pixel in m_pixelPanel.Children)
                {
                    pixel.Selected += value;
                }
                PixelEvents = value;
            }
        }

        public MouseWheelEventHandler ZoomHandler
        {
            private get
            {
                return ZoomEventHandler;
            }
            set
            {
                ZoomEventHandler = value;
                m_parentPanel.MouseWheel += value;
                m_pixelPanel.MouseWheel += value;
            }
        }

        // For setting all pixels to the same color
        public void SetColor(Color newColor, bool selectedOnly)
        {
            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                if (selectedOnly == false || pixel.IsSelected == true)
                {
                    pixel.Color = newColor;
                }
            }

            AssociatedView.SetColor(newColor, selectedOnly);
        }

        public int Count
        {
            get
            {
                return m_pixelPanel.Children.Count;
            }
        }

        public Pixel this[int index]
        {
            get
            {
                Pixel pixel = null;

                if (index < m_pixelPanel.Children.Count)
                {
                    pixel = m_pixelPanel.Children[index] as Pixel;
                }

                return pixel;
            }
        }

        public double ZoomIn()
        {
            double zoomAmount = 0.0;

            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                zoomAmount = pixel.ZoomIn();
            }
            return zoomAmount;
        }

        public double ZoomOut()
        {
            double zoomAmount = 0.0;

            // Assume each pixel scales the same
            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                zoomAmount = pixel.ZoomOut();
            }

            return zoomAmount;
        }

        public void Zoom(double amount)
        {
            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                pixel.Zoom(amount);
            }
        }

        public void Reset(int numPixels)
        {
            m_pixelPanel.Children.Clear();

            for (int count = 0; count < numPixels; count++)
            {
                Pixel pixel = new Pixel();

                pixel.Selected += Handler;
                pixel.Color = Colors.Black;

                Add(pixel);
            }
        }

        public void Add(Pixel pixel)
        {
            pixel.Selected += Handler;
            m_pixelPanel.Children.Add(pixel);
            pixel.Id = m_pixelPanel.Children.Count;
        }

        #region THREE_D_MODEL
        public ModelVisual3D Generate3DView(double offset)
        {
            HelixToolkit.Wpf.BoxVisual3D strip = new HelixToolkit.Wpf.BoxVisual3D();
            double y = offset;
            double z = 0.0;
            double spacing = 0.5;

            strip.Fill = Brushes.AliceBlue;
            strip.Length = (m_pixelPanel.Children.Count + 1) * spacing;
            strip.Width = spacing * 1.5;
            strip.Height = spacing * 1.5;
            strip.Center = new Point3D(0, offset, -spacing / 1.5);
            double x = 0.0 -((m_pixelPanel.Children.Count - 1) * (spacing / 2));

            // Each pixel will need to draw itself
            // Each strip will need to place the pixel on the path
            foreach (Pixel pixel in m_pixelPanel.Children)
            {
                HelixToolkit.Wpf.SphereVisual3D pixelModel = pixel.Generate3DView(spacing / 2) as HelixToolkit.Wpf.SphereVisual3D;

                pixelModel.Center = new Point3D(x, y, z);

                x += spacing;
                strip.Children.Add(pixelModel);
            }

            return strip;
        }
        #endregion // THREE_D_MODEL
    }
}
