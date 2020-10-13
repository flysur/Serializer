using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using System.Threading;
using UnityEngine.UI;

public class QRCodeScan : MonoBehaviour {

    #region 扫描二维码

    /// <summary>
    /// 摄像头实时显示的画面
    /// </summary>
    private WebCamTexture m_webCameraTexture;
    
    private int reqW = 640;
    private int reqH = 480;

    public RawImage textureCamera;
    public Transform lineObj;


    /// <summary>
    /// 显示烧苗结果
    /// </summary>
    private string resultText;

    #endregion

    private bool m_bScan = false;
    private List<Camera> m_cameras = new List<Camera>();

    private Thread qrThread;
    // Use this for initialization
    void Start () {
		
	}
#if Windows_Intranet
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 600, 800));
        if (m_bScan)
        {
            if (GUILayout.Button("退出扫描", GUILayout.Width(200), GUILayout.Height(60)))
            {
                Debug.Log("U3D:退出扫描");;
                this.StopCamera();
            }

            //显示扫描结果
            if (resultText != null && resultText.Length > 0)
            {
                GUI.Label(new Rect(0, 380, Screen.width, Screen.height), "扫描结果:" + resultText);
            }
            else
            {
                GUI.Label(new Rect(0, 380, Screen.width, Screen.height), "No Data" + paras);
            }
        }
        GUILayout.EndArea();

    }  
