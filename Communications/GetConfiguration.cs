using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class GetConfiguration : Command
    {
        public GetConfiguration()
        {
            CommandType = Communications.CommandType.GetConfiguration;
        }

        public Int16 BoardId
        {
            get;
            set;
        }

        public override int Length()
        {
            int length = base.Length();

            length += sizeof(Int16); // BoardId

            return length;
        }

        public override void PrepareSend()
        {
            byte[] sendData = DataStream;

            int baseOffset = base.Length();

            base.PrepareSend();

            byte[] tempBytes = BitConverter.GetBytes(BoardId);
            tempBytes.CopyTo(sendData, baseOffset);
        }

        public override void ReceiveData(byte[] dataStream)
        {
            int baseOffset = base.Length();

            base.ReceiveData(dataStream);

            BoardId = Convert.ToInt16(dataStream[baseOffset]);
        }
    }
}
