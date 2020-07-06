using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public enum EMsgpackType
{
	PositiveFixNum	=0x00,				//0xxxxxxx	 0x00 - 0x7f
	FixMap =0x80,	 						//1000xxxx	 0x80 - 0x8f
	FixArray= 0x90,							//1001xxxx	 0x90 - 0x9f
	FixRaw	=0xa0,							//101xxxxx	 0xa0 - 0xbf
	nil	=0xc0,									//11000000	 0xc0
	reserved=0xc1,							// 11000001	 0xc1
	False =0xc2,	 							//11000010	 0xc2
	True =0xc3,	 							//11000011	 0xc3
	//reserved	 								//11000100	 0xc4
	//reserved	 								//11000101	 0xc5
	//reserved	 								//11000110	 0xc6
	//reserved	 								//11000111	 0xc7
	//reserved	 								//11001000	 0xc8
	//reserved	 								//11001001	 0xc9
	Float	=0xca, 								//11001010	 0xca
	Double	=0xcb,	 						//11001011	 0xcb
	Uint8	=0xcc,	 						//11001100	 0xcc
	Uint16	=0xcd,	 						//11001101	 0xcd
	Uint32	=0xce,	 						//11001110	 0xce
	Uint64	=0xcf,	 						//11001111	 0xcf
	Int8	=0xd0,	 	 						//11010000	 0xd0
	Int16		=0xd1, 							//11010001	 0xd1
	Int32		=0xd2, 							//11010010	 0xd2
	Int64		=0xd3, 							//11010011	 0xd3
	//reserved	 								//11010100	 0xd4
	//reserved	 								//11010101	 0xd5
	//reserved	 								//11010110	 0xd6
	//reserved	 								//11010111	 0xd7
	//reserved	 								//11011000	 0xd8
	//reserved	 								//11011001	 0xd9
	Raw16 = 0xda,	 						//11011010	 0xda
	Raw32 = 0xdb,	 						//11011011	 0xdb
	Array16 = 0xdc,	 						//11011100	 0xdc
	Array32 = 0xdd,						//11011101	 0xdd
	Map16 = 0xde,							//11011110	 0xde
	Map32 = 0xdf,	 						//11011111	 0xdf
	NegativeFixNum = 0xe0,	 		//111xxxxx	 0xe0 - 0xff
}
/// <summary>
/// Msgpack.
/// 读取后需要手动释放；
/// </summary>
public class Msgpack
{
	byte[] bytes;
	byte[] _tmp = new byte[9];
	byte[] tmp = new byte[8];
	byte[] tmp1 = new byte[8];
	EMsgpackType type;
	int msgpacLength =-1;
	int index;
	int length =0;
	//Stream stream;
	
	/// <summary>
	/// 构造；maxLength(比最大长度大即可);
	/// </summary>
	public Msgpack(int maxLength)
	{
		bytes = new byte[maxLength];
		index =0;
	}
	
	//public Msgpack()
	//{
	//	bytes = new byte[1024*24];
	//	index =0;
	//}
	public Msgpack(Stream s)
	{
		index =0;
		bytes = ((MemoryStream)s).ToArray();
	}
	#region writer
	
	public byte[] GetBytes()
	{
		byte[] _bytes = new byte[index+4];//在头上加个长度
		BitConverter.GetBytes(index+4).CopyTo(_bytes,0);//添加包长度
		Array.Copy(bytes,0,_bytes,4,index);
		return _bytes;
	}
	private void WriteIn(long x)
	{
		tmp[0] = (byte)(x >> 56);
		tmp[1] = (byte)(x >> 48);
		tmp[2] = (byte)(x >> 40);
		tmp[3] = (byte)(x >> 32);
		tmp[4] = (byte)(x >> 24);
		tmp[5] = (byte)(x >> 16);
		tmp[6] = (byte)(x >>  8);
		tmp[7] = (byte)x;
		Array.Copy(tmp,0,bytes,index,8);
		index +=8;
	}
	private void WriteIn(int x)
	{
		tmp[0] = (byte)(x >> 24);
		tmp[1] = (byte)(x >> 16);
		tmp[2] = (byte)(x >> 8);
		tmp[3] = (byte)x;
		Array.Copy(tmp,0,bytes,index,4);
		index +=4;
	}
	private void WriteIn(short x)
	{
		tmp[0] = (byte)(x >> 8);
		tmp[1] = (byte)x;
		Array.Copy(tmp,0,bytes,index,2);
		index +=2;
	}
	private void WriteIn(byte x)
	{
		bytes[index] =x;
		++index;
	}
	private void WriteIn(byte[] _bytes,int i,int len)
	{
		for(int j=i;j<len;++j)
		{
			bytes[index++] =_bytes[j];
		}
	}
	
