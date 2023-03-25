using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace LightBringer.Visuals
{
    /// <summary>
    /// Basic container for strips so we can manage them easier
    /// Allows me to also select all pixels within a frame container
    /// </summary>
    public class StripPanel : StackPanel
    {

        public StripPanel()
        {
        }

        public void Clear()
        {
            foreach (StripView strip in Children)
            {
                for (int index = 0; index < strip.Count; index++)
                {
                    strip[index].AssociatedView = null;
                }
                strip.AssociatedView = null; // this is going away
            }

            Children.Clear();
        }

        public void AddStrip(StripView strip)
        {
            Children.Add(strip);
        }

        public void ColorSelectedPixels(Color pixelColor)
        {
            foreach (StripView strip in Children)
            {
                strip.SetColor(pixelColor, true);
            }
        }

        public bool SelectPixels(Rect selectionArea, Vector parentOffset)
        {
            bool pixelsSelected = false;

            Vector panelOffset = VisualTreeHelper.GetOffset(this) + parentOffset;

            foreach (StripView strip in Children)
            {
                if (strip.SelectPixels(selectionArea, panelOffset) == true)
                {
                    pixelsSelected = true;
                }
            }
            return pixelsSelected;
        }

        public void DeSelectPixels()
        {
            foreach (StripView strip in Children)
            {
                strip.DeSelectPixels();   
            }
        }

        #region PROPERTIES

        public int StripCount
        {
            get
            {
                return Children.Count;
            }
        }

        public StripView this[int index]
        {
            get
            {
                StripView child = null;

                if (index < Children.Count)
                {
                    child = Children[index] as StripView;
                }

                return child;
            }

            set
            {
                if (index < Children.Count)
                {
                    Children[index] = value;
                }
            }
        }
        #endregion // PROPERTIES
    }
}
