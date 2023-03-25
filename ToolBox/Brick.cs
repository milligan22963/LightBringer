using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace ToolBox
{
    public class Brick : ModelVisual3D, INotifyPropertyChanged
    {
        private const int g_BrickSides = 6;

        private const int g_Top = 0;
        private const int g_Bottom = 1;
        private const int g_Front = 2;
        private const int g_Back = 3;
        private const int g_Left = 4;
        private const int g_Right = 5;

        private HelixToolkit.Wpf.RectangleVisual3D[] m_brick;
        private double m_width = 0.0;
        private double m_length = 0.0;
        private double m_depth = 0.0;
        private Brush m_fillBrush = new SolidColorBrush(Colors.White);

        public event PropertyChangedEventHandler PropertyChanged;

        public Brick()
        {
            m_brick = new HelixToolkit.Wpf.RectangleVisual3D[g_BrickSides];
            for (int index = 0; index < g_BrickSides; index++)
            {
                m_brick[index] = new HelixToolkit.Wpf.RectangleVisual3D();
                m_brick[index].Fill = m_fillBrush;
                m_brick[index].Origin = new Point3D(0, 
                    1.0 * index, 0);
            }
            Width = 10.0;
            Length = 10.0;
            Depth = 10.0;
        }

        /*
            <helix:RectangleVisual3D Fill="#FFCCC5C5" Length="30" Origin="0,0,0"></helix:RectangleVisual3D>
            <helix:RectangleVisual3D Fill="#FFA63EC7" Length="10" Width="1" Origin="15,0,-0.5" Normal="1,1,1"></helix:RectangleVisual3D>
            <helix:RectangleVisual3D Fill="#FF3EC743" Length="30" Width="1" Origin="0,5,-0.5" Normal="0,1,0"></helix:RectangleVisual3D>
            <helix:RectangleVisual3D Fill="#FF3EC743" Length="30" Width="1" Origin="0,-5,-0.5" Normal="0,1,0"></helix:RectangleVisual3D>
            <helix:RectangleVisual3D Fill="#FFCCC5C5" Length="30" Origin="0,0,-1"></helix:RectangleVisual3D>
         */
        #region PROPERTIES
        public double Width
        {
            get
            {
                return m_width;
            }
            set
            {
                m_width = value;

                // adjust rectangles
                m_brick[g_Top].Width = m_width;
                m_brick[g_Bottom].Width = m_width;
                m_brick[g_Front].Width = m_width;
                m_brick[g_Back].Width = m_width;
                OnPropertyChanged("Width");
            }
        }

        public double Length
        {
            get
            {
                return m_length;
            }
            set
            {
                m_length = value;

                // Adjust rectangles
                m_brick[g_Front].Length = m_length;
                m_brick[g_Back].Length = m_length;
                m_brick[g_Left].Length = m_length;
                m_brick[g_Right].Length = m_length;
                OnPropertyChanged("Length");
            }
        }

        public double Depth
        {
            get
            {
                return m_depth;
            }
            set
            {
                m_depth = value;

                // Adjust rectanlges
                m_brick[g_Top].Length = m_depth;
                m_brick[g_Bottom].Length = m_depth;
                m_brick[g_Left].Width = m_depth;
                m_brick[g_Right].Width = m_depth;
                OnPropertyChanged("Depth");
            }
        }

        public Brush Fill
        {
            get
            {
                return m_fillBrush;
            }
            set
            {
                m_fillBrush = value;
                m_brick[g_Top].Fill = m_fillBrush;
                m_brick[g_Bottom].Fill = m_fillBrush;
                m_brick[g_Left].Fill = m_fillBrush;
                m_brick[g_Right].Fill = m_fillBrush;
                m_brick[g_Front].Fill = m_fillBrush;
                m_brick[g_Back].Fill = m_fillBrush;

                OnPropertyChanged("Fill");  
            }
        }
        #endregion // PROPERTIES

        #region IPROPERTYCHANGE
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion // IPROPERTYCHANGE
    }
}
