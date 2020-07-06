using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// 使用反射的序列化
/// </summary>
public class MySerializer
{
	/// <summary>
	/// 数据体最后包涵的变长数组的长度
	/// </summary>
	private static int arrayLen =0;
	private static bool setArrayLen =false;
	#region 序列化
	public static byte[] Serialize(object o)
	{
		using (MemoryStream ms = new MemoryStream ()) 
		{
			Serialize (ms,o);
			return ms.ToArray ();
		}
	}
	public static MemoryStream SerializeStream(object o)
	{
		using (MemoryStream ms = new MemoryStream ()) 
		{
			Serialize (ms,o);
			return ms;
		}
	}
	public static void Serialize(Stream strm,object o)
	{
		if (o != null && o.GetType().IsPrimitive)
		{
			throw new NotSupportedException ();
		}
		else
		{
			arrayLen =0;
			setArrayLen = false;
			SerializeWriter writer = new SerializeWriter (strm);
			Serialize(writer,o);
		}
		
	}
	static void SerializeString(SerializeWriter writer, string s,int length)
	{
		writer.Write(s,length);
	}
	static void SerializeString(SerializeWriter writer, string s)
	{
		writer.Write(s);
	}
	static void Serialize(SerializeWriter writer, object o)
	{
		Serialize(writer,o,o.GetType());
	}
	static void Serialize(SerializeWriter writer, object o,Type type)
	{
		if (type.IsPrimitive)//基础类型
		{
			if (type.Equals (typeof (int))) writer.Write ((int)o);
			else if (type.Equals (typeof (uint))) writer.Write ((uint)o);
			else if (type.Equals (typeof (float))) writer.Write ((float)o);
			else if (type.Equals (typeof (double))) writer.Write ((double)o);
			else if (type.Equals (typeof (long))) writer.Write ((long)o);
			else if (type.Equals (typeof (ulong))) writer.Write ((ulong)o);
			else if (type.Equals (typeof (bool))) writer.Write ((bool)o);
			else if (type.Equals (typeof (byte))) writer.Write ((byte)o);
			else if (type.Equals (typeof (sbyte))) writer.Write ((sbyte)o);
			else if (type.Equals (typeof (short))) writer.Write ((short)o);
			else if (type.Equals (typeof (ushort))) writer.Write ((ushort)o);
			else if (type.Equals (typeof (char))) writer.Write ((ushort)(char)o);
			else throw new NotSupportedException ();
			return;
		}
		if(type.Equals(typeof(Msgpack)))
		{
			writer.Write(((Msgpack)o).GetBytes());
			return;
		}
		if (type.Equals(typeof(string))) 
		{
			SerializeString(writer,(string)o);
			return;
		}
		if (type.IsArray) 
		{
			Array ary = (Array)o;
			if(ary!= null)
			{
				if(ary.Length >0)
				{
					Type at = ary.GetValue(0).GetType();
					if(type.Equals (typeof (sbyte)) || type.Equals (typeof (byte)))
					{
						writer.Write((byte)o);
					}
					else
					{
						for (int i = 0; i < ary.Length; i ++)
							Serialize(writer, ary.GetValue(i),at);
					}
				}
			}
			return;
		}
 
 		ReflectionCache Cache = new ReflectionCache(type);
		int lastOrder =int.MinValue;
		for(int i=0;i<Cache.FieldList.Count;++i)
		{
			if(Cache.FieldList[i].order > lastOrder)
			{
				lastOrder = Cache.FieldList[i].order ;//test
				Type t =Cache.FieldList[i].info.FieldType;
				if(t.Equals(typeof(string)))
				{
					int length = Cache.FieldList[i].Length;
					if(length>0)
					{
						SerializeString(writer,(string)Cache.FieldList[i].info.GetValue(o),length);
					}
					else
					{
						SerializeString(writer,(string)Cache.FieldList[i].info.GetValue(o));
					}
				}
				else
				{
					Serialize(writer,Cache.FieldList[i].info.GetValue(o),t);
				}
			}
			else
			{
				throw new Exception("not found this order member: "+i);
			}
		}
	}
	#endregion
	
