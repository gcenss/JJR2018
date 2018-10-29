using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jjr2018.Models;
using jjr2018.Entity.shhouse;
using System.Data.SqlClient;
using jjr2018.Common;
using System.Data;
using System.Configuration;
using System.Data.Entity;
using Newtonsoft.Json.Converters;


namespace jjr2018.Controllers
{
    public class GR_NewHouse_MSGController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();



        //
        /// <summary>
        ///  新房收藏 http://192.168.1.223/GR_Favourite/Favourite_NewHouse_list    // 新房收藏
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Reviews_NewHouse_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });          
        }



    }
}