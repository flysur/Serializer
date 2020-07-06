using System;
using System.IO;
using System.Text;
using UnityEngine;

public class SerializeReader
{
	Stream strm;
	public Stream Strm
	{
		get{return strm;}
	}
	public void Clear()
	{
		if(strm != null)
		{
			strm.Dispose();
			strm = null;
		}
	}
	byte[] buf0 = new byte[8];
	public SerializeReader(Stream strm)
	{
		this.strm =strm;
	}
	
	void ReadToBuffer(int start,int length)
	{
		try
		{
			strm.Read(buf0,start,length);
		}
		catch(Exception e)
		{
            Debug.LogError(e.ToString());
		}
	}
	
	public bool ReadBool()
	{
        if (strm.Position < strm.Length)
            return (strm.ReadByte() == 1) ? true : false;
        else
            return false;
	}
	public sbyte ReadSbyte()
	{
        if (strm.Position < strm.Length)
            return (sbyte)strm.ReadByte();
        else
            return 0;
	}
	public byte ReadByte()
	{
        if (strm.Position < strm.Length)
            return (byte)strm.ReadByte();
        else
            return 0;
	}
	public byte[] ReadBytes(int len)
	{
		byte[] bytes = new byte[len];
        if(strm.Position + len <= strm.Length)
        {
            for (int i = 0; i < len; ++i)
            {
                bytes[i] = (byte)strm.ReadByte();
            }
        }
		return bytes;
	}
	public sbyte[] ReadSBytes(int len)
	{
		sbyte[] sbytes = new sbyte[len];
        if (strm.Position + len <= strm.Length)
        {
            for (int i = 0; i < len; ++i)
            {
                sbytes[i] = (sbyte)strm.ReadByte();
            }
        }
		return sbytes;
	}
	
	public short ReadInt16()
	{
		ReadToBuffer(0,2);
		int sss = BitConverter.ToInt16(buf0,0);
		return BitConverter.ToInt16(buf0,0);
	}
	public ushort ReadUInt16()
	{
		ReadToBuffer(0,2);
		return BitConverter.ToUInt16(buf0,0);
	}
	
	public int ReadInt32()
	{
		ReadToBuffer(0,4);
		return BitConverter.ToInt32(buf0,0);
	}
	public uint ReadUInt32()
	{
		ReadToBuffer(0,4);
		return BitConverter.ToUInt32(buf0,0);
	}
	public float ReadFloat()
	{
		ReadToBuffer(0,4);
		return BitConverter.ToSingle(buf0,0);
	}
	
	public double ReadDouble()
	{
		ReadToBuffer(0,8);
		return BitConverter.ToDouble(buf0,0);
	}
	public long ReadInt64()
	{
		ReadToBuffer(0,8);
		return BitConverter.ToInt64(buf0,0);
	}
	public ulong ReadUInt64()
	{
		ReadToBuffer(0,8);
		return BitConverter.ToUInt64(buf0,0);
	}
	
	public string ReadString()
	{
        if (strm.Position >= strm.Length)
            return string.Empty;
		byte[] bytes=new byte[strm.Length-strm.Position];
		strm.Read(bytes,0,bytes.Length);
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            bytes = Helper.UTF32ToUTF8(bytes);
#endif
        return Helper.TrimEnd(CONST.MyEncoding.GetString(bytes));
	}
	public string ReadString(int length)
	{
        if (strm.Position + length > strm.Length)
            return string.Empty;
		byte[] bytes=new byte[length];
		strm.Read(bytes,0,length);
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            bytes = Helper.UTF32ToUTF8(bytes);
#endif
        return Helper.TrimEnd(CONST.MyEncoding.GetString(bytes));
	}
	
}
