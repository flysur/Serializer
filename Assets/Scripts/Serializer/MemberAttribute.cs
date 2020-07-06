using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field,AllowMultiple = false, Inherited = true)]
public class MemberAttribute : Attribute
{
	/// <summary>
	/// The order.
	/// 从0开始
	/// </summary>
	public int order;
	/// <summary>
	/// The length.
	/// 字符串、数组的Length
	/// </summary>
	public int Length;
	public bool IsArrayLength;
	public MemberAttribute(int order)
	{
		this.order = order;
		this.Length=0;
		IsArrayLength =false;
	}
	public MemberAttribute(int order,int Length)
	{
		this.order = order;
		this.Length=Length;
		IsArrayLength =false;
	}
	public MemberAttribute(int order,string array)
	{
		this.order = order;
		this.Length=0;
		IsArrayLength =true;
	}
}