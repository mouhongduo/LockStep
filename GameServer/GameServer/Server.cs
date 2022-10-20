using System.Net;
using System.Net.Sockets;
using LitJson;

namespace MHD
{
    public class Server
    {
        //端口
        private const int port = 8008;
        //客户端列表
        private List<Client> clients = new List<Client>();

        private static void Main(string[] args)
        {
            Console.WriteLine("服务端启动...");
            Server server = new Server();
            server.Init();
        }

        //服务端初始化
        private void Init()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            try
            {
                while (true)
                {
                    Console.WriteLine("等待客户端接入...");
                    TcpClient client = listener.AcceptTcpClient();
                    Client clientInstance = new Client(client, this);
                    clients.Add(clientInstance);
                    Console.WriteLine($"{client.Client.RemoteEndPoint}接入.");
                }
            }
            catch (Exception error)
            {
                throw new Exception(error.ToString());
            }
        }

        /// <summary>
        /// 广播：向所有客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(string data)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Send(data);
            }
        }
        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="client"></param>
        public void Remove(Client client)
        {
            if (clients.Contains(client))
            {
                clients.Remove(client);
            }
        }
    }
}
