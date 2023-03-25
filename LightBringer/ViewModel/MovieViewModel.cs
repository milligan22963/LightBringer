using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LightBringer.ViewModel
{
    public class MovieViewModel : ViewModelBase
    {
        #region DATA
        static string m_MovieClipboardName = "LBMovie";
        readonly ObservableCollection<FrameViewModel> m_children;
        #endregion // DATA

        #region CONSTRUCTOR

        public MovieViewModel(ControllerViewModel parent, DataModel.Movie movie)
            : base(parent) // movies have controllers as parents
        {
            m_children = new ObservableCollection<FrameViewModel>();

            AssociatedData = movie;

            foreach (DataModel.Frame frame in movie.Frames)
            {
                Children.Add(new FrameViewModel(this, frame));
            }
        }
        
        #endregion // CONSTRUCTOR

        #region SEEK_AND_DESTROY

        override public ViewModelBase Find(int associatedId)
        {
            ViewModelBase associatedObject = base.Find(associatedId);

            if (associatedObject == null)
            {
                foreach (FrameViewModel cvm in m_children)
                {
                    associatedObject = cvm.Find(associatedId) as ViewModelBase;

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
        public ObservableCollection<FrameViewModel> Children
        {
            get
            {
                return m_children;
            }
        }

        override public ViewModelBase AddChild()
        {
            DataModel.Frame frame = AssociatedData.AddChild() as DataModel.Frame;
            FrameViewModel FrameView = new FrameViewModel(this, frame);

            // Add in a child object
            Children.Add(FrameView);

            return FrameView;
        }

        public ViewModelBase AddChild(DataModel.Frame frame)
        {
            FrameViewModel FrameView = new FrameViewModel(this, frame);

            Children.Add(FrameView);

            return FrameView;
        }

        override public void RemoveChild(ViewModelBase child)
        {
            FrameViewModel realChild = child as FrameViewModel;

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
                return m_MovieClipboardName;
            }
        }

        override public string InstanceIdentityName
        {
            get
            {
                return m_MovieClipboardName;
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
                        foreach (FrameViewModel fvm in Children)
                        {
                            if (fvm.IsDirty == true)
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

                    foreach (FrameViewModel fvm in Children)
                    {
                        fvm.IsDirty = false;
                    }
                }
            }
        }

        public string MovieName
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
