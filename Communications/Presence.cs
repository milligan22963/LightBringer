using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class Presence : Command
    {
        public Presence()
        {
            CommandType = Communications.CommandType.Presence;
        }

        public Int16 BoardId
        {
            get;
            set;
        }

        public Int16 Version
        {
            get;
            set;
        }

        /// <summary>
        /// ClientType - may be 0 if being requested by the server, if client announcing then should be set
        /// </summary>
        public Int16 ClientType
        {
            get;
            set;
        }

        public override int Length()
        {
            int length = base.Length();

            length += sizeof(Int16); // BoardId
            length += sizeof(Int16); // Version
            length += sizeof(Int16); // client type

            return length;
        }

        public override void PrepareSend()
        {
            byte[] sendData = DataStream;

            int baseOffset = base.Length();

            base.PrepareSend();

            byte[] tempBytes = BitConverter.GetBytes(BoardId);
            tempBytes.CopyTo(sendData, baseOffset);

            tempBytes = BitConverter.GetBytes(Version);
            tempBytes.CopyTo(sendData, baseOffset + sizeof(Int16));

            tempBytes = BitConverter.GetBytes(ClientType);
            tempBytes.CopyTo(sendData, baseOffset + (sizeof(Int16) * 2));
        }

        public override void ReceiveData(byte[] dataStream)
        {
            int baseOffset = base.Length();

            base.ReceiveData(dataStream);

            BoardId = Convert.ToInt16(dataStream[baseOffset]);
            Version = Convert.ToInt16(dataStream[baseOffset + sizeof(Int16)]);
            ClientType = Convert.ToInt16(dataStream[baseOffset + (sizeof(Int16) * 2)]);
        }
    }
}
