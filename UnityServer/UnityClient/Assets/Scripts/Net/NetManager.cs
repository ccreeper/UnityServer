using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
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
        m_Socket.BeginConnect(ip, port,ConnectCallback,this);
        m_Ip = ip;
        m_Port = port;

    }

    void InitState() {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ReadBuffer = new ByteArray();
        m_IsConnecting = false;
        m_IsClosing = false;
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
            m_IsConnecting = false;
            Debug.Log("Socket Connect Success");

            m_Socket.BeginReceive(m_ReadBuffer.Bytes, m_ReadBuffer.WriteIndex, m_ReadBuffer.Remain, 0,ReceiveCallback,this);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket Connect Fail:" + e.ToString());
            m_IsConnecting = false;
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
            socket.BeginReceive(m_ReadBuffer.Bytes, m_ReadBuffer.WriteIndex, m_ReadBuffer.Remain, 0, ReceiveCallback, this);
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

    }

    /// <summary>
    /// 关闭连接，默认正常关闭
    /// </summary>
    /// <param name="normal"></param>
    public void Close(bool normal = true) {
        if (m_Socket == null || m_IsConnecting)
            return;
        SecretKey = "";
        m_Socket.Close();
        InvokeEvent(NetEvent.Close, normal.ToString());
        Debug.Log("Close Socket ");
    }

    public void SetSecretKey(string key) {
        SecretKey = key;
    }
}