#endif


    string lastText = "";

    private bool getQRCode = false;

    void ScanQRCode()
    {
        Debug.Log("Scanning!");
        var color32s = Array.ConvertAll<Color, Color32>(color, info => (Color32)info);
        Debug.Log("color32s.Length = " + color32s.Length);
        var res = barcodeReader.Decode(color32s, w, h);
        if (res != null)
            resultText = res.Text;
        Debug.Log("res = " + res);
        if (!string.IsNullOrEmpty(resultText))
        {
            Debug.Log("resultText = " + resultText);
            if (resultText.StartsWith("xyd"))
            {
                //一个新的扫描结果
                if (resultText != lastText)
                {
                    m_bScan = false;
                    lastText = resultText.Substring(3);
                    getQRCode = true;
                }
            }
        }
        if (qrThread != null)
        {
            qrThread.Abort();
            qrThread = null;
        }
            
       

    }

    private float m_spaceTime = 0;
    private int w;
    private int h;
    private Color[] color;
    private BarcodeReader barcodeReader;
    // Update is called once per frame
    void Update () {
        if (m_bScan)
        {
            ShowToUI();
            m_spaceTime += Time.deltaTime;
            if (m_spaceTime >= 1f)
            {
                m_spaceTime = 0;
                if (m_webCameraTexture != null)
                {
                    w = (int) (m_webCameraTexture.width*0.8f);
                    h = (int) (m_webCameraTexture.height*0.8f);
                    color = null;
                    color = m_webCameraTexture.GetPixels((m_webCameraTexture.width - w)/2,
                        (m_webCameraTexture.height - h)/2, w, h);
                    qrThread = new Thread(ScanQRCode);
                    if (barcodeReader == null)
                    {
                        barcodeReader = new BarcodeReader { AutoRotate = true, TryInverted = true };
                    }
                    qrThread.Start();
                }
            }
        }
        if (getQRCode)
        {
            //NetManager.Instance.SendQrLoginMsg(GameApp.Instance.UserID, GameApp.Instance.PassWord, lastText);
            StopCamera();
            getQRCode = false;
            //GameApp.Instance.QRCodeQuit = true;
            //UILoginController.Instance.uiLogin.SDKQuit();
        }
    }

    public void StartScanQR()
    {
        getQRCode = false;
        this.gameObject.SetActive(true);
        //Debug.Log("启动扫码界面！");
        //lineObj.transform.localPosition=new Vector3(0,172,0);
        //lineObj.transform.DOLocalMove(new Vector3(0, -210, 0), 2.75f).SetLoops(-1,LoopType.Yoyo);
        StartCoroutine(Scan());
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        StartScanQR();
#endif
    }

    public IEnumerator Scan()
    {
        resultText=String.Empty;
        w = 0;
        h = 0;
        color = null;
        barcodeReader = null;
        Debug.Log("申请摄像头权限");
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("摄像头权限申请成功！");
            //调用摄像头并将画面显示在屏幕RawImage上
            WebCamDevice[] tDevices = WebCamTexture.devices;    //获取所有摄像头
            if (tDevices != null && tDevices.Length > 0)
            {
                textureCamera.gameObject.SetActive(true);
                string tDeviceName = tDevices[0].name;  //获取第一个摄像头，用第一个摄像头的画面生成图片信息
                //m_webCameraTexture = new WebCamTexture(tDeviceName, Screen.width, Screen.height);//名字,宽,高
                //m_webCameraTexture = new WebCamTexture(tDeviceName, 640, 480);//名字,宽,高

                if (Screen.width < 1280)
                {
                    reqW = 640;
                    reqH = 480;
                }
                else
                {
                    reqW = 1280;
                    reqH = 720;
                }
                ////// 在 Unity界面上显示  扫描的内容
                ////m_webCameraTexture = new WebCamTexture(tDeviceName);
                ////m_webCameraTexture.requestedWidth = reqW;
                ////m_webCameraTexture.requestedHeight = reqH;
                if (textureCamera == null)
                {
                    Debug.LogError("textureCamera is empty");
                }
                m_webCameraTexture = new WebCamTexture(tDeviceName, reqW, reqH);//名字,宽,高
                textureCamera.texture = m_webCameraTexture;   //赋值图片信息

                m_webCameraTexture.Play();  //开始实时显示

                //                 m_cameras.Clear();
                //                 var cam = Camera.allCameras;
                //                 foreach (var item in cam)
                //                 {
                //                     if (item.gameObject.activeSelf)
                //                     {
                //                         item.gameObject.SetActive(false);
                //                         m_cameras.Add(item);
                //                     }
                //                 }
                m_bScan = true;
                getQRCode = false;
                Debug.Log("可以扫码了！");
            }
            else
            {
                Debug.Log("未检测到摄像头");
            }

        }
        else
        {
            Debug.Log("无访问摄像头权限!");
            //UIManager.Instance.CreateTip(true, 268);
        }
    }

    #region  摄像头相关
    string paras = "xx";

    /// <summary>
    /// //调整角度
    /// </summary>
    public void ShowToUI()
    {
        if (m_webCameraTexture != null)
        {

            ////if (Application.platform == RuntimePlatform.IPhonePlayer)
            ////{
            ////    mRawImage.rectTransform.localScale = new Vector3(1, -1, 0);
            ////}
            float scaleY = m_webCameraTexture.videoVerticallyMirrored ? -1f : 1f; // Find if the camera is mirrored or not  
            textureCamera.transform.localScale = new Vector3(1560f, scaleY*960f, 1f); // Swap the mirrored camera  


            int orient = -m_webCameraTexture.videoRotationAngle;
            textureCamera.transform.localEulerAngles = new Vector3(0, 0, orient);

            paras = scaleY + " | " + orient;
        }
    }


    public void StopCamera()
    {
        if (qrThread != null)
        {
            qrThread.Abort();
            qrThread = null;
        }
        //lineObj.transform.DOKill();
        resultText = String.Empty;
        w = 0;
        h = 0;
        color = null;
        barcodeReader = null;
        this.gameObject.SetActive(false);
        textureCamera.gameObject.SetActive(false);
        m_bScan = false;
        if (m_webCameraTexture != null)
        {
            m_webCameraTexture.Stop();
            UnityEngine.Object.Destroy(m_webCameraTexture);
            m_webCameraTexture = null;
            m_bScan = false;
        }
    }
    #endregion

}
