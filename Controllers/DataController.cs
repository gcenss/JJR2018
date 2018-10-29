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
using Newtonsoft.Json.Converters;

namespace jjr2018.Controllers
{
    public class DataController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 今日使用
        /// <param name = "times" > 起始时间 </ param >
        /// < param name="timee">结束时间</param>
        /// <returns></returns>

        public string RentSaleData(string times, string timee)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd";
            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {
                    string where = "";
                    string nomal = " and DateDiff(dd,addtime,getdate())=0 ";
                    if (!string.IsNullOrEmpty(times))
                    {
                        where = " and addtime >='" + times+"'";
                        nomal = "";
                    }
                    if (!string.IsNullOrEmpty(timee))
                    {
                        where += " and addtime <='" + timee+"'";
                        nomal = "";
                    }
                    var statist_day = db.Database.DynamicSqlQuery($@"select * from statist_day where userid =@userid{ nomal }{where }", new SqlParameter[] { new SqlParameter("@userid", User.userid) });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "今日使用", data = statist_day }, timeFormat);
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "暂无数据，请稍后再试！" });
                }
            }
        }


        /// <summary>
        /// 今日使用
        /// <returns></returns>
        public string TodayData()
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd";
            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {   
                    //经纪人个人使用
                    var statist_day = db.statist_day.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.addtime, DateTime.Now) == 0 && p.userid == User.userid).FirstOrDefault();
             
                    if (User.user_member.roleid == 3 || User.user_member.roleid == 1)
                    {
                        statist_day = db.statist_day.Where(p => System.Data.Entity.DbFunctions.DiffMonths(p.addtime, DateTime.Now) == 0 && p.userid == User.userid).FirstOrDefault();

                    }
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "今日使用",
                        data = new
                        {
                            addtime = statist_day.addtime,
                            statistday = new
                            {
                                statist_day.houseimgs,
                                statist_day.housenum,
                                statist_day.housenum_down,
                                statist_day.housenum_up,
                                statist_day.housetopnum,
                                statist_day.housetotal,
                                statist_day.refamount,
                                statist_day.refsnum,
                                statist_day.refynum,
                                statist_day.rentadd,
                                statist_day.rentdel,
                                statist_day.rentrefsnum,
                                statist_day.rentrefynum,
                                statist_day.saleadd,
                                statist_day.saledel,
                                statist_day.salerefsnum,
                                statist_day.salerefynum
                            }
                        }
                    }, timeFormat);
                    //门店个人使用


                }
                catch(Exception e)
                {

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "暂无数据，请稍后再试！" });
                }

            }

        }


        /// <summary>
        /// 小区点击量排行
        /// <param name = "SectionID" >区域 </ param >
        /// <returns></returns>
        public string StatistVillage(string SectionID)
        {
            try
            {
               
               if (string.IsNullOrEmpty(SectionID))
                {
                  
                    //日统计
                    DataTable dt = Utils.Query(shvillageconn, "select b.Name ,villageid,SUM(clicknum)num from statist_village a left join NewHouse b on a.villageid=b.ID  where DateDiff(dd, createtime, getdate())= 0 group by villageid,b.name order by num desc");

                    //月统计
                    string sql = @"select b.Name ,villageid,SUM(clicknum)num from statist_village a left join NewHouse b on a.villageid=b.ID where datediff(month,createtime,getdate())=0 group by villageid,b.name order by num desc";
                    DataTable dt1 = Utils.Query(shvillageconn, sql);
                    //季度统计
                    string sql2 = @"select b.Name ,villageid,SUM(clicknum)num from statist_village a left join NewHouse b on a.villageid=b.ID where createtime>DateAdd(Month,-3,getdate()) group by villageid,b.name order by num desc";
                    DataTable dt2 = Utils.Query(shvillageconn, sql2);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "小区访问量统计！",
                        data = new {
                              dt,
                              dt1,
                              dt2
                        }
                    });
                }
                else
                {
                    //当日区域统计
                    string sql = @"select top 10 * from statist_village  where datediff(month,createtime,getdate())=0 and SectionID='" + SectionID + "' order  by clicknum desc";
                    DataTable dt = Utils.Query(shvillageconn, sql); 
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "区域统计", data = dt });
                }

            }
            catch(Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "暂无数据，请稍后再试！" });
            }
        }

        /// <summary>
        /// 房源点击量排行
        /// <param name = "SectionID" >区域 </ param >
        /// <returns></returns>
        public string StatistHouse(string SectionID)
        {
            try {

                //日统计
                DataTable dt = Utils.Query(shvillageconn, "select b.villagename ,houseid,SUM(clicknum)num from statist_house a left join house_sale_list_wuxi b on a.houseid=b.saleid  where DateDiff(dd, createtime, getdate())= 0 and  b.userid="+ User.userid+" group by houseid,villagename order by num desc");

                //月统计
                string sql = @"select b.villagename ,houseid,SUM(clicknum)num from statist_house a left join house_sale_list_wuxi b on a.houseid=b.saleid  where  DateDiff(month, createtime, getdate())= 0 and  b.userid=" + User.userid + " group by houseid,villagename order by num desc";
                DataTable dt1 = Utils.Query(shvillageconn, sql);
                //季度统计
                string sql2 = @"select b.villagename ,houseid,SUM(clicknum)num from statist_house a left join house_sale_list_wuxi b on a.houseid=b.saleid  where  createtime>DateAdd(Month,-3,getdate()) and  b.userid=" + User.userid + " group by houseid,villagename order by num desc";
                DataTable dt2 = Utils.Query(shvillageconn, sql2);

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "房源访问量统计！",
                    data = new
                    {
                        dt,
                        dt1,
                        dt2
                    }
                });

            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "暂无数据，请稍后再试！" });
            }
        }
    }
}