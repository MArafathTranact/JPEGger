using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPEGgerServer
{
    public class TCPListener
    {
        private TcpListener listener;
        private ArrayList handlerList;
        private ArrayList socketList;

        public void StartListener()
        {
            try
            {
                handlerList = new ArrayList();
                socketList = new ArrayList();
                IPAddress ipAddress = IPAddress.Parse(Logger.GetAppSettingValue("Ip"));
                listener = new TcpListener(ipAddress, int.Parse(Logger.GetAppSettingValue("Port")));
                listener.Start();

                LogEvents($" JPEGger Server is listening on ");
                LogEvents($" Waiting for a connection... ");

                while (true)
                {
                    try
                    {
                        var client = listener.AcceptSocket();
                        if (client != null)
                        {
                            IPEndPoint remoteIpEndPoint = client.RemoteEndPoint as IPEndPoint;
                            LogEvents($" Client Connection in on {IPAddress.Parse(((IPEndPoint)client.RemoteEndPoint).Address.ToString()) } , Port {((IPEndPoint)client.RemoteEndPoint).Port}");

                            ClientHandler clientHandler = new ClientHandler(client);
                            Thread thread = new Thread(clientHandler.HandleRequest);
                            handlerList.Add(clientHandler);
                            socketList.Add(client);
                            LogEvents($" Client Handler created ");
                            thread.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogEvents($"Exception at TCPListener.StartListener: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                LogEvents($"Exception at TCPListener.StartListener: {ex.Message}");
            }
        }

        public void StopListener()
        {
            try
            {
                foreach (var item in handlerList)
                {
                    try
                    {
                        ClientHandler handler = (ClientHandler)item;
                        handler.StopClient();
                    }
                    catch (Exception ex)
                    {
                        LogEvents($"Exception at TCPListener.StopListener: {ex.Message}");
                    }

                }

                handlerList.Clear();
                socketList.Clear();
                handlerList = null;
                socketList = null;

                if (listener != null)
                {
                    listener.Stop();
                    listener = null;
                }
            }
            catch (Exception ex)
            {
                LogEvents($"Exception at TCPListener.StopServer: {ex.Message}");
            }


        }

        private void LogEvents(string input)
        {
            Logger.LogWithNoLock($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}:{input}");
        }

    }
}
