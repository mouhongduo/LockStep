using System;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using LitJson;
using UnityEngine.UI;
public class Client : MonoBehaviour
{
    private string ipAddress;
    private int port;
    private bool isConnected;
    private Thread connectThread;
    private Thread readDataThread;
    private TcpClient tcpClient;
    private NetworkStream stream;
    //将数据存于队列 依次取出
    private Queue<string> queue = new Queue<string>();

    public GameObject MainPlayer;
    public GameObject SecondPlayer;
    private float lastTime;
    private int whoIsMe = 1;
    private void Start()
    {
        GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(Match);
    }

    private void Match()
    {
        ipAddress = "127.0.0.1";
        port = 8008;
        lastTime = 0;
        connectThread = new Thread(ConnectThead);
        connectThread.Start();
    }
    //连接线程
    private void ConnectThead()
    {
        tcpClient = new TcpClient();
        tcpClient.BeginConnect(ipAddress, port, ConnectThreadCallBack, tcpClient);
        float waitTime = 0f;
        while (!isConnected)
        {
            Thread.Sleep(20);
            waitTime += 0.02f;
            if (waitTime > 3f)
            {
                waitTime = 0f;
                throw new Exception("连接超时");
            }
        }
    }

    private void Update()
    {
        lastTime += Time.deltaTime;
        if (lastTime > 0.03f)
        {
            lastTime -= 0.03f;
            Vector3 f1 = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W))
            {
                f1 += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(KeyCode.S))
            {
                f1 += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.A))
            {
                f1 += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                f1 += new Vector3(1, 0, 0);
            }

            Vector3 f2 = new Vector3(0, 0, 0);
            if(queue.Count > 0)
            {
                string json = queue.Dequeue();
                SimpleData simpleData1 = JsonMapper.ToObject<SimpleData>(json);
                string intruction = simpleData1.content;
                
                if (intruction.StartsWith("Team"))
                {
                    string[] strs = intruction.Split(':');
                    whoIsMe = int.Parse(strs[1]);
                    Debug.Log(whoIsMe);
                }
                else
                {
                    string[] strs = intruction.Split(':');
                    f2.x = float.Parse(strs[0]);
                    f2.y = float.Parse(strs[1]);
                    f2.z = float.Parse(strs[2]);
                }
                
            }

            if(whoIsMe == 1)
            {
                MainPlayer.GetComponent<Rigidbody>().velocity = (f1.normalized);
                SecondPlayer.GetComponent<Rigidbody>().velocity = (f2.normalized);
            }
            else
            {
                MainPlayer.GetComponent<Rigidbody>().AddForce(f2.normalized);
                SecondPlayer.GetComponent<Rigidbody>().AddForce(f1.normalized);
            }

            if(f1.magnitude != 0)
            {
                SimpleData simpleData = new SimpleData()
                {
                    content = f1.x + ":" + f1.y + ":" + f1.z
                };

                //使用LitJson序列化
                string data = JsonMapper.ToJson(simpleData);
                SendData(data);
            }
            
        }
        
    }

    private void ConnectThreadCallBack(IAsyncResult result)
    {
        tcpClient = result.AsyncState as TcpClient;
        if (tcpClient.Connected)
        {
            isConnected = true;
            tcpClient.EndConnect(result);
            stream = tcpClient.GetStream();
            readDataThread = new Thread(ReadDataThread);
            readDataThread.Start();
        }
    }
    //读取数据线程
    private void ReadDataThread()
    {
        try
        {
            while (isConnected)
            {
                byte[] buffer = new byte[1024];
                int length = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, length);
                queue.Enqueue(data);
            }
        }
        catch (Exception error)
        {
            throw new Exception(error.ToString());
        }
    }
    //程序退出时关闭线程
    private void OnApplicationQuit()
    {
        stream?.Close();
        connectThread?.Abort();
        readDataThread?.Abort();
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="content"></param>
    public void SendData(string content)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        stream.Write(buffer, 0, buffer.Length);
    }
}

[Serializable]
public class SimpleData
{
    /// <summary>
    /// 字符内容
    /// </summary>
    public string content;
}