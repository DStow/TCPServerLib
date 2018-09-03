using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPServerLib
{
    public class Server
    {
        public delegate string MessageReceivedDelegate(string message, string ip);

        private int _port = 0;
        private MessageReceivedDelegate _receivedEvent;

        private TcpListener _listener;
        private bool _serverRunning = false;

        private Thread _serverThread;

        public Server(int port, MessageReceivedDelegate messageRecievedEvent)
        {
            _port = port;
            _receivedEvent = messageRecievedEvent;
        }

        public void StartServer()
        {
            _listener = new TcpListener(_port);
            _listener.Start();
            _serverRunning = true;

            _serverThread = new Thread(AcceptMessages);
            _serverThread.Start();
        }

        private void AcceptMessages()
        {
            try
            {
                byte[] bytes = new byte[256];
                string data = null;

                while (_serverRunning)
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    data = null;

                    NetworkStream stream = client.GetStream();

                    int i = 0;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: " + data);

                        // Work with the data
                        data = _receivedEvent(data, client.Client.RemoteEndPoint.ToString());

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Returend " + msg);
                    }

                    client.Close();
                }

                _listener.Stop();
            }
            catch (ThreadAbortException)
            {
                // End this bitch
                _listener.Stop();
            }
            catch (SocketException)
            {
                // End this bitch
                _listener.Stop();
            }
            finally
            {
                _listener.Stop();
            }
        }

        public void CloseServer()
        {
            _listener.Stop();
            _serverRunning = false;
            _serverThread.Abort();
        }

    }
}