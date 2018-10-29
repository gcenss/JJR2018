using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jjr2018.Entity.shhouse;
using System.Data.Entity.Core.Objects;
using System.Data;
using jjr2018.Common;
using Newtonsoft.Json;
using jjr2018.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Linq.Dynamic;
using jjr2018.Entity.shvillage;

namespace jjr2018.Controllers
{
    public class GR_house_customizeController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /////  字段：cusid 说明：  类型：int 长度：4 可为空：False  
        /////  字段：userid 说明：发布人  类型：int 长度：4 可为空：True 
        /////  字段：字段：areaID 说明：区域  类型：int 长度：4 可为空：True 
        /////  字段：字段：tradingareaID 说明：商圈  类型：int 长度：4 可为空：True 
        /////  字段：字段：countyIdList 说明：小区id逗号隔开最多五个  类型：varchar 长度：50 可为空：True 
        /////  字段：jiageId 说明：  类型：int 长度：4 可为空：True 
        /////  字段：room 说明：室  类型：int 长度：4 可为空：True
        /////  字段：housetype 说明：1求购2求租  类型：int 长度：4 可为空：True 
        /////  字段：rentype 说明：租赁方式   类型：int 长度：4 可为空：True
        /////  字段：linkman 说明：联系人  类型：varchar 长度：20 可为空：True 
        /////  字段：tel 说明：联系电话  类型：varchar 长度：50 可为空：True 
        /////  字段：iscustomize 说明：是否被定制  类型：bit 长度：1 可为空：True 
        /////  字段：isdel 说明：是否被删除  类型：bit 长度：1 可为空：True 
        /////  字段：addtime 说明：添加时间  类型：datetime 长度：8 可为空：True 
        /////  字段：updatetime 说明：更新时间  类型：datetime 长度：8 可为空：True 
        /////  字段：addip 说明：添加IP  类型：varchar 长度：16 可为空：True 
        /////  字段：city 说明：城市  类型：int 长度：4 可为空：True 



