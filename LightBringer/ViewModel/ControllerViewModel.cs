using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LightBringer.ViewModel
{
    public class ControllerViewModel : ViewModelBase
    {
        #region DATA
        readonly ObservableCollection<MovieViewModel> m_children;
        #endregion // DATA

        #region CONSTRUCTOR

        public ControllerViewModel(DataModel.Controller controller)
            : base(null) // controllers are the top item in the tree
        {
            m_children = new ObservableCollection<MovieViewModel>();

            AssociatedData = controller;

            foreach (DataModel.Movie movie in controller.Movies)
            {
                Children.Add(new MovieViewModel(this, movie));
            }
        }
        
        #endregion // CONSTRUCTOR

        #region SEEK_AND_DESTROY

        override public ViewModelBase Find(int associatedId)
        {
            ViewModelBase associatedObject = base.Find(associatedId);

            if (associatedObject == null)
            {
                foreach (MovieViewModel mvm in m_children)
                {
                    associatedObject = mvm.Find(associatedId) as ViewModelBase;

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
        override public ViewModelBase AddChild()
        {
            DataModel.Movie movie = AssociatedData.AddChild() as DataModel.Movie;
            MovieViewModel movieView = new MovieViewModel(this, movie);

            // Add in a child object
            Children.Add(movieView);

            return movieView;
        }

        public ViewModelBase AddChild(DataModel.Movie movie)
        {
            MovieViewModel movieView = new MovieViewModel(this, movie);

            Children.Add(movieView);

            return movieView;
        }

        override public void RemoveChild(ViewModelBase child)
        {
            MovieViewModel realChild = child as MovieViewModel;

            if (child != null)
            {
                m_children.Remove(realChild);
            }
        }

        public void InsertChild(MovieViewModel mvm)
        {
            Children.Add(mvm); // add in this one based on a cut and paste etc.
        }

        public ObservableCollection<MovieViewModel> Children
        {
            get
            {
                return m_children;
            }
        }
        #endregion

        #region DATA_FIELDS

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
                        foreach (MovieViewModel mvm in Children)
                        {
                            if (mvm.IsDirty == true)
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

                    foreach (MovieViewModel mvm in Children)
                    {
                        mvm.IsDirty = false;
                    }
                }
            }
        }

        public string ControllerTypeName
        {
            get
            {
                DataModel.Controller controller = AssociatedData as DataModel.Controller;
                return controller.AssociatedControllerType.ControllerName;
            }
        }

        public string ControllerName
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
