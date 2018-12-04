using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Services;
namespace CommunicationServers.Sockets
{
    public class SocketClient
    {
        private static byte[] buffer = new byte[1024];
        public event NewMessage NewMessageEvent;

        private Socket clientSocket;
        public Socket ClientSocket
        {
            get
            {
                return clientSocket;
            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="Port"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool Connnect(string Port, string IP)
        {
            var Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket = Socket;
            IPAddress ip = IPAddress.Any;
            if (!string.IsNullOrWhiteSpace(IP))
            {
                IPAddress.TryParse(IP, out ip );
            }
            else
            {
                return false;
            }
            try
            {
                Socket.Connect(ip, Convert.ToInt32(Port));
                Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), Socket);
                return true;
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "连接服务器时发生错误");
                return false;
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
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "接收服务器时发生错误");
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="ServerSocket"></param>
        /// <param name="Message"></param>
        public void Send(string Message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(Message);
            this.clientSocket.Send(buffer);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            try
            {
                clientSocket.Close();
                return true;
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex, "与服务器断开连接发生错误");
                return false;
            }
        }

        /// <summary>
        /// 判断网络连接状态
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (clientSocket.Connected)
            {
                return true;
            }
            return false;
        }

    }
}
