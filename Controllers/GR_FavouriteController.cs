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
using System.Data.Entity;
using Newtonsoft.Json.Converters;
using jjr2018.Entity.shvillage;
using jjr2018.Entity.efwnewhouse;
using jjr2018.Entity.shhouse;

namespace jjr2018.Controllers
{
    public class GR_FavouriteController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();



        /// <summary>
        /// 二手房收藏 http://192.168.1.223/GR_Favourite/Favourite_house_sale_list    // 二手房收藏 2租房 3小区 4委托买房 5委托租房 6新房 7浏览二手房 8浏览租房 9浏览新房
        /// </summary>
        /// <param name="pagesize">条数</param>
        /// <param name="pageindex">页码</param>
        /// <returns></returns>
        public string Favourite_house_sale_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {
                    // 1二手房 

                    #region 用这种
                    //  int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 1).Count();

                    // int allcount = db.house_sale_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.saleid && f.housetype == 1 && f.userid == User.userid)).Count();

                    int allcount = (from a in db.favourite_house_village
                                join b in db.house_sale_list_wuxi on a.houseid equals b.saleid
                                join c in db.house_sale_search_wuxi on b.saleid equals c.saleid
                                where a.userid == User.userid && a.housetype == 1
                                select new { a.id }).Count();

               

                var Favourite_house_sale_list_temp = (
                        from a in db.favourite_house_village
                        join b in db.house_sale_list_wuxi on a.houseid equals b.saleid
                        join c in db.house_sale_search_wuxi on b.saleid equals c.saleid
                        where a.userid == User.userid && a.housetype == 1
                        select new
                    {
                        a.id,
                        b.saleid,
                        b.address,
                        b.addtime,
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


                    }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);



                Dictionary<string, object>[] Favourite_house_sale_list = new Dictionary<string, object>[Favourite_house_sale_list_temp.Count()];
                int i = 0;
                foreach (var temp in Favourite_house_sale_list_temp)
                {
                    Favourite_house_sale_list[i] = new Dictionary<string, object>();
                    Favourite_house_sale_list[i].Add("id", temp.id);
                    Favourite_house_sale_list[i].Add("saleid", temp.saleid);
                    Favourite_house_sale_list[i].Add("address", temp.address);

                    Favourite_house_sale_list[i].Add("addtime", temp.addtime);
                    Favourite_house_sale_list[i].Add("county", temp.county);
                    Favourite_house_sale_list[i].Add("directionsvar", temp.directionsvar);
                    Favourite_house_sale_list[i].Add("fitmentvar", temp.fitmentvar);
                    Favourite_house_sale_list[i].Add("layer", temp.layer);
                    Favourite_house_sale_list[i].Add("linkman", temp.linkman);
                    Favourite_house_sale_list[i].Add("naturevar", temp.naturevar);
                    Favourite_house_sale_list[i].Add("shangquan", temp.shangquan);
                    Favourite_house_sale_list[i].Add("smallpath", temp.smallpath);
                    Favourite_house_sale_list[i].Add("tel", temp.tel);
                    Favourite_house_sale_list[i].Add("titles", temp.titles);
                    Favourite_house_sale_list[i].Add("totallayer", temp.totallayer);
                    Favourite_house_sale_list[i].Add("villagename", temp.villagename);

                    Favourite_house_sale_list[i].Add("hall", temp.hall);
                    Favourite_house_sale_list[i].Add("toilet", temp.toilet);
                    Favourite_house_sale_list[i].Add("tags", temp.tags);
                    Favourite_house_sale_list[i].Add("minprice", temp.minprice);
                    Favourite_house_sale_list[i].Add("minarea", temp.minarea);
                    Favourite_house_sale_list[i].Add("room", temp.room);
                    Favourite_house_sale_list[i].Add("labelstate", temp.labelstate);
                    Favourite_house_sale_list[i].Add("avgprice", temp.avgprice);
                    Favourite_house_sale_list[i].Add("isdel", temp.isdel);
                    Favourite_house_sale_list[i].Add("isaudit", temp.isaudit);
                    Favourite_house_sale_list[i].Add("state", temp.state);
                    Favourite_house_sale_list[i].Add("userid", temp.userid);

                    int userid = Convert.ToInt32(temp.userid);
                    string username = ""; //用户名
                    string zongdianval = "";//总店
                    string mendianval = "";//门店

                    int ISOrder = 0;

                    if (userid > 0)
                    {
                        var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                        if (user_member != null)
                        {
                            username = user_member.username;
                            string deptpath = user_member.deptpath;
                            if (!string.IsNullOrEmpty(deptpath))
                            {
                                string[] sArray = deptpath.Split(',');
                                if (sArray.Length >= 2)
                                {
                                    int deptid = Convert.ToInt32(sArray[1]);
                                    var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                    if(user_dept_zongdian!=null) zongdianval = user_dept_zongdian.deptname;
                                }
                                if (sArray.Length >= 3)
                                {
                                    int deptid = Convert.ToInt32(sArray[2]);
                                    var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                    if (user_dept_mendian != null) mendianval = user_dept_mendian.deptname;
                                }
                            }
                        }
                        string sql_c = $@"select count(1) from house_agent_sms where  houseid='{temp.saleid}' and housetype=1 and OrderUserid='{ User.userid  }'";
                        ISOrder = db.Database.SqlQuery<int>(sql_c).First();
                    }

                    Favourite_house_sale_list[i].Add("username", username);
                    Favourite_house_sale_list[i].Add("mendianval", mendianval);
                    Favourite_house_sale_list[i].Add("zongdianval", zongdianval);
                    Favourite_house_sale_list[i].Add("isorder", ISOrder);


                    i++;
                }





                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "二手房关注列表",
                    data = new
                    {
                        Favourite_house_sale_list,
                        allcount
                    }
                }, timeFormat);
                #endregion

                #region 暂时不用
                //int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 1).Count();
                //var Listfavourite_house_village  = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 1).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                //if (Listfavourite_house_village.Count > 0)
                //{
                //    Dictionary<string, object>[] house_sale_list_Content = new Dictionary<string, object>[Listfavourite_house_village.Count];                         
                //    for (int i = 0; i < Listfavourite_house_village.Count; i++)
                //    {
                //    //  Dictionary<string, object> house_sale_list_Content_temp = new Dictionary<string, object>();
                //    int houseid =Convert.ToInt32( Listfavourite_house_village[i].houseid);
                //        var house_sale_search_wuxi1 = db.house_sale_search_wuxi.Where(x => x.saleid == houseid).ToList();
                //        var Listhouse_sale_list_wuxi1 = db.house_sale_list_wuxi.Where(x => x.saleid == houseid).ToList();
                //        if (house_sale_search_wuxi1.Count > 0 && Listhouse_sale_list_wuxi1.Count > 0)
                //        {
                //            var house_sale_search_wuxi_temp = house_sale_search_wuxi1[0];
                //            var Listhouse_sale_list_wuxi_temp = Listhouse_sale_list_wuxi1[0];
                //            house_sale_list_Content[i] = new Dictionary<string, object>();
                //            house_sale_list_Content[i].Add("id", Listfavourite_house_village[i].id);

                //            house_sale_list_Content[i].Add("saleid", Listhouse_sale_list_wuxi_temp.saleid);
                //            house_sale_list_Content[i].Add("address", Listhouse_sale_list_wuxi_temp.address);
                //            house_sale_list_Content[i].Add("addtime", Listhouse_sale_list_wuxi_temp.addtime);
                //            house_sale_list_Content[i].Add("county", Listhouse_sale_list_wuxi_temp.county);
                //            house_sale_list_Content[i].Add("directionsvar", Listhouse_sale_list_wuxi_temp.directionsvar);
                //            house_sale_list_Content[i].Add("fitmentvar", Listhouse_sale_list_wuxi_temp.fitmentvar);
                //            house_sale_list_Content[i].Add("layer", Listhouse_sale_list_wuxi_temp.layer);
                //            house_sale_list_Content[i].Add("linkman", Listhouse_sale_list_wuxi_temp.linkman);
                //            house_sale_list_Content[i].Add("naturevar", Listhouse_sale_list_wuxi_temp.naturevar);
                //            house_sale_list_Content[i].Add("shangquan", Listhouse_sale_list_wuxi_temp.shangquan);
                //            house_sale_list_Content[i].Add("smallpath", Listhouse_sale_list_wuxi_temp.smallpath);
                //            house_sale_list_Content[i].Add("tel", Listhouse_sale_list_wuxi_temp.tel);
                //            house_sale_list_Content[i].Add("titles", Listhouse_sale_list_wuxi_temp.titles);
                //            house_sale_list_Content[i].Add("totallayer", Listhouse_sale_list_wuxi_temp.totallayer);
                //            house_sale_list_Content[i].Add("villagename", Listhouse_sale_list_wuxi_temp.villagename);
                //            house_sale_list_Content[i].Add("hall", Listhouse_sale_list_wuxi_temp.hall);
                //            house_sale_list_Content[i].Add("toilet", Listhouse_sale_list_wuxi_temp.toilet);
                //            house_sale_list_Content[i].Add("tags", Listhouse_sale_list_wuxi_temp.tags);

                //            house_sale_list_Content[i].Add("minprice", house_sale_search_wuxi_temp.minprice);
                //            house_sale_list_Content[i].Add("minarea", house_sale_search_wuxi_temp.minarea);
                //            house_sale_list_Content[i].Add("room", house_sale_search_wuxi_temp.room);
                //            house_sale_list_Content[i].Add("labelstate", house_sale_search_wuxi_temp.labelstate);
                //            house_sale_list_Content[i].Add("avgprice", house_sale_search_wuxi_temp.avgprice);

                //            if (house_sale_search_wuxi_temp.villageid.HasValue && house_sale_search_wuxi_temp.villageid.Value > 0)
                //            {
                //                using (var dbshvillage = new shvillageEntities())
                //                {
                //                    var model_NewHouse = dbshvillage.NewHouse.Where(p => p.ID == house_sale_search_wuxi_temp.villageid.Value).ToList();
                //                    if (model_NewHouse.Count > 0)
                //                    {
                //                        house_sale_list_Content[i].Add("newMap_x", model_NewHouse[0].newMap_x);
                //                        house_sale_list_Content[i].Add("newMap_y", model_NewHouse[0].newMap_y);
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                house_sale_list_Content[i].Add("newMap_x", 0);
                //                house_sale_list_Content[i].Add("newMap_y", 0);
                //            }
                //        }

                //        #region 暂时不用
                //        //else
                //        //{
                //        //    house_sale_list_Content[i] = new Dictionary<string, object>();
                //        //    house_sale_list_Content[i].Add("id", Listfavourite_house_village[i].id);

                //        //    house_sale_list_Content[i].Add("saleid", null);
                //        //    house_sale_list_Content[i].Add("address", null);
                //        //    house_sale_list_Content[i].Add("addtime", null);
                //        //    house_sale_list_Content[i].Add("county", null);
                //        //    house_sale_list_Content[i].Add("directionsvar", null);
                //        //    house_sale_list_Content[i].Add("fitmentvar", null);
                //        //    house_sale_list_Content[i].Add("layer", null);
                //        //    house_sale_list_Content[i].Add("linkman", null);
                //        //    house_sale_list_Content[i].Add("naturevar", null);
                //        //    house_sale_list_Content[i].Add("shangquan", null);
                //        //    house_sale_list_Content[i].Add("smallpath", null);
                //        //    house_sale_list_Content[i].Add("tel", null);
                //        //    house_sale_list_Content[i].Add("titles", null);
                //        //    house_sale_list_Content[i].Add("totallayer", null);
                //        //    house_sale_list_Content[i].Add("villagename", null);
                //        //    house_sale_list_Content[i].Add("hall", null);
                //        //    house_sale_list_Content[i].Add("toilet", null);
                //        //    house_sale_list_Content[i].Add("tags", null);

                //        //    house_sale_list_Content[i].Add("minprice", null);
                //        //    house_sale_list_Content[i].Add("minarea", null);
                //        //    house_sale_list_Content[i].Add("room", null);
                //        //    house_sale_list_Content[i].Add("labelstate", null);
                //        //    house_sale_list_Content[i].Add("avgprice", null);

                //        //} 
                //        #endregion
                //    }
                //    Dictionary<string, object> data = new Dictionary<string, object>();
                //    data.Add("house_sale_list", house_sale_list_Content);

                //    return JsonConvert.SerializeObject(new repmsg
                //    {
                //        state = 1,
                //        msg = "二手房关注列表",
                //        data= data,
                //    }, timeFormat);
                //}
                #endregion

                #region 另外一种方法
                //  int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 1).Count();

                // // int allcount = db.house_sale_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.saleid && f.housetype == 1 && f.userid == User.userid)).Count();
                //var favouritelist = (from a in db.favourite_house_village
                //                         join b in db.house_sale_search_wuxi on a.houseid equals b.saleid into ab
                //                         from abi in ab.DefaultIfEmpty()
                //                         join c in db.house_sale_list_wuxi on b.saleid equals c.saleid into abc
                //                         from abci in abc.DefaultIfEmpty()
                //                         join d in db.house_sale_detail_wuxi on c.saleid equals d.saleid into abcd
                //                         from abcdi in abcd.DefaultIfEmpty()
                //                         where a.userid == User.userid && a.housetype == 1
                //                         select new
                //                         {
                //                             //暂定
                //                             a.id,
                //                             a.addtime,
                //                             b.saleid,
                //                             c.smallpath,
                //                             c.villagename,
                //                             c.titles,
                //                             c.hall,
                //                             c.toilet,
                //                             c.county,
                //                             c.directionsvar,
                //                             c.tags,
                //                             c.shangquan,
                //                             c.naturevar,
                //                             b.minprice,
                //                             b.minarea,
                //                             b.room,
                //                             b.isaudit,
                //                             c.layer,
                //                             c.totallayer,
                //                             c.fitmentvar,
                //                             abcdi.buildid,
                //                             abcdi.roomid,
                //                             abcdi.remark,
                //                             b.isdel,
                //                             b.state

                //                         }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);


                //return JsonConvert.SerializeObject( new repmsg
                //{
                //    state = 1,
                //    msg = "二手房关注列表",
                //    data = new
                //    {
                //        favouritelist,
                //        allcount
                //    }
                //}, timeFormat); 
                #endregion

                return "";
            }
                catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
            }
        }
        }


        /// <summary>
        ///  租房收藏 http://192.168.1.223/GR_Favourite/Favourite_house_rent_list    // 租房收藏
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_house_rent_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {



                    #region 用这种
                    //int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 2).Count();

                    int allcount = (from a in db.favourite_house_village
                                    join b in db.house_rent_list_wuxi on a.houseid equals b.rentid 
                                    join c in db.house_rent_search_wuxi on b.rentid equals c.rentid 
                                    where a.userid == User.userid && a.housetype == 2
                                    select new { a.id }).Count();

                   



                    // int allcount = db.house_rent_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.rentid && f.housetype == 2 && f.userid == User.userid)).Count();
                    var Favourite_house_rent_list_temp = (


                        from a in db.favourite_house_village
                        join b in db.house_rent_list_wuxi on a.houseid equals b.rentid
                        join c in db.house_rent_search_wuxi on b.rentid equals c.rentid
                        where a.userid == User.userid && a.housetype == 2
                        select new
                                                     {
                                                         a.id,
                                                         b.rentid,
                                                         b.address,
                                                         b.addtime,
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
                                                         c.userid

                                                     }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                    Dictionary<string, object>[] Favourite_house_rent_list = new Dictionary<string, object>[Favourite_house_rent_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in Favourite_house_rent_list_temp)
                    {
                        Favourite_house_rent_list[i] = new Dictionary<string, object>();
                        Favourite_house_rent_list[i].Add("id", temp.id);
                        Favourite_house_rent_list[i].Add("rentid", temp.rentid);
                        Favourite_house_rent_list[i].Add("address", temp.address);

                        Favourite_house_rent_list[i].Add("addtime", temp.addtime);
                        Favourite_house_rent_list[i].Add("county", temp.county);
                        Favourite_house_rent_list[i].Add("directionsvar", temp.directionsvar);
                        Favourite_house_rent_list[i].Add("fitmentvar", temp.fitmentvar);
                        Favourite_house_rent_list[i].Add("layer", temp.layer);
                        Favourite_house_rent_list[i].Add("linkman", temp.linkman);
                        Favourite_house_rent_list[i].Add("naturevar", temp.naturevar);
                        Favourite_house_rent_list[i].Add("shangquan", temp.shangquan);
                        Favourite_house_rent_list[i].Add("smallpath", temp.smallpath);
                        Favourite_house_rent_list[i].Add("tel", temp.tel);
                        Favourite_house_rent_list[i].Add("titles", temp.titles);
                        Favourite_house_rent_list[i].Add("totallayer", temp.totallayer);
                        Favourite_house_rent_list[i].Add("villagename", temp.villagename);

                        Favourite_house_rent_list[i].Add("hall", temp.hall);
                        Favourite_house_rent_list[i].Add("toilet", temp.toilet);
                        Favourite_house_rent_list[i].Add("tags", temp.tags);
                        Favourite_house_rent_list[i].Add("minprice", temp.minprice);
                        Favourite_house_rent_list[i].Add("minarea", temp.minarea);
                        Favourite_house_rent_list[i].Add("room", temp.room);
                        Favourite_house_rent_list[i].Add("labelstate", temp.labelstate);
                        Favourite_house_rent_list[i].Add("avgprice", temp.avgprice);
                        Favourite_house_rent_list[i].Add("isdel", temp.isdel);
                        Favourite_house_rent_list[i].Add("isaudit", temp.isaudit);
                        Favourite_house_rent_list[i].Add("state", temp.state);
                        Favourite_house_rent_list[i].Add("userid", temp.userid);

                        int userid = Convert.ToInt32(temp.userid);
                        string username = "";
                        string zongdianval = "";
                        string mendianval = "";
                        int ISOrder = 0;

                        if (userid > 0)
                        {
                            var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                            if (user_member != null)
                            { 
                                username = user_member.username;

                            string deptpath = user_member.deptpath;
                            if (!string.IsNullOrEmpty(deptpath))
                            {
                                string[] sArray = deptpath.Split(',');
                                if (sArray.Length >= 2)
                                {
                                    int deptid = Convert.ToInt32(sArray[1]);
                                    var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_zongdian != null) zongdianval = user_dept_zongdian.deptname;
                                }
                                if (sArray.Length >= 3)
                                {
                                    int deptid = Convert.ToInt32(sArray[2]);
                                    var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_mendian != null) mendianval = user_dept_mendian.deptname;
                                }
                            }
                            }
                            string sql_c = $@"select count(1) from house_agent_sms where  houseid='{temp.rentid}' and housetype=2 and OrderUserid='{ User.userid  }'";
                            ISOrder = db.Database.SqlQuery<int>(sql_c).First();
                        }


                        Favourite_house_rent_list[i].Add("username", username);
                        Favourite_house_rent_list[i].Add("mendianval", mendianval);
                        Favourite_house_rent_list[i].Add("zongdianval", zongdianval);
                        Favourite_house_rent_list[i].Add("isorder", ISOrder);

                        i++;
                    }


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "租房关注列表",
                        data = new
                        {
                            Favourite_house_rent_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion




                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!"+ e });
                }
            }
        }


        /// <summary>
        /// 3小区收藏 http://192.168.1.223/GR_Favourite/Favourite_house_sale_list    3小区收藏 
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_Community_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                //try
                //{

                    // 3小区

                    #region 跨数据库  

                  // int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 3).Count();
                    
                    using (var dbshvillage = new shvillageEntities())
                    {
                        int[] NewHouseID = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 3).Select(x => x.houseid.Value).ToArray();


              

                        int allcount = (from a in dbshvillage.NewHouse where NewHouseID.Any(x => x == a.ID) select new {a.ID}).Count();
                        var Favourite_Community_list_temp = (from a in dbshvillage.NewHouse
                                                        where NewHouseID.Any(x => x == a.ID)
                                                        select new
                                                        {
                                                            a.ID,
                                                            a.UserID,
                                                            a.Name,
                                                            a.SectionID,
                                                            a.ProTypeID,
                                                            a.BuildType,
                                                            a.MetroType,
                                                            a.Virescence,
                                                            a.rjl,
                                                            a.Area,
                                                            a.MinPrice,
                                                            a.AvePrice,
                                                            a.MaxPrice,
                                                            //a.PriceUnit,
                                                            a.PriceInfo,
                                                            a.Address,
                                                            a.slc,
                                                            a.Tel,
                                                            a.Url,
                                                            a.SaleDate,
                                                            a.DeliverDate,
                                                            a.Developer,
                                                            a.Sale,
                                                            a.PMName,
                                                            a.Intro,
                                                            a.lpdp,
                                                            a.zbpt,
                                                            a.lczk,
                                                            a.jtzk,
                                                            a.KFSInfo,
                                                            a.xkz,
                                                            a.bmrs,
                                                            a.LetterID,
                                                            a.BBSID,
                                                            a.OutsideImage,
                                                            a.Park,
                                                            a.Fettle,
                                                            a.UpdateTime,
                                                            a.AddDate,
                                                            a.SortID,
                                                            a.Commend,
                                                            a.HitCount,
                                                            a.City,
                                                            a.Tags,
                                                            a.Map_X,
                                                            a.Map_Y,
                                                            a.isSigns,
                                                            a.Signs,
                                                            a.isPrice,
                                                            a.Title,
                                                            a.RegionID,
                                                            a.ISSell,
                                                            a.SellTel,
                                                            a.ShortTel,
                                                            a.newMap_x,
                                                            a.newMap_y,
                                                            a.zonghushu,
                                                            a.TotalPrice,
                                                            a.ProPrice,
                                                            a.Deleted
                                                        }).OrderByDescending(p => p.ID).Skip(pagesize * (pageindex - 1)).Take(pagesize);



                    Dictionary<string, object>[] Favourite_Community_list = new Dictionary<string, object>[Favourite_Community_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in Favourite_Community_list_temp)
                    {
                        Favourite_Community_list[i] = new Dictionary<string, object>();



                        Favourite_Community_list[i].Add("ID", temp.ID);
                        Favourite_Community_list[i].Add("UserID", temp.UserID);
                        Favourite_Community_list[i].Add("Name", temp.Name);
                        Favourite_Community_list[i].Add("SectionID", temp.SectionID);
                        Favourite_Community_list[i].Add("ProTypeID", temp.ProTypeID);
                        Favourite_Community_list[i].Add("BuildType", temp.BuildType);
                        Favourite_Community_list[i].Add("MetroType", temp.MetroType);
                        Favourite_Community_list[i].Add("Virescence", temp.Virescence);
                        Favourite_Community_list[i].Add("rjl", temp.rjl);
                        Favourite_Community_list[i].Add("Area", temp.Area);
                        Favourite_Community_list[i].Add("MinPrice", temp.MinPrice);
                        Favourite_Community_list[i].Add("AvePrice", temp.AvePrice);
                        Favourite_Community_list[i].Add("MaxPrice", temp.MaxPrice);
                        //Favourite_Community_list[i].Add("PriceUnit", temp.PriceUnit);
                        Favourite_Community_list[i].Add("PriceInfo", temp.PriceInfo);
                        Favourite_Community_list[i].Add("Address", temp.Address);
                        Favourite_Community_list[i].Add("slc", temp.slc);
                        Favourite_Community_list[i].Add("Tel", temp.Tel);
                        Favourite_Community_list[i].Add("Url", temp.Url);
                        Favourite_Community_list[i].Add("SaleDate", temp.SaleDate);
                        Favourite_Community_list[i].Add("DeliverDate", temp.DeliverDate);
                        Favourite_Community_list[i].Add("Developer", temp.Developer);
                        Favourite_Community_list[i].Add("Sale", temp.Sale);
                        Favourite_Community_list[i].Add("PMName", temp.PMName);
                        Favourite_Community_list[i].Add("Intro", temp.Intro);
                        Favourite_Community_list[i].Add("lpdp", temp.lpdp);
                        Favourite_Community_list[i].Add("zbpt", temp.zbpt);
                        Favourite_Community_list[i].Add("lczk", temp.lczk);
                        Favourite_Community_list[i].Add("jtzk", temp.jtzk);
                        Favourite_Community_list[i].Add("KFSInfo", temp.KFSInfo);
                        Favourite_Community_list[i].Add("xkz", temp.xkz);
                        Favourite_Community_list[i].Add("bmrs", temp.bmrs);
                        Favourite_Community_list[i].Add("LetterID", temp.LetterID);
                        Favourite_Community_list[i].Add("BBSID", temp.BBSID);
                        Favourite_Community_list[i].Add("OutsideImage", temp.OutsideImage);
                        Favourite_Community_list[i].Add("Park", temp.Park);
                        Favourite_Community_list[i].Add("Fettle", temp.Fettle);
                        Favourite_Community_list[i].Add("UpdateTime", temp.UpdateTime);
                        Favourite_Community_list[i].Add("AddDate", temp.AddDate);
                        Favourite_Community_list[i].Add("SortID", temp.SortID);
                        Favourite_Community_list[i].Add("Commend", temp.Commend);
                        Favourite_Community_list[i].Add("HitCount", temp.HitCount);
                        Favourite_Community_list[i].Add("City", temp.City);
                        Favourite_Community_list[i].Add("Tags", temp.Tags);
                        Favourite_Community_list[i].Add("Map_X", temp.Map_X);
                        Favourite_Community_list[i].Add("Map_Y", temp.Map_Y);
                        Favourite_Community_list[i].Add("isSigns", temp.isSigns);
                        Favourite_Community_list[i].Add("Signs", temp.Signs);
                        Favourite_Community_list[i].Add("isPrice", temp.isPrice);
                        Favourite_Community_list[i].Add("Title", temp.Title);
                        Favourite_Community_list[i].Add("RegionID", temp.RegionID);
                        Favourite_Community_list[i].Add("ISSell", temp.ISSell);
                        Favourite_Community_list[i].Add("SellTel", temp.SellTel);
                        Favourite_Community_list[i].Add("ShortTel", temp.ShortTel);
                        Favourite_Community_list[i].Add("newMap_x", temp.newMap_x);
                        Favourite_Community_list[i].Add("newMap_y", temp.newMap_y);
                        Favourite_Community_list[i].Add("zonghushu", temp.zonghushu);
                        Favourite_Community_list[i].Add("TotalPrice", temp.TotalPrice);
                        Favourite_Community_list[i].Add("ProPrice", temp.ProPrice);
                        Favourite_Community_list[i].Add("Deleted", temp.Deleted);



                        int house_sale_count = 0;
                        int house_rent_count = 0;



                        if (temp.ID > 0)
                        {  
                             house_sale_count = db.house_sale_search_wuxi.Where(p => p.villageid == temp.ID).Count();
                             house_rent_count = db.house_rent_search_wuxi.Where(p => p.villageid == temp.ID).Count();
                        }


                        Favourite_Community_list[i].Add("house_sale_count", house_sale_count);
                        Favourite_Community_list[i].Add("house_rent_count", house_rent_count);
        

                        i++;
                    }

                    return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "小区关注列表",
                            data = new
                            {
                                Favourite_Community_list,
                                allcount
                            }
                        }, timeFormat);
                    }
                    #endregion



                    return "";
                //}
                //catch (Exception e)
                //{
                //    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                //}
            }
        }


        /// <summary>
        /// 委托买房收藏 有问题 不管他 http://192.168.1.223/GR_Favourite/Favourite_require_sale_list    //
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_require_sale_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {


                    // 4委托买房

                    #region 用这种 委托买房
                  //  int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 4).Count();
                   
                    int allcount = (from a in db.favourite_house_village
                                    join b in db.house_require_wuxi on a.houseid equals b.eid 
                                    where a.userid == User.userid && a.housetype == 4 && b.housetype == 1
                                    select new { a.id }).Count();



                    var Favourite_require_sale_list = (from a in db.favourite_house_village
                                                       join b in db.house_require_wuxi on a.houseid equals b.eid

                                                       where a.userid == User.userid && a.housetype == 4 && b.housetype == 1
                                                       select new
                                                       {
                                                           a.id,
                                                           b.userid,
                                                           b.title,
                                                           b.minprice,
                                                           b.minarea,
                                                           b.habitableroom,
                                                           b.housetype,
                                                           b.remark,
                                                           b.linkman,
                                                           b.tel,
                                                           b.addtime,
                                                           b.unixdate,
                                                           b.addip,
                                                           b.countyid

                                                       }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "委托买房收藏列表",
                        data = new
                        {
                            Favourite_require_sale_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion




                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }


        /// <summary>
        /// 委托租房收藏不管他  http://192.168.1.223/GR_Favourite/Favourite_require_rent_list    //
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_require_rent_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {

                    //5委托租房

                    #region 用这种
                    //int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 5).Count();

                    int allcount = (from a in db.favourite_house_village
                                    join b in db.house_require_wuxi on a.houseid equals b.eid 
                                    where a.userid == User.userid && a.housetype == 5 && b.housetype == 2
                                    select new
                                    { a.id }).Count();

                    // int allcount = db.house_rent_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.rentid && f.housetype == 5 && f.userid == User.userid)).Count();
                    var Favourite_require_rent_list = (from a in db.favourite_house_village
                                                       join b in db.house_require_wuxi on a.houseid equals b.eid 

                                                       where a.userid == User.userid && a.housetype == 5 && b.housetype == 2
                                                       select new
                                                       {
                                                           a.id,
                                                           b.userid,
                                                           b.title,
                                                           b.minprice,
                                                           b.minarea,
                                                           b.habitableroom,
                                                           b.housetype,
                                                           b.remark,
                                                           b.linkman,
                                                           b.tel,
                                                           b.addtime,
                                                           b.unixdate,
                                                           b.addip,
                                                           b.countyid

                                                       }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "租房关注列表",
                        data = new
                        {
                            Favourite_require_rent_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion


                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }

        //
        /// <summary>
        ///  新房收藏 http://192.168.1.223/GR_Favourite/Favourite_NewHouse_list    // 新房收藏
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_NewHouse_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {

                    //6新房

                    #region 跨数据库  

                   // int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 6).Count();
                    using (var dbefwnewhouse = new efwnewhouseEntities())
                    {
                        int[] NewHouseID = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 6).Select(x => x.houseid.Value).ToArray();

                        int allcount = (from a in dbefwnewhouse.NewHouse
                                        where NewHouseID.Any(x => x == a.ID)
                                        select new
                                        {  a.ID
                                        }).Count();

                        var Favourite_NewHouse_list = (from a in dbefwnewhouse.NewHouse
                                                       where NewHouseID.Any(x => x == a.ID)                                                     
                                                       select new
                                                       {
                                                           a.ID,

                                                           a.UserID,
                                                           a.Name,
                                                           a.SectionID,
                                                           a.ProTypeID,
                                                           a.BuildType,
                                                           a.MetroType,
                                                           a.Virescence,
                                                           a.rjl,
                                                           a.Area,
                                                           a.MinPrice,
                                                           a.AvePrice,
                                                           a.MaxPrice,
                                                           //a.PriceUnit,
                                                           a.PriceInfo,
                                                           a.Address,
                                                           a.slc,
                                                           a.Tel,
                                                           a.Url,
                                                           a.SaleDate,
                                                           a.DeliverDate,
                                                           a.Developer,
                                                           a.Sale,
                                                           a.PMName,
                                                           a.Intro,
                                                           a.lpdp,
                                                           a.zbpt,
                                                           a.lczk,
                                                           a.jtzk,
                                                           a.KFSInfo,
                                                           a.xkz,
                                                           a.bmrs,
                                                           a.LetterID,
                                                           a.BBSID,
                                                           a.OutsideImage,
                                                           a.Park,
                                                           a.Fettle,
                                                           a.UpdateTime,
                                                           a.AddDate,
                                                           a.SortID,
                                                           a.Commend,
                                                           a.HitCount,
                                                           a.City,
                                                           a.Tags,
                                                           a.Map_X,
                                                           a.Map_Y,
                                                           a.isSigns,
                                                           a.Signs,
                                                           a.isPrice,
                                                           a.Title,
                                                           a.RegionID,
                                                           a.ISSell,
                                                           a.SellTel,
                                                           a.ShortTel,
                                                           a.newMap_x,
                                                           a.newMap_y,
                                                           a.zonghushu,
                                                           a.TotalPrice,
                                                           a.ProPrice,
                                                           a.Deleted

                                                       }).OrderByDescending(p => p.ID).Skip(pagesize * (pageindex - 1)).Take(pagesize);
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "新房列表",
                            data = new
                            {
                                Favourite_NewHouse_list,
                                allcount
                            }
                        }, timeFormat);
                    }
                    #endregion

                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }


        /// <summary>
        /// 浏览二手房 http://192.168.1.223/GR_Favourite/Browse_house_sale_list    //7浏览二手房
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Browse_house_sale_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {

                    //7浏览二手房

                    #region 用这种
                    //int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 7).Count();

                    // int allcount = db.house_sale_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.saleid && f.housetype == 7 && f.userid == User.userid)).Count();


                    int allcount = (from a in db.favourite_house_village
                                    join b in db.house_sale_list_wuxi on a.houseid equals b.saleid 
                                    join c in db.house_sale_search_wuxi on b.saleid equals c.saleid
                                    where a.userid == User.userid && a.housetype == 7
                                    select new { a.id }).Count();



                    var Browse_house_sale_list_temp = (from a in db.favourite_house_village
                                                  join b in db.house_sale_list_wuxi on a.houseid equals b.saleid 
                                                  join c in db.house_sale_search_wuxi on b.saleid equals c.saleid 
                                                  where a.userid == User.userid && a.housetype == 7
                                                  select new
                                                  {
                                                      a.id,
                                                      b.saleid,
                                                      b.address,
                                                      b.addtime,
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
                                                      c.userid

                                                  }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                    Dictionary<string, object>[] Browse_house_sale_list = new Dictionary<string, object>[Browse_house_sale_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in Browse_house_sale_list_temp)
                    {
                        Browse_house_sale_list[i] = new Dictionary<string, object>();
                        Browse_house_sale_list[i].Add("id", temp.id);
                        Browse_house_sale_list[i].Add("saleid", temp.saleid);
                        Browse_house_sale_list[i].Add("address", temp.address);

                        Browse_house_sale_list[i].Add("addtime", temp.addtime);
                        Browse_house_sale_list[i].Add("county", temp.county);
                        Browse_house_sale_list[i].Add("directionsvar", temp.directionsvar);
                        Browse_house_sale_list[i].Add("fitmentvar", temp.fitmentvar);
                        Browse_house_sale_list[i].Add("layer", temp.layer);
                        Browse_house_sale_list[i].Add("linkman", temp.linkman);
                        Browse_house_sale_list[i].Add("naturevar", temp.naturevar);
                        Browse_house_sale_list[i].Add("shangquan", temp.shangquan);
                        Browse_house_sale_list[i].Add("smallpath", temp.smallpath);
                        Browse_house_sale_list[i].Add("tel", temp.tel);
                        Browse_house_sale_list[i].Add("titles", temp.titles);
                        Browse_house_sale_list[i].Add("totallayer", temp.totallayer);
                        Browse_house_sale_list[i].Add("villagename", temp.villagename);

                        Browse_house_sale_list[i].Add("hall", temp.hall);
                        Browse_house_sale_list[i].Add("toilet", temp.toilet);
                        Browse_house_sale_list[i].Add("tags", temp.tags);
                        Browse_house_sale_list[i].Add("minprice", temp.minprice);
                        Browse_house_sale_list[i].Add("minarea", temp.minarea);
                        Browse_house_sale_list[i].Add("room", temp.room);
                        Browse_house_sale_list[i].Add("labelstate", temp.labelstate);
                        Browse_house_sale_list[i].Add("avgprice", temp.avgprice);
                        Browse_house_sale_list[i].Add("isdel", temp.isdel);
                        Browse_house_sale_list[i].Add("isaudit", temp.isaudit);
                        Browse_house_sale_list[i].Add("state", temp.state);
                        Browse_house_sale_list[i].Add("userid", temp.userid);

                        int userid = Convert.ToInt32(temp.userid);
                        string username = "";
                        string zongdianval = "";
                        string mendianval = "";
                        int ISOrder = 0;

                        if (userid > 0)
                        {
                            var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                            if (user_member != null)
                            { 
                                username = user_member.username;

                            string deptpath = user_member.deptpath;
                            if (!string.IsNullOrEmpty(deptpath))
                            {
                                string[] sArray = deptpath.Split(',');
                                if (sArray.Length >= 2)
                                {
                                    int deptid = Convert.ToInt32(sArray[1]);
                                    var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_zongdian != null) zongdianval = user_dept_zongdian.deptname;
                                }
                                if (sArray.Length >= 3)
                                {
                                    int deptid = Convert.ToInt32(sArray[2]);
                                    var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_mendian != null) mendianval = user_dept_mendian.deptname;
                                }
                            }}
                            string sql_c = $@"select count(1) from house_agent_sms where  houseid='{temp.saleid}' and housetype=1 and OrderUserid='{ User.userid  }'";
                            ISOrder = db.Database.SqlQuery<int>(sql_c).First();
                        }


                        Browse_house_sale_list[i].Add("username", username);
                        Browse_house_sale_list[i].Add("mendianval", mendianval);
                        Browse_house_sale_list[i].Add("zongdianval", zongdianval);
                        Browse_house_sale_list[i].Add("isorder", ISOrder);

                        i++;
                    }
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "浏览二手房列表",
                        data = new
                        {
                            Browse_house_sale_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion




                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }


        /// <summary>
        /// 浏览租房 http://192.168.1.223/GR_Favourite/Browse_house_rent_list    浏览租房
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Browse_house_rent_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {

                    // 8浏览租房

                    #region 用这种
                    // int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 8).Count();
                    // int allcount = db.house_rent_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.rentid && f.housetype == 8 && f.userid == User.userid)).Count();

                    int allcount = (from a in db.favourite_house_village
                                    join b in db.house_rent_list_wuxi on a.houseid equals b.rentid 
                                    join c in db.house_rent_search_wuxi on b.rentid equals c.rentid 
                                    where a.userid == User.userid && a.housetype == 8
                                    select new
                                    {
                                        a.id
                                    }).Count();
                    var Browse_house_rent_list_temp = (from a in db.favourite_house_village
                                                  join b in db.house_rent_list_wuxi on a.houseid equals b.rentid 
                                                  join c in db.house_rent_search_wuxi on b.rentid equals c.rentid 
                                                  where a.userid == User.userid && a.housetype == 8
                                                  select new
                                                  {
                                                      a.id,
                                                      b.rentid,
                                                      b.address,
                                                      b.addtime,
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
                                                      c.userid
                                                  }).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);


                    Dictionary<string, object>[] Browse_house_rent_list = new Dictionary<string, object>[Browse_house_rent_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in Browse_house_rent_list_temp)
                    {
                        Browse_house_rent_list[i] = new Dictionary<string, object>();
                        Browse_house_rent_list[i].Add("id", temp.id);
                        Browse_house_rent_list[i].Add("saleid", temp.rentid);
                        Browse_house_rent_list[i].Add("address", temp.address);

                        Browse_house_rent_list[i].Add("addtime", temp.addtime);
                        Browse_house_rent_list[i].Add("county", temp.county);
                        Browse_house_rent_list[i].Add("directionsvar", temp.directionsvar);
                        Browse_house_rent_list[i].Add("fitmentvar", temp.fitmentvar);
                        Browse_house_rent_list[i].Add("layer", temp.layer);
                        Browse_house_rent_list[i].Add("linkman", temp.linkman);
                        Browse_house_rent_list[i].Add("naturevar", temp.naturevar);
                        Browse_house_rent_list[i].Add("shangquan", temp.shangquan);
                        Browse_house_rent_list[i].Add("smallpath", temp.smallpath);
                        Browse_house_rent_list[i].Add("tel", temp.tel);
                        Browse_house_rent_list[i].Add("titles", temp.titles);
                        Browse_house_rent_list[i].Add("totallayer", temp.totallayer);
                        Browse_house_rent_list[i].Add("villagename", temp.villagename);

                        Browse_house_rent_list[i].Add("hall", temp.hall);
                        Browse_house_rent_list[i].Add("toilet", temp.toilet);
                        Browse_house_rent_list[i].Add("tags", temp.tags);
                        Browse_house_rent_list[i].Add("minprice", temp.minprice);
                        Browse_house_rent_list[i].Add("minarea", temp.minarea);
                        Browse_house_rent_list[i].Add("room", temp.room);
                        Browse_house_rent_list[i].Add("labelstate", temp.labelstate);
                        Browse_house_rent_list[i].Add("avgprice", temp.avgprice);
                        Browse_house_rent_list[i].Add("isdel", temp.isdel);
                        Browse_house_rent_list[i].Add("isaudit", temp.isaudit);
                        Browse_house_rent_list[i].Add("state", temp.state);
                        Browse_house_rent_list[i].Add("userid", temp.userid);

                        int userid = Convert.ToInt32(temp.userid);
                        string username = "";
                        string zongdianval = "";
                        string mendianval = "";
                        int ISOrder = 0;

                        if (userid > 0)
                        {
                            var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                            if (user_member != null) { 
                                username = user_member.username;

                            string deptpath = user_member.deptpath;
                            if (!string.IsNullOrEmpty(deptpath))
                            {
                                string[] sArray = deptpath.Split(',');
                                if (sArray.Length >= 2)
                                {
                                    int deptid = Convert.ToInt32(sArray[1]);
                                    var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_zongdian != null) zongdianval = user_dept_zongdian.deptname;
                                }
                                if (sArray.Length >= 3)
                                {
                                    int deptid = Convert.ToInt32(sArray[2]);
                                    var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_mendian != null) mendianval = user_dept_mendian.deptname;
                                }
                            }
                            }
                            string sql_c = $@"select count(1) from house_agent_sms where  houseid='{temp.rentid}' and housetype=2 and OrderUserid='{ User.userid  }'";
                            ISOrder = db.Database.SqlQuery<int>(sql_c).First();
                        }


                        Browse_house_rent_list[i].Add("username", username);
                        Browse_house_rent_list[i].Add("mendianval", mendianval);
                        Browse_house_rent_list[i].Add("zongdianval", zongdianval);
                        Browse_house_rent_list[i].Add("isorder", ISOrder);


                        i++;
                    }
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "浏览租房列表",
                        data = new
                        {
                            Browse_house_rent_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion
                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }


        //
        /// <summary>
        /// 浏览新房 http://192.168.1.223/GR_Favourite/Browse_NewHouse_list    浏览新房
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Browse_NewHouse_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {
                    //9浏览新房     
                    #region 跨数据库  and (Fettle='在售' or Fettle='新盘' or Fettle='尾盘' or Fettle='地块')

                   // int allcount = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 9).Count();
                    using (var dbefwnewhouse = new efwnewhouseEntities())
                    {
                        int[] NewHouseID = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 9).Select(x => x.houseid.Value).ToArray();

                        int allcount = (from a in dbefwnewhouse.NewHouse
                                        where NewHouseID.Any(x => x == a.ID)                                      
                                        select new
                                        {
                                            a.ID
                                        }).Count();


                        var Browse_NewHouse_list = (from a in dbefwnewhouse.NewHouse
                                                    where NewHouseID.Any(x => x == a.ID)                                                   
                                                    select new
                                                    {
                                                        a.ID,
                                                        a.UserID,
                                                        a.Name,
                                                        a.SectionID,
                                                        a.ProTypeID,
                                                        a.BuildType,
                                                        a.MetroType,
                                                        a.Virescence,
                                                        a.rjl,
                                                        a.Area,
                                                        a.MinPrice,
                                                        a.AvePrice,
                                                        a.MaxPrice,
                                                        //a.PriceUnit,
                                                        a.PriceInfo,
                                                        a.Address,
                                                        a.slc,
                                                        a.Tel,
                                                        a.Url,
                                                        a.SaleDate,
                                                        a.DeliverDate,
                                                        a.Developer,
                                                        a.Sale,
                                                        a.PMName,
                                                        a.Intro,
                                                        a.lpdp,
                                                        a.zbpt,
                                                        a.lczk,
                                                        a.jtzk,
                                                        a.KFSInfo,
                                                        a.xkz,
                                                        a.bmrs,
                                                        a.LetterID,
                                                        a.BBSID,
                                                        a.OutsideImage,
                                                        a.Park,
                                                        a.Fettle,
                                                        a.UpdateTime,
                                                        a.AddDate,
                                                        a.SortID,
                                                        a.Commend,
                                                        a.HitCount,
                                                        a.City,
                                                        a.Tags,
                                                        a.Map_X,
                                                        a.Map_Y,
                                                        a.isSigns,
                                                        a.Signs,
                                                        a.isPrice,
                                                        a.Title,
                                                        a.RegionID,
                                                        a.ISSell,
                                                        a.SellTel,
                                                        a.ShortTel,
                                                        a.newMap_x,
                                                        a.newMap_y,
                                                        a.zonghushu,
                                                        a.TotalPrice,
                                                        a.ProPrice,
                                                        a.Deleted
                                                    }).OrderByDescending(p => p.ID).Skip(pagesize * (pageindex - 1)).Take(pagesize);
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "浏览新房列表",
                            data = new
                            {
                                Browse_NewHouse_list,
                                allcount
                            }
                        }, timeFormat);
                    }
                    #endregion


                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }



        /// <summary>
        /// 添加收藏和浏览 除经纪人外  http://192.168.1.223/GR_Favourite/Favourite_Add       // 二手房收藏 2租房 3小区 4委托买房 5委托租房 6新房 7浏览二手房 8浏览租房 9浏览新房
        /// </summary>   
        /// <param name="housetype"></param>
        /// <param name="houseid"></param>

        /// <returns></returns>
        public string Favourite_Add(int housetype, int houseid)
        {
            if (housetype <= 0 || houseid <= 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "参数有误,提交失败!", data = null });
            }


            using (var db = new shhouseEntities())
            {
                var intuser = db.Database.ExecuteSqlCommand("if  exists( select  *  from  favourite_house_village  where  userid=@userid and housetype=@housetype and houseid=@houseid and city=@city )   begin select  -99 end   "
                    + " else  begin    insert into  favourite_house_village(userid, housetype, houseid, addtime, city)   " +
                    " values(@userid, @housetype, @houseid, GetDate(), @city)   select  scope_identity() end",
                         new SqlParameter[]{
                                new SqlParameter("@userid", User.userid),
                                new SqlParameter("@housetype", housetype),
                                new SqlParameter("@houseid", houseid),
                                new SqlParameter("@city", "wuxi")
                              });

                if (intuser.ToString() == "-99" || intuser.ToString() == "-1")
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "已收藏请勿重复提交!", data = null });
                }
                else if (CharString.IntConvert(intuser) > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "添加成功!", data = null });
                }

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
        }


        /// <summary>
        /// 删除收藏和浏览 除经纪人外  http://192.168.1.223/GR_Favourite/Favourite_Delete       // 二手房收藏 2租房 3小区 4委托买房 5委托租房 6新房 7浏览二手房 8浏览租房 9浏览新房
        /// </summary>   
        /// <param name="housetype"></param>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string Favourite_Delete(int housetype, int houseid)
        {
            try
            {
                using (var db = new shhouseEntities())
                {
                    var intuser = db.Database.ExecuteSqlCommand(" delete from favourite_house_village where  userid=@userid and housetype=@housetype and houseid=@houseid and city=@city ",
                             new SqlParameter[]{
                                new SqlParameter("@userid", User.userid),
                                new SqlParameter("@housetype", housetype),
                                new SqlParameter("@houseid", houseid),
                                new SqlParameter("@city", "wuxi")
                                  });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "取消成功!", data = null });
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
        }

        //AgentCollection
        /// <summary>
        /// 经纪人收藏 http://192.168.1.223/GR_Favourite/Favourite_Agent_list    
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string Favourite_Agent_list(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {
                    // 

                    #region 用这种 
                    //int allcount = db.AgentCollection.Where(p => p.UserID == User.userid).Count();


                    int allcount = (from a in db.AgentCollection
                                    join b in db.user_details on a.AgentID equals b.userid 
                                    join c in db.user_search_all_wuxi on a.AgentID equals c.UserID 
                                    where a.UserID == User.userid
                                    select new
                                    {
                                        a.ID
                                    }).Count();

                    // int allcount = db.house_rent_search_wuxi.SelectMany(a => db.favourite_house_village.Where(f => f.houseid == a.rentid && f.housetype == 4 && f.userid == User.userid)).Count();
                    var Favourite_Agent_list_temp = (from a in db.AgentCollection
                                                       join b in db.user_details on a.AgentID equals b.userid 
                                                       join c in db.user_search_all_wuxi on a.AgentID equals c.UserID 
                                                       where a.UserID == User.userid
                                                     select new
                                                       {
                                                           a.ID,
                                                           a.AddTime,
                                                           b.userid,
                                                           b.mobile,
                                                           b.photoname,
                                                           b.gradeid,
                                                           b.origin,
                                                           b.remark,
                                                           b.realname,
                                                           c.HasImg,
                                                           c.RealAudit,
                                                           c.searchTitle,
                                                           c.StarLevel,
                                                           c.LastLoginTime,
                                                           b.know_area,
                                                           b.know_village
                                                       }).OrderByDescending(p => p.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize);




                    Dictionary<string, object>[] Favourite_Agent_list = new Dictionary<string, object>[Favourite_Agent_list_temp.Count()];
                    int i = 0;
                    foreach (var temp in Favourite_Agent_list_temp)
                    {
                        Favourite_Agent_list[i] = new Dictionary<string, object>();
                        
                        Favourite_Agent_list[i].Add("ID", temp.ID);
                        Favourite_Agent_list[i].Add("AddTime", temp.AddTime);
                        Favourite_Agent_list[i].Add("userid", temp.userid);
                        Favourite_Agent_list[i].Add("mobile", temp.mobile);
                        Favourite_Agent_list[i].Add("photoname", temp.photoname);
                        Favourite_Agent_list[i].Add("gradeid", temp.gradeid);
                        Favourite_Agent_list[i].Add("origin", temp.origin);
                        Favourite_Agent_list[i].Add("remark", temp.remark);
                        Favourite_Agent_list[i].Add("realname", temp.realname);
                        Favourite_Agent_list[i].Add("HasImg", temp.HasImg);
                        Favourite_Agent_list[i].Add("RealAudit", temp.RealAudit);
                        Favourite_Agent_list[i].Add("searchTitle", temp.searchTitle);
                        Favourite_Agent_list[i].Add("StarLevel", temp.StarLevel);
                        Favourite_Agent_list[i].Add("LastLoginTime", temp.LastLoginTime);
                        Favourite_Agent_list[i].Add("know_area", Utils.chuli(temp.know_area));
                        Favourite_Agent_list[i].Add("know_village", temp.know_village);

                        int userid = Convert.ToInt32(temp.userid);
                        string username = "";
                        string zongdianval = "";
                        string mendianval = "";
                     

                        if (userid > 0)
                        {
                            var user_member = db.user_member.FirstOrDefault(p => p.userid == userid);
                            if (user_member != null)
                            {
                                username = user_member.username;

                                string deptpath = user_member.deptpath;
                                if (!string.IsNullOrEmpty(deptpath))
                                {
                                    string[] sArray = deptpath.Split(',');
                                    if (sArray.Length >= 2)
                                    {
                                        int deptid = Convert.ToInt32(sArray[1]);
                                        var user_dept_zongdian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_zongdian != null) zongdianval = user_dept_zongdian.deptname;
                                    }
                                    if (sArray.Length >= 3)
                                    {
                                        int deptid = Convert.ToInt32(sArray[2]);
                                        var user_dept_mendian = db.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                        if (user_dept_mendian != null) mendianval = user_dept_mendian.deptname;
                                    }
                                }
                            }
                        }
                        Favourite_Agent_list[i].Add("username", username);
                        Favourite_Agent_list[i].Add("mendianval", mendianval);
                        Favourite_Agent_list[i].Add("zongdianval", zongdianval);   

                        i++;
                    }
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "经纪人收藏列表",
                        data = new
                        {
                            Favourite_Agent_list,
                            allcount
                        }
                    }, timeFormat);
                    #endregion




                    return "";
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }
        }


        /// <summary>
        ///  添加收藏经纪人  http://192.168.1.223/GR_Favourite/Favourite_Agent_Add   
        /// </summary>
        /// <param name="AgentID"></param>
        /// <returns></returns>
        public string Favourite_Agent_Add(int AgentID)
        {
            if (AgentID <= 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "参数有误,提交失败!", data = null });
            }
            //判断是不是经纪人
          
            using (var db = new shhouseEntities())
            {
                int isAgent = db.user_member.Where(p => p.userid == AgentID && p.roleid == 4).Count();

                if (isAgent != 1)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "非经纪人不能关注", data = null });
                }

                var intuser = db.Database.ExecuteSqlCommand("if  exists(select * from AgentCollection where UserID=@UserID and AgentID=@AgentID )   begin select  -99 end   "
                    + " else  begin    insert into  AgentCollection(UserID, AgentID,AddTime)   " +
                    " values(@UserID, @AgentID,getdate())   select  scope_identity() end",
                         new SqlParameter[]{
                                new SqlParameter("@UserID", User.userid),
                                new SqlParameter("@AgentID", AgentID)
                              });

                if (intuser.ToString() == "-99" || intuser.ToString() == "-1")
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "已收藏请勿重复提交!", data = null });
                }
                else if (CharString.IntConvert(intuser) > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "添加成功!", data = null });
                }

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
        }


        /// <summary>
        /// 删除收藏经纪人  http://192.168.1.223/GR_Favourite/Favourite_Agent_Delete   
        /// </summary>   
        /// <param name="housetype"></param>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string Favourite_Agent_Delete(int AgentID)
        {
            if (AgentID <= 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "参数有误,提交失败!", data = null });
            }
            try
            {
                using (var db = new shhouseEntities())
                {
                    var intuser = db.Database.ExecuteSqlCommand(" delete from AgentCollection where  UserID=@UserID and AgentID=@AgentID ",
                             new SqlParameter[]{
                                 new SqlParameter("@UserID", User.userid),
                                new SqlParameter("@AgentID", AgentID)
                                  });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "取消成功!", data = null });
                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
        }




    }
}