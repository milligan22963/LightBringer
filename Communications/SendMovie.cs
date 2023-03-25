using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class SendMovie : Command
    {
        public SendMovie()
        {
            CommandType = Communications.CommandType.SendMovie;
        }

        public override int Length()
        {
            int commandLength = base.Length();

            commandLength += sizeof(byte); // id of movie

            if (Movie == null)
            {
                Movie = new MovieObject();
            }

            commandLength += Movie.Length();

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

            if (Movie == null)
            {
                Movie = new MovieObject();
            }

            Movie.ReceiveData(dataStream, offset);
        }

        #region PROPERTIES
        public Int16 Identity
        {
            get;
            set;
        }

        public MovieObject Movie
        {
            get;
            set;
        }
        #endregion
    }
}
