using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CONST
{
#if UNITY_WP8
    //wp8仅仅支持UTF8和UTF16；服务器端用的是UFT32；所以在WP8中用MyEncoding都需要特殊处理；
    public static readonly System.Text.Encoding MyEncoding = System.Text.Encoding.UTF8;
#else
    public static readonly System.Text.Encoding MyEncoding = System.Text.Encoding.UTF32;
#endif
}