	private void WriteIn(string x)
	{
		if (x == null)
		{
			throw new ArgumentNullException ("null string!");
		}
		char[] array = x.ToCharArray ();
        byte[] _bytes = CONST.MyEncoding.GetBytes(array);
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            _bytes = Helper.UTF8ToUTF32(_bytes);
#endif
        Array.Copy(_bytes, 0, bytes, index, _bytes.Length);
		index +=_bytes.Length;
		array = null;
		_bytes = null;
	}
	private void WriteType(EMsgpackType type)
	{
		WriteIn((byte)type);
	}
	
	public void Write(string s)
	{
		s+="\0";
		if(s.Length<0x20)
		{
			byte type= (byte)EMsgpackType.FixRaw;
			type = (byte)(type|(byte)s.Length);
			WriteIn(type);
			WriteIn(s);
		}
		else if(s.Length>0xFFFF)
		{
			WriteType(EMsgpackType.Raw32);
			WriteIn((Int32)s.Length);
			WriteIn(s);
		}
		else
		{
			WriteType(EMsgpackType.Raw16);
			WriteIn((Int16)s.Length);
			WriteIn(s);
		}
	}
	
	public void Write (byte x)
	{
		if (x < 128) 
		{
			WriteIn(x);
		} 
		else 
		{
			WriteIn((byte)0xcc);
			WriteIn(x);
		}
	}
	public void Write (ushort x)
	{
		if (x < 0x100) 
		{
			Write((byte)x);
		} 
		else 
		{
			WriteIn((byte)0xcc);
			WriteIn((byte)(x >> 8));
			WriteIn((byte)x);
		}
	}
	public void Write (char x)
	{
		Write ((ushort)x);
	}

	public void Write (uint x)
	{
		if (x < 0x10000) 
		{
			Write ((ushort)x);
		} 
		else 
		{
			WriteIn((byte)0xce);
			WriteIn((byte)(x >> 24));
			WriteIn((byte)(x >> 16));
			WriteIn((byte)(x >>  8));
			WriteIn((byte)x);
		}
	}

	public void Write (ulong x)
	{
		if (x < 0x100000000) 
		{
			Write ((uint)x);
		} 
		else 
		{
			WriteIn((byte)0xcf);
			WriteIn((byte)(x >> 56));
			WriteIn((byte)(x >> 48));
			WriteIn((byte)(x >> 40));
			WriteIn((byte)(x >> 32));
			WriteIn((byte)(x >> 24));
			WriteIn((byte)(x >> 16));
			WriteIn((byte)(x >>  8));
			WriteIn((byte)x);
		}
	}

	public void Write (sbyte x)
	{
		if (x >= -32 && x <= -1) 
		{
			WriteIn ((byte)(0xe0 | (byte)x));
		} 
		else if (x >= 0 && x <= 127) 
		{
			WriteIn ((byte)x);
		} 
		else 
		{
			WriteIn((byte)0xd0);
			WriteIn((byte)x);
		}
	}

	public void Write (short x)
	{
		if (x >= sbyte.MinValue && x <= sbyte.MaxValue) 
		{
			Write ((sbyte)x);
		} 
		else 
		{
			WriteIn((byte)0xd1);
			WriteIn((byte)(x >> 8));
			WriteIn((byte)x);
		}
	}

	public void Write (int x)
	{
		if (x >= short.MinValue && x <= short.MaxValue) 
		{
			Write ((short)x);
		} 
		else 
		{
			WriteIn((byte)0xd2);
			WriteIn((byte)(x >> 24));
			WriteIn((byte)(x >> 16));
			WriteIn((byte)(x >>  8));
			WriteIn((byte)x);
		}
	}

	public void Write (long x)
	{
		if (x >= int.MinValue && x <= int.MaxValue) 
		{
			Write ((int)x);
		} 
		else 
		{
			WriteIn((byte)0xd3);
			WriteIn((byte)(x >> 56));
			WriteIn((byte)(x >> 48));
			WriteIn((byte)(x >> 40));
			WriteIn((byte)(x >> 32));
			WriteIn((byte)(x >> 24));
			WriteIn((byte)(x >> 16));
			WriteIn((byte)(x >>  8));
			WriteIn((byte)x);
		}
	}

