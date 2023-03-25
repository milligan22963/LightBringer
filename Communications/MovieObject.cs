using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    // if a movie ends it will go back to the default,
    // we can trigger movies based on events.
    public enum ObjectType
    {
        Default,
        Key,
        Event
    }

    public class MovieObject
    {
        public MovieObject()
        {
        }

        public int Length()
        {
            int commandLength = 0;

            commandLength += sizeof(byte); // I will convert the Type to a byte
            if (Type == ObjectType.Event)
            {
                commandLength += Pin.Length;
            }
            commandLength += sizeof(Int16); // number of frames per movie

            // if there is at least one frame then they should all have the same length
            if (Frames != null)
            {
                commandLength += (Frames.Length * Frames[0].Length());  // all frames should be the same length
            }

            return commandLength;
        }

        public void PrepareToSend(byte[] dataStream, int offset)
        {
            dataStream[offset++] = (byte)Type;

            if (Type == ObjectType.Event)
            {
                Pin.PrepareForSending(dataStream, offset);
                offset += Pin.Length;
            }
            if (Frames != null)
            {
                byte[] tempBytes = BitConverter.GetBytes((Int16)Frames.Length);
                tempBytes.CopyTo(dataStream, offset);
                offset += tempBytes.Length;

                foreach (FrameObject frame in Frames)
                {
                    frame.PrepareToSend(dataStream, offset);
                    offset += frame.Length();
                }
            }
            else
            {
                byte[] tempBytes = BitConverter.GetBytes((Int16)0);
                tempBytes.CopyTo(dataStream, offset);
            }
        }

        public void ReceiveData(byte[] dataStream, int offset)
        {
            Type = (ObjectType)dataStream[offset++];
            if (Type == ObjectType.Event)
            {
                Pin = new HardwarePin();

                Pin.ReceiveData(dataStream, offset);
                offset += Pin.Length;
            }
            Int16 frameCount = Convert.ToInt16(dataStream[offset]);
            offset += sizeof(Int16);

            if (frameCount > 0)
            {
                Frames = new FrameObject[frameCount];

                for (int index = 0; index < frameCount; index++)
                {
                    Frames[index] = new FrameObject();
                    Frames[index].ReceiveData(dataStream, offset);
                    offset += Frames[index].Length();
                }
            }
            else
            {
                Frames = null;
            }
        }

        #region PROPERTIES
        public FrameObject[] Frames
        {
            get;
            set;
        }

        public ObjectType Type
        {
            get;
            set;
        }

        public HardwarePin Pin
        {
            get;
            set;
        }
        #endregion
    }
}
