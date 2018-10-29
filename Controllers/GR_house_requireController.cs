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



namespace jjr2018.Controllers
{
    public class GR_house_requireController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 列表  http://192.168.1.223/GR_house_require/ListByUser
        /// </summary>
        /// <param name="housetype"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ListByUser(int housetype = 1, int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new shhouseEntities())
            {
                try
                {
                    int allcount = db.house_require_wuxi.Where(p => p.housetype == housetype&& p.userid == User.userid &&p.isdel==0).Count();
                    var house_require_wuxi_list_temp = db.house_require_wuxi.Where(p => p.housetype == housetype && p.userid == User.userid && p.isdel == 0).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                    Dictionary<string, object>[] house_require_wuxi_list = new Dictionary<string, object>[house_require_wuxi_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in house_require_wuxi_list_temp)
                    {
                        house_require_wuxi_list[i] = new Dictionary<string, object>();

                        house_require_wuxi_list[i].Add("eid", temp.eid);
                        house_require_wuxi_list[i].Add("userid", temp.userid);
                        house_require_wuxi_list[i].Add("countyid", temp.countyid);
                        house_require_wuxi_list[i].Add("title", temp.title);
                        house_require_wuxi_list[i].Add("minprice", temp.minprice);
                        house_require_wuxi_list[i].Add("minarea", temp.minarea);
                        house_require_wuxi_list[i].Add("habitableroom", temp.habitableroom);
                        house_require_wuxi_list[i].Add("rentype", temp.rentype);
                        house_require_wuxi_list[i].Add("housetype", temp.housetype);
                        house_require_wuxi_list[i].Add("remark", temp.remark);
                        house_require_wuxi_list[i].Add("linkman", temp.linkman);
                        house_require_wuxi_list[i].Add("tel", temp.tel);
                        house_require_wuxi_list[i].Add("hitcount", temp.hitcount);
                        house_require_wuxi_list[i].Add("isdel", temp.isdel);
                        house_require_wuxi_list[i].Add("addtime", temp.addtime);
                        house_require_wuxi_list[i].Add("unixdate", temp.unixdate);
                        house_require_wuxi_list[i].Add("addip", temp.addip);
                        house_require_wuxi_list[i].Add("city", temp.city);

                        int countyid = Convert.ToInt32(temp.countyid);
                    string county = "";


                    base_area mybase_area = db.base_area.FirstOrDefault(p => p.areaid == countyid);
                    if(mybase_area!=null) county = mybase_area.areaname;



                    house_require_wuxi_list[i].Add("county", county);  
                        i++;
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "求租求租列表",
                        data = new
                        {
                            house_require_wuxi_list,
                            allcount
                        }
                    }, timeFormat);
            }
                catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
            }
        }
        }

        /// <summary>
        /// 详细 http://192.168.1.223/GR_house_require/Find
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        public string Find(int eid)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var house_require_wuxi = ent.house_require_wuxi.FirstOrDefault(p => p.eid == eid);
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = house_require_wuxi
                    }, timeFormat);
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "网路异常"
                    });
                }
            }
        }

        /// <summary>
        /// 添加 http://192.168.1.223/GR_house_require/Add
        /// </summary>
        /// <param name="title"></param>
        /// <param name="minprice"></param>
        /// <param name="minarea"></param>
        /// <param name="housetype"></param>
        /// <param name="remark"></param>
        /// <param name="linkman"></param>
        /// <param name="tel"></param>
        /// <param name="county"></param>
        /// <param name="habitableroom"></param>
        /// <returns></returns>
        [HttpPost]
        public string Add(
        string title,string minprice,string minarea,int housetype,string remark,string linkman,
                string tel,string county,string habitableroom)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            DateTime time = DateTime.Now;
            string addtime = time.ToString();
            int unixdate = Utils.GetUnixNum(time);
            string addip = Utils.GetRealIP();
            int _hid = 0;
            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {

                    int countyid = 0;
                    try
                    {
                        countyid= db.Database.SqlQuery<int>(@"select areaid from dbo.base_area where  areaname=@county", new SqlParameter[] { new SqlParameter("@county", county) }).First();
                    }
                    catch
                    { }
                    house_require_wuxi myhouse_require_wuxi = new house_require_wuxi();
                    myhouse_require_wuxi.userid = userid;
                    myhouse_require_wuxi.countyid = countyid;
                    myhouse_require_wuxi.title = title;
                    myhouse_require_wuxi.minprice = minprice;
                    myhouse_require_wuxi.minarea = minarea;
                    myhouse_require_wuxi.habitableroom = habitableroom;
                    myhouse_require_wuxi.rentype = 0;
                    myhouse_require_wuxi.housetype = housetype;
                    myhouse_require_wuxi.remark = remark;
                    myhouse_require_wuxi.linkman = linkman;
                    myhouse_require_wuxi.tel = linkman + ",," + tel;
                    myhouse_require_wuxi.hitcount = 0;
                    myhouse_require_wuxi.isdel = 0;
                    myhouse_require_wuxi.addtime = time;
                    myhouse_require_wuxi.unixdate = unixdate;
                    myhouse_require_wuxi.addip = addip;
                    db.house_require_wuxi.Add(myhouse_require_wuxi);
                    db.SaveChanges();
                    int eid = myhouse_require_wuxi.eid;

                    if (eid > 0)
                    {
                        // 添加积分
                        appUserScore.ScoreAdd(userid.ToString(), userscore.suggestRent, "app求租房源积分");

                         return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "发布成功",
                            data = eid
                        }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state =2,
                            msg = "发布失败",
                            data = null
                        });
                    }                
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "发布失败，请稍后再试"
                    });
                }
            }
        }


        /// <summary>
        /// 修改  http://192.168.1.223/GR_house_require/Edit
        /// </summary>
        /// <param name="eid"></param>
        /// <param name="title"></param>
        /// <param name="minprice"></param>
        /// <param name="minarea"></param>
        /// <param name="housetype"></param>
        /// <param name="remark"></param>
        /// <param name="linkman"></param>
        /// <param name="tel"></param>
        /// <param name="county"></param>
        /// <param name="habitableroom"></param>
        /// <returns></returns>
        [HttpPost]
        public string Edit(int eid, string title, string minprice, string minarea, int housetype, string remark, string linkman,string tel, string county, string habitableroom)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {
                    int countyid = 0;
                    try
                    {
                        countyid= db.Database.SqlQuery<int>(@"select areaid from dbo.base_area where  areaname=@county", new SqlParameter[] { new SqlParameter("@county", county) }).First();
                    }
                    catch
                    { }

                    house_require_wuxi myhouse_require_wuxi = db.house_require_wuxi.Find(eid);
                    if (userid != myhouse_require_wuxi.userid)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "非本人信息不能修改",
                            data = null
                        });
                    }

                    myhouse_require_wuxi.countyid = countyid;
                    myhouse_require_wuxi.title = title;
                    myhouse_require_wuxi.minprice = minprice;
                    myhouse_require_wuxi.minarea = minarea;
                    myhouse_require_wuxi.habitableroom = habitableroom;
                    myhouse_require_wuxi.rentype = 0;
                    myhouse_require_wuxi.housetype = housetype;
                    myhouse_require_wuxi.remark = remark;
                    myhouse_require_wuxi.linkman = linkman;
                    myhouse_require_wuxi.tel = linkman + ",," + tel;    
                    
                    int isok=  db.SaveChanges();   
                    if (isok > 0)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "修改成功",
                            data = eid
                        }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "修改失败",
                            data = null
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "修改失败，请稍后再试"
                    });
                }
            }
        }
        /// <summary>
        /// 删除求购求租 http://192.168.1.223/GR_house_require/Delete
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        public string Delete(int eid) 
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var house_require_wuxi = ent.house_require_wuxi.FirstOrDefault(p => p.eid == eid);
                    if (User.userid != house_require_wuxi.userid)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "非本人信息不能修改",
                            data = null
                        });
                    }
                    house_require_wuxi.isdel = 1;
                    int isok = ent.SaveChanges();
                    if (isok > 0)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "修改成功",
                            data = eid
                        }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "修改失败",
                            data = null
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "网路异常"
                    });
                }
            }
        }
    }

}