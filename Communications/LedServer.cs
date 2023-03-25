using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Communications
{
    public class LedServer
    {
        int m_clientBroadcastPort;  // port to broadcast commands on 27742
        int m_serverListenPort;     // port used by server to hear clients 27842

        private Socket m_udpReceiver = null;
        private byte[] m_receiveBuffer;
        private SocketAsyncEventArgs m_receiveArgs = null;
        private bool m_isClosing = false;
        string m_errorMessage = string.Empty;

        // based lossely off of http://www.codeproject.com/Articles/463947/Working-with-Sockets-in-Csharp
        public LedServer()
        {
            m_receiveBuffer = new byte[8192]; // 8k buffer, default value may need to be adjusted
            MulticastAddress = new IPAddress(new byte[] { 239, 1, 1, 1 }); // http://en.wikipedia.org/wiki/Multicast_address
        }

        public void Start()
        {
            IPEndPoint localEP = new IPEndPoint(ServerIpAddress, ServerListenPort);
            m_udpReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Set the socket to reuse an existing address, if any.
            m_udpReceiver.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            m_udpReceiver.Bind(localEP);
            m_udpReceiver.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastAddress, ServerIpAddress));

            m_receiveArgs = new SocketAsyncEventArgs();

            m_receiveArgs.UserToken = m_udpReceiver;
            m_receiveArgs.RemoteEndPoint = new IPEndPoint(MulticastAddress, ServerListenPort);
            m_receiveArgs.SetBuffer(m_receiveBuffer, 0, m_receiveBuffer.Length);
            m_receiveArgs.Completed += OnReceive;

            m_isClosing = false;

            StartReceiveOperation(m_udpReceiver, m_receiveArgs);
        }

        public void SendCommand(Command outgoingCommand)
        {
            IPEndPoint localEP = new IPEndPoint(ServerIpAddress, 0);
            IPEndPoint remoteEP = new IPEndPoint(MulticastAddress, ClientBroadcastPort);
            UdpClient targetClient = new UdpClient(localEP);

            targetClient.EnableBroadcast = true;

            outgoingCommand.PrepareSend();

            targetClient.Send(outgoingCommand.DataStream, outgoingCommand.Length(), remoteEP);

            targetClient.Close();
        }

        private void StartReceiveOperation(Socket udpReceiver, SocketAsyncEventArgs args)
        {
            if (udpReceiver != null)
            {
                try
                {
                    udpReceiver.ReceiveAsync(args);
                }

                catch (ArgumentException e)
                {
                    m_errorMessage = e.Message;
                }

                catch (InvalidOperationException e)
                {
                    m_errorMessage = e.Message;
                }

                catch (NotSupportedException e)
                {
                    m_errorMessage = e.Message;
                }

                catch (SocketException e)
                {
                    m_errorMessage = e.Message;
                }
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            Socket udpReceiver = e.UserToken as Socket;

            // Receive data from the socket of choice
            if ((udpReceiver != null) && (m_isClosing != true))
            {
                // Process incoming data
                // that was stored in m_receiveBuffer
                if ((e.LastOperation == SocketAsyncOperation.Receive) && (e.BytesTransferred > 0))
                {
                    // we need to ensure we have enough bytes for the command in question and
                    // if there is more than one command, that we parse it properly
                    Command incomingCommand = Factory.GetInstance().GenerateCommand(m_receiveBuffer);

                    switch (incomingCommand.CommandType)
                    {
                        case CommandType.PresenceResponse:
                            {
                            }
                            break;
                        case CommandType.ConfigurationResponse:
                            {
                            }
                            break;
                    }
                }
                StartReceiveOperation(m_udpReceiver, m_receiveArgs);
            }
        }

        public void Stop()
        {
            m_isClosing = true;

            // shutdown our socket and clean up
            if (m_udpReceiver != null)
            {
                m_udpReceiver.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(MulticastAddress, ServerIpAddress));
                m_udpReceiver.Close();
                m_udpReceiver = null;
            }
        }
        #region PROPERTIES

        public int ClientBroadcastPort
        {
            get
            {
                return m_clientBroadcastPort;
            }
            set
            {
                m_clientBroadcastPort = value;
            }
        }

        public int ServerListenPort
        {
            get
            {
                return m_serverListenPort;
            }
            set
            {
                m_serverListenPort = value;
            }
        }

        public IPAddress ServerIpAddress
        {
            get;
            set;
        }

        public IPAddress MulticastAddress   
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get
            {
                return m_errorMessage;
            }
        }
        #endregion
    }
}