	#region 反序列化
	public static T Deserialize<T>(byte[] bytes)
	{
		return Deserialize<T>(bytes,0,bytes.Length);
	}
	public static T Deserialize<T>(byte[] buf, int offset, int size)
	{
		using (MemoryStream ms = new MemoryStream (buf, offset, size)) 
		{
			return Deserialize<T>(ms);
		}
	}
	static T Deserialize<T>(Stream strm)
	{
		if (typeof (T).IsPrimitive)
				throw new NotSupportedException ();
		arrayLen =0;
		setArrayLen = false;
		SerializeReader reader = new SerializeReader (strm);
		return (T)Deserialize(reader,typeof(T));
	}
	static object DeserializePrimitive(SerializeReader reader,Type type)
	{
		if (type.IsPrimitive) 
		{
			if (type.Equals (typeof (int))) 
			{
				int i32 =  reader.ReadInt32();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = i32;
				}
				return i32;
			}
			else if (type.Equals (typeof (uint)))
			{
				uint ui32 =  reader.ReadUInt32();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = (int)ui32;
				}
				return ui32;
			}
			else if (type.Equals (typeof (long)))
			{
				long i64 =  reader.ReadInt64();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = (int)i64;
				}
				return i64;
			}
			else if (type.Equals (typeof (ulong))) 
			{
				ulong ui64 =  reader.ReadUInt64();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = (int)ui64;
				}
				return ui64;
			}
			else if (type.Equals (typeof (byte)))
			{
				byte b8 =  reader.ReadByte();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = b8;
				}
				return b8;
			}
			else if (type.Equals (typeof (sbyte))) 
			{
				sbyte sb8 =  reader.ReadSbyte();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = sb8;
				}
				return sb8;
			}
			else if (type.Equals (typeof (short))) 
			{
				short i16 =  reader.ReadInt16();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = i16;
				}
				return i16;
			}
			else if (type.Equals (typeof (ushort)))
			{
				ushort ui16 =  reader.ReadUInt16();
				if(setArrayLen)
				{
					setArrayLen = false;
					arrayLen = ui16;
				}
				return ui16;
			}
			else if (type.Equals (typeof (float)))
				return reader.ReadFloat();
			else if (type.Equals (typeof (double))) 
				return reader.ReadDouble();
			else if (type.Equals (typeof (bool))) 
				return reader.ReadBool();
			else if (type.Equals (typeof (char))) 
				return (char)reader.ReadUInt16();
			throw new NotSupportedException ("NotSupported this type "+ type.ToString());
		}
		throw new NotSupportedException("NotSupported this type "+ type.ToString());
	}
	static string DeserializeString(SerializeReader reader)
	{
		return reader.ReadString();
	}
	static string DeserializeString(SerializeReader reader,int length)
	{
		return reader.ReadString(length);
	}
	static object Deserialize(SerializeReader reader,Type type)
	{
		if(type.IsPrimitive)
	    {
	    	return DeserializePrimitive(reader,type);
	    }
	    else if(type.Equals(typeof(string)))
	    {
	    	return DeserializeString(reader);
	    }
		else if(type.Equals(typeof(Msgpack)))
		{
			byte[] bytes = reader.ReadBytes((int)reader.Strm.Length);
			MemoryStream ms = new MemoryStream(bytes);
			return new Msgpack(ms);
		}
		object o = Activator.CreateInstance(type,true);
		//object o = Activator.CreateInstance("test", false, BindingFlags.CreateInstance, null, new object[] { dataSource }, null, null);
		//object o = Activator.CreateInstance(type

		ReflectionCache Cache =new ReflectionCache(type);
		int lastOrder =int.MinValue;
		for(int i=0;i<Cache.FieldList.Count;++i)
		{
			if(Cache.FieldList[i].order > lastOrder)
			{
				setArrayLen = Cache.FieldList[i].isArrayLen;
				lastOrder =Cache.FieldList[i].order;
				FieldInfo fileInfo = Cache.FieldList[i].info;
				Type t =Cache.FieldList[i].info.FieldType;
				if(t.Equals(typeof(string)))
				{
					if(Cache.FieldList[i].Length>0)
					{
						fileInfo.SetValue(o,DeserializeString(reader,Cache.FieldList[i].Length));
					}
					else
					{
						fileInfo.SetValue(o,DeserializeString(reader));
					}
				}
				else
				{
					if(t.IsArray)
					{
						int len = (Cache.FieldList[i].Length>0)?Cache.FieldList[i].Length:MySerializer.arrayLen;
						if(len>0)
						{
							Type at = t.GetElementType();
							if(at.Equals (typeof (sbyte)) || at.Equals (typeof (byte)))
							{
								byte[] ary = reader.ReadBytes(len);
								fileInfo.SetValue(o,ary);
							}
							else
							{
								Array ary = Array.CreateInstance (at,len);
								for (int j = 0; j < ary.Length; ++j)
								{
									ary.SetValue(Deserialize(reader,at),j);
								}
								fileInfo.SetValue(o,ary);
							}
						}
						else
						{
							//throw new Exception("the array length is error."+t);
							continue;
						}
					}
					else
						fileInfo.SetValue(o,Deserialize(reader,t));
				}
			}
			else
			{
				throw new Exception("Not found this order member: "+i+","+Cache.FieldList[i].order+","+lastOrder);
			}
		}
		return o;
	}
	#endregion
}
