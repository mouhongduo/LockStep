using System.Text;
using System.Net.Sockets;
using LitJson;
namespace MHD
{
	[Serializable]
	public class SimpleData
	{
		/// <summary>
		/// 字符内容
		/// </summary>
		public string content;
	}
	public class Client
	{
		
		private Server server;
		private TcpClient tcpClient;
		private NetworkStream stream;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="tcpClient"></param>
		/// <param name="server"></param>
		public Client(TcpClient tcpClient, Server server)
		{
			this.server = server;
			this.tcpClient = tcpClient;
			//启动线程 读取数据
			Thread thread = new Thread(TcpClientThread);
			thread.Start();
		}

		private void TcpClientThread()
		{
			stream = tcpClient.GetStream();
			Console.WriteLine(stream.ToString());
			//使用固定长度
			byte[] buffer = new byte[1024];

			try
			{
				while (true)
				{
					int length = stream.Read(buffer, 0, buffer.Length);
					if (length != 0)
					{
						string data = Encoding.UTF8.GetString(buffer, 0, length);
						//解包
						Unpack(data);
					}
				}
			}
			catch (Exception error)
			{
				Console.WriteLine(error.ToString());
			}
			finally
			{
				server.Remove(this);
			}
		}
		//拆包：解析数据
		//拆包：解析数据
		private void Unpack(string data)
		{
			SimpleData simpleData = JsonMapper.ToObject<SimpleData>(data);
			Console.WriteLine(simpleData.content);
		}
		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="data"></param>
		public void Send(string data)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(data);
			stream.Write(buffer, 0, buffer.Length);
		}
	}
}