	public void WriteNil ()
	{
		WriteIn((byte)0xc0);
	}
	public void Write (bool x)
	{
		WriteIn((byte)(x ? 0xc3 : 0xc2));
	}
	public void Write (float x)
	{
		byte[] raw = BitConverter.GetBytes (x);

		_tmp[0] = 0xca;
		if (BitConverter.IsLittleEndian) 
		{
			_tmp[1] = raw[3];
			_tmp[2] = raw[2];
			_tmp[3] = raw[1];
			_tmp[4] = raw[0];
		}
		else 
		{
			_tmp[1] = raw[0];
			_tmp[2] = raw[1];
			_tmp[3] = raw[2];
			_tmp[4] = raw[3];
		}
		WriteIn (_tmp,0,5);
	}
	public void Write (double x)
	{
		byte[] raw = BitConverter.GetBytes (x); 

		_tmp[0] = 0xcb;
		if (BitConverter.IsLittleEndian) 
		{
			_tmp[1] = raw[7];
			_tmp[2] = raw[6];
			_tmp[3] = raw[5];
			_tmp[4] = raw[4];
			_tmp[5] = raw[3];
			_tmp[6] = raw[2];
			_tmp[7] = raw[1];
			_tmp[8] = raw[0];
		} 
		else 
		{
			_tmp[1] = raw[0];
			_tmp[2] = raw[1];
			_tmp[3] = raw[2];
			_tmp[4] = raw[3];
			_tmp[5] = raw[4];
			_tmp[6] = raw[5];
			_tmp[7] = raw[6];
			_tmp[8] = raw[7];
		}
		WriteIn(_tmp, 0, 9);
	}
	#endregion
	
	#region reader

