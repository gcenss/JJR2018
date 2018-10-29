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

using System.Text;
using jjr2018.Entity.shvillage;
using System.Data.Entity;

namespace jjr2018.Controllers
{
    public class GR_NewHouse_Price_LineController : Controller
    {
        //private string shhouseconnstr = ConfigurationManager.ConnectionStrings["shhouseconn"].ConnectionString;
        //private string shvillageconnstr = ConfigurationManager.ConnectionStrings["shvillageconn"].ConnectionString;
        //public SqlConnection shvillageconn { get; private set; }
        //public SqlConnection shhouseconn { get; private set; }

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        /// <summary>
        /// 个人会员登录 密码登录 http://192.168.1.223/GR_NewHouse_Price_Line/Price_LineByVillageid
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public string Price_LineByVillageid(int villageid )
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            // string sqlnn = "select MaxPrice,AddDate from(select top 6 ID,MaxPrice,AddDate from NewHouse_Price_Line where NHID=" + houseid.ToString() + " order by ID desc)as a order by id";


            using (var dbshvillage = new shvillageEntities())
            {

            //  var Price_Line = dbshvillage.NewHouse_Price_Line.Where(p=>p.NHID==villageid&&p.City=="无锡").Select(s => new { AddDate =s.AddDate.Value, MaxPrice = s.MaxPrice }).OrderByDescending(p => p.AddDate).Take(6);

             var Price_Line = (from p in dbshvillage.NewHouse_Price_Line where ( p.NHID == villageid && p.City == "无锡")
                               select new  {ID=p.ID, AddDate= DbFunctions.TruncateTime(p.AddDate), MaxPrice =p.MaxPrice }).OrderByDescending(S => S.ID).Take(6);


                DateTime bdt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime sdt = bdt.AddMonths(-1);

                Double byMaxPrice = 0;
                var myNewHouse_Price_Lineby = dbshvillage.NewHouse_Price_Line.Where(x => x.NHID == villageid && x.AddDate >= bdt).FirstOrDefault();
                if (myNewHouse_Price_Lineby != null) { byMaxPrice = Convert.ToDouble(myNewHouse_Price_Lineby.MaxPrice.ToString()); }

                Double sybyMaxPrice = 0;
                var myNewHouse_Price_Linesy = dbshvillage.NewHouse_Price_Line.Where(x => x.NHID == villageid && x.AddDate < bdt&& x.AddDate >= sdt).FirstOrDefault();
                if (myNewHouse_Price_Linesy != null) { sybyMaxPrice = Convert.ToDouble(myNewHouse_Price_Linesy.MaxPrice.ToString()); }

                Double hb = 0;
                if (byMaxPrice != 0 && sybyMaxPrice != 0)
                {
                    hb = (byMaxPrice - sybyMaxPrice) / sybyMaxPrice;
                }






                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区价格走势", data= new { Price_Line, byMaxPrice, sybyMaxPrice, hb }}, timeFormat);





            }



             
        }









    }
}