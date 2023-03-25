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
using SharedInterfaces;

namespace LightBringer.Swatches
{
    public delegate void SwatchEventHandler(object sender, SwatchEvent e);

    /// <summary>
    /// Interaction logic for Swatch.xaml
    /// </summary>
    public partial class Swatch : UserControl, IPersistence
    {
        public event SwatchEventHandler Selected = null;
        public event SwatchEventHandler Modified = null;
        public event SwatchEventHandler Deleted = null;

        private const string m_alphaAttribute = "Alpha";
        private const string m_redAttribute = "Red";
        private const string m_greenAttribute = "Green";
        private const string m_blueAttribute = "Blue";
        private Point m_startDragPoint = new Point();
        private bool m_dragStarted = false;

        private Xceed.Wpf.Toolkit.ColorCanvas m_swatchColorPicker = null;

        public Swatch()
        {
            InitializeComponent();

            m_swatchColorPicker = new Xceed.Wpf.Toolkit.ColorCanvas();
            m_swatchColorPicker.Background = App.Current.FindResource("DefaultBackgroundGradient") as Brush;
            m_swatchColorPicker.SelectedColor = Colors.Black;

            m_swatchColorPicker.SelectedColorChanged += colorPicker_SelectedColorChanged;

            AttachContextMenu();
        }

        void AttachContextMenu()
        {
            // Create our context menu
            SwatchCanvas.ContextMenu = new ContextMenu();

            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Modify";
            menuItem.Items.Add(m_swatchColorPicker);
            menuItem.Icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/LightBringer;component/Resources/Icons/document-edit.png"))
                , Width = 16
                ,Height = 16
            };

            SwatchCanvas.ContextMenu.Items.Add(menuItem); // add in modify swatch
            
            menuItem = new MenuItem();
            menuItem.Header = "Delete";
            menuItem.Command = new ThirdParty.Command.RelayCommand(DeleteSwatch);
            menuItem.Icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/LightBringer;component/Resources/Icons/document-delete.png"))
                , Width = 16
                , Height = 16
            };

            SwatchCanvas.ContextMenu.Items.Add(menuItem);
        }

        void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            CurrentColor = e.NewValue;
            OnModified(new SwatchEvent(CurrentColor));
        }

        public void Load(XmlReader reader)
        {
            Color storedColor = new Color();

            storedColor.A = Convert.ToByte(reader.GetAttribute(m_alphaAttribute));
            storedColor.R = Convert.ToByte(reader.GetAttribute(m_redAttribute));
            storedColor.G = Convert.ToByte(reader.GetAttribute(m_greenAttribute));
            storedColor.B = Convert.ToByte(reader.GetAttribute(m_blueAttribute));

            CurrentColor = storedColor;
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString(m_alphaAttribute, m_swatchColorPicker.SelectedColor.A.ToString());
            writer.WriteAttributeString(m_redAttribute, m_swatchColorPicker.SelectedColor.R.ToString());
            writer.WriteAttributeString(m_greenAttribute, m_swatchColorPicker.SelectedColor.G.ToString());
            writer.WriteAttributeString(m_blueAttribute, m_swatchColorPicker.SelectedColor.B.ToString());
        }

        public void Load(BinaryReader reader)
        {
            // Unused for now
        }

        public void Save(BinaryWriter writer)
        {
            // unused for now
        }

        // Invoke the Selected event; called whenever this swatch is clicked on
        protected virtual void OnSelected(SwatchEvent e)
        {
            if (Selected != null)
            {
                Selected(this, e);
            }
        }

        protected virtual void OnModified(SwatchEvent e)
        {
            if (Modified != null)
            {
                Modified(this, e);
            }
        }

        protected virtual void OnDeleted(SwatchEvent e)
        {
            if (Deleted != null)
            {
                Deleted(this, e);
            }
        }

        public Color CurrentColor
        {
            get
            {
                return m_swatchColorPicker.SelectedColor;
            }

            set
            {
                m_swatchColorPicker.SelectedColor = value;
                SwatchCanvas.Background = new SolidColorBrush(value);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // User selected this swatch
            // Post a notification to indicate the color selection
            OnSelected(new SwatchEvent(CurrentColor));
            m_dragStarted = false;
        }

        public void DeleteSwatch()
        {
            OnDeleted(new SwatchEvent(CurrentColor));
        }

        private void SwatchCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_dragStarted = true;
            m_startDragPoint = e.GetPosition(null);
        }

        private void SwatchCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
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
                    DataObject dragData = new DataObject("SwatchColor", CurrentColor.ToString());
                    DragDrop.DoDragDrop(sender as Canvas, dragData, DragDropEffects.Move);
                    m_dragStarted = false;
                }
            }
        }
    }
}
