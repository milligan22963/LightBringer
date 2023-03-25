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

namespace LightBringer.Toolbox
{
    public enum ToolType
    {
        SingleSelection,
        MultiSelection,
        EyeDropper,
        Cut,
        Copy,
        Other
    }

    public enum ToolAction
    {
        Select,
        Paint,
        Query,
        Edit
    }

    /// <summary>
    /// Interaction logic for ToolButton.xaml
    /// </summary>
    public partial class ToolButton : System.Windows.Controls.Primitives.ToggleButton
    {
        Uri m_imageUri = null;
        Uri m_cursorUri = null;

        public ToolButton()
        {
            InitializeComponent();
            Cursor = Cursors.Arrow; // default to arrow cursor
            WindowCursor = Cursor; // default to arrow, user can override to have tool specific cursor at need
            ToolType = ToolType.Other;
            Action = ToolAction.Edit;
        }

        public ToolType ToolType
        {
            get;
            set;
        }

        public ToolAction Action
        {
            get;
            set;
        }

        public Uri SourceURI
        {
            get
            {
                return m_imageUri;
            }
            set
            {
                m_imageUri = value;
                ToggleImage.Source = new BitmapImage(m_imageUri);
            }
        }

        public Uri CursorURI
        {
            get
            {
                return m_cursorUri;
            }
            set
            {
                m_cursorUri = value;
                System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(m_cursorUri);
                WindowCursor = new System.Windows.Input.Cursor(info.Stream);
            }
        }

        public Cursor WindowCursor
        {
            get;
            set;
        }
    }
}
