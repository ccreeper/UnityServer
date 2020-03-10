using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetManager : Singleton<NetManager>
{
    public enum NetEvent {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3
    }

    public string PUBLICKEY = "OceanServer";

    public string SecretKey { get;private set; }

    private Socket m_Socket;

    private ByteArray m_ReadBuffer;

    private string m_Ip;
    private int m_Port;

    //连接状态
    private bool m_IsConnecting = false;
    private bool m_IsClosing = false;

    public delegate void EventListener(string str);
    private Dictionary<NetEvent, EventListener> m_ListenerDic = new Dictionary<NetEvent, EventListener>();

    public delegate void ProtoListener(MsgBase msgBase);
    private Dictionary<ProtocolEnum, ProtoListener> m_ProtoDic = new Dictionary<ProtocolEnum, ProtoListener>();

    private List<MsgBase> m_MsgList;
    private List<MsgBase> m_UnityMsgList;

    //发送消息队列
    private Queue<ByteArray> m_WriteQueue;

    //未处理消息的数量
    private int m_MsgCount = 0;

    private Thread m_MsgThread;
    private Thread m_HeartThread;

    //最后一次接受心跳包的时间
    static long lastPongTime;
    //最后一次发送心跳包的时间
    static long lastPingTime;
    //心跳包时间间隔
    public static long m_PingInterval = 30;

    //掉线
    private bool m_Disconnect = false;
    //是否成功连接过
    private bool m_HasConnected = false;
    //此次连接是否是重连，一般用于重连的自动登录
    private bool m_ReConnect = false;

    void InitState()
    {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_MsgList = new List<MsgBase>();
        m_UnityMsgList = new List<MsgBase>();
        m_ReadBuffer = new ByteArray();
        m_WriteQueue = new Queue<ByteArray>();
        m_MsgCount = 0;
        lastPongTime = GetTimeStamp();
        lastPingTime = GetTimeStamp();
        m_IsConnecting = false;
        m_IsClosing = false;
    }

    public void AddEventListener(NetEvent netEvent,EventListener listener){
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            //多事件监听
            m_ListenerDic[netEvent] += listener;
        }
        else {
            m_ListenerDic[netEvent] = listener;
        }
    }

    /// <summary>
    /// 协议监听
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="listener"></param>
    public void AddProtoListener(ProtocolEnum protocolEnum, ProtoListener listener) {
        //一个协议对应唯一一个监听
        m_ProtoDic[protocolEnum] = listener;
    }

    public void InvokeProtoListener(ProtocolEnum proto,MsgBase msg) {
        if (m_ProtoDic.ContainsKey(proto)) {
            m_ProtoDic[proto].Invoke(msg);
        }
    }

    public void RemoveEventListener(NetEvent netEvent, EventListener listener) {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent] -= listener;
            if (m_ListenerDic[netEvent] == null) {
                m_ListenerDic.Remove(netEvent);
            }
        }
    }

    void InvokeEvent(NetEvent netEvent, string str) {
        if (m_ListenerDic.ContainsKey(netEvent)){ 
            m_ListenerDic[netEvent].Invoke(str);
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port) {
        if (m_Socket != null && m_Socket.Connected) {
            Debug.LogError("连接失败，处于已连接状态...");
            return;
        }
        if (m_IsConnecting) {
            Debug.LogError("正在连接中...");
            return;
        }
        InitState();
        m_Socket.NoDelay = true;
        m_IsConnecting = true;
        m_Socket.BeginConnect(ip, port,ConnectCallback,m_Socket);
        m_Ip = ip;
        m_Port = port;

    }



    /// <summary>
    /// 连接服务器回调
    /// </summary>
    /// <param name="result"></param>
    void ConnectCallback(IAsyncResult result) {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndConnect(result);
            InvokeEvent(NetEvent.ConnectSucc,"");

            m_HasConnected = true;
            //连接成功后，创建线程处理协议
            m_MsgThread = new Thread(MsgThread);
            //设置后台可运行
            m_MsgThread.IsBackground = true;
            m_MsgThread.Start();
            m_IsConnecting = false;

            //心跳包线程
            m_HeartThread = new Thread(PingThread);
            m_HeartThread.IsBackground = true;
            m_HeartThread.Start();

            //连接成功后请求密钥
            ProtocolMrg.SecretRequest();
            Debug.Log("Socket Connect Success");

            m_Socket.BeginReceive(m_ReadBuffer.Bytes, m_ReadBuffer.WriteIndex, m_ReadBuffer.Remain, 0,ReceiveCallback,m_Socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket Connect Fail:" + e.ToString());
            m_IsConnecting = false;
        }
    }

    /// <summary>
    /// 心跳包发送线程
    /// </summary>
    void PingThread() {
        while (m_Socket != null && m_Socket.Connected) {
            long timeNow = GetTimeStamp();
            if (timeNow - lastPingTime > m_PingInterval) {
                MsgPingHeart msgPing = new MsgPingHeart();
                SendMessage(msgPing);
                lastPingTime = GetTimeStamp();
            }

            //心跳包过长没有回复，可能故障，非正常退出
            if (timeNow - lastPongTime > m_PingInterval * 4) {
                Close(false);
            }
        }
    }

    //处理Unity游戏中的协议，由MonoBehaviour的Update中再调用
    public void Update() {
        //服务器是否连接过且当前断线
        if (m_Disconnect && m_HasConnected) {
            //弹框，确定是否重连
            //重连
            ReConnect();
            //TODO:退出


            m_Disconnect = false;
        }
        MsgUpdate();
    }

    /// <summary>
    /// 重新连接
    /// </summary>
    public void ReConnect() {
        Connect(m_Ip, m_Port);
        m_ReConnect = true;
    }

    void MsgUpdate() {
        if (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgCount == 0)
                return;
            MsgBase msgBase = null;
            lock (m_UnityMsgList) {
                if (m_UnityMsgList.Count > 0) {
                    msgBase = m_UnityMsgList[0];
                    m_UnityMsgList.RemoveAt(0);
                    m_MsgCount--;
                }
            }
            if (msgBase != null) {
                InvokeProtoListener(msgBase.ProtoType, msgBase);
            }
        }
    }

    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="msg"></param>
    public void SendMessage(MsgBase msg) {
        if (m_Socket == null || !m_Socket.Connected)
            return;
        if (m_IsConnecting) {
            Debug.LogError("正在连接服务器，发送失败...");
        }
        if (m_IsClosing) {
            Debug.LogError("正在关闭连接中，发送失败...");
        }
        try
        {
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] headBytes = BitConverter.GetBytes(len);
            byte[] sendBytes = new byte[len + headBytes.Length];
            Array.Copy(headBytes, 0, sendBytes, 0, headBytes.Length);
            Array.Copy(nameBytes, 0, sendBytes, headBytes.Length, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, headBytes.Length+nameBytes.Length, bodyBytes.Length);
            ByteArray byteArray = new ByteArray(sendBytes);
            int count = 0;
            lock (m_WriteQueue) {
                m_WriteQueue.Enqueue(byteArray);
                count = m_WriteQueue.Count;
            }

            //防止异步时同时进入，反复发送同一条数据
            if (count == 1) {
                m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, m_Socket);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Send Message Error..."+e.ToString());
            Close();

        }
    }

    /// <summary>
    /// 发送消息回调
    /// </summary>
    /// <param name="result"></param>
    void SendCallback(IAsyncResult result) {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            if (socket == null || !socket.Connected)
                return;
            int count = socket.EndSend(result);

            //判断是否发送完整
            ByteArray byteArray;
            lock (m_WriteQueue) {
                byteArray = m_WriteQueue.First();
            }
            byteArray.ReadIndex += count;

            //发送完整
            if (byteArray.Length == 0) {
                lock (m_WriteQueue) {
                    m_WriteQueue.Dequeue();
                    if (m_WriteQueue.Count > 0)
                    {
                        byteArray = m_WriteQueue.First();
                    }
                    else {
                        byteArray = null;
                    }
                }
            }

            //1. 发送不完整
            //2. 发送完整，但存在第二条数据
            if (byteArray != null)
            {
                socket.BeginSend(byteArray.Bytes, byteArray.ReadIndex, byteArray.Length, 0, SendCallback, socket);
            }
            //确保关闭连接前，先把消息发送完
            else if (m_IsClosing) {
                RealClose();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SendCallback Error" + e.ToString());
            Close();
        }
    }

    void MsgThread() {
        while (m_Socket != null && m_Socket.Connected) {
            if (m_MsgList.Count <= 0)
                continue;
            MsgBase msgBase = null;
            lock (m_MsgList) {
                if (m_MsgList.Count > 0) {
                    msgBase = m_MsgList[0];
                    m_MsgList.RemoveAt(0);
                }
                if (msgBase != null)
                {
                    if (msgBase is MsgPingHeart)
                    {
                        lastPongTime = GetTimeStamp();
                        Debug.Log("接受到心跳包...当前时间  "+DateTime.Now);
                        m_MsgCount--;
                    }
                    else {
                        //非后台协议
                        lock (m_UnityMsgList) {
                            m_UnityMsgList.Add(msgBase);
                        }
                    }
                }
                else {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 接受消息回调
    /// </summary>
    /// <param name="result"></param>
    void ReceiveCallback(IAsyncResult result) {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            //EndXXX不能再BeginXXX的回调前使用，否则会阻塞，直到接收到数据或出错
            //EndXXX一般都在回调中调用，且必须调用
            int count = socket.EndReceive(result);
            if (count <= 0) {
                //关闭连接
                Close();
                return;
            }
            m_ReadBuffer.WriteIndex += count;
            OnReceiveData();
            //分包的情况，缓冲区不足，则扩充
            if (m_ReadBuffer.Remain < 8) {
                m_ReadBuffer.MoveBytes();
                m_ReadBuffer.Resize(m_ReadBuffer.Length *2);
            }
            //再次等待数据接收
            socket.BeginReceive(m_ReadBuffer.Bytes, m_ReadBuffer.WriteIndex, m_ReadBuffer.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("数据接收失败" + e);
            Close();
        }
    }


    /// <summary>
    /// 处理数据
    /// </summary>
    void OnReceiveData() {
        if (m_ReadBuffer.Length <= 4 || m_ReadBuffer.ReadIndex < 0)
            return;
        int readIndex = m_ReadBuffer.ReadIndex;
        byte[] bytes = m_ReadBuffer.Bytes;
        int bodyLength = BitConverter.ToInt32(bytes, readIndex);
        if (m_ReadBuffer.Length < bodyLength + 4) {
            //分包情况
            return;
        }
        m_ReadBuffer.ReadIndex += 4;

        int nameCount = 0;
        ProtocolEnum proto = ProtocolEnum.None;
        proto = MsgBase.DecodeName(m_ReadBuffer.Bytes, m_ReadBuffer.ReadIndex,out nameCount);
        if (proto == ProtocolEnum.None) {
            Debug.LogError("OnReceiveData MsgBase.DecodeName Fail");
            Close();
            return;
        }
        m_ReadBuffer.ReadIndex += nameCount;

        int bodyCount = bodyLength - nameCount ;
        try
        {
            MsgBase msg = MsgBase.Decode(proto, m_ReadBuffer.Bytes, m_ReadBuffer.ReadIndex, bodyCount);
            if (msg == null) {
                Debug.LogError("接收数据协议内容出错...");
                Close();
                return;
            }
            m_ReadBuffer.ReadIndex += bodyCount;
            m_ReadBuffer.CheckAndReads();

            //部分游戏中的协议不应在多线程中处理，后续操作应在unity的update中进行
            //还有部分协议（如心跳包）即使在后台也要执行,所以用专门的线程处理协议的操作
            //综上，先进行存储，再分类专门处理
            lock (m_MsgList) {
                m_MsgList.Add(msg);
            }
            m_MsgCount++;

            if (m_ReadBuffer.Length > 4) {
                //黏包的情况，继续解析
                OnReceiveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket OnReceiveData Error:" + e.ToString());
            Close();
            return;
        }

    }

    /// <summary>
    /// 关闭连接，默认正常关闭
    /// </summary>
    /// <param name="normal"></param>
    public void Close(bool normal = true) {
        if (m_Socket == null || m_IsConnecting)
            return;
        if (m_WriteQueue.Count > 0)
        {
            m_IsClosing = true;
        }
        else
        {
            RealClose(normal);
        }
    }

    void RealClose(bool normal = true) {

        SecretKey = "";
        m_Socket.Close();
        InvokeEvent(NetEvent.Close, normal.ToString());
        m_Disconnect = true;
        if (m_HeartThread != null && m_HeartThread.IsAlive)
        {
            m_HeartThread.Abort();
            m_HeartThread = null;
        }
        if (m_MsgThread != null && m_MsgThread.IsAlive)
        {
            m_MsgThread.Abort();
            m_MsgThread = null;
        }
        Debug.Log("Close Socket ");
    }

    public void SetSecretKey(string key) {
        SecretKey = key;
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}
