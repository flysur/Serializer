/* 注：记得把文件另存为编码为[UTF-8 有BOM]才能完美支持中文
 *
 * 功能：二进制块
 *
 * 时间：2013.03.05 
 */ 

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;


public class BinarySerializer 
{
	byte[] data;
	int size;
	int index =0;
	public BinarySerializer(byte[] pData)
	{
		this.data = pData;
		size = data != null ? data.Length : 0;
		index = 0;
    }
    public BinarySerializer(byte[] pData, int nOffset, int nLen)
    {
        if(pData == null)
        {
            this.data = null;
            size = 0;
            index = 0;
        }
        else
        {
            this.data = pData;
            if (nOffset < 0)
                nOffset = 0;
            if (nOffset + nLen > data.Length)
                nLen = data.Length - nOffset;
            size = nLen;
            index = nOffset;
        }
    }
    private void MoveNext(int n)
	{
		index+=n;
	}
	public byte ReadByte()
    {
        if (index + 1 > size)
            return 0;
        byte temp = data[index];
		MoveNext(1);
		return temp;
	}
	public sbyte ReadSbyte()
    {
        if (index + 1 > size)
            return 0;
        sbyte temp = (sbyte)data[index];
		MoveNext(1);
		return temp;
	}
	
	public short ReadShort()
    {
        if (index + 2 > size)
            return 0;
        short temp = BitConverter.ToInt16(data,index);
		MoveNext(2);
		return temp;
	}
	public ushort ReadUshort()
    {
        if (index + 2 > size)
            return 0;
        ushort temp = BitConverter.ToUInt16(data,index);
		MoveNext(2);
		return temp;
	}
	
	public int ReadInt()
	{
        if (index + 4 > size)
            return 0;
		int temp = BitConverter.ToInt32(data,index);
		MoveNext(4);
		return temp;
	}
	public uint ReadUInt()
    {
        if (index + 4 > size)
            return 0;
        uint temp = BitConverter.ToUInt32(data,index);
		MoveNext(4);
		return temp;
	}
	
	public long ReadLong()
    {
        if (index + 8 > size)
            return 0;
        long temp = BitConverter.ToInt64(data,index);
		MoveNext(8);
		return temp;
	}
	public ulong ReadULong()
    {
        if (index + 8 > size)
            return 0;
        ulong temp = BitConverter.ToUInt64(data,index);
		MoveNext(8);
		return temp;
	}
	
	public float ReadFloat()
    {
        if (index + 4 > size)
            return 0;
        float temp = BitConverter.ToSingle(data,index);
		MoveNext(4);
		return temp;
	}
	public double ReadDouble()
    {
        if (index + 8 > size)
            return 0;
        double temp = BitConverter.ToDouble(data,index);
		MoveNext(8);
		return temp;
	}
	
	public string ReadString()
    {
        if (index + 4 > size)
            return string.Empty;
        int length = ReadInt();
        if (length > 0)
        {
            if (index + length > size)
                return string.Empty;
            byte[] temp = new byte[length];
            Array.Copy(data, index, temp, 0, length);
            MoveNext(length);
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            temp = Helper.UTF32ToUTF8(temp);
#endif
            return CONST.MyEncoding.GetString(temp);
        }
        return "";
	}
}
//specify the type   0x80004005 Unsepecified