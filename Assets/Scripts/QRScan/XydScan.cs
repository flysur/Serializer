using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BraySDK;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode.Internal;

/// <summary>
/// 示例代码：扫描二维码
/// </summary>
public class XydScan : MonoBehaviour
{


    #region 扫描二维码
    //摄像头实时显示的画面
    private WebCamTexture m_webCameraTexture;

    private int reqW = 640;
    private int reqH = 480;

    private Thread qrThread;
    private bool m_bQuit = false;

    /// <summary>
    /// 定义一个用于存储调用电脑或手机摄像头画面的RawImage
    /// </summary>
    public RawImage rawImageCamera;

    public AspectRatioFitter fit;

    /// <summary>
    /// 显示扫描结果
    /// </summary>
    private string resultText;

    #endregion

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ShowToUI();
    }

    [SerializeField]
    private Texture m_texture;

    void OnGUI()
    {


        // GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 200, 200), m_texture);

        GUILayout.BeginArea(new Rect(10, 10, 600, 800));

        if (m_bScan)
        {
            if (GUILayout.Button("退出扫描", GUILayout.Width(200), GUILayout.Height(60)))
            {
                Debug.Log("U3D:退出扫描");


                this.CancelInvoke();
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

    string lastText = "";

    void ScanQRCode()
    {
        Debug.Log("U3D:ScanQRCode");
        var barcodeReader = new BarcodeReader { AutoRotate = true, TryInverted = true };

        while (true)
        {
            if (m_bQuit)
                break;

            try
            {
                if (m_webCameraTexture != null)
                {
                    int w = (int)(m_webCameraTexture.width * 0.8);
                    int h = (int)(m_webCameraTexture.height * 0.8);
                    var color = m_webCameraTexture.GetPixels((m_webCameraTexture.width - w) / 2, (m_webCameraTexture.height - h) / 2, w, h);
                    List<Color32> color32s = new List<Color32>();
                    foreach (var item in color)
                    {
                        color32s.Add(item);
                    }
                    var res = barcodeReader.Decode(color32s.ToArray(), w,
                        h);
                    if (res != null)
                        resultText = res.Text;
                    if (string.IsNullOrEmpty(resultText))
                    {
                        if (resultText.StartsWith("xyd"))
                        {
                            //一个新的扫描结果
                            if (resultText != lastText)
                            {
                                lastText = resultText.Substring(3);
                                //NetManager.Instance.SendQrLoginMsg(GameApp.Instance.UserID, GameApp.Instance.PassWord, lastText);
                                Debug.LogError("验证完成");
                                StopCamera();
                            }
                        }

                    }

                    Thread.Sleep(200);
                }
            }
            catch
            {
            }
        }
    }

    private bool m_bScan = false;
    private List<Camera> m_cameras = new List<Camera>();

    #region  摄像头相关
    /// <summary>
    /// 调用摄像头
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);


        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            resultText = "";
            //调用摄像头并将画面显示在屏幕RawImage上
            WebCamDevice[] tDevices = WebCamTexture.devices;    //获取所有摄像头
            if (tDevices != null && tDevices.Length > 0)
            {
                rawImageCamera.gameObject.SetActive(true);
                m_bScan = true;
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
                m_webCameraTexture = new WebCamTexture(tDeviceName, reqW, reqH);//名字,宽,高
                rawImageCamera.texture = m_webCameraTexture;   //赋值图片信息

                m_webCameraTexture.Play();  //开始实时显示

                m_cameras.Clear();
                var cam = Camera.allCameras;
                foreach (var item in cam)
                {
                    if (item.gameObject.activeSelf)
                    {
                        item.gameObject.SetActive(false);
                        m_cameras.Add(item);
                    }
                }

                //循环扫描
                //InvokeRepeating("ScanQRCode", 1f, 1f);
                m_bQuit = false;
                qrThread = new Thread(ScanQRCode);
                qrThread.Start();
            }
            else
            {
                Debug.Log("未检测到摄像头");
            }
        }
        else
        {
            Debug.Log("无访问摄像头权限");
        }
    }

    string paras = "xx";

    /// <summary>
    /// //调整角度
    /// </summary>
    public void ShowToUI()
    {
        if (m_webCameraTexture != null)
        {
            float ratio = (float)m_webCameraTexture.width / (float)m_webCameraTexture.height;
            fit.aspectRatio = ratio; // Set the aspect ratio  

            Debug.Log("ratio: " + ratio);

            ////if (Application.platform == RuntimePlatform.IPhonePlayer)
            ////{
            ////    mRawImage.rectTransform.localScale = new Vector3(1, -1, 0);
            ////}
            float scaleY = m_webCameraTexture.videoVerticallyMirrored ? -1f : 1f; // Find if the camera is mirrored or not  
            rawImageCamera.rectTransform.localScale = new Vector3(1f, scaleY, 1f); // Swap the mirrored camera  


            int orient = -m_webCameraTexture.videoRotationAngle;
            rawImageCamera.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

            paras = ratio + " | " + scaleY + " | " + orient;
        }
    }


    public void StopCamera()
    {
        rawImageCamera.gameObject.SetActive(false);
        m_bScan = false;
        foreach (var item in m_cameras)
        {
            if (item != null && item.gameObject != null)
            {
                item.gameObject.SetActive(true);
            }
        }

        if (m_webCameraTexture != null)
        {
            if (qrThread != null)
                qrThread.Abort();
            m_bQuit = true;
            m_webCameraTexture.Stop();
            UnityEngine.Object.Destroy(m_webCameraTexture);
            m_webCameraTexture = null;
        }
    }

    #endregion

}
