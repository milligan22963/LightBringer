using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public enum CommandType
    {
        Presence,
        PresenceResponse,
        GetConfiguration,
        ConfigurationResponse,
        SetConfiguration,
        SendMovie,
        SendAsset,
        SendFrame,
        Show
    }

    // type of daughter board
    public enum BoardType
    {
        LedPhoenixFourChannel,
        UserSpecified
    }

    public enum StripType
    {
        LPD8806,        // strips based on the LPD8806
        WS2812,         // strips based on the ws2812 aka neo-pixels
        SolidColor,    // single color strip
        TriColor       // RGB strip non-controllable i.e. no single pixel color
    }

    public enum StripColor
    {
        All,
        White,
        Red,
        Green,
        Blue,
        Yellow
    }

    public class Command
        {
        static Int16 m_nextCommandId = 1;

        byte[] m_dataStream = null;

        Int16 m_id; // the id of this command so it can be acknowledged

        public Command()
        {
            m_id = m_nextCommandId++;
        }

        public Int16 Id
        {
            get
            {
                return m_id;
            }
            private set
            {
                m_id = value;
            }
        }

        public CommandType CommandType
        {
            get;
            protected set;
        }

        public byte[] DataStream
        {
            get
            {
                if (m_dataStream == null)
                {
                    m_dataStream = new byte[Length()];
                }

                return m_dataStream;
            }
        }

        public virtual int Length()
        {
            int length = 0;

            length += sizeof(Int16); // id
            length += sizeof(byte); // command type

            return length;
        }

        public virtual void PrepareSend()
        {
            byte[] dataStream = DataStream;

            byte[] tempBytes = BitConverter.GetBytes(m_id);
            tempBytes.CopyTo(dataStream, 0);

            dataStream[sizeof(Int16)] = (byte)CommandType;
        }

        public virtual void ReceiveData(byte[] data)
        {
            m_id = Convert.ToInt16(data);
            CommandType = (Communications.CommandType)data[sizeof(Int16)];
        }
    }
}
