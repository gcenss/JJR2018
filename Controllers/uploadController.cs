using Aliyun.OSS;
using jjr2018.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using jjr2018.Common;

namespace jjr2018.Controllers
{
    /// <summary>
    /// 采用前端直传OSS方案，本接口只输出令牌。
    /// 1. 房源图片上传时设置过期时间，如果房源未成功发布，则oss到期自动删除垃圾图片。
    /// 2. 房源发布成功后，后端批量去掉对应图片的过期时间即可。
    /// </summary>
    public class uploadController : Controller// jjrbasicController
    {
        /// <summary>
        /// 获取前端直传令牌
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string gettoken()
        {
            string s = OssHelper.gettoken_esf();
            return s;
        }

        [HttpGet]
        public string gettoken4zf()
        {
            string s = OssHelper.gettoken_zf();
            return s;
        }
        [HttpGet]
        public string gettoken4face()
        {
            string s = OssHelper.gettoken_jjrface();
            return s;
        }
    }
}