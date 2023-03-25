using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LightBringer.ViewModel
{
    public class FrameViewModel : ViewModelBase
    {
        #region DATA
        static string m_FrameClipboardName = "LBFrame";
        readonly ObservableCollection<StripViewModel> m_children;                
        #endregion // DATA

        #region CONSTRUCTOR

        public FrameViewModel(MovieViewModel parent, DataModel.Frame Frame)
            : base(parent) // frames have shows as parents
        {
            m_children = new ObservableCollection<StripViewModel>();

            AssociatedData = Frame;

            foreach (DataModel.Strip strip in Frame.Strips)
            {
                Children.Add(new StripViewModel(this, strip));
            }
        }
        
        #endregion // CONSTRUCTOR

        #region SEEK_AND_DESTROY

        override public ViewModelBase Find(int associatedId)
        {
            ViewModelBase associatedObject = base.Find(associatedId);

            if (associatedObject == null)
            {
                foreach (StripViewModel svm in m_children)
                {
                    associatedObject = svm.Find(associatedId) as ViewModelBase;

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

        public ObservableCollection<StripViewModel> Children
        {
            get
            {
                return m_children;
            }
        }

        override public ViewModelBase AddChild()
        {
            DataModel.Strip strip = AssociatedData.AddChild() as DataModel.Strip;
            StripViewModel stripView = new StripViewModel(this, strip);

            // Add in a child object
            Children.Add(stripView);

            return stripView;
        }

        public ViewModelBase AddChild(DataModel.Strip strip)
        {
            StripViewModel stripView = new StripViewModel(this, strip);

            Children.Add(stripView);

            return stripView;
        }

        override public void RemoveChild(ViewModelBase child)
        {
            StripViewModel realChild = child as StripViewModel;

            if (child != null)
            {
                m_children.Remove(realChild);
            }
        }
        #endregion

        
        #region DATA_FIELDS

        static public string IdentityName
        {
            get
            {
                return m_FrameClipboardName;
            }
        }

        override public string InstanceIdentityName
        {
            get
            {
                return m_FrameClipboardName;
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
                        foreach (StripViewModel svm in Children)
                        {
                            if (svm.IsDirty == true)
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

                    foreach (StripViewModel svm in Children)
                    {
                        svm.IsDirty = false;
                    }
                }
            }
        }

        public string FrameName
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
