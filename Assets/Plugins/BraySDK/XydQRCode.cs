using UnityEngine;
using System.Collections;
using BraySDK;


/// <summary>
/// 示例代码：生成二维码
/// </summary>
public class XydQRCode : MonoBehaviour {


	#region  生成二维码
	/// <summary>
	/// 显示二维码
	/// </summary>
	public Texture2D QRTextture;
	#endregion


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){

		GUILayout.BeginArea(new Rect(10, 10, 600, 800));

		//========== 仅PC客户端使用===========
		if (GUILayout.Button("生成二维码登录",GUILayout.Width(200), GUILayout.Height(60)))
		{
			Debug.Log("U3D:生成二维码登录");

			//控制当前二维码过期：用一个定时器刷新二维码，可以一分钟，时间你们自己定，测试定为8秒
			InvokeRepeating("UpdateQRCode", 0.1f, 60.0f);
		}
		
		//显示生成的二维码
		if (QRTextture != null) {
			GUI.DrawTexture(new Rect(220, 10, 350, 350), QRTextture);
		}

		GUILayout.EndArea();

	}


	void UpdateQRCode()
	{
		Debug.Log("U3D:UpdateQRCode");
		
		//生成二维码
		string code = "";
		QRTextture = QRCodeUtils.RenderQRCode(350, 350, ref code);
		
		if (QRTextture != null && code != null)
		{
			Debug.Log("code:" + code);
			//上报当前code和时间戳到游戏服务器保存
			//...
			Debug.Log("上报完成");
		}
		
	}


	void ReceiveGameServer()
	{
		bool isLogin = true;
		if (isLogin) {
			//游戏服务器通知登陆成功
			
			//关掉定时器
			this.CancelInvoke();
			
			//进入选取富界面
			//...

		}
	}



}
