using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Helper
{

    /// <summary>
    /// 去掉字符串后面的"\0"
    /// Trims the end.
    /// </summary>
    /// <returns>
    /// The end.
    /// </returns>
    /// <param name='s'>
    /// S.
    /// </param>
    public static string TrimEnd(string s)
    {
        int end = s.IndexOf("\0");
        if (end >= 0)
            return s.Substring(0, end);//去掉后面多余的"\0";
        return s;
    }

    /// <summary>
    /// 将日期转为秒数(以0时区为准,参数为中国时区时间)
    /// </summary>
    /// <param name="dTime"></param>
    /// <returns>返回UTC秒数</returns>
    public static long DataToSeconds(System.DateTime dTime)
    {
        if (null == dTime)
            return 0;
        System.DateTime dt = new System.DateTime(1970, 1, 1, 8, 0, 0);
        System.TimeSpan dt1 = dTime - dt;
        return dt1.Ticks / 10000000;
    }

    #region 字符编码转换

    public static byte[] UTF8ToUTF32(byte[] utf8)
    {
        return UTF8ToUTF32(utf8, int.MaxValue);
    }
    public static byte[] UTF8ToUTF32(byte[] utf8, int maxLength)
    {
        List<byte> bytes = new List<byte>();
        uint u = 0;
        int index = 0;
        int len = 0;
        while (index < utf8.Length && bytes.Count < maxLength)
        {
            u = 0;
            len = UTF8ToUTF32(utf8, index, out u);
            index += len;
            if (len > 0)
            {
                bytes.AddRange(BitConverter.GetBytes(u));
            }
            else
            {
                return null;
            }
        }
        return bytes.ToArray();
    }
    public static byte[] UTF32ToUTF8(byte[] utf32)
    {
        if (utf32.Length % 4 != 0)
        {
            return null;
        }
        List<byte> bytes = new List<byte>();
        byte[] temp = null;
        uint u = 0;
        for (int i = 0; i < utf32.Length / 4; i += 4)
        {
            temp = null;
            u = BitConverter.ToUInt32(utf32, i);
            temp = UTF32ToUTF8(u);
            if (temp == null)
            {
                return null;
            }
            bytes.AddRange(temp);
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// 转换UTF32编码到UTF8编码
    /// </summary>
    /// <param name="UTF32">要转换的UTF32编码</param>
    /// <returns>存储UTF8编码的字节数组</returns> 
    public static byte[] UTF32ToUTF8(uint UTF32)
    {
        uint[] CodeUp =
            {
                0x80,			    //U+00000000  U+0000007F
		        0x800,			//U+00000080  U+000007FF
		        0x10000,		//U+00000800  U+0000FFFF
		        0x200000,		//U+00010000  U+001FFFFF
		        0x4000000,	//U+00200000  U+03FFFFFF
		        0x80000000	//U+04000000  U+7FFFFFFF
            };

        // 根据UTF32编码范围确定对应的UTF8编码字节数
        int len = -1;
        for (int i = 0; i < CodeUp.Length; i++)
        {
            if (UTF32 < CodeUp[i])
            {
                len = i + 1;
                break;
            }
        }

        if (len == -1) return null;   // 无效的UTF32编码

        // 转换为UTF8编码
        byte[] UTF8 = new byte[len];
        byte[] Prefix = { 0, 0xC0, 0xE0, 0xF0, 0xF8, 0xFC };
        for (int i = len - 1; i > 0; --i)
        {
            UTF8[i] = (byte)((UTF32 & 0x3F) | 0x80);
            UTF32 >>= 6;
        }

        UTF8[0] = (byte)(UTF32 | Prefix[len - 1]);

        return UTF8;
    }

    /// <summary>
    /// 转换UTF8编码到UTF32编码
    /// </summary>
    /// <param name="UTF8">UTF8编码的字节数组</param>
    /// <param name="index">要转换的起始索引位置</param>
    /// <param name="UTF32">要输出的UTF32编码</param>
    /// <returns>字节数组中参与编码转换的字节长度</returns> 
    public static int UTF8ToUTF32(byte[] UTF8, int index, out uint UTF32)
    {
        if (index < 0 || index >= UTF8.Length)
        {
            UTF32 = 0xFFFFFFFF;
            return 0;
        }

        byte b = UTF8[index];
        if (b < 0x80)
        {
            UTF32 = b;
            return 1;
        }

        if (b < 0xC0 || b > 0xFD)
        {	// 非法UTF8
            UTF32 = 0xFFFFFFFF;
            return 0;
        }

        int len;
        if (b < 0xE0)
        {
            UTF32 = (uint)(b & 0x1F);
            len = 2;
        }
        else if (b < 0xF0)
        {
            UTF32 = (uint)(b & 0x0F);
            len = 3;
        }
        else if (b < 0xF8)
        {
            UTF32 = (uint)(b & 7);
            len = 4;
        }
        else if (b < 0xFC)
        {
            UTF32 = (uint)(b & 3);
            len = 5;
        }
        else
        {
            UTF32 = (uint)(b & 1);
            len = 6;
        }

        if (index + len > UTF8.Length)
        {   // 非法UTF8
            UTF32 = 0xFFFFFFFF;
            return 0;
        }

        for (int i = 1; i < len; i++)
        {
            b = UTF8[index + i];
            if (b < 0x80 || b > 0xBF)
            {	// 非法UTF8
                UTF32 = 0xFFFFFFFF;
                return 0;
            }

            UTF32 = (uint)((UTF32 << 6) + (b & 0x3F));
        }

        return len;
    }

    /// <summary>
    /// 转换UTF8编码到Unicode字符串
    /// </summary>
    /// <param name="UTF8">用于转换的UTF8编码字节数组</param>
    /// <param name="index">要转换的起始索引位置</param>
    /// <param name="count">要转换的字节数</param>
    /// <returns>转换后的Unicode字符串</returns> 
    public static string UTF8ToString(byte[] UTF8, int index, int count)
    {
        if (index < 0 || index >= UTF8.Length || count == 0)
        {
            return null;
        }

        // 计算实际能够转换的字节数
        if (count < 0 || index + count > UTF8.Length)
        {
            count = UTF8.Length - index;
        }

        StringBuilder sb = new StringBuilder();
        int i = 0;
        do
        {
            uint UTF32;
            int len = UTF8ToUTF32(UTF8, index, out UTF32);
            if (len == 0 || i + len > count)
            {   // 转换失败
                return null;
            }

            string Unicode = UTF32ToString(UTF32);
            if (Unicode == null)
            {   // 转换失败
                return null;
            }

            sb.Append(Unicode);

            i += len;
            index += len;
        } while (i < count);

        return sb.ToString();
    }

    /// <summary>
    /// 转换UTF32编码到Unicode字符串
    /// </summary>
    /// <param name="UTF32">要转换的UTF32编码</param>
    /// <returns>转换后的Unicode字符串</returns> 
    public static string UTF32ToString(uint UTF32)
    {
        StringBuilder sb = new StringBuilder(2);
        if (UTF32 <= 0xFFFF)
        {   // 基本平面字符
            sb.Append(Convert.ToChar(UTF32));
            return sb.ToString();
        }
        else if (UTF32 <= 0xEFFFF)
        {   // 对于辅助平面字符，使用代理项对
            sb.Append(Convert.ToChar(0xD800 + (UTF32 >> 10) - 0x40));
            sb.Append(Convert.ToChar(0xDC00 + (UTF32 & 0x03FF)));
            return sb.ToString();
        }
        else
        {   // 超出编码范围
            return null;
        }
    }

    /// <summary>
    /// 转换Unicode字符串到UTF32编码
    /// </summary>
    /// <param name="Unicode">用于转换的Unicode字符串</param>
    /// <param name="index">要转换的起始索引位置</param>
    /// <param name="UTF32">要输出的UTF32编码</param>
    /// <returns>参与编码转换的字符个数</returns> 
    public static int StringToUTF32(string Unicode, int index, out uint UTF32)
    {
        if (index < 0 || index >= Unicode.Length)
        {
            UTF32 = 0xFFFFFFFF;
            return 0;
        }

        ushort CodeA = Convert.ToUInt16(Unicode[index]);
        if (CodeA >= 0xD800 && CodeA <= 0xDFFF)
        {   // 代理项区域（Surrogate Area）
            if (CodeA < 0xDC00 && index + 2 <= Unicode.Length)
            {
                ushort CodeB = Convert.ToUInt16(Unicode[index + 1]);
                if (CodeB >= 0xDC00 && CodeB <= 0xDFFF)
                {
                    UTF32 = (uint)((CodeB & 0x03FF) + (((CodeA & 0x03FF) + 0x40) << 10));
                    return 2;
                }
            }

            // 非法UTF16
            UTF32 = 0xFFFFFFFF;
            return 0;
        }
        else
        {
            UTF32 = CodeA;
            return 1;
        }
    }

    /// <summary>
    /// 转换Unicode字符串到UTF8编码
    /// </summary>
    /// <param name="Unicode">要转换的Unicode字符串</param>
    /// <param name="index">要转换的起始索引位置</param>
    /// <param name="count">要转换的字符个数</param>
    /// <returns>存储UTF8编码的字节数组</returns> 
    public static byte[] StringToUTF8(string Unicode, int index, int count)
    {
        if (string.IsNullOrEmpty(Unicode) || index < 0 || index >= Unicode.Length || count == 0)
        {
            return null;
        }

        // 计算实际能够转换的字符数
        if (count < 0 || index + count > Unicode.Length)
        {
            count = Unicode.Length - index;
        }

        // 基本平面字符，其UTF8编码不超过3个字节，扩展平面字符则不会超过6个字节
        byte[] UTF8Package = new byte[Unicode.Length * 3];
        int AppendIndex = 0;
        int i = 0;
        do
        {
            uint UTF32;
            int len = StringToUTF32(Unicode, index, out UTF32);
            if (len == 0 || i + len > count)
            {   // 转换失败
                return null;
            }

            byte[] UTF8 = UTF32ToUTF8(UTF32);
            if (UTF8 == null)
            {   // 转换失败
                return null;
            }

            UTF8.CopyTo(UTF8Package, AppendIndex);
            AppendIndex += UTF8.Length;

            i += len;
            index += len;
        } while (i < count);

        // 调整到实际大小
        Array.Resize(ref UTF8Package, AppendIndex);

        return UTF8Package;
    }
    #endregion
}

