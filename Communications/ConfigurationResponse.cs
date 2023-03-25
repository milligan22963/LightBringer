using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class ConfigurationResponse : Command
    {
        public ConfigurationResponse()
        {
            CommandType = Communications.CommandType.ConfigurationResponse;
        }

        public Int16 BoardId
        {
            get;
            set;
        }

        public BoardType BoardType
        {
            get;
            set;
        }

        public byte StripCount
        {
            get;
            set;
        }

        public byte[] StripLengths
        {
            get;
            set;
        }

        public StripType[] StripTypes
        {
            get;
            set;
        }

        public StripColor[] Colors
        {
            get;
            set;
        }

        public HardwarePin[] Pins
        {
            get;
            set;
        }

        public override int Length()
        {
            int length = base.Length();

            length += sizeof(Int16); // BoardId
            length += sizeof(byte); // will cast down to a byte for board type
            length += sizeof(byte); // strip count
            length += StripLengths.Length; // how many bytes of lengths, one for each strip
            length += StripTypes.Length; // each one will be cast to a byte
            length += Colors.Length;

            length += sizeof(byte); // one for the number of pins
            if ((Pins != null) && (Pins.Length > 0))
            {
                length += Pins.Length * Pins[0].Length;
            }
            return length;
        }

        public override void PrepareSend()
        {
            byte[] sendData = DataStream;

            int baseOffset = base.Length();

            base.PrepareSend();

            byte[] tempBytes = BitConverter.GetBytes(BoardId);
            tempBytes.CopyTo(sendData, baseOffset);

            baseOffset += sizeof(Int16);

            sendData[baseOffset] = (byte)BoardType;
            baseOffset++;

            sendData[baseOffset] = StripCount;

            for (int index = 0; index < StripCount; index++)
            {
                sendData[baseOffset] = StripLengths[index];
                baseOffset++;
            }

            for (int index = 0; index < StripCount; index++)
            {
                sendData[baseOffset] = (byte)StripTypes[index];
                baseOffset++;   
            }

            for (int index = 0; index < StripCount; index++)
            {
                sendData[baseOffset] = (byte)Colors[index];
                baseOffset++;
            }

            // dump out each pin
            if (Pins != null)
            {
                sendData[baseOffset++] = (byte)Pins.Length;
                foreach (HardwarePin pin in Pins)
                {
                    pin.PrepareForSending(sendData, baseOffset);
                    baseOffset += pin.Length;
                }
            }
            else
            {
                sendData[baseOffset++] = 0; // no pins
            }
        }

        public override void ReceiveData(byte[] dataStream)
        {
            int baseOffset = base.Length();

            base.ReceiveData(dataStream);

            BoardId = Convert.ToInt16(dataStream[baseOffset]);
            baseOffset += sizeof(Int16);

            BoardType = (BoardType)dataStream[baseOffset];
            baseOffset++;

            StripCount = dataStream[baseOffset];
            
            StripLengths = new byte[StripCount];
            baseOffset++;

            // load each of the lengths
            for (int index = 0; index < StripCount; index++)
            {
                StripLengths[index] = dataStream[baseOffset];
                baseOffset++;
            }

            StripTypes = new StripType[StripCount];

            // load each type
            for (int index = 0; index < StripCount; index++)
            {
                StripTypes[index] = (StripType)dataStream[baseOffset];
                baseOffset++;
            }

            Colors = new StripColor[StripCount];
            for (int index = 0; index < StripCount; index++)
            {
                Colors[index] = (StripColor)dataStream[baseOffset];
                baseOffset++;
            }

            byte pinCount = dataStream[baseOffset++];
            if (pinCount > 0)
            {
                Pins = new HardwarePin[pinCount];
                for (int index = 0; index < pinCount; index++)
                {
                    HardwarePin pin = new HardwarePin();

                    pin.ReceiveData(dataStream, baseOffset);
                
                    Pins[index] = pin;

                    baseOffset += Pins.Length;
                }
            }
        }
    }
}
