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
using Newtonsoft.Json.Converters;
using jjr2018.Entity.efwnews;

namespace jjr2018.Controllers
{
    public class GR_CMS_Article_ContentController : Controller
    {
     

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        /// <summary>
        /// 二手房日成交量分析列表   http://192.168.4.223/ CMS_Article_Content/ByClass90_list
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ByClass90_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new efwnewsEntities())
            {
                try
                {

                    int Class = 90;
                    var datas = db.CMS_Article_Content.Where(p => p.ClassID == Class).Select(p => new { p.ArticleID, p.ClassID, p.Title, p.UpdateTime }).OrderByDescending(p => p.ArticleID).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                    var datas_c = db.CMS_Article_Content.Where(p => p.ClassID == Class).Count();
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "",
                    data = new
                    {
                        cms_article_content = datas,
                        count = datas_c
                    }
                }, timeFormat);

            }
                catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常!", data = null });
            }
        }


        }
    }
}