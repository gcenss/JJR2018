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
using Dapper;
using System.Reflection;

namespace jjr2018.Controllers
{
    public class  salesController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 查询房源列表
        /// </summary>
        /// <param name="housetype">1已发布，2置顶，3下架，4草稿</param>
        /// <param name="keyword"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string List(int housetype = 1, string keyword = "", int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";
            switch (housetype)
            {
                case 1:
                    where1.Add("a.isdel=0");
                    break;
                case 2:
                    where1.Add("a.istop=1 and a.isdel=0 and a.topend>=getdate()");
                    break;
                case 3:
                    where1.Add("a.isdel=-1");
                    break;
                case 4:
                    where1.Add("a.isdel=-10");
                    break;
            }
            where1.Add($"a.userid={userid}");
            where1.Add("a.labelstate<>9");
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("a.titletag like @keyword");
                where2.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select saleid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice,smallpath,ISNULL(sharenum,0)sharenum,housesharenum,
customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villagename,addtime,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum,directionsvar,fitmentvar from (SELECT a.saleid, b.titles, b.shangquan, a.room, b.hall, b.toilet, a.minarea, b.layer,g.num sharenum,h.num as housesharenum,  
b.totallayer, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,b.villagename,b.addtime,a.updatetime,d.clicknum,b.smallpath,directionsvar, fitmentvar,ROW_NUMBER() over(order by unixdate desc,a.saleid desc) as rows
FROM house_sale_search_wuxi a INNER JOIN house_sale_list_wuxi b ON a.saleid = b.saleid
LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.saleid
INNER JOIN house_sale_detail_wuxi c on a.saleid = c.saleid 
left join(select sum(num)num,ContentID from ShareLog  where type=2 group by ContentID)g on a.saleid=g.ContentID 
left join(select sum(num)num,ContentID from ShareLog  where type=1 group by ContentID)h on a.saleid=h.ContentID{ _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";
            string sql_c = $@"select count(1) from house_sale_search_wuxi a { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                 
                    var datas_fb = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where isdel=0 and labelstate<>9 and userid="+ userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    var datas_zd = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where istop=1 and isdel=0 and topend>=getdate() and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    var datas_xj = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where isdel=-1 and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas,
                            count = datas_c,
                            count_fb= datas_fb,
                            count_zd= datas_zd,
                            count_xj= datas_xj
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                       
                        msg = "没有找到房源信息"
                    });
                }
            }
        }

        /// <summary>
        /// 同list，dapper方式，备用
        /// </summary>
        /// <param name="housetype"></param>
        /// <param name="keyword"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string list2(int housetype = 1, string keyword = "", int pagesize = 20, int pageindex = 1)
        {

            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            DynamicParameters dp = new DynamicParameters();
           // List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";

            switch (housetype)
            {
                case 1:
                    where1.Add("a.isdel=0");
                    break;
                case 2:
                    where1.Add("a.istop=1 and a.isdel=0");
                    break;
                case 3:
                    where1.Add("a.isdel=-1");
                    break;
                case 4:
                    where1.Add("a.isdel=-10");
                    break;
            }
            where1.Add($"a.userid={userid}");
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("a.titletag like @keyword");
                dp.Add("@keyword", "%" + keyword + "%");
                //where2.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select saleid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice, 
customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villagename,addtime,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum from (SELECT a.saleid, b.titles, b.shangquan, a.room, b.hall, b.toilet, a.minarea, b.layer, 
b.totallayer, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,b.villagename,b.addtime,a.updatetime,d.clicknum,ROW_NUMBER() over(order by unixdate desc,a.saleid desc) as rows
FROM house_sale_search_wuxi a INNER JOIN house_sale_list_wuxi b ON a.saleid = b.saleid
LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.saleid
INNER JOIN house_sale_detail_wuxi c on a.saleid = c.saleid { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from house_sale_search_wuxi a { _where }";
            
                try
                {
                    //var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    //var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = conn.Query(sql, dp).ToList(),
                            count = conn.QuerySingle<int>(sql_c, dp)
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "没有找到房源信息"
                    });
                }
            
        }
        /// <summary>
        /// 查询房源
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string Find(int houseid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var info1 = ent.house_sale_search_wuxi.FirstOrDefault(p => p.saleid == houseid && p.userid == userid);
                    var info2 = ent.house_sale_list_wuxi.FirstOrDefault(p => p.saleid == houseid && p.userid == userid);
                    var info3 = ent.house_sale_detail_wuxi.FirstOrDefault(p => p.saleid == houseid && p.userid == userid);
                    var info4 = ent.house_sale_img_wuxi.Where(p => p.houseid == houseid && (p.pictypeid == 0 || p.pictypeid == null)).Select(p => p.imgurl).ToList();
                    var info5 = ent.house_sale_img_wuxi.Where(p => p.houseid == houseid && p.pictypeid == 1).Select(p => p.imgurl).ToList();
                    var info6 = ent.user_search_all_wuxi.FirstOrDefault(p => p.UserID == userid);
                    //更新时间
                    DateTime dttime = new DateTime();
                    dttime = DateTime.Parse(info1.updatetime.ToString());
                    TimeSpan t = DateTime.Now - dttime;
                    string str = "(" + dttime.ToString("yyyy-MM-dd") + ")";
                    if (t.TotalSeconds > 0)
                    {
                        if (t.Days > 0 && t.Days <= 7)
                        {
                            t = DateTime.Now.Date - dttime.Date;
                            str = " （" + t.Days.ToString() + "天前更新）";
                        }
                        else if (t.Days > 0 && t.Days > 7)
                        {

                        }
                        else if (t.Hours > 0)
                            str = " （" + Math.Round(t.TotalHours).ToString() + "小时前更新）";
                        else if (t.Minutes > 0)
                            str = " （" + Math.Round(t.TotalMinutes).ToString() + "分钟前更新）";
                        else
                            str = " （" + Math.Round(t.TotalSeconds).ToString() + "秒前更新）";
                    }
                    else
                        str = " （刚刚更新）";

                    if (info1 == null || info2 == null || info3 == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到房源信息"
                        });
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            houseid = houseid,
                            villageid = info1.villageid,
                            room = info1.room,
                            hall = info2.hall,
                            toilet = info2.toilet,
                            minprice = info1.minprice,
                            minarea = info1.minarea,
                            layer = info2.layer,
                            totallayer = info2.totallayer,
                            nature = info1.nature,
                            directions = info1.directions,
                            fitment = info1.fitment,
                            villagename = info2.villagename,
                            address = info2.address,
                            titles = info2.titles,
                            tags = info2.tags,
                            smallpath = info2.smallpath,
                            imgcount = info2.imgcount,
                            isyear5 = info1.isyear5,
                            customid = info2.customid,
                            condition = info3.condition,
                            remark = info3.remark,
                            infrastructure = info3.infrastructure,
                            imgurlht = info3.imgurlht,
                            shangquan = info2.shangquan,
                            county = info2.county,
                            lift = info2.lift,
                            buildyear = info2.buildyear,
                            isonly = info2.isonly,
                            yjrate = info2.yjrate,
                            istop = info1.istop,
                            updatetime = str,
                            addtime = info2.addtime,
                            imgs1 = string.Join(",", info4),
                            imgs2 = string.Join(",", info5),
                            isdel = info1.isdel,
                            searchTitle=info6.searchTitle
                        }
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "没有找到房源信息"
                    });
                }
            }
        }

        /// <summary>
        ///  提交二手房房源
        /// </summary>
        /// <param name="villageid">小区ID</param>
        /// <param name="room">室</param>
        /// <param name="hall">厅</param>
        /// <param name="toilet">卫</param>
        /// <param name="minprice">价格</param>
        /// <param name="minarea">面积</param>
        /// <param name="layer">楼层</param>
        /// <param name="totallayer">总楼层</param>
        /// <param name="nature">房屋性质id</param>
        /// <param name="directions">朝向id</param>
        /// <param name="fitment">装修id</param>
        /// <param name="villagename">小区名称</param>
        /// <param name="address">小区地址</param>
        /// <param name="titles">标题</param>
        /// <param name="tags">标签</param>
        /// <param name="smallpath">列表页缩略图/封面图</param>
        /// <param name="imgcount">图片总数</param>
        /// <param name="isyear5">是否满5年，0：满2年，1：满5年，2：不满2年</param>
        /// <param name="customid">用户自定义的内部编号</param>
        /// <param name="condition">屋内配套设施</param>
        /// <param name="remark">简介</param>
        /// <param name="infrastructure">周边配套</param>
        /// <param name="imgurlht">户型图</param>
        /// <param name="shangquan">商圈名</param>
        /// <param name="county">区域名</param>
        /// <param name="lift">是否有电梯，0：无，1：有</param>
        /// <param name="buildyear">建筑年代，如：1990</param>
        /// <param name="isonly">是否唯一一套，0：不是，1：是</param>
        /// <param name="yjrate">佣金比率</param>
        /// <param name="imgs1">房源图</param>
        /// <param name="imgs2">户型图</param>
        /// <param name="isdel">0上架，-1删除，-10草稿</param>
        /// <returns></returns>
        [HttpPost]
        public string Add(int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
            string tags, string smallpath, short imgcount, short isyear5, string customid, string condition, string remark,
            string infrastructure, string imgurlht, string shangquan, string county, int lift, int buildyear, int isonly,
            decimal yjrate, string imgs1, string imgs2, short isdel, int sourcefrom)
        {

            int userid = User.userid;
            string addip = Utils.GetRealIP();
            string keywords = User.keywords;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@villageid", villageid);
            dp.Add("@room", room);
            dp.Add("@hall", hall);
            dp.Add("@toilet", toilet);
            dp.Add("@minprice", minprice);
            dp.Add("@minarea", minarea);
            dp.Add("@layer", layer);
            dp.Add("@totallayer", totallayer);
            dp.Add("@nature", nature);
            dp.Add("@directions", directions);
            dp.Add("@fitment", fitment);
            dp.Add("@villagename", villagename);
            dp.Add("@address", address);
            dp.Add("@titles", Common.Utils.replace1(titles, keywords));
            dp.Add("@tags", tags);
            dp.Add("@smallpath", smallpath);
            dp.Add("@imgcount", imgcount);
            dp.Add("@isyear5", isyear5);
            dp.Add("@customid", customid);
            dp.Add("@condition", condition);
            dp.Add("@remark", Common.Utils.replace1(remark, keywords));
            dp.Add("@infrastructure", infrastructure);
            dp.Add("@imgurlht", imgurlht);
            dp.Add("@shangquan", shangquan);
            dp.Add("@county", county);
            dp.Add("@lift", lift);
            dp.Add("@buildyear", buildyear);
            dp.Add("@isonly", isonly == 1);
            dp.Add("@yjrate", yjrate);
            dp.Add("@imgs1", imgs1);
            dp.Add("@imgs2", imgs2);
            dp.Add("@addip", addip);
            dp.Add("@isdel", isdel);
            dp.Add("@sourcefrom", sourcefrom);
            dp.Add("@houseid", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("salehouse_add_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");

                if (s == 1)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = s,
                        msg = dp.Get<string>("@msg"),
                        data = dp.Get<int>("@houseid")
                    });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = dp.Get<string>("@msg")
                    });
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = "提交失败"
                });
            }
        }

        /// <summary>
        /// 修改房源
        /// </summary>
        [HttpPost]
        [ValidateInput(false)]
        public string Edit(int houseid, int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
            string tags, string smallpath, short imgcount, short isyear5, string customid, string condition, string remark,
            string infrastructure, string imgurlht, string shangquan, string county, int lift, int buildyear, int isonly,
            decimal yjrate, string imgs1, string imgs2, short isdel)
        {
            int userid = User.userid;
            string keywords = User.keywords;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@houseid", houseid);
            dp.Add("@userid", userid);
            dp.Add("@villageid", villageid);
            dp.Add("@room", room);
            dp.Add("@hall", hall);
            dp.Add("@toilet", toilet);
            dp.Add("@minprice", minprice);
            dp.Add("@minarea", minarea);
            dp.Add("@layer", layer);
            dp.Add("@totallayer", totallayer);
            dp.Add("@nature", nature);
            dp.Add("@directions", directions);
            dp.Add("@fitment", fitment);
            dp.Add("@villagename", villagename);
            dp.Add("@address", address);
            dp.Add("@titles", Common.Utils.replace1(titles, keywords));
            dp.Add("@tags", tags);
            dp.Add("@smallpath", smallpath);
            dp.Add("@imgcount", imgcount);
            dp.Add("@isyear5", isyear5);
            dp.Add("@customid", customid);
            dp.Add("@condition", condition);
            dp.Add("@remark", Common.Utils.replace1(remark, keywords));
            dp.Add("@infrastructure", infrastructure);
            dp.Add("@imgurlht", imgurlht);
            dp.Add("@shangquan", shangquan);
            dp.Add("@county", county);
            dp.Add("@lift", lift);
            dp.Add("@buildyear", buildyear);
            dp.Add("@isonly", isonly == 1);
            dp.Add("@yjrate", yjrate);
            dp.Add("@imgs1", imgs1);
            dp.Add("@imgs2", imgs2);
            dp.Add("@isdel", isdel);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("salehouse_edit_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");
                if (s == 1)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = s,
                        msg = dp.Get<string>("@msg"),
                        data = dp.Get<int>("@houseid")
                    });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = dp.Get<string>("@msg")
                    });
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = "提交失败"
                });
            }
        }

        /// <summary>
        /// 查询房源的置顶价格
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string GetEB(int houseid)
        {
            int ebx = 0;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    int? sqid = ent.house_sale_search_wuxi.Where(p => p.saleid == houseid).Select(p => p.shangquanid).First();
                    if (sqid.HasValue && sqid > 0)
                    {
                        int? eb = ent.base_area.Where(p => p.areaid == sqid.Value).Select(p => p.topEB).First();
                        if (eb.HasValue)
                        {
                            ebx = eb.Value;
                        }
                        else
                        {
                            ebx = 0;
                        }
                    }
                    else
                    {

                        ebx = 0;
                    }
                }
                catch
                {
                    ebx = 0;
                }
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "",
                    data = ebx
                });
            }
        }

        /// <summary>
        /// 房源置顶
        /// </summary>
        /// <param name="houseid"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        [HttpGet]
        public string SetOnTop(int houseid, int days)
        {
            int userid = User.userid;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@saleid", houseid);
            dp.Add("@days", days);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("salehouse_ontop_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = s,
                    msg = dp.Get<string>("@msg")
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = e.Message
                });
            }
        }


        /// <summary>
        /// 设置房源上架
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string SetOnSale(int houseid)
        {
            int userid = User.userid;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@saleid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("salehouse_onsale_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = s,
                    msg = dp.Get<string>("@msg")
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = e.Message
                });
            }
        }


        /// <summary>
        /// 设置房源下架
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string SetOffSale(int houseid)
        {
            int userid = User.userid;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@saleid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size:100);


            try
            {
                int c = conn.Execute("salehouse_offsale_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");
            
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = s,
                    msg = dp.Get<string>("@msg")
                });
            }
            catch(Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg =e.Message
                });
            }

            
        }


        /// <summary>
        /// 手动刷新房源
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string SetRefresh(int houseid)
        {
            int userid = User.userid;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@saleid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("salehouse_refresh_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = s,
                    msg = dp.Get<string>("@msg")
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = e.Message
                });
            }
            
        }


        /// <summary>
        /// 彻底删除
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string Delete(int houseid)
        {
            int userid = User.userid;
            DynamicParameters dp = new DynamicParameters();
            dp.Add("@userid", userid);
            dp.Add("@saleid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);


            try
            {
                int c = conn.Execute("salehouse_delete_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
                int s = dp.Get<int>("@state");

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = s,
                    msg = dp.Get<string>("@msg")
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = e.Message
                });
            }
        }

        [HttpGet]
        public string ListTempl(string keyword, int? type, int pageindex=1, int pagesize = 20)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";
            where1.Add($"userid={userid}");
            if (type.HasValue)
            {
                where1.Add($"ttype={type}");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("titletag like @keyword");
                where2.Add(new SqlParameter("", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select templateid, ttype, tname, createtime,tcontent
from (SELECT templateid, ttype, tname, createtime,tcontent, 
ROW_NUMBER() over(order by templateid desc) as rows
FROM house_template { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from house_template { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var parms1 = new object[] { };
                    if (where2.Count > 0)
                    {
                        parms1 = where2.ToArray();
                    }
                    var datas = ent.Database.DynamicSqlQuery(sql, parms1);
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, parms1).First();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas,
                            count = datas_c
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "没有找到模板信息"
                    });
                }
            }
        }

        [HttpPost]
        public string AddTempl(int ttype, string tname, string tcontent)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tt = new house_template()
                    {
                        ttype = ttype,
                        tname = tname,
                        tcontent = tcontent,
                        userid = userid,
                        createtime = DateTime.Now
                    };
                    ent.house_template.Add(tt);
                    ent.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "提交成功"
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpPost]
        public string EditTempl(int templateid, int ttype, string tname, string tcontent)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        tm.ttype = ttype;
                        tm.tname = tname;
                        tm.tcontent = tcontent;
                        ent.SaveChanges();
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "提交成功"
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpGet]
        public string DeleteTempl(int templateid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        ent.house_template.Remove(tm);
                        ent.SaveChanges();

                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "提交成功"
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpGet]
        public string FindTempl(int templateid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "",
                            data = tm
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        //重算房源量
        [HttpGet]
        public string resetcount()
        {
            string sql = @"
select * from (
select a.userid, houseusenum,
isnull(c1,0) c1, isnull( c2 ,0) c2
from user_member a 
left join
(select userid, count(1) c1 from house_sale_search_wuxi where isdel=0 and labelstate<>9 group by userid) b on a.userid=b.userid
left join
(select userid, count(1) c2 from house_rent_search_wuxi where isdel=0 group by userid) c on a.userid=c.userid
) t where (c1+c2)<>houseusenum;
";
            List<dynamic> items = conn.Query(sql).ToList();
            foreach(var obj in items)
            {
                conn.Execute($"update user_member set houseusenum={obj.c1 + obj.c2} where userid={obj.userid}");
            }
            return "1";
        }
    }

}