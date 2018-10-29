using jjr2018.Models;
using jjr2018.WxPayAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode.Codec;

namespace jjr2018.Controllers
{
    public class testController : Controller
    {
        // GET: test
        public ActionResult Index(int s)
        {
            NativePay nativePay = new NativePay();
            //string url = nativePay.GetPayUrl("1", "套餐一", "02180323000000001949", 1);
            
            string url = "sdsssssssssssssssssssssssssssssssssssssssss";

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = s;

            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(url, Encoding.Default);
            //string _root = Server.MapPath($"~/wxpaycode/{166191}/");
            //if (!System.IO.Directory.Exists(_root))
            //{
            //    System.IO.Directory.CreateDirectory(_root);
            //}
            //string _filename = Guid.NewGuid().ToString().Replace("-", "") + ".jpg";
            //image.Save(_root + _filename, System.Drawing.Imaging.ImageFormat.Jpeg);

            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return File(ms.ToArray(), "image/jpeg");
        }
    }
}