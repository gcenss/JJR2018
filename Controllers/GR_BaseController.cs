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


namespace jjr2018.Controllers
{
    public class GR_BaseController : Controller
    {
        private string shhouseconnstr = ConfigurationManager.ConnectionStrings["shhouseconn"].ConnectionString;
        private string shvillageconnstr = ConfigurationManager.ConnectionStrings["shvillageconn"].ConnectionString;
        public SqlConnection shvillageconn { get; private set; }
        public SqlConnection shhouseconn { get; private set; }

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 基础数据 base_samtype 表所有数据 /Base/Base_Samtype 
        /// </summary>
        /// <returns></returns>
        public string Base_Samtype()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据", data = base_samtype });
            }
        }

        /// <summary>
        /// 房源标签 /Base/HouseFlag 
        /// </summary>
        /// <returns></returns>
        public string HouseFlag()
        {
            using (var db = new shhouseEntities())
            {
                var house_tags = db.house_tags.OrderByDescending(p => p.parentid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "房源标签", data = house_tags });
            }
        }



        /// <summary>
        /// 小区查询模拟查询 /Base/CommunityByKeywords_tops 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string CommunityByKeywords_tops(string keywords,int? tops) 
        {
            shvillageconn = new SqlConnection(shvillageconnstr);
            string sql = "select top "+ tops.ToString() + " Name,ID,Address,SectionID,quyu from NewHouse where cityid=3";
            if (!string.IsNullOrEmpty(keywords))
            {

                sql += " and Name like'%" + keywords + "%'";
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "关键字不能为空", data = null });
            }
            if (tops == null && tops<1)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "参数有误", data = null });
            }
            DataTable dt = Utils.Query(shvillageconn, sql);
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区查询模拟查询", data = dt });
        }




        /// <summary>
        /// 小区查询模拟查询 高级 /Base/CommunityByKeywords 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string CommunityByKeywords(string keywords)  
        {
            if (!string.IsNullOrEmpty(keywords))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区查询模拟查询", data = Utils.GetCommunityByKeywords(System.Web.HttpUtility.UrlDecode(keywords)) });
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "关键字不能为空", data = null });
            }
           
        }


        /// <summary>
        /// 基础数据 商圈 /Base/Base_Shangquan 
        /// </summary>
        /// <returns></returns>
        public string Base_Shangquan()
        {
            shhouseconn = new SqlConnection(shhouseconnstr);
            string sql = "select areaid,areaname,parentid from base_area where parentid in(select areaid from base_area where parentid=3)";
            DataTable dt = Utils.Query(shhouseconn, sql);
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 商圈", data = dt });
        }


        /// <summary>
        /// 基础数据 区域 /Base/Base_County 
        /// </summary>
        /// <returns></returns>
        public string Base_County()
        {

            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_area.Where(p => p.parentid == 3).OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 区域", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 商圈by区域 /Base/Base_ShangquanByCounty 
        /// </summary>
        /// <returns></returns>
        public string Base_ShangquanByCounty(int Countyid=-1 )
        {


            if (Countyid == -1)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "区域不能为空", data = null });
            }


            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_area.Where(p => p.parentid == Countyid).OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 商圈by区域", data = base_samtype });
            }
        }





        /// <summary>
        /// 基础数据 装修情况 /Base/Base_Fitment 
        /// </summary>
        /// <returns></returns>
        public string Base_Fitment()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 1 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 装修情况", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 屋内设施 /Base/Base_Condition 
        /// </summary>
        /// <returns></returns>
        public string Base_Condition()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 2 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 屋内设施", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 租金区间 /Base/Base_Rentprice 
        /// </summary>
        /// <returns></returns>
        public string Base_Rentprice()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 3 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 租金区间", data = base_samtype });
            }
        }

        ///// <summary>
        ///// 基础数据 物业类型 /Base/Base_Property 
        ///// </summary>
        ///// <returns></returns>
        //public string Base_Property()
        //{
        //    using (var db = new shhouseEntities())
        //    {
        //        var base_samtype = db.base_samtype.Where(p => p.parentid == 4 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
        //        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 面积", data = base_samtype });
        //    }
        //}

        /// <summary>
        /// 基础数据 房屋类型 /Base/Base_Nature 
        /// </summary>
        /// <returns></returns>
        public string Base_Nature()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 4 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 房屋类型", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 产权 /Base/Base_PropertyRight 
        /// </summary>
        /// <returns></returns>
        public string Base_PropertyRight()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 5 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 产权", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 交易类型 /Base/Base_Transaction 
        /// </summary>
        /// <returns></returns>
        public string Base_Transaction()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 6 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 交易类型", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 有效期 /Base/Base_Validity 
        /// </summary>
        /// <returns></returns>
        public string Base_Validity()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 7 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 有效期", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 出租单位 /Base/Base_RentalUnit 
        /// </summary>
        /// <returns></returns>
        public string Base_RentalUnit()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 8 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 出租单位", data = base_samtype });
            }
        }



        /// <summary>
        /// 基础数据 房屋朝向 /Base/Base_Directions 
        /// </summary>
        /// <returns></returns>
        public string Base_Directions()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 9 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 房屋朝向", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 押金要求 /Base/Base_Deposit 
        /// </summary>
        /// <returns></returns>
        public string Base_Deposit()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 10 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 押金要求", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 付款要求 /Base/Base_Payment 
        /// </summary>
        /// <returns></returns>
        public string Base_Payment()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 11 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 付款要求", data = base_samtype });
            }
        }



        /// <summary>
        /// 基础数据 租房方式 /Base/Base_Rentype 
        /// </summary>
        /// <returns></returns>
        public string Base_Rentype()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 12 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 租房方式", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 价格 /Base/Base_Prices 
        /// </summary>
        /// <returns></returns>
        public string Base_Prices()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 13 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 价格", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 面积 /Base/Base_Area 
        /// </summary>
        /// <returns></returns>
        public string Base_Area()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 14 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 面积", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 时间段 /Base/Base_TimeSlot 
        /// </summary>
        /// <returns></returns>
        public string Base_TimeSlot()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 15 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 时间段", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 支付方式 /Base/Base_Renttype 
        /// </summary>
        /// <returns></returns>
        public string Base_Renttype()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 76&& p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 支付方式", data = base_samtype });
            }
        }

        /// <summary>
        /// 基础数据 小区均价 /Base/Base_Avgprice 
        /// </summary>
        /// <returns></returns>
        public string Base_Avgprice()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 82 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 小区均价", data = base_samtype });
            }
        }


        /// <summary>
        /// 基础数据 楼层区间 /Base/Base_Layer 
        /// </summary>
        /// <returns></returns>
        public string Base_Layer()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 89 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 楼层区间", data = base_samtype });
            }
        }




        /// <summary>
        /// 基础数据 个人服务 /Base/Base_PersonalService 
        /// </summary>
        /// <returns></returns>
        public string Base_PersonalService()
        {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.Where(p => p.parentid == 131 && p.citypy == "wuxi").OrderBy(p => p.parentid).OrderBy(p => p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据 个人服务", data = base_samtype });
            }
        }

        //        typeid typename    parentid parentpath  isshow orderid readme space1  space2 citypy
        //1	装修情况	0	0,	0	100				wuxi
        //2	屋内设施	0	0,	0	95				wuxi
        //3	租金区间	0	0,	0	5				wuxi
        //4	物业类型	0	0,	0	90				wuxi
        //5	产权	0	0,	0	85				wuxi
        //6	交易类型	0	0,	0	75				wuxi
        //7	有效期	0	0,	0	999				wuxi
        //8	出租单位	0	0,	0	999				wuxi
        //9	房屋朝向	0	0,	0	70				wuxi
        //10	押金要求	0	0,	0	999				wuxi
        //11	付款要求	0	0,	0	999				wuxi
        //12	租房方式	0	0,	0	999				wuxi
        //13	售价区间	0	0,	0	1				wuxi
        //14	面积区间	0	0,	0	10				wuxi
        //15	时间段	0	0,	0	999				wuxi
        //76	支付方式	0	0,	0	70		NULL NULL    wuxi
        //82	小区均价	0	0,	0	15				wuxi
        //89	楼层区间	0	0,	0	80				wuxi
        //118	面积区间(赤壁)    0	0,	0	0				chibi
        //131	个人服务	0	0,	0	0	NULL NULL    NULL wuxi


    }
}