using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LightBringer.ViewModel
{
    public class PixelViewModel : ViewModelBase
    {
        #region DATA
        
        #endregion // DATA

        #region CONSTRUCTOR

        public PixelViewModel(StripViewModel parent, DataModel.Pixel pixel)
            : base(parent) // pixels have strips as parents
        {
            AssociatedData = pixel;

            // Alas our children are bare
        }
        
        #endregion // CONSTRUCTOR

        #region DATA_FIELDS

        public void SetColor(Color newColor)
        {
            DataModel.Pixel pixel = AssociatedData as DataModel.Pixel;

            if (pixel != null)
            {
                pixel.PixelColor = newColor;
                OnPropertyChanged("CurrentColorAsBrush");
                IsDirty = true;
            }
        }

        public SolidColorBrush CurrentColorAsBrush
        {
            get
            {
                return new SolidColorBrush(CurrentColor);
            }
        }

        public Color CurrentColor
        {
            get
            {
                Color returnValue = new Color();

                DataModel.Pixel pixel = AssociatedData as DataModel.Pixel;

                if (pixel != null)
                {
                    returnValue = pixel.PixelColor;
                }

                return returnValue;
            }
        }

        public string PixelName
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

        public int EnumerationId
        {
            get
            {
                DataModel.Pixel pixel = AssociatedData as DataModel.Pixel;

                return pixel.EnumerationId;
            }
            set
            {
                DataModel.Pixel pixel = AssociatedData as DataModel.Pixel;

                pixel.EnumerationId = value;
            }
        }
        #endregion //DATA_FIELDS
    }
}
