using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Services;
namespace CommunicationServers.Sockets
{
    public delegate void NewConnnetion(Socket socket);
    public delegate void NewMessage(Socket socket, string Message);

    public class SocketServer
    {
        private static int MaxConnectionCount = 1024;
        private static byte[] buffer = new byte[1024];
        public event NewConnnetion NewConnnectionEvent;
        public event NewMessage NewMessageEvent;

        /// <summary>
        /// 侦听
        /// </summary>
        /// <param name="Port">端口号</param>
        /// <param name="IP">IP地址</param>
        /// <returns></returns>
        public bool Listen(string Port, string IP ="")
        {
            try
            {
                if (PortIsUsed(Convert.ToInt32(Port))){ throw new Exception("网络端口已被占用"); }
                var Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Any;
                if (!string.IsNullOrWhiteSpace(IP))
                {
                    IPAddress.TryParse(IP, out ip);
                }
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(Port));
                Socket.Bind(point);
                Socket.Listen(MaxConnectionCount);
                //创建监听线程
                Socket.BeginAccept(new AsyncCallback(AcceptNewClient), Socket);
                return true;
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "侦听发生错误");
                return false;
            }
        }

        /// <summary>
        /// 接收新的客户端请求
        /// </summary>
        /// <param name="ar"></param>
        Socket ServerSocket;
        public void AcceptNewClient(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                ServerSocket = socket;
                var client = socket.EndAccept(ar);
                NewConnnectionEvent(client);
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
                socket.BeginAccept(new AsyncCallback(AcceptNewClient), socket);
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "接受新的客户端请求时错误");
            }
            
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                var length = socket.EndReceive(ar);
                var message = Encoding.ASCII.GetString(buffer, 0, length);
                NewMessageEvent(socket, message);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "接收消息时错误");
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Message"></param>
        public void Send(Socket ClientSocket , string Message )
        {
            byte[] buffer = Encoding.ASCII.GetBytes(Message);
            ClientSocket.Send(buffer);
        }

        /// <summary>
        /// 断开Socket连接
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <returns></returns>
        public bool Disconnect(Socket ClientSocket)
        {
            try
            {
                ClientSocket.Close();
                ClientSocket.Dispose();
                return true;
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "关闭Socket连接错误");
                return false;
            }
        }

        public bool IsConnected(Socket ClientSocket)
        {
            if (ClientSocket.Connected)
            {
                return true;
            }
            return false;
        }

        private bool PortIsUsed(int Port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            int count = ipEndPoints.Where(m => m.Port == Port).ToList().Count;
            Debug.WriteLine(count);
            if (count==1)
            {
                return true;
            }
            return false;
        }

        //能不能产生一个客户端断开后事件提示
    }
}
