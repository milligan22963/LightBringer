using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace LightBringer.ViewModel
{
    public class StripViewModel : ViewModelBase
    {
        #region DATA
        static string m_StripClipboardName = "LBStrip";
        readonly ObservableCollection<PixelViewModel> m_children;        
        #endregion // DATA

        #region CONSTRUCTOR

        public StripViewModel(FrameViewModel parent, DataModel.Strip strip)
            : base(parent) // strips have frames as parents
        {
            m_children = new ObservableCollection<PixelViewModel>();

            AssociatedData = strip;

            foreach (DataModel.Pixel pixel in strip.Pixels)
            {
                Children.Add(new PixelViewModel(this, pixel));
                pixel.EnumerationId = Children.Count;
            }
        }
        
        #endregion // CONSTRUCTOR

        #region CLASS_SPECIFIC
        public void SetPixelCount(int pixels, Color startingColor)
        {
            // Make sure it is a new count
            if (pixels > m_children.Count)
            {
                for (int index = m_children.Count; index < pixels; index++)
                {
                    PixelViewModel newChild = AddChild() as PixelViewModel;
                    DataModel.Pixel pixel = newChild.AssociatedData as DataModel.Pixel;

                    pixel.PixelColor = startingColor;
                }
                IsDirty = true;
            }
            else if (pixels < m_children.Count)
            {
                for (int index = pixels; index > m_children.Count; index--)
                {
                    m_children.RemoveAt(index);
                }
                IsDirty = true;
            }
        }

        public int StripId
        {
            get
            {
                int stripId = 0;
                DataModel.Strip strip = AssociatedData as DataModel.Strip;

                if (strip != null)
                {
                    stripId = strip.StripId;
                }

                return stripId;
            }
            set
            {
                DataModel.Strip strip = AssociatedData as DataModel.Strip;

                if (strip != null)
                {
                    strip.StripId = value;
                    this.OnPropertyChanged("StripId");
                    IsDirty = true;
                }
            }
        }

        #endregion
        #region SEEK_AND_DESTROY

        override public ViewModelBase Find(int associatedId)
        {
            ViewModelBase associatedObject = base.Find(associatedId);

            if (associatedObject == null)
            {
                foreach (PixelViewModel pvm in m_children)
                {
                    associatedObject = pvm.Find(associatedId) as ViewModelBase;

                    if (associatedObject != null)
                    {
                        break; // found it
                    }
                }
            }

            return associatedObject;
        }

        #endregion // SEEK_AND_DESTROY

        #region CHILDREN
        public ObservableCollection<PixelViewModel> Children
        {
            get
            {
                return m_children;
            }
        }

        override public ViewModelBase AddChild()
        {
            DataModel.Pixel pixel = AssociatedData.AddChild() as DataModel.Pixel;
            PixelViewModel pixelView = new PixelViewModel(this, pixel);

            // Add in a child object
            Children.Add(pixelView);

            pixel.EnumerationId = Children.Count;

            return pixelView;
        }

        override public void RemoveChild(ViewModelBase child)
        {
            PixelViewModel realChild = child as PixelViewModel;

            if (child != null)
            {
                m_children.Remove(realChild);
            }
        }
        #endregion

        #region DATA_FIELDS

        public void SetColor(Color pixelColor, bool selectedOnly)
        {
            DataModel.Strip strip = AssociatedData as DataModel.Strip;
            if (strip != null)
            {
                strip.SetColor(pixelColor, selectedOnly);
            }
        }

        static public string IdentityName
        {
            get
            {
                return m_StripClipboardName;
            }
        }

        override public string InstanceIdentityName
        {
            get
            {
                return m_StripClipboardName;
            }
        }

        public override bool IsDirty
        {
            get
            {
                bool isDirty = base.IsDirty;

                // if this part isn't dirty, check the associated data
                if (isDirty == false)
                {
                    if (AssociatedData != null)
                    {
                        isDirty = AssociatedData.IsDirty;
                    }

                    // Check the kids
                    if (isDirty == false)
                    {
                        foreach (PixelViewModel pvm in Children)
                        {
                            if (pvm.IsDirty == true)
                            {
                                isDirty = true;
                                break;
                            }
                        }
                    }
                }
                return isDirty;
            }
            set
            {
                base.IsDirty = value;

                // Tell the kids about the clear
                if (value == false)
                {
                    if (AssociatedData != null)
                    {
                        AssociatedData.IsDirty = false;
                    }

                    foreach (PixelViewModel pvm in Children)
                    {
                        pvm.IsDirty = false;
                    }
                }
            }
        }

        public string StripName
        {
            get
            {
                return AssociatedData.DataName;
            }

            set
            {
                AssociatedData.SetName(value);
                IsDirty = true; // we have been soiled
            }
        }

        #endregion // DATA_FIELDS
    }
}
