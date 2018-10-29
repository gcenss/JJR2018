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
    public class House_Agent_SmsController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 预约看房二手房   
        /// </summary>
        /// <param name="pagesize">条数</param>
        /// <param name="pageindex">页码</param>
        /// <returns></returns>

        public string Order_house_sale_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {

                    int datas_c = (from a in db.house_agent_sms
                                   join b in db.house_sale_list_wuxi on a.houseid equals b.saleid
                                   join c in db.house_sale_search_wuxi on b.saleid equals c.saleid
                                   where a.userid == User.userid && a.housetype == 1
                                   select new { a.id }).Count();

                    var datas_temp = (from a in db.house_agent_sms
                                      join b in db.house_sale_list_wuxi on a.houseid equals b.saleid
                                      join c in db.house_sale_search_wuxi on b.saleid equals c.saleid

                                      where a.userid == User.userid && a.housetype == 1
                                      select new
                                      {
                                          b.saleid,
                                          b.address,
                                          saleaddtime = b.addtime,
                                          b.county,
                                          b.directionsvar,
                                          b.fitmentvar,
                                          b.layer,
                                          b.linkman,
                                          b.naturevar,
                                          b.shangquan,
                                          b.smallpath,
                                          b.tel,
                                          b.titles,
                                          b.totallayer,
                                          b.villagename,
                                          b.hall,
                                          b.toilet,
                                          b.tags,
                                          c.minprice,
                                          c.minarea,
                                          c.room,
                                          c.labelstate,
                                          c.avgprice,
                                          c.isdel,
                                          c.isaudit,
                                          c.state,
                                          c.userid,
                                          a.housetype,
                                          a.addtime,
                                          a.OrderUserid


                                      }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                    Dictionary<string, object>[] datas = new Dictionary<string, object>[datas_temp.Count()];
                    int i = 0;
                    foreach (var temp in datas_temp)
                    {
                        datas[i] = new Dictionary<string, object>();
                        datas[i].Add("saleid", temp.saleid);
                        datas[i].Add("address", temp.address);
                        datas[i].Add("saleaddtime", temp.saleaddtime);
                        datas[i].Add("county", temp.county);
                        datas[i].Add("directionsvar", temp.directionsvar);
                        datas[i].Add("fitmentvar", temp.fitmentvar);
                        datas[i].Add("layer", temp.layer);
                        datas[i].Add("linkman", temp.linkman);
                        datas[i].Add("naturevar", temp.naturevar);
                        datas[i].Add("shangquan", temp.shangquan);
                        datas[i].Add("smallpath", temp.smallpath);
                        datas[i].Add("tel", temp.tel);
                        datas[i].Add("titles", temp.titles);
                        datas[i].Add("totallayer", temp.totallayer);
                        datas[i].Add("villagename", temp.villagename);
                        datas[i].Add("hall", temp.hall);
                        datas[i].Add("toilet", temp.toilet);
                        datas[i].Add("tags", temp.tags);
                        datas[i].Add("minprice", temp.minprice);
                        datas[i].Add("minarea", temp.minarea);
                        datas[i].Add("room", temp.room);
                        datas[i].Add("labelstate", temp.labelstate);
                        datas[i].Add("avgprice", temp.avgprice);
                        datas[i].Add("isdel", temp.isdel);
                        datas[i].Add("isaudit", temp.isaudit);
                        datas[i].Add("state", temp.state);
                        datas[i].Add("userid", temp.userid);
                        datas[i].Add("housetype", temp.housetype);
                        datas[i].Add("addtime", temp.addtime);
                        datas[i].Add("OrderUserid", temp.OrderUserid);
                        int userid = Convert.ToInt32(temp.OrderUserid);
                        string username = ""; //用户名
                        string zongdianval = "";//总店
                        string mendianval = "";//门店  
                        string photoname = "";//头像  

                        if (userid > 0)
                        {
                            var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                            username = user_member.username;
                            var user_details = db.user_details.FirstOrDefault(p => p.userid == userid);
                            photoname = user_details.photoname;
                            string deptpath = user_member.deptpath;
                            if (!string.IsNullOrEmpty(deptpath))
                            {
                                string[] sArray = deptpath.Split(',');
                                if (sArray.Length >= 2)
                                {
                                    int deptid = Convert.ToInt32(sArray[1]);
                                    var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                    zongdianval = user_dept_zongdian.deptname;
                                }
                                if (sArray.Length >= 3)
                                {
                                    int deptid = Convert.ToInt32(sArray[2]);
                                    var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                    mendianval = user_dept_mendian.deptname;
                                }
                            }
                        }
                        datas[i].Add("username", username);
                        datas[i].Add("mendianval", mendianval);
                        datas[i].Add("zongdianval", zongdianval);
                        datas[i].Add("photoname", photoname);
                        i++;
                    }

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
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!", data = null });
                }
            }
        }



        /// <summary>
        ///  预约看房租房 http://192.168.1.223/GR_House_Agent_Sms/Order_house_rent_list    // 租房收藏
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Order_house_rent_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                //try
                //{
                int datas_c = (from a in db.house_agent_sms
                               join b in db.house_rent_list_wuxi on a.houseid equals b.rentid
                               join c in db.house_rent_search_wuxi on b.rentid equals c.rentid
                               where a.userid == User.userid && a.housetype == 2
                               select new { a.id }).Count();

                var datas_temp = (from a in db.house_agent_sms
                                  join b in db.house_rent_list_wuxi on a.houseid equals b.rentid
                                  join c in db.house_rent_search_wuxi on b.rentid equals c.rentid
                                  where a.userid == User.userid && a.housetype == 2
                                  select new
                                  {
                                      b.rentid,
                                      b.address,
                                      rentaddtime = b.addtime,
                                      b.county,
                                      b.directionsvar,
                                      b.fitmentvar,
                                      b.layer,
                                      b.linkman,
                                      b.naturevar,
                                      b.shangquan,
                                      b.smallpath,
                                      b.tel,
                                      b.titles,
                                      b.totallayer,
                                      b.villagename,
                                      b.hall,
                                      b.toilet,
                                      b.tags,
                                      c.minprice,
                                      c.minarea,
                                      c.room,
                                      c.labelstate,
                                      c.avgprice,
                                      c.isdel,
                                      c.isaudit,
                                      c.state,
                                      c.userid,
                                      a.housetype,
                                      a.addtime,
                                      a.OrderUserid

                                  }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                Dictionary<string, object>[] datas = new Dictionary<string, object>[datas_temp.Count()];
                int i = 0;
                foreach (var temp in datas_temp)
                {
                    datas[i] = new Dictionary<string, object>();
                    datas[i].Add("rentid", temp.rentid);
                    datas[i].Add("address", temp.address);
                    datas[i].Add("rentaddtime", temp.rentaddtime);
                    datas[i].Add("county", temp.county);
                    datas[i].Add("directionsvar", temp.directionsvar);
                    datas[i].Add("fitmentvar", temp.fitmentvar);
                    datas[i].Add("layer", temp.layer);
                    datas[i].Add("linkman", temp.linkman);
                    datas[i].Add("naturevar", temp.naturevar);
                    datas[i].Add("shangquan", temp.shangquan);
                    datas[i].Add("smallpath", temp.smallpath);
                    datas[i].Add("tel", temp.tel);
                    datas[i].Add("titles", temp.titles);
                    datas[i].Add("totallayer", temp.totallayer);
                    datas[i].Add("villagename", temp.villagename);
                    datas[i].Add("hall", temp.hall);
                    datas[i].Add("toilet", temp.toilet);
                    datas[i].Add("tags", temp.tags);
                    datas[i].Add("minprice", temp.minprice);
                    datas[i].Add("minarea", temp.minarea);
                    datas[i].Add("room", temp.room);
                    datas[i].Add("labelstate", temp.labelstate);
                    datas[i].Add("avgprice", temp.avgprice);
                    datas[i].Add("isdel", temp.isdel);
                    datas[i].Add("isaudit", temp.isaudit);
                    datas[i].Add("state", temp.state);
                    datas[i].Add("userid", temp.userid);
                    datas[i].Add("housetype", temp.housetype);
                    datas[i].Add("addtime", temp.addtime);
                    datas[i].Add("OrderUserid", temp.OrderUserid);
                    int userid = Convert.ToInt32(temp.OrderUserid);
                    string username = ""; //用户名
                    string zongdianval = "";//总店
                    string mendianval = "";//门店     
                    string photoname = "";//头像  


                    //sql += " update user_details set photoname=@photoname where userid=@userid ";

                    if (userid > 0)
                    {
                        var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                        username = user_member.username;
                        var user_details = db.user_details.FirstOrDefault(p => p.userid == userid);
                        photoname = user_details.photoname;

                        string deptpath = user_member.deptpath;
                        if (!string.IsNullOrEmpty(deptpath))
                        {
                            string[] sArray = deptpath.Split(',');
                            if (sArray.Length >= 2)
                            {
                                int deptid = Convert.ToInt32(sArray[1]);
                                var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                zongdianval = user_dept_zongdian.deptname;
                            }
                            if (sArray.Length >= 3)
                            {
                                int deptid = Convert.ToInt32(sArray[2]);
                                var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                mendianval = user_dept_mendian.deptname;
                            }
                        }
                    }
                    datas[i].Add("username", username);
                    datas[i].Add("mendianval", mendianval);
                    datas[i].Add("zongdianval", zongdianval);
                    datas[i].Add("photoname", photoname);
                    i++;
                }

                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "",
                    data = new
                    {
                        rents = datas,
                        count = datas_c
                    }
                }, timeFormat);

                //}
                //catch (Exception e)
                //{
                //    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!", data = null });
                //}
            }


        }
    }
}