        /// <summary>
        /// 定制列表（用户） http://192.168.1.223/GR_house_customize/ListByUser
        /// </summary>
        ///  <param name="housetype">定制类型 1为求购 2为求租 默认1 int</param>
        /// <param name="isdel">是否被删除 默认null bool</param>
        /// <param name="SortName">排序字段 字符型 值为该表的任意一个字段名 默认值  addtime</param>
        /// <param name="IsDesc">是否是倒序 bool型 倒序为true 正序为false 默认为 true</param>
        /// <returns></returns>
        public string ListByUser(int housetype = 1, bool? isdel = null, string SortName = "addtime", bool IsDesc = true)
        {
            try
            {
                timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                using (shhouseEntities db = new shhouseEntities())
                {
                    IQueryable<house_customize_wuxi> result = db.house_customize_wuxi;
                    result = result.Where(p => p.housetype == housetype);
                    result = result.Where(p => p.userid == User.userid);
                    if (isdel != null) result = result.Where(p => p.isdel == isdel);
                    int count = result.Count();
                    var house_customize_wuxi_List_temp = result.OrderBy(string.Format("{0} {1}", SortName, IsDesc ? "desc" : "asc")).ToList();

                    Dictionary<string, object>[] house_customize_wuxi_List = new Dictionary<string, object>[house_customize_wuxi_List_temp.Count()];
                    int i = 0;
                    using (shvillageEntities dbshvillage = new shvillageEntities())
                    {
                        foreach (var temp in house_customize_wuxi_List_temp)
                        {
                            house_customize_wuxi_List[i] = new Dictionary<string, object>();
                            house_customize_wuxi_List[i].Add("cusid", temp.cusid);
                            house_customize_wuxi_List[i].Add("userid", temp.userid);
                            house_customize_wuxi_List[i].Add("areaID", temp.areaID);
                            house_customize_wuxi_List[i].Add("tradingareaID", temp.tradingareaID);
                            house_customize_wuxi_List[i].Add("countyIdList", temp.countyIdList);
                            house_customize_wuxi_List[i].Add("jiageId", temp.jiageId);
                            house_customize_wuxi_List[i].Add("room", temp.room);
                            house_customize_wuxi_List[i].Add("housetype", temp.housetype);
                            house_customize_wuxi_List[i].Add("rentype", temp.rentype);
                            house_customize_wuxi_List[i].Add("linkman", temp.linkman);
                            house_customize_wuxi_List[i].Add("tel", temp.tel);
                            house_customize_wuxi_List[i].Add("iscustomize", temp.iscustomize);
                            house_customize_wuxi_List[i].Add("isdel", temp.isdel);
                            house_customize_wuxi_List[i].Add("addtime", temp.addtime);
                            house_customize_wuxi_List[i].Add("updatetime", temp.updatetime);
                            house_customize_wuxi_List[i].Add("addip", temp.addip);
                            house_customize_wuxi_List[i].Add("city", temp.city);

                            string areaname = "";
                            if (temp.areaID != null && temp.areaID != 0)
                            {
                                var areaIDtemp = db.base_area.Where(p => p.areaid == temp.areaID).OrderBy(p => p.areaid).FirstOrDefault();
                                if (areaIDtemp != null)
                                { areaname = areaIDtemp.areaname; }
                            }
                            house_customize_wuxi_List[i].Add("areaname", areaname);

                            string tradingareaname = "";
                            if (temp.tradingareaID != null && temp.tradingareaID != 0)
                            {
                                var tradingareaIDtemp = db.base_area.Where(p => p.areaid == temp.tradingareaID).OrderBy(p => p.areaid).FirstOrDefault();
                                if (tradingareaIDtemp != null)
                                { tradingareaname = tradingareaIDtemp.areaname; }
                            }
                            house_customize_wuxi_List[i].Add("tradingareaname", tradingareaname);



                            if (!string.IsNullOrWhiteSpace(temp.countyIdList))
                            {
                                string[] str_countyIdList = temp.countyIdList.Split(',').ToArray();
                                int[] int_countyIdList = Array.ConvertAll(str_countyIdList, new Converter<string, int>(Utils.StrToInt));
                                var countyList = dbshvillage.NewHouse.Where(p => int_countyIdList.Contains(p.ID)).Select(a => new { ID = a.ID, title = a.Name }).OrderBy(p => p.ID).ToList();
                                house_customize_wuxi_List[i].Add("countyList", countyList);
                            }
                            else
                            {
                                house_customize_wuxi_List[i].Add("countyList", null);
                            }


                            string jiagename = null;
                            if (temp.jiageId != null && temp.jiageId != 0)
                            {
                                var jiagetemp = db.base_samtype.Where(p => p.typeid == temp.jiageId).FirstOrDefault();
                                if (jiagetemp != null) { jiagename = jiagetemp.typename; }
                            }
                            house_customize_wuxi_List[i].Add("jiagename", jiagename);

                            string rentypename = null;
                            if (temp.rentype != null)
                            {
                                if (temp.rentype == 1) rentypename = "整租";
                                if (temp.rentype == 2) rentypename = "合租";
                            }


                            house_customize_wuxi_List[i].Add("rentypename", rentypename);

                            int AgentizeNumber = db.house_agentize_wuxi.Where(p => p.Cusid == temp.cusid && p.IsDel != 1).Count();
                            house_customize_wuxi_List[i].Add("AgentizeNumber", AgentizeNumber);

                            string photoname = null;
                            if (temp.userid != null && temp.userid != 0)
                            {
                                var user_detailstemp = db.user_details.Where(p => p.userid == temp.userid).FirstOrDefault();
                                if (user_detailstemp != null) { photoname = user_detailstemp.photoname; }
                            }
                            house_customize_wuxi_List[i].Add("photoname", photoname);

                            i++;
                        }
                    }
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "", data = new { house_customize_wuxi_List, count } }, timeFormat);
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }
        // 字段：tradingareaID 说明：商圈 类型：int 长度：4 可为空：True

