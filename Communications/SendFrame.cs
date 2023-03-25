using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class SendFrame : Command
    {
        public SendFrame()
        {
            CommandType = Communications.CommandType.SendFrame;
        }

        public override int Length()
        {
            int commandLength = base.Length();

            commandLength += sizeof(byte); // id of strip

            if (Frame == null)
            {
                Frame = new FrameObject();
            }

            commandLength += Frame.Length();

            return commandLength;
        }

        public override void PrepareSend()
        {
            byte[] dataStream = DataStream;

            int offset = base.Length();

            base.PrepareSend();

            dataStream[offset++] = (byte)Id;
        }

        public override void ReceiveData(byte[] dataStream)
        {
            int offset = base.Length();

            base.ReceiveData(dataStream);

            Identity = dataStream[offset++];

            if (Frame == null)
            {
                Frame = new FrameObject();
            }

            Frame.ReceiveData(dataStream, offset);
        }

        /// <summary>
        /// Gets or sets the id for the strip which will be 0 based
        /// </summary>
        public byte Identity
        {
            get;
            set;
        }

        public FrameObject Frame
        {
            get;
            set;
        }
    }
}