	public object Read()
	{
		if(msgpacLength<0 && index==0)
		{//首先读取包长度
			msgpacLength = BitConverter.ToInt32(bytes,index);
			index+=4;
		}
		else
		{
			int x = bytes[index];
			++index;
			if (x >= 0x00 && x <= 0x7f) 
			{
				type = EMsgpackType.PositiveFixNum;
			} 
			else if (x >= 0xe0 && x <= 0xff) 
			{
				type = EMsgpackType.NegativeFixNum;
			} 
			else if (x >= 0xa0 && x <= 0xbf) 
			{
				type = EMsgpackType.FixRaw;
				length = x&0x1F;
			} 
			else if (x >= 0x90 && x <= 0x9f) 
			{
				type = EMsgpackType.FixArray;
			}
			else if (x >= 0x80 && x <= 0x8f) 
			{
				type = EMsgpackType.FixMap;
			} 
			else 
			{
				type= (EMsgpackType)x;
			}
			//暂时只写了FixRaw/Raw16/Raw32
			switch(type)
			{
			case EMsgpackType.FixRaw:
				return ReadFixRaw(length);
			case EMsgpackType.Raw16:
				return ReadRaw16();
			case EMsgpackType.Raw32:
				return ReadRaw32();
			case EMsgpackType.nil:
				return 0;
			case EMsgpackType.False:
				return false;
			case EMsgpackType.True:
				return true;
			case EMsgpackType.Float:
				return ReadFloat();
			case EMsgpackType.Double:
				return ReadDouble();
			case EMsgpackType.NegativeFixNum:
					return((x & 0x1f) - 0x20);
			case EMsgpackType.PositiveFixNum:
					return (x & 0x7f);
			case EMsgpackType.Uint8:
				return ReadUInt8();
			case EMsgpackType.Uint16:
				return ReadUInt16();
			case EMsgpackType.Uint32:
				return ReadUInt32();
			case EMsgpackType.Uint64:
				return ReadUInt64();
			case EMsgpackType.Int8:
				return ReadInt8();
			case EMsgpackType.Int16:
				return ReadInt16();
			case EMsgpackType.Int32:
				return ReadInt32();
			case EMsgpackType.Int64:
				return ReadInt64();
				
			case EMsgpackType.FixArray:
			case EMsgpackType.FixMap:
			case EMsgpackType.Array16:
			case EMsgpackType.Map16:
			case EMsgpackType.Array32:
			case EMsgpackType.Map32:
				//todo FixMap FixArray
				return null;
			}
			return null;
		}
		return null;
	}
	private string ReadFixRaw(int length)
	{
		return ReadString(length);
	}
	private string ReadRaw16()
	{
		int length = ReadInt16();
		return ReadString(length);
	}
	private string ReadRaw32()
	{
		int length = ReadInt32();
		return ReadString(length);
	}
	private float ReadFloat()
	{
		Array.Copy(bytes,index,tmp,0,4);
		index +=4;
		if (BitConverter.IsLittleEndian) 
		{
			tmp1[0] = tmp[3];
			tmp1[1] = tmp[2];
			tmp1[2] = tmp[1];
			tmp1[3] = tmp[0];
			return BitConverter.ToSingle (tmp1, 0);
		} 
		else 
		{
			return BitConverter.ToSingle (tmp, 0);
		}
	}
	private double ReadDouble()
	{
		Array.Copy(bytes,index,tmp,0,8);
		index +=8;
		if (BitConverter.IsLittleEndian) 
		{
			tmp1[0] = tmp[7];
			tmp1[1] = tmp[6];
			tmp1[2] = tmp[5];
			tmp1[3] = tmp[4];
			tmp1[4] = tmp[3];
			tmp1[5] = tmp[2];
			tmp1[6] = tmp[1];
			tmp1[7] = tmp[0];
			return BitConverter.ToDouble (tmp1, 0);
		} 
		else 
		{
			return BitConverter.ToDouble (tmp, 0);
		}
	}
	private ulong ReadUInt64()
	{
		Array.Copy(bytes,index,tmp,0,8);index +=8;
		return ((ulong)tmp[0] << 56) | ((ulong)tmp[1] << 48) | ((ulong)tmp[2] << 40) | ((ulong)tmp[3] << 32) | ((ulong)tmp[4] << 24) | ((ulong)tmp[5] << 16) | ((ulong)tmp[6] << 8) | (ulong)tmp[7];
	}
	private uint ReadUInt32()
	{
		Array.Copy(bytes,index,tmp,0,4);index +=8;
		return ((uint)tmp[0] << 24) | ((uint)tmp[1] << 16) | ((uint)tmp[2] << 8) | (uint)tmp[3];
	}
	private ushort ReadUInt16()
	{
		Array.Copy(bytes,index,tmp,0,2);index +=2;
		return (ushort)(((ushort)tmp[0] << 8) | (ushort)tmp[1]);
	}
	private byte ReadUInt8()
	{
		byte b = bytes[index];
		++index;
		return b;
	}
	
	private long ReadInt64()
	{
		Array.Copy(bytes,index,tmp,0,8);index +=8;
		return ((long)tmp[0] << 56) | ((long)tmp[1] << 48) | ((long)tmp[2] << 40) | ((long)tmp[3] << 32) | ((long)tmp[4] << 24) | ((long)tmp[5] << 16) | ((long)tmp[6] << 8) | (long)tmp[7];
	}
	private int ReadInt32()
	{
		Array.Copy(bytes,index,tmp,0,4);index +=4;
		return (tmp[0] << 24) | (tmp[1] << 16) | (tmp[2] << 8) | tmp[3];
	}
	private short ReadInt16()
	{
		Array.Copy(bytes,index,tmp,0,2);index +=2;
		return (short)((tmp[0] << 8) | tmp[1]);
	}
	private sbyte ReadInt8()
	{
		sbyte sb = (sbyte)bytes[index];
		++index;
		return sb;
	}
	private string ReadString(int len)
	{
		//len长度包涵/0
		//C#读取不需要/0
		byte[] _bytes=new byte[len];
		Array.Copy(bytes,index,_bytes,0,len-1);index +=len;
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            _bytes = Helper.UTF32ToUTF8(_bytes);
#endif
        return CONST.MyEncoding.GetString(_bytes);
	}
	private string ReadString()
	{
		if(msgpacLength-index<=0)
			return string.Empty;
		byte[] _bytes=new byte[msgpacLength-index];
		Array.Copy(bytes,index,_bytes,0,_bytes.Length);index +=_bytes.Length;
#if UNITY_WP8
            //WP8 UTF8编码特殊处理
            _bytes = Helper.UTF32ToUTF8(_bytes);
#endif
        return CONST.MyEncoding.GetString(_bytes);
	}
	#endregion
}
