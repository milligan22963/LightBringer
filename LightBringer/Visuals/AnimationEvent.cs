using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBringer.Visuals
{
    public class AnimationEvent : EventArgs
    {
        public AnimationEvent()
        {
            FramePercentage = 100.0;
        }

        public AnimationEvent(int frameId)
        {
            FrameId = frameId;
            FramePercentage = 100.0; // the percentage of the frame that has been processed i.e. if being transformed then this will be from 0 to 100
        }

        public int FrameId
        {
            get;
            set;
        }

        public double FramePercentage
        {
            get;
            set;
        }
    }
}
