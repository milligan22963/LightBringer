using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LightBringer.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Data

        readonly ViewModelBase m_parent;

        bool m_isExpanded;
        bool m_isSelected;
        bool m_isDirty;

        #endregion // Data

        #region Constructors
        public ViewModelBase(ViewModelBase parent)
        {
            m_parent = parent;

            AssociatedData = null;
        }

        // Force to use other constructor
        private ViewModelBase()
        {
        }

        #endregion // constructors

        #region SEEK_AND_DESTROY
        
        virtual public ViewModelBase Find(int associatedId)
        {
            ViewModelBase associatedObject = null;

            if (AssociatedData.Id == associatedId)
            {
                associatedObject = this;
            }
            return associatedObject;
        }

        #endregion // SEEK_AND_DESTROY

        #region Presentation Members

        #region Children

        virtual public ViewModelBase AddChild()
        {
            // Add in a child object
            return null;
        }

        virtual public void RemoveChild(ViewModelBase child)
        {
            // We've got nothing
        }

        #endregion // Children

        public DataModel.DataModelBase AssociatedData
        {
            get;
            set;
        }

        virtual public string InstanceIdentityName
        {
            get
            {
                return null;
            }
        }

       #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get 
            {
                return m_isExpanded; 
            }

            set
            {
                if (value != m_isExpanded)
                {
                    m_isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if ((m_isExpanded == true) && (m_parent != null))
                {
                    m_parent.IsExpanded = true;
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return m_isSelected; 
            }

            set
            {
                if (value != m_isSelected)
                {
                    m_isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region IsDirty
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is dirty i.e. has changed.
        /// </summary>
        public virtual bool IsDirty
        {
            get
            {
                return m_isDirty;
            }

            set
            {
                if (value != IsDirty)
                {
                    m_isDirty = value;
                    this.OnPropertyChanged("IsDirty");
                }

                if (AssociatedData != null)
                {
                    AssociatedData.IsDirty = value; // update our associated data
                }

                // Expand all the way up to the root so we will know if any of the children
                // are dirty and in need of a save
                if ((m_isDirty == true) && (m_parent != null))
                {
                    m_parent.IsDirty = true;
                }
            }
        }
        #endregion // IsDirty

        #region Parent

        public ViewModelBase Parent
        {
            get 
            {
                return m_parent; 
            }
        }

        #endregion // Parent

        #endregion // Presentation Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged Members
    }
}
