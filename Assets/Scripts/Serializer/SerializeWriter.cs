using System;
using System.IO;
using System.Text;

public class SerializeWriter
{
	Stream strm;
	public SerializeWriter(Stream strm)
	{
		this.strm =strm;
	}
	public byte[] bytes
	{
		get
		{
			return ((MemoryStream)strm).ToArray();
		}
	}
	public void Clear()
	{
		if(strm != null)
		{
			strm.Dispose();
			strm = null;
		}
	}
	public void Write(bool t)
	{	
		strm.WriteByte((byte)(t?1:0));
	}
	public void Write(byte t)
	{
		strm.WriteByte(t);
	}
	public void Write(sbyte t)
	{
		strm.WriteByte((byte)t);
	}
	
	public void Write(short t)
	{
		strm.Write(BitConverter.GetBytes(t),0,2);
	}
	public void Write(ushort t)
	{
		strm.Write(BitConverter.GetBytes(t),0,2);
	}
	
	public void Write(int t)
	{
		strm.Write(BitConverter.GetBytes(t),0,4);
	}
	public void Write(uint t)
	{
		strm.Write(BitConverter.GetBytes(t),0,4);
	}
	public void Write(float t)
	{
		strm.Write(BitConverter.GetBytes(t),0,4);
	}
	
	public void Write(double t)
	{
		strm.Write(BitConverter.GetBytes(t),0,8);
	}
	public void Write(long t)
	{
		strm.Write(BitConverter.GetBytes(t),0,8);
	}
	public void Write(ulong t)
	{
		strm.Write(BitConverter.GetBytes(t),0,8);
	}
	public void Write(byte[] bytes)
	{
		strm.Write(bytes,0,bytes.Length);
	}
	
	public void Write(string t)
	{
		if (t == null)
		{
			t = string.Empty;
			//throw new ArgumentNullException ("null string!");
		}
        byte[] bytes = CONST.MyEncoding.GetBytes(t);
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            bytes = Helper.UTF8ToUTF32(bytes);
#endif
        strm.Write(bytes, 0, bytes.Length);
	}
	public void Write(string t,int length)
	{
		if (t == null)
		{
			t = string.Empty;
			//throw new ArgumentNullException ("null string!");
		}
		char[] array = t.ToCharArray (); 
		byte[] bytes = new byte[length];
        CONST.MyEncoding.GetBytes(array).CopyTo(bytes, 0);
#if UNITY_WP8
        //WP8 UTF8编码特殊处理
        bytes = Helper.UTF8ToUTF32(bytes,length);
#endif         
        strm.Write(bytes, 0, bytes.Length);
		array =null;
		bytes=null;
	}
}