        /// <summary>
        /// 定制列表查询 http://192.168.1.223/GR_house_customize/ListByWhere
        /// </summary>
        /// <param name="cusid">cusid 说明：  类型：int </param>
        /// <param name="userid">userid 说明：发布人  类型：int</param>
        /// <param name="areaID">areaID 说明：区域 类型：int</param>
        /// <param name="tradingareaID">tradingareaID 说明：商圈 类型：int</param>
        /// <param name="countyIdList">countyIdList 说明：小区id逗号隔开最多五个  类型：varchar</param>
        /// <param name="jiageId">jiageId 说明：  类型：int</param>
        /// <param name="room">room 说明：室  类型：int </param>
        /// <param name="housetype">housetype 说明：1求购2求租  类型：int</param>
        /// <param name="rentype">rentype 说明：租赁方式  </param>
        /// <param name="linkman">linkman 说明：联系人  类型：varchar</param>
        /// <param name="tel">tel 说明：联系电话  类型：varchar</param>
        /// <param name="iscustomize">iscustomize 说明：是否被定制  类型：bit</param>
        /// <param name="isdel">isdel 说明：是否被删除  类型：bit </param>
        /// <param name="addtime">addtime 说明：添加时间  类型：datetime</param>
        /// <param name="updatetime">updatetime 说明：更新时间  类型：datetime </param>
        /// <param name="addip">addip 说明：添加IP  类型：varchar</param>
        /// <param name="city">city 说明：城市  类型：int</param>
        /// <param name="IsAgentize">IsAgentize 说明：是否分配  类型：bit</param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="SortName">排序字段 字符型 值为该表的任意一个字段名 默认值  addtime</param>
        /// <param name="IsDesc">是否是倒序 bool型 倒序为true 正序为false 默认为 true</param>
        /// <returns></returns>
        public string ListByWhere(int? cusid = null, int? userid = null, int? areaID = null, int? tradingareaID = null, string countyIdList = null, int? jiageId = null
            , int? room = null, int? housetype = null, int? rentype = null, string linkman = null, string tel = null, bool? iscustomize = null
            , bool? isdel = null, DateTime? addtime = null, DateTime? updatetime = null, string addip = null, int? city = null, bool? IsAgentize = null
            , int pageindex = 1, int pagesize = 20, string SortName = "addtime", bool IsDesc = true)
        {
            try
            {
                timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                using (var db = new shhouseEntities())
                {
                    IQueryable<house_customize_wuxi> result = db.house_customize_wuxi;
                    if (cusid != null) result = result.Where(p => p.cusid == cusid);
                    if (userid != null) result = result.Where(p => p.userid == userid);
                    if (areaID != null) result = result.Where(p => p.areaID == areaID);

                    if (!string.IsNullOrWhiteSpace(countyIdList))
                    {
                        string[] str_countyIdList = countyIdList.Split(',');
                        for (int m = 0; m < str_countyIdList.Length; m++)
                        {
                            string temp_countyIdList = str_countyIdList[m];
                            result = result.Where(p => (p.countyIdList == temp_countyIdList || p.countyIdList.Contains("," + temp_countyIdList + ",") || p.countyIdList.StartsWith(temp_countyIdList + ",") || p.countyIdList.EndsWith("," + temp_countyIdList)));
                        }
                    }
                    if (jiageId != null) result = result.Where(p => p.jiageId == jiageId);
                    if (room != null) result = result.Where(p => p.room == room);
                    if (housetype != null) result = result.Where(p => p.housetype == housetype);
                    if (rentype != null) result = result.Where(p => p.rentype == rentype);
                    if (linkman != null) result = result.Where(p => p.linkman.Contains(linkman));
                    if (tel != null) result = result.Where(p => p.tel.Contains(tel));
                    if (iscustomize != null) result = result.Where(p => p.iscustomize == iscustomize);
                    if (isdel != null) result = result.Where(p => p.isdel == isdel);
                    if (addtime != null) result = result.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.addtime, addtime) == 0);
                    if (updatetime != null) result = result.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.updatetime, updatetime) == 0);
                    if (addip != null) result = result.Where(p => p.addip == addip);
                    if (city != null) result = result.Where(p => p.city == city);
                    if (IsAgentize != null)
                    {
                        if (Convert.ToBoolean(IsAgentize))
                        {
                            result = result.Where(p => db.house_agentize_wuxi.Where(a => a.IsDel != 1).GroupBy(a => a.Cusid).Select(a => new { Cusid = a.Key, Count = a.Count() }).Where(a => a.Count >= 3).Select(a => a.Cusid).Contains(p.cusid));

                            //  result = result.Where(p => db.house_agentize_wuxi.Where(a => a.IsDel != 1).GroupBy(a => a.Cusid).Select(a => new { Cusid = a.Key, Count = a.Count() }).Where(a => a.Count >= 3).Select(a => a.Cusid).Contains(p.cusid));
                        }
                        else
                        {
                            result = result.Where(p => !db.house_agentize_wuxi.Where(a => a.IsDel != 1).GroupBy(a => a.Cusid).Select(a => new { Cusid = a.Key, Count = a.Count() }).Where(a => a.Count >= 3).Select(a => a.Cusid).Contains(p.cusid));
                            //被经纪人自己抢了的不算未分配
                            result = result.Where(p => !db.house_agentize_wuxi.Where(a => a.Agentid == User.user_member.userid).Select(a => a.Cusid).Contains(p.cusid));

                        }
                    }


                    int count = result.Count();
                    var house_customize_wuxi_List_temp = result.OrderBy(string.Format("{0} {1}", SortName, IsDesc ? "desc" : "asc"));

                    Dictionary<string, object>[] house_customize_wuxi_List = new Dictionary<string, object>[house_customize_wuxi_List_temp.Count()];
                    int i = 0;
                    using (shvillageEntities dbshvillage = new shvillageEntities())
                    {
                        foreach (var temp in house_customize_wuxi_List_temp)
                        {
                            house_customize_wuxi_List[i] = new Dictionary<string, object>();
                            house_customize_wuxi_List[i].Add("cusid", temp.cusid);
                            house_customize_wuxi_List[i].Add("userid", temp.userid);
                            house_customize_wuxi_List[i].Add("areaID", temp.areaID);
                            house_customize_wuxi_List[i].Add("tradingareaID", temp.tradingareaID);
                            house_customize_wuxi_List[i].Add("countyIdList", temp.countyIdList);
                            house_customize_wuxi_List[i].Add("jiageId", temp.jiageId);
                            house_customize_wuxi_List[i].Add("room", temp.room);
                            house_customize_wuxi_List[i].Add("housetype", temp.housetype);
                            house_customize_wuxi_List[i].Add("rentype", temp.rentype);
                            house_customize_wuxi_List[i].Add("linkman", temp.linkman);
                            house_customize_wuxi_List[i].Add("tel", temp.tel);
                            house_customize_wuxi_List[i].Add("iscustomize", temp.iscustomize);
                            house_customize_wuxi_List[i].Add("isdel", temp.isdel);
                            house_customize_wuxi_List[i].Add("addtime", temp.addtime);
                            house_customize_wuxi_List[i].Add("updatetime", temp.updatetime);
                            house_customize_wuxi_List[i].Add("addip", temp.addip);
                            house_customize_wuxi_List[i].Add("city", temp.city);

                            string areaname = "";
                            if (temp.areaID != null && temp.areaID != 0)
                            {
                                var areaIDtemp = db.base_area.Where(p => p.areaid == temp.areaID).OrderBy(p => p.areaid).FirstOrDefault();
                                if (areaIDtemp != null)
                                { areaname = areaIDtemp.areaname; }
                            }
                            house_customize_wuxi_List[i].Add("areaname", areaname);

                            string tradingareaname = "";
                            if (temp.tradingareaID != null && temp.tradingareaID != 0)
                            {
                                var tradingareaIDtemp = db.base_area.Where(p => p.areaid == temp.tradingareaID).OrderBy(p => p.areaid).FirstOrDefault();
                                if (tradingareaIDtemp != null)
                                { tradingareaname = tradingareaIDtemp.areaname; }
                            }
                            house_customize_wuxi_List[i].Add("tradingareaname", tradingareaname);



                            if (!string.IsNullOrWhiteSpace(temp.countyIdList))
                            {
                                string[] str_countyIdList = temp.countyIdList.Split(',').ToArray();
                                int[] int_countyIdList = Array.ConvertAll(str_countyIdList, new Converter<string, int>(Utils.StrToInt));
                                var countyList = dbshvillage.NewHouse.Where(p => int_countyIdList.Contains(p.ID)).Select(a => new { ID = a.ID, title = a.Name }).OrderBy(p => p.ID).ToList();
                                house_customize_wuxi_List[i].Add("countyList", countyList);
                            }
                            else
                            {
                                house_customize_wuxi_List[i].Add("countyList", null);
                            }


                            string jiagename = null;
                            if (temp.jiageId != null && temp.jiageId != 0)
                            {
                                var jiagetemp = db.base_samtype.Where(p => p.typeid == temp.jiageId).FirstOrDefault();
                                if (jiagetemp != null) { jiagename = jiagetemp.typename; }
                            }
                            house_customize_wuxi_List[i].Add("jiagename", jiagename);

                            string rentypename = null;
                            if (temp.rentype != null)
                            {
                                if (temp.rentype == 1) rentypename = "整租";
                                if (temp.rentype == 2) rentypename = "合租";
                            }
                            house_customize_wuxi_List[i].Add("rentypename", rentypename);

                            int AgentizeNumber = db.house_agentize_wuxi.Where(p => p.Cusid == temp.cusid && p.IsDel != 1).Count();
                            house_customize_wuxi_List[i].Add("AgentizeNumber", AgentizeNumber);


                            string photoname = null;
                            if (temp.userid != null && temp.userid != 0)
                            {
                                var user_detailstemp = db.user_details.Where(p => p.userid == temp.userid).FirstOrDefault();
                                if (user_detailstemp != null) { photoname = user_detailstemp.photoname; }
                            }
                            house_customize_wuxi_List[i].Add("photoname", photoname);



                            i++;
                        }
                    }
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "", data = new { house_customize_wuxi_List, count } }, timeFormat);
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }

        /// <summary>
        /// 定制详情 http://192.168.1.223/GR_house_customize/Find
        /// </summary>
        /// <param name="cusid">cusid 说明：  类型：int </param>
        /// <param name="isdel">isdel 说明：是否被删除  类型：bit </param>
        /// <returns></returns>
        public string Find(int cusid, bool? isdel = null)
        {
            try
            {
                timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                using (var db = new shhouseEntities())
                {
                    IQueryable<house_customize_wuxi> result = db.house_customize_wuxi;
                    result = result.Where(p => p.cusid == cusid);
                    if (isdel != null) result = result.Where(p => p.isdel == isdel);
                    var myhouse_customize_wuxi = result.FirstOrDefault();
                    if (myhouse_customize_wuxi != null)
                    {

                        Dictionary<string, object>[] house_customize_wuxi_List = new Dictionary<string, object>[1];
                        house_customize_wuxi_List[0] = new Dictionary<string, object>();
                        house_customize_wuxi_List[0].Add("cusid", myhouse_customize_wuxi.cusid);
                        house_customize_wuxi_List[0].Add("userid", myhouse_customize_wuxi.userid);
                        house_customize_wuxi_List[0].Add("areaID", myhouse_customize_wuxi.areaID);
                        house_customize_wuxi_List[0].Add("tradingareaID", myhouse_customize_wuxi.tradingareaID);
                        house_customize_wuxi_List[0].Add("countyIdList", myhouse_customize_wuxi.countyIdList);
                        house_customize_wuxi_List[0].Add("jiageId", myhouse_customize_wuxi.jiageId);
                        house_customize_wuxi_List[0].Add("room", myhouse_customize_wuxi.room);
                        house_customize_wuxi_List[0].Add("housetype", myhouse_customize_wuxi.housetype);
                        house_customize_wuxi_List[0].Add("rentype", myhouse_customize_wuxi.rentype);
                        house_customize_wuxi_List[0].Add("linkman", myhouse_customize_wuxi.linkman);
                        house_customize_wuxi_List[0].Add("tel", myhouse_customize_wuxi.tel);
                        house_customize_wuxi_List[0].Add("iscustomize", myhouse_customize_wuxi.iscustomize);
                        house_customize_wuxi_List[0].Add("isdel", myhouse_customize_wuxi.isdel);
                        house_customize_wuxi_List[0].Add("addtime", myhouse_customize_wuxi.addtime);
                        house_customize_wuxi_List[0].Add("updatetime", myhouse_customize_wuxi.updatetime);
                        house_customize_wuxi_List[0].Add("addip", myhouse_customize_wuxi.addip);
                        house_customize_wuxi_List[0].Add("city", myhouse_customize_wuxi.city);

                        string areaname = "";
                        if (myhouse_customize_wuxi.areaID != null && myhouse_customize_wuxi.areaID != 0)
                        {
                            var areaID = db.base_area.Where(p => p.areaid == myhouse_customize_wuxi.areaID).OrderBy(p => p.areaid).FirstOrDefault();
                            if (areaID != null)
                            { areaname = areaID.areaname; }
                        }
                        house_customize_wuxi_List[0].Add("areaname", areaname);

                        string tradingareaname = "";
                        if (myhouse_customize_wuxi.tradingareaID != null && myhouse_customize_wuxi.tradingareaID != 0)
                        {
                            var tradingareaID = db.base_area.Where(p => p.areaid == myhouse_customize_wuxi.tradingareaID).OrderBy(p => p.areaid).FirstOrDefault();
                            if (tradingareaID != null)
                            { tradingareaname = tradingareaID.areaname; }
                        }
                        house_customize_wuxi_List[0].Add("tradingareaname", tradingareaname);


                        using (var dbshvillage = new shvillageEntities())
                        {
                            if (!string.IsNullOrWhiteSpace(myhouse_customize_wuxi.countyIdList))
                            {
                                string[] str_countyIdList = myhouse_customize_wuxi.countyIdList.Split(',').ToArray();
                                int[] int_countyIdList = Array.ConvertAll(str_countyIdList, new Converter<string, int>(Utils.StrToInt));
                                var countyList = dbshvillage.NewHouse.Where(p => int_countyIdList.Contains(p.ID)).Select(a => new { ID = a.ID, title = a.Name }).OrderBy(p => p.ID).ToList();
                                house_customize_wuxi_List[0].Add("countyList", countyList);
                            }
                            else
                            {
                                house_customize_wuxi_List[0].Add("countyList", null);
                            }
                        }

                        string jiagename = null;
                        if (myhouse_customize_wuxi.jiageId != null && myhouse_customize_wuxi.jiageId != 0)
                        {
                            var jiage = db.base_samtype.Where(p => p.typeid == myhouse_customize_wuxi.jiageId).FirstOrDefault();
                            if (jiage != null) { jiagename = jiage.typename; }
                        }
                        house_customize_wuxi_List[0].Add("jiagename", jiagename);

                        string rentypename = "不限";
                        if (myhouse_customize_wuxi.rentype != null)
                        {
                            if (myhouse_customize_wuxi.rentype == 1) rentypename = "整租";
                            if (myhouse_customize_wuxi.rentype == 2) rentypename = "合租";
                        }
                        house_customize_wuxi_List[0].Add("rentypename", rentypename);

                        int AgentizeNumber = db.house_agentize_wuxi.Where(p => p.Cusid == myhouse_customize_wuxi.cusid && p.IsDel != 1).Count();
                        house_customize_wuxi_List[0].Add("AgentizeNumber", AgentizeNumber);



                        string photoname = null;
                        if (myhouse_customize_wuxi.userid != null && myhouse_customize_wuxi.userid != 0)
                        {
                            var user_details = db.user_details.Where(p => p.userid == myhouse_customize_wuxi.userid).FirstOrDefault();
                            if (user_details != null) { photoname = user_details.photoname; }
                        }
                        house_customize_wuxi_List[0].Add("photoname", photoname);

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "", data = house_customize_wuxi_List }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "未找到该数据", data = null }, timeFormat);
                    }
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }



        /// <summary>
        /// 定制添加 http://192.168.1.223/GR_house_customize/Add
        /// </summary>
        /// <param name="housetype">housetype 说明：1求购2求租  类型：int</param>
        /// <param name="areaID">areaID 说明：区域  类型：int 不限为null</param>
        /// <param name="tradingareaID">tradingareaID 说明：商圈 类型：int 不限为null</param>
        /// <param name="countyIdList">countyIdList 说明：小区id逗号隔开最多五个  类型：varchar 不限为null</param>
        /// <param name="jiageId">jiageId 说明：  类型：int 不限为null</param>
        /// <param name="room">room 说明：室  类型：int 不限为null</param>     
        /// <param name="linkman">linkman 说明：联系人  类型：varchar</param>
        /// <param name="tel">tel 说明：联系电话  类型：varchar</param>
        /// <param name="iscustomize">iscustomize 说明：是否被定制  类型：bit</param>
        /// <param name="rentype">rentype 说明：租赁方式 根据参数表来 不限为null</param>
        /// <returns></returns>
        [HttpPost]
        public string Add(int housetype, string linkman, string tel, bool iscustomize, int? areaID = null, int? tradingareaID = null, string countyIdList = null, int? jiageId = null, int? room = null, int? rentype = null)
        {
            //try
            //{

            using (var db = new shhouseEntities())
            {


                if (housetype != 1 && housetype != 2) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,定制类型值错误", data = null });

                //areaID    是否存在
                if (areaID != null)
                {
                    int tempint2 = db.base_area.Where(p => p.areaid == areaID).Count();
                    if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,该区域（AreaID=" + areaID + "）不存在", data = null });
                }

                //tradingareaID    是否存在
                if (tradingareaID != null)
                {
                    int tempint2 = db.base_area.Where(p => p.areaid == tradingareaID).Count();
                    if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,该商圈（tradingareaID=" + tradingareaID + "）不存在", data = null });
                }

                if (!string.IsNullOrWhiteSpace(countyIdList))
                {
                    using (var dbshvillage = new shvillageEntities())
                    {
                        string[] str_countyIdList = countyIdList.Split(',').ToArray();
                        for (int m = 0; m < str_countyIdList.Length; m++)
                        {
                            int temp_countyIdList = int.Parse(str_countyIdList[m]);
                            int tempint3 = dbshvillage.NewHouse.Where(p => p.ID == temp_countyIdList).Count();
                            if (tempint3 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,countyIdList（" + countyIdList + "）中(" + temp_countyIdList + ")不存在", data = null });
                        }
                    }
                }
                else
                {
                    countyIdList = null;
                }



                //jiageId 是否存在
                if (jiageId != null)
                {
                    if (housetype == 1)
                    {
                        int tempint2 = db.base_samtype.Where(p => p.parentid == 13 && p.citypy == "wuxi" && p.typeid == jiageId).Count();
                        if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,该价格区间（jiageId=" + jiageId + "）不存在", data = null });
                    }
                    if (housetype == 2)
                    {
                        int tempint2 = db.base_samtype.Where(p => p.parentid == 3 && p.citypy == "wuxi" && p.typeid == jiageId).Count();
                        if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,该租金区间（jiageId=" + jiageId + "）不存在", data = null });
                    }
                }

                //rentype
                if (housetype == 2)
                {
                    if (rentype != null)
                    {
                        if (rentype != 1 && rentype != 2) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,租赁方式值错误", data = null });

                        //int tempint2 = db.base_samtype.Where(p => p.parentid == 12 && p.citypy == "wuxi" && p.typeid == rentype).Count();
                        //if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,该租赁方式（rentype=" + rentype + "）不存在", data = null });

                    }
                }

                if (string.IsNullOrWhiteSpace(linkman))
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,联系人不能为空", data = null });
                }
                if (string.IsNullOrWhiteSpace(tel))
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败,联系人电话不能为空", data = null });
                }

                house_customize_wuxi myhouse_customize_wuxi = new house_customize_wuxi();
                myhouse_customize_wuxi.userid = User.userid;
                myhouse_customize_wuxi.areaID = areaID;
                myhouse_customize_wuxi.countyIdList = countyIdList;
                myhouse_customize_wuxi.jiageId = jiageId;
                myhouse_customize_wuxi.room = room;
                myhouse_customize_wuxi.housetype = housetype;
                myhouse_customize_wuxi.rentype = rentype;
                myhouse_customize_wuxi.linkman = linkman;
                myhouse_customize_wuxi.tel = tel;
                myhouse_customize_wuxi.iscustomize = iscustomize;
                myhouse_customize_wuxi.isdel = false;
                myhouse_customize_wuxi.addtime = DateTime.Now;
                myhouse_customize_wuxi.updatetime = null;
                myhouse_customize_wuxi.addip = Utils.GetRealIP();
                myhouse_customize_wuxi.city = User.user_member.city;

                db.house_customize_wuxi.Add(myhouse_customize_wuxi);
                int r = db.SaveChanges();
                if (r > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "添加成功", data = myhouse_customize_wuxi.cusid });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败", data = null });
                }
            }
            //}
            //catch (Exception e)
            //{
            //    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            //}
        }





        /// <summary>
        /// 定制修改 http://192.168.1.223/GR_house_customize/Edit
        /// </summary>
        /// <param name="cusid">cusid 说明：  类型：int</param>
        /// <param name="housetype">housetype 说明：1求购2求租  类型：int</param>
        /// <param name="areaID">areaID 说明：区域  类型：string 为空是不限 为null是不修改</param>
        ///  <param name="tradingareaID">tradingareaID 说明：商圈  类型：string 为空是不限 为null是不修改</param>
        /// <param name="countyIdList">countyIdList 说明：小区id逗号隔开最多五个  类型：varchar 为空是不限 为null是不修改</param>
        /// <param name="jiageId">jiageId 说明：  类型：string 为空是不限 为null是不修改</param>
        /// <param name="room">room 说明：室  类型：string 为空是不限 为null是不修改</param>
        /// <param name="rentype">rentype 说明：租赁方式 1为整租 2为合租 为空是不限 为null是不修改</param>
        /// <param name="linkman">linkman 说明：联系人  类型：varchar</param>
        /// <param name="tel">tel 说明：联系电话  类型：varchar</param>
        /// <param name="iscustomize">iscustomize 说明：是否被定制  类型：bit</param>
        /// <param name="isdel">isdel 说明：是否被删除  类型：bit </param>
        /// <returns></returns>
        [HttpPost]
        public string Edit(int cusid, int? housetype = null, string areaID = null, string tradingareaID = null, string countyIdList = null, string jiageId = null
            , string room = null, string rentype = null, string linkman = null, string tel = null, bool? iscustomize = null
            , bool? isdel = null
          )
        {
            try
            {
                bool IsEdit = false;
            using (var db = new shhouseEntities())
            {

                house_customize_wuxi myhouse_customize_wuxi = db.house_customize_wuxi.Find(cusid);
                if (myhouse_customize_wuxi == null)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "未找到要修改的信息", data = null });
                }
                else
                {
                    //housetype
                    if (housetype != null && myhouse_customize_wuxi.housetype != housetype)
                    {
                        if (housetype != 1 && housetype != 2) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,定制类型值错误" });
                        myhouse_customize_wuxi.housetype = Convert.ToInt32(housetype);
                        if (myhouse_customize_wuxi.housetype == 1) myhouse_customize_wuxi.rentype = null;
                        IsEdit = true;
                    } 

                    if (areaID != null)
                    {
                        int? areaIDtemp = null;
                        if (!string.IsNullOrWhiteSpace(areaID)) areaIDtemp = Convert.ToInt32(areaID);
                        if (myhouse_customize_wuxi.areaID != areaIDtemp)
                        {
                            if (areaIDtemp != null)
                            {
                                int tempint2 = db.base_area.Where(p => p.areaid == areaIDtemp).Count();
                                if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该区域（AreaID=" + areaID + "）不存在", data = null });
                            }
                            myhouse_customize_wuxi.areaID = areaIDtemp;
                            IsEdit = true;
                        }
                    }

                    if (tradingareaID != null)
                    {
                        int? tradingareaIDtemp = null;
                        if (!string.IsNullOrWhiteSpace(tradingareaID)) tradingareaIDtemp = Convert.ToInt32(tradingareaID);
                        if (myhouse_customize_wuxi.tradingareaID != tradingareaIDtemp)
                        {
                            if (tradingareaIDtemp != null)
                            {
                                int tempint2 = db.base_area.Where(p => p.areaid == tradingareaIDtemp).Count();
                                if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该商圈（tradingareaID=" + tradingareaID + "）不存在", data = null });
                            }
                            myhouse_customize_wuxi.tradingareaID = tradingareaIDtemp;
                            IsEdit = true;
                        }
                    }




                    //countyIdList
                    if (countyIdList != null)
                    {
                        if (string.IsNullOrWhiteSpace(countyIdList)) countyIdList = null;
                        if (myhouse_customize_wuxi.countyIdList != countyIdList)
                        {
                            if (!string.IsNullOrWhiteSpace(countyIdList))
                            {
                                using (var dbshvillage = new shvillageEntities())
                                {
                                    string[] str_countyIdList = countyIdList.Split(',').ToArray();
                                    for (int m = 0; m < str_countyIdList.Length; m++)
                                    {
                                        int temp_countyIdList = int.Parse(str_countyIdList[m]);
                                        int tempint3 = dbshvillage.NewHouse.Where(p => p.ID == temp_countyIdList).Count();
                                        if (tempint3 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,countyIdList（" + countyIdList + "）中(" + temp_countyIdList + ")不存在", data = null });
                                    }
                                }
                            }
                            myhouse_customize_wuxi.countyIdList = countyIdList;
                            IsEdit = true;
                        }
                    }


                    //jiageId
                    if (jiageId != null)
                    {
                        int? jiageIdtemp = null;
                        if (!string.IsNullOrWhiteSpace(jiageId)) jiageIdtemp = Convert.ToInt32(jiageId);
                        if (myhouse_customize_wuxi.jiageId != jiageIdtemp)
                        {
                            if (jiageIdtemp != null)
                            {
                                if (myhouse_customize_wuxi.housetype == 1)
                                {
                                    int tempint2 = db.base_samtype.Where(p => p.parentid == 13 && p.citypy == "wuxi" && p.typeid == jiageIdtemp).Count();
                                    if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该价格区间（jiageId=" + jiageId + "）不存在", data = null });
                                }
                                if (myhouse_customize_wuxi.housetype == 2)
                                {
                                    int tempint2 = db.base_samtype.Where(p => p.parentid == 3 && p.citypy == "wuxi" && p.typeid == jiageIdtemp).Count();
                                    if (tempint2 == 0) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该租金区间（jiageId=" + jiageId + "）不存在", data = null });
                                }
                            }
                            myhouse_customize_wuxi.jiageId = jiageIdtemp;
                            IsEdit = true;
                        }
                    }

                    //rentype
                    if (rentype != null)
                    {
                        int? rentypetemp = null;
                        if (!string.IsNullOrWhiteSpace(rentype)) rentypetemp = Convert.ToInt32(rentype);
                        if (myhouse_customize_wuxi.rentype != rentypetemp)
                        {
                            if (rentypetemp != null)
                            {
                                if (myhouse_customize_wuxi.housetype == 1)
                                {
                                    myhouse_customize_wuxi.rentype = null;
                                }
                                if (myhouse_customize_wuxi.housetype == 2)
                                {
                                    if (rentypetemp != 1 && rentypetemp != 2) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,租赁方式值错误", data = null });
                                }
                            }
                            myhouse_customize_wuxi.rentype = rentypetemp;
                            IsEdit = true;
                        }
                    }

                    //room
                    if (room != null)
                    {
                        int? roomtemp = null;
                        if (!string.IsNullOrWhiteSpace(room)) roomtemp = Convert.ToInt32(room);
                        if (myhouse_customize_wuxi.room != roomtemp)
                        {
                            myhouse_customize_wuxi.room = roomtemp;
                            IsEdit = true;
                        }
                    }

                    //linkman
                    if (linkman != null && myhouse_customize_wuxi.linkman != linkman)
                    {
                        if (string.IsNullOrWhiteSpace(linkman)) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该 linkman 不能为空", data = null });

                        myhouse_customize_wuxi.linkman = linkman;
                        IsEdit = true;
                    }
                    //tel
                    if (tel != null && myhouse_customize_wuxi.tel != tel)
                    {
                        if (string.IsNullOrWhiteSpace(tel)) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,该 tel 不能为空", data = null });

                        myhouse_customize_wuxi.tel = tel;
                        IsEdit = true;
                    }

                    //iscustomize
                    if (iscustomize != null && myhouse_customize_wuxi.iscustomize != iscustomize)
                    {
                        myhouse_customize_wuxi.iscustomize = iscustomize;
                        IsEdit = true;
                    }

                    //isdel
                    if (isdel != null && myhouse_customize_wuxi.isdel != isdel)
                    {
                        myhouse_customize_wuxi.isdel = isdel;
                        IsEdit = true;
                    }

                    if (IsEdit)
                    {
                        int isok = db.SaveChanges();
                        if (isok > 0)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功", data = myhouse_customize_wuxi.cusid }, timeFormat);
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败", data = null });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功,没有需要修改的内容", data = myhouse_customize_wuxi.cusid }, timeFormat);
                    }
                }
            }


        }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e
    });
            }
        }

    }

}