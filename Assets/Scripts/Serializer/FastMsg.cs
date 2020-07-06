using System;
using System.Collections.Generic;
using System.Text;

// 功能：快速读写消息结构

class FastMsg
{
    public static _Ty Deserialize<_Ty>(byte[] bytes, int nSize = -1) where _Ty : ISerializable, new()
    {
        if (nSize < 0)
            nSize = bytes.Length;
        CSerialize ar = CSerialize.ReadStream(bytes, nSize);
        ar.ResetStream(SerializeType.read, bytes, nSize);
        _Ty obj = new _Ty();
        obj.Serialize(ar);
        return obj;
    }
    public static void SendGameMsg<_Ty>(_Ty msg) where _Ty : ISerializable
    {
        CSerialize ar = CSerialize.WriteStream();
        msg.Serialize(ar);
    }
}
