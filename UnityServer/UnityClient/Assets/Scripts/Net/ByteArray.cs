using System;

//处理数据字节
public class ByteArray
{
    //默认缓冲区大小
    public const int DEFAULT_SIZE = 1024;

    //初始大小
    private int m_InitSize = 0;

    //读写位置
    public int ReadIndex { get; set; }
    public int WriteIndex { get; set; }

    //当前容量，不足时*2扩容
    private int capacity = 0;

    //剩余空间
    public int Remain { get { return capacity - WriteIndex; } }

    //数据长度
    public int Length { get { return WriteIndex - ReadIndex; } }
    public byte[] Bytes { get; set; }

    public ByteArray()
    {
        Bytes = new byte[DEFAULT_SIZE];
        capacity = DEFAULT_SIZE;
        m_InitSize = DEFAULT_SIZE;
        ReadIndex = 0;
        WriteIndex = 0;

    }

    public ByteArray(byte[] bytes)
    {
        this.Bytes = bytes;
        this.capacity = bytes.Length;
        this.m_InitSize = DEFAULT_SIZE;
        this.ReadIndex = 0;
        this.WriteIndex = bytes.Length;
    }
    /// <summary>
    /// 检测并移动数据
    /// </summary>
    public void CheckAndReads()
    {
        //当数据量很小时，移动数据开销小
        if (Length < 8)
        {
            MoveBytes();
        }
    }

    public void MoveBytes()
    {
        if (ReadIndex < 0)
        {
            return;
        }
        Array.Copy(Bytes, ReadIndex, Bytes, 0, Length);
        WriteIndex = Length;
        ReadIndex = 0;
    }





    /// <summary>
    /// 扩充容量
    /// </summary>
    public void Resize(int size)
    {
        if (ReadIndex < 0)
            return;
        if (size < Length || size < DEFAULT_SIZE) return;
        int n = 1024;
        while (n < size)
        {
            n *= 2;
        }
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(Bytes, ReadIndex, newBytes, 0, Length);
        Bytes = newBytes;
        WriteIndex = Length;
        ReadIndex = 0;
    }
}