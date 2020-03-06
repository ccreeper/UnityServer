using ProtoBuf;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class MsgBase
{
    public virtual ProtocolEnum ProtoType { get; set; }


    //整个协议分为3部分， 协议头(整个协议长度)-----》协议名称(名称编码长度+名称编码)  -----》具体协议内容

    /// <summary>
    /// 协议头编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
        Int16 len = (Int16)nameBytes.Length;
        //长度为len+2个字节存Int16的len
        byte[] bytes = new byte[2 + len];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    /// <summary>
    /// 协议头解码
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length)
        {
            return ProtocolEnum.None;
        }
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset + 2 + len > bytes.Length)
            return ProtocolEnum.None;
        count = 2 + len;
        try
        {
            string name = Encoding.UTF8.GetString(bytes, offset + 2, len);
            return (ProtocolEnum)Enum.Parse(typeof(ProtocolEnum), name);
        }
        catch (Exception e)
        {
            Debug.LogError("协议解析出错,不存在的协议:" + e.ToString());
            return ProtocolEnum.None;
        }
    }


    /// <summary>
    /// 协议序列化加密
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string secret = string.IsNullOrEmpty(NetManager.Instance.SecretKey) ?
            NetManager.Instance.PUBLICKEY : NetManager.Instance.SecretKey;
        using (var memory = new MemoryStream())
        {
            //将协议序列化
            Serializer.Serialize(memory, msgBase);
            byte[] bytes = memory.ToArray();
           
            bytes = AES.AESEncrypt(bytes, secret);
            return bytes;
        }
    }

    /// <summary>
    /// 协议反序列化解密
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int offset, int count)
    {
        if (count <= 0)
        {
            Debug.LogError("协议解密出错，数据长度为0...");
        }
        try
        {
            byte[] newBytes = new byte[count];
            Array.Copy(bytes, offset, newBytes, 0, count);

            string secret = string.IsNullOrEmpty(NetManager.Instance.SecretKey) ?
                NetManager.Instance.PUBLICKEY : NetManager.Instance.SecretKey;
            newBytes = AES.AESDecrypt(newBytes, secret);
            using (var memory = new MemoryStream(newBytes, 0, newBytes.Length))
            {
                Type t = System.Type.GetType(protocol.ToString());
                return (MsgSecret)Serializer.NonGeneric.Deserialize(t, memory);
            }

        }
        catch (Exception e)
        {
            Debug.LogError("协议解析出错：" + e.ToString());
            return null;
        }
    }

}