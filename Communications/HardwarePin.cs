using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    public enum PinType
    {
        Data = 0x01,
        Clock = 0x02,
        Select = 0x04,
        Trigger = 0x08,
        All = 0xff
    }

    public class HardwarePin
    {
        public HardwarePin()
        {
        }

        public void PrepareForSending(byte[] dataStream, int offset)
        {
            dataStream[offset++] = (byte)Id;
            dataStream[offset] = (byte)PinType;
        }

        public void ReceiveData(byte[] dataStream, int offset)
        {
            Id = dataStream[offset++];
            PinType = (PinType)dataStream[offset];
        }

        #region PROPERTIES
        public int Length
        {
            get
            {
                int length = 0;

                length++; // id
                length++; // type

                return length;
            }
        }

        public byte Id
        {
            get;
            set;
        }

        public PinType PinType
        {
            get;
            set;
        }
        #endregion
    }
}
