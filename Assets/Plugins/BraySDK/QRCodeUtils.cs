using UnityEngine;
using System.Collections;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;

namespace BraySDK
{

    public class QRCodeUtils
    {


        #region  生成

        //定义一个UI，来接收图片
        //	public RawImage QRImage;

        /// <summary>
        /// 随机数产生
        /// </summary>
        /// <returns>The validate N.</returns>
        /// <param name="intLength">Int length.</param>
        private static string GetValidateNO(int intLength)
        {
            string strMyCode = "";
            string[] baseCode = new string[32] { "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            int myrdLength = 32;

            if (intLength < 4)
            {
                intLength = 4;
            }

            if (intLength > 32)
            {
                intLength = 32;
            }

            object[] bytes = new object[intLength];
            //Random rd = new Random();
            for (int i = 0; i < intLength; i++)
            {
                //int myrd = rd.Next(myrdLength);
                int myrd = UnityEngine.Random.Range(0, myrdLength);
                bytes[i] = baseCode[myrd];
                strMyCode = strMyCode + bytes[i].ToString();

                //rd = new Random(myrd * unchecked((int)DateTime.Now.Ticks) + i);
            }

            return strMyCode.ToUpper();
        }

        /// <summary>
        /// Encode the specified textForEncoding, width and height.
        /// </summary>
        /// <param name="textForEncoding">Text for encoding.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        private static Color32[] Encode(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width
                }
            };
            return writer.Write(textForEncoding);
        }

        /// <summary>
        /// Renders the QR code.
        /// </summary>
        /// <returns>The QR code.</returns>
        /// <param name="width">二维码图片宽</param>
        /// <param name="height">二维码图片高</param>
        public static Texture2D RenderQRCode(int width, int height, ref string codeText)
        {

            //需要返回其它对象可以在这里修改

            Texture2D encoded = new Texture2D(width, height);

            //int RandKey = UnityEngine.Random.Range (1000, 9999);
            //string textForEncoding = "xyd"+RandKey.ToString ();
            string textForEncoding = "xyd" + codeText;

            codeText = textForEncoding;
            
            var color32 = Encode(textForEncoding, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();

            //		QRImage.texture = encoded;

            return encoded;
        }

        #endregion




        #region  扫描

        /// <summary>
        /// 将画面中的二维码信息检索出来
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string Decode(Color32[] colors, int width, int height)
        {
            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(colors, width, height);
            if (result != null)
            {
                return result.Text;
            }
            return "";
        }


        #endregion



    }

}
