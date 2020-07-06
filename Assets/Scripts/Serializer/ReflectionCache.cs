using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public struct MInfo
{
	/// <summary>
	/// The length.
	/// 如果该属性为字符串，则表示字符串的长度；
	/// 如果该属性为数组，则表示数组的长度；
	/// </summary>
	public int Length;
	//public PropertyInfo info;
	public FieldInfo info;
	
	public int order;
	public bool isArrayLen;
}
public class ReflectionCache
{
	const BindingFlags FieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
	public IList<MInfo> FieldList { get; private set; }

	public ReflectionCache (Type t)
	{
		//PropertyInfo[] props = t.GetProperties(FieldBindingFlags); 
		FieldInfo[] Fields = t.GetFields(FieldBindingFlags); 
		MInfo mInfo = new MInfo();
		List<MInfo>  list = new List<MInfo>(Fields.Length);
		for (int i = 0; i < Fields.Length;++i)
		{
			var atts = Fields[i].GetCustomAttributes(typeof(MemberAttribute), false);
            foreach(MemberAttribute m in atts)
            {
                mInfo.Length = m.Length;
                mInfo.info = Fields[i];
                mInfo.order = m.order + GetOrderOffset(Fields[i], t);
                mInfo.isArrayLen = m.IsArrayLength;
                list.Add(mInfo);
                break;
			}
            atts = null;
		}
		list.Sort(CompareByOrder);
		FieldList = list;
        Fields = null;
	}
	private static int CompareByOrder(MInfo m1, MInfo m2)
    {
		return (m1.order == m2.order)?0: ( (m1.order>m2.order)?1:-1 );
    }
	const int maxFields =200;
	const int maxFather =30;
	public int GetOrderOffset(FieldInfo fi,Type ReflectionT)
	{
		int index =0;
		if(fi.DeclaringType != ReflectionT)
		{
			Type t = fi.DeclaringType;
			while(t != typeof(System.Object))
			{
				++index;
				t = t.BaseType;
			}
			return maxFields*(index-maxFather);
		}
		return 0;
	}
}