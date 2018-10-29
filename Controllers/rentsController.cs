using jjr2018.Common;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace jjr2018.Controllers
{
    public class rentsController : jjrbasicController
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
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("a.titletag like @keyword");
                where2.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select rentid,titles,villagename,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice,Istop,addtime,updatetime,smallpath, 
customid,isdel,hitcount from (SELECT a.rentid, b.titles,b.villagename,b.shangquan, a.room, b.hall, b.toilet, a.minarea, b.layer,smallpath,
b.totallayer, a.minprice,a.Istop, b.customid,a.isdel,a.hitcount,b.addtime,a.updatetime, ROW_NUMBER() over(order by unixdate desc,a.rentid desc) as rows
FROM house_rent_search_wuxi a INNER JOIN house_rent_list_wuxi b ON a.rentid = b.rentid
INNER JOIN house_rent_detail_wuxi c on a.rentid = c.rentid { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from house_rent_search_wuxi a { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                  

                    var datas_fb = ent.Database.SqlQuery<int>("select count(1) from house_rent_search_wuxi a where isdel=0 and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    var datas_zd = ent.Database.SqlQuery<int>("select count(1) from house_rent_search_wuxi a where istop=1 and isdel=0 and topend>=getdate() and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    var datas_xj = ent.Database.SqlQuery<int>("select count(1) from house_rent_search_wuxi a where isdel=-1 and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas,
                            count = datas_c,
                            count_fb = datas_fb,
                            count_zd = datas_zd,
                            count_xj = datas_xj
                        }
                    }, timeFormat);
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
                    var info1 = ent.house_rent_search_wuxi.FirstOrDefault(p => p.rentid == houseid && p.userid == userid);
                    var info2 = ent.house_rent_list_wuxi.FirstOrDefault(p => p.rentid == houseid && p.userid == userid);
                    var info3 = ent.house_rent_detail_wuxi.FirstOrDefault(p => p.rentid == houseid && p.userid == userid);
                    var info4 = ent.house_rent_img_wuxi.Where(p => p.houseid == houseid && (p.pictypeid == 0 || p.pictypeid == null)).Select(p => p.imgurl).ToList();
                    var info5 = ent.house_rent_img_wuxi.Where(p => p.houseid == houseid && p.pictypeid == 1).Select(p => p.imgurl).ToList();


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
                            renttype = info1.renttype,
                            rentype = info1.rentype,
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
                            updatetime = str,
                            addtime = info2.addtime,
                            istop= info1.istop,
                            imgs1 = string.Join(",", info4),
                            imgs2 = string.Join(",", info5),
                            isdel = info1.isdel
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
        /// 
        /// </summary>
        /// <param name="villageid"></param>
        /// <param name="room"></param>
        /// <param name="hall"></param>
        /// <param name="toilet"></param>
        /// <param name="minprice"></param>
        /// <param name="minarea"></param>
        /// <param name="layer"></param>
        /// <param name="totallayer"></param>
        /// <param name="nature"></param>
        /// <param name="directions"></param>
        /// <param name="fitment"></param>
        /// <param name="villagename"></param>
        /// <param name="address"></param>
        /// <param name="titles"></param>
        /// <param name="smallpath"></param>
        /// <param name="imgcount"></param>
        /// <param name="customid"></param>
        /// <param name="condition"></param>
        /// <param name="remark"></param>
        /// <param name="imgurlht"></param>
        /// <param name="shangquan"></param>
        /// <param name="county"></param>
        /// <param name="lift"></param>
        /// <param name="buildyear"></param>
        /// <param name="renttype"></param>
        /// <param name="rentype"></param>
        /// <param name="imgs1"></param>
        /// <param name="imgs2"></param>
        /// <param name="isdel"></param>
        /// <param name="sourcefrom"></param>
        /// <returns></returns>
        [HttpPost]
        public string Add(int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
            string smallpath, short imgcount, string customid, string condition, string remark,string imgurlht,
            string shangquan, string county, int lift, int buildyear, int renttype, int rentype,
            string imgs1, string imgs2, short isdel, int sourcefrom)
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
            dp.Add("@smallpath", smallpath);
            dp.Add("@imgcount", imgcount);
            dp.Add("@customid", customid);
            dp.Add("@condition", condition);
            dp.Add("@remark", Common.Utils.replace1(remark, keywords));
            dp.Add("@imgurlht", imgurlht);
            dp.Add("@shangquan", shangquan);
            dp.Add("@county", county);
            dp.Add("@lift", lift);
            dp.Add("@buildyear", buildyear);
            dp.Add("@renttype", renttype);
            dp.Add("@isdel", isdel);
            dp.Add("@sourcefrom", sourcefrom);
            dp.Add("@addip", addip);
            dp.Add("@rentype", rentype);
            dp.Add("@imgs1", imgs1);
            dp.Add("@imgs2", imgs2);
            dp.Add("@houseid", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

            try
            {
                int c = conn.Execute("renthouse_add_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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

        
        [HttpPost]
        [ValidateInput(false)]
        public string Edit(int houseid, int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
            string smallpath, short imgcount, string customid, string condition, string remark, string imgurlht,
            string shangquan, string county, int lift, int buildyear, int renttype, int rentype, short isdel,
            string imgs1, string imgs2)
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
            dp.Add("@smallpath", smallpath);
            dp.Add("@imgcount", imgcount);
            dp.Add("@customid", customid);
            dp.Add("@condition", condition);

            dp.Add("@remark", Common.Utils.replace1(remark, keywords));
            dp.Add("@imgurlht", imgurlht);
            dp.Add("@shangquan", shangquan);
            dp.Add("@county", county);
            dp.Add("@lift", lift);

            dp.Add("@buildyear", buildyear);
            dp.Add("@renttype", renttype);
            dp.Add("@isdel", isdel);
            dp.Add("@rentype", rentype);
            dp.Add("@imgs1", imgs1);

            dp.Add("@imgs2", imgs2);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

            try
            {
                int c = conn.Execute("renthouse_edit_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
            dp.Add("@rentid", houseid);
            dp.Add("@days", days);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("renthouse_ontop_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
            dp.Add("@rentid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("renthouse_onsale_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
            dp.Add("@rentid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("renthouse_offsale_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
            dp.Add("@rentid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("renthouse_refresh_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
            dp.Add("@rentid", houseid);
            dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            try
            {
                int c = conn.Execute("renthouse_delete_jjr2018", param: dp, commandType: CommandType.StoredProcedure);
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
    }
}