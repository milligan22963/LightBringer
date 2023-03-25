using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SharedInterfaces
{
    public enum TransformCategory
    {
        eIgnore,
        eTime,
        eColor,
        eMovement,
        eMusic
    }

    public enum TransformOrigin
    {
        eSystem,
        eUser
    }

    public enum TransformIconSize
    {
        eSmall,
        eMedium,
        eLarge,
        eXLarge
    }

    public interface ITransform : IPersistence
    {
        TransformCategory Category();
        TransformOrigin Origin();

        /*
         * LinkTransform
         * 
         * Used to link a transform with another one.  Used in particular with
         * a time based transform associated with a non-time based transform.
         * 
         * params:
         *      linkedTransform - the transform to link to
         *      
         * returns:
         *      none
         */
        void LinkTransform(ITransform linkedTransform);

        /*
         * RenderFrames
         * 
         * Used to render a number of frames using the given transform
         * 
         * params:
         * 
         * returns:
         *      a list of colors for each passed in pixels
         * 
         */
        List<Color> RenderFrames(int currentFrame, int totalFrames, int stripId, List<Color> startingPixels, List<Color> endingPixels);

        /*
         * ComputeFrameCount
         *
         */
        int ComputeFrameCount(double frameRate);

        /*
         *  Sets the icon size
         *  
         * where small is 16x16, medium 32x32, large 64x64, and xlarge 128x128
         */
        void SetIconSize(TransformIconSize size);

        /// <summary>
        /// Used to configure this transform
        /// </summary>
        void ConfigureTransform();
    }
}
