using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    public class Factory
    {
        static Factory m_instance = null;

        private Factory()
        {
        }

        public static Factory GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new Factory();
            }

            return m_instance;
        }

        public Command GenerateCommand(byte[] dataStream)
        {
            Command command = new Command();

            command.ReceiveData(dataStream);

            return GenerateCommand(command.CommandType, dataStream);
        }

        public Command GenerateCommand(CommandType type, byte [] dataStream = null)
        {
            Command command = null;

            switch (type)
            {
                case CommandType.Presence:
                    {
                        command = new Presence();
                    }
                    break;

                case CommandType.PresenceResponse:
                    {
                        command = new PresenceResponse();
                    }
                    break;

                case CommandType.GetConfiguration:
                    {
                        command = new GetConfiguration();
                    }
                    break;

                case CommandType.ConfigurationResponse:
                    {
                        command = new ConfigurationResponse();
                    }
                    break;

                case CommandType.SetConfiguration:
                    {
                        command = new SetConfiguration();
                    }
                    break;

                case CommandType.SendFrame:
                    {
                        command = new SendFrame();
                    }
                    break;

                case CommandType.SendMovie:
                    {
                        command = new SendMovie();
                    }
                    break;
            }

            if (dataStream != null)
            {
                command.ReceiveData(dataStream);
            }
            return command;
        }
    }
}
