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
using Dapper;

namespace jjr2018.Controllers
{
    public class house_agentizeController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 经纪人查看客户需求列表通用 http://192.168.4.111/house_agentize/ListByWhere
        /// </summary>
        /// <param name="AgentID">经纪人ID 类型 int</param>
        /// <param name="IsLink">是否联系 类型 int</param>
        /// <param name="IsFollow">是否跟进 类型 int</param>
        /// <param name="IsDel">是否委托 类型 int</param>
        /// <param name="housetype">housetype 说明：1求购2求租  类型：int</param>
        /// <param name="SortName">排序字段名</param>
        /// <param name="IsDesc">升序降序</param>
        /// <param name="IsAgentize">待分配已分配状态</param>
        /// <returns></returns>
        public string ListByWhere(int? AgentID = null, int? IsLink = null, int? IsFollow = null, int? IsDel = null, int? housetype = null, string SortName = "exe_date", bool IsDesc = true, bool? IsAgentize = null)
        {

            try
            {
                timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                using (var db = new shhouseEntities())
                {
                    IQueryable<house_agentize_wuxi> result = db.house_agentize_wuxi;
                    if (AgentID != null) result = result.Where(p => p.Agentid == AgentID);
                    if (IsLink != null) {

                        DataTable dt = Utils.Query(shhouseconn, "SELECT ID FROM house_agentize_wuxi  a LEFT JOIN house_customize_wuxi b ON A.Cusid=B.cusid WHERE a.isLink=0 and a.isdel = 0 and a.IsFollow = 0 and  b.isdel = 0 and b.iscustomize = 1");
                        int[] IDss = new int[dt.Rows.Count];
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            IDss[j] = int.Parse(dt.Rows[j][0].ToString());
                        }
                        result = result.Where(p => IDss.Contains(p.ID));

                    }
                    if (IsFollow != null)
                    {
                        DataTable dt = Utils.Query(shhouseconn, "SELECT ID FROM house_agentize_wuxi  a LEFT JOIN house_customize_wuxi b ON A.Cusid=B.cusid WHERE a.isfollow = 1 and a.isdel = 0 and b.isdel = 0 and iscustomize = 1");
                        int[] IDss = new int[dt.Rows.Count];
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            IDss[j] = int.Parse(dt.Rows[j][0].ToString());
                        }
                        result = result.Where(p => IDss.Contains(p.ID));


                    }
                    //包含客户取消,经纪人自己取消,客户禁止经纪人
                    if (IsDel != null) {
                        if (IsDel == 1) {
                           
                            DataTable dt = Utils.Query(shhouseconn, "select ID from house_agentize_wuxi where (IsDel=1 or Cusid in(select cusid from house_customize_wuxi where isdel=1 or iscustomize=0))");
                            int[] IDss = new int[dt.Rows.Count];
                            for (int j = 0; j < dt.Rows.Count; j++) {
                                IDss[j] = int.Parse(dt.Rows[j][0].ToString());
                            }
                            result = result.Where(p => IDss.Contains(p.ID));
                        }
                        else {
                            result = result.Where(p => p.IsDel == IsDel);
                        }

                    }
        

                    if (IsAgentize != null)
                    {
                        if (Convert.ToBoolean(IsAgentize))
                        {
                            result = result.Where(p => db.house_agentize_wuxi.Where(a => a.IsDel != 1).GroupBy(a => a.Cusid).Select(a => new { Cusid = a.Key, Count = a.Count() }).Where(a => a.Count >= 3).Select(a => a.Cusid).Contains(p.Cusid));
                        }
                        else
                        {
                            result = result.Where(p => !db.house_agentize_wuxi.Where(a => a.IsDel != 1).GroupBy(a => a.Cusid).Select(a => new { Cusid = a.Key, Count = a.Count() }).Where(a => a.Count >= 3).Select(a => a.Cusid).Contains(p.Cusid));
                        }
                    }
                    if (housetype != null) result = result.Where(p => db.house_customize_wuxi.Where(a => a.housetype == housetype).Select(a => a.cusid).Contains(p.Cusid));
                    int count = result.Count();
                    var house_agentize_wuxi_List_temp = result.OrderBy(string.Format("{0} {1}", SortName, IsDesc ? "desc" : "asc"));

                    Dictionary<string, object>[] house_agentize_wuxi_List = new Dictionary<string, object>[house_agentize_wuxi_List_temp.Count()];
                    int i = 0;
                    foreach (var temp in house_agentize_wuxi_List_temp)
                    {
                        house_customize_wuxi cus = db.house_customize_wuxi.Where(p => p.cusid == temp.Cusid).FirstOrDefault();
                        house_agentize_wuxi_List[i] = new Dictionary<string, object>();
                        //house_agentize_wuxi_List_temp表
                        house_agentize_wuxi_List[i].Add("ID", temp.ID);
                        house_agentize_wuxi_List[i].Add("AgentID", temp.Agentid);
                        house_agentize_wuxi_List[i].Add("IsLink", temp.IsLink);
                        house_agentize_wuxi_List[i].Add("IsFollow", temp.IsFollow);
                        house_agentize_wuxi_List[i].Add("IsDel", temp.IsDel);
                        house_agentize_wuxi_List[i].Add("exe_date", temp.exe_date);
                        //house_customize_wuxi_List_temp表
                        house_agentize_wuxi_List[i].Add("cusid", cus.cusid);
                        house_agentize_wuxi_List[i].Add("userid", cus.userid);
                        house_agentize_wuxi_List[i].Add("areaID", cus.areaID);
                        house_agentize_wuxi_List[i].Add("tradingareaID", cus.tradingareaID);
                        house_agentize_wuxi_List[i].Add("countyIdList", cus.countyIdList);
                        house_agentize_wuxi_List[i].Add("jiageId", cus.jiageId);
                        house_agentize_wuxi_List[i].Add("room", cus.room);
                        house_agentize_wuxi_List[i].Add("housetype", cus.housetype);
                        house_agentize_wuxi_List[i].Add("rentype", cus.rentype);
                        house_agentize_wuxi_List[i].Add("linkman", cus.linkman);
                        house_agentize_wuxi_List[i].Add("tel", cus.tel);
                        house_agentize_wuxi_List[i].Add("iscustomize", cus.iscustomize);
                        house_agentize_wuxi_List[i].Add("cusisdel", cus.isdel);
                        house_agentize_wuxi_List[i].Add("city", cus.city);
                        house_agentize_wuxi_List[i].Add("addtime", cus.addtime);

                        string areaname = "";
                        if (cus.areaID != null && cus.areaID != 0)
                        {
                            var areaIDtemp = db.base_area.Where(p => p.areaid == cus.areaID).OrderBy(p => p.areaid).FirstOrDefault();
                            if (areaIDtemp != null)
                            { areaname = areaIDtemp.areaname; }
                        }
                        house_agentize_wuxi_List[i].Add("areaname", areaname);

                        string tradingareaname = "";
                        if (cus.tradingareaID != null && cus.tradingareaID != 0)
                        {
                            var tradingareaIDtemp = db.base_area.Where(p => p.areaid == cus.tradingareaID).OrderBy(p => p.areaid).FirstOrDefault();
                            if (tradingareaIDtemp != null)
                            { tradingareaname = tradingareaIDtemp.areaname; }
                        }
                        house_agentize_wuxi_List[i].Add("tradingareaname", tradingareaname);


                        using (shvillageEntities dbshvillage = new shvillageEntities())
                        {
                            if (!string.IsNullOrWhiteSpace(cus.countyIdList))
                            {
                                string[] str_countyIdList = cus.countyIdList.Split(',').ToArray();
                                int[] int_countyIdList = Array.ConvertAll(str_countyIdList, new Converter<string, int>(Utils.StrToInt));
                                var countyList = dbshvillage.NewHouse.Where(p => int_countyIdList.Contains(p.ID)).Select(a => new { ID = a.ID, title = a.Name }).OrderBy(p => p.ID).ToList();
                                house_agentize_wuxi_List[i].Add("countyList", countyList);
                            }
                            else
                            {
                                house_agentize_wuxi_List[i].Add("countyList", null);
                            }
                        }


                        string jiagename = null;
                        if (cus.jiageId != null && cus.jiageId != 0)
                        {
                            var jiagetemp = db.base_samtype.Where(p => p.typeid == cus.jiageId).FirstOrDefault();
                            if (jiagetemp != null) { jiagename = jiagetemp.typename; }
                        }
                        house_agentize_wuxi_List[i].Add("jiagename", jiagename);

                        string rentypename = null;
                        if (cus.rentype != null && cus.rentype != 0)
                        {
                            var rentypetemp = db.base_samtype.Where(p => p.typeid == cus.rentype).FirstOrDefault();
                            if (rentypetemp != null) { rentypename = rentypetemp.typename; }
                        }
                        house_agentize_wuxi_List[i].Add("rentypename", rentypename);

                        int AgentizeNumber = db.house_agentize_wuxi.Where(p => p.Cusid == cus.cusid && p.IsDel != 1).Count();
                        house_agentize_wuxi_List[i].Add("AgentizeNumber", AgentizeNumber);


                        string photoname = null;
                        if (cus.userid != null && cus.userid != 0)
                        {
                            var user_detailstemp = db.user_details.Where(p => p.userid == cus.userid).FirstOrDefault();
                            if (user_detailstemp != null) { photoname = user_detailstemp.photoname; }
                        }
                        house_agentize_wuxi_List[i].Add("photoname", photoname);

                        i++;
                    }

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "", data = new { house_agentize_wuxi_List, count } }, timeFormat);
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }
        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IsLink"></param>
        /// <param name="IsFollow"></param>
        /// <param name="IsDel"></param>
        /// <returns></returns>
        public string EditState(int ID,int? IsLink = null, int? IsFollow = null, int? IsDel = null)
        {
            try
            {
                using (var db = new shhouseEntities())
                {
                    house_agentize_wuxi agentize = db.house_agentize_wuxi.Find(ID);
                    if (IsDel != null) agentize.IsDel = IsDel;
                    if (IsLink != null) agentize.IsLink = IsLink;
                    if (IsFollow != null) agentize.IsFollow = IsFollow;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功!" });
                }
             }
            catch (Exception e)
            {

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }


        }

        /// <summary>
        /// 添加取消委托记录
        /// </summary>
        /// <param name="FollowID">跟进原因ID </param>
        /// <param name="Followtext">跟进原因文本</param>
        /// <param name="Cusid">需求ID</param>
        /// <returns></returns>
        public string AddFollow(int FollowID, string Followtext, int Cusid)
        {
            try
            {
                timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                using (var db = new shhouseEntities())
                {
                    var follow = new house_agentfollow_wuxi
                    {
                        Cusid = Cusid,
                        AgentID = User.userid,
                        FollowText = Followtext,
                        exe_date = DateTime.Now
                    };
                    db.house_agentfollow_wuxi.Add(follow);
                    db.SaveChanges();
                }
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "取消委托成功!" });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }


        /// <summary>
        /// 获取客户需求
        /// </summary>
        /// <param name="Cusid">需求ID</param>
        /// <returns></returns>
        public string GetCus(int Cusid)
        {
            try
            {
                var param = new Dapper.DynamicParameters();
                param.Add("@Agentid", User.userid);
                param.Add("@Cusid", Cusid);

                param.Add("@state", 0, DbType.Int32, ParameterDirection.Output);
                param.Add("@msg", 0, DbType.String, ParameterDirection.Output, size: 100);
                var res2 = shhouseconn.Execute("exchange_GetCus", param, null, null, CommandType.StoredProcedure);

                int _state = param.Get<int>("@state");
                string msg = param.Get<string>("@msg");
                return JsonConvert.SerializeObject(new
                {
                    state = _state,
                    msg = msg
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常，请稍后再试!", data = e });
            }
        }



    }
}