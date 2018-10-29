using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using Newtonsoft.Json.Converters;

namespace jjr2018.Controllers
{
    public class jbshoppingController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        [HttpGet]
        public string list(int pageindex = 1, int pagesize = 20)
        {
            var count = shhouseconn.QuerySingle<dynamic>("select count(1) cc from site_shoppings where enabled=1").cc;
            var data = shhouseconn.Query($"select * from(select productid,title,img,points,stock,nump,ishot,exchanges,Row_Number() over(order by ishot desc, productid desc) rowid from site_shoppings where enabled=1) t where rowid>={(pageindex - 1) * pagesize + 1} and rowid<={pageindex * pagesize}");
            
            return JsonConvert.SerializeObject(new
            {
                state = 1,
                msg = "",
                data = new
                {
                    count = count,
                    list = data,
                    allscore=User.user_details.scoretotal
                }
            });

        }

        [HttpGet]
        public string buy(int productid, int num,string name,string mobile,string address)
        {
            try{
                var param = new DynamicParameters();
                param.Add("@userid", User.userid);
                param.Add("@productid", productid);
                param.Add("@num", num);
                param.Add("@name", name);
                param.Add("@mobile", mobile);
                param.Add("@address", address);

                param.Add("@state", 0, DbType.Int32, ParameterDirection.Output);
                param.Add("@msg", 0, DbType.String, ParameterDirection.Output, size: 100);
                var res2 = shhouseconn.Execute("exchange_jbshopping", param, null, null, CommandType.StoredProcedure);

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
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!", data = null });
            }
        }

        //兑换记录

        [HttpGet]
        public string shoppinghistory(int pagesize = 10, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            try
            {
                using (var db = new shhouseEntities())
                {
                    int datas_c = (from a in db.site_shoppinghistory
                                   where a.userid == User.userid
                                   select new { a.shoppingid }).Count();


                    var datas_temp = (from a in db.site_shoppinghistory
                                      join b in db.site_shoppings on a.productid equals b.productid
                                      where a.userid == User.userid 
                                      select new
                                      {
                                         a.productid,
                                         a.nums,
                                         a.title,
                                         a.points,
                                         a.totalpoints,
                                         a.buydate,
                                         a.name,
                                         a.mobile,
                                         a.address,
                                         b.img,
                                         b.desct,
                                         b.nump,
                                         b.ishot,
                                         b.exchanges,
                                         b.enabled,
                                         b.stock
                                      }).OrderByDescending(p => p.buydate).Skip(pagesize * (pageindex - 1)).Take(pagesize);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas_temp,
                            count = datas_c
                        }
                    }, timeFormat);

                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!", data = null });
            }
        }

        //兑换详情
        [HttpGet]
        public string shoppinghistorydetail(int shoppingID) {

            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            try
            {
                using (var db = new shhouseEntities())
                {
                    var datas_temp = (from a in db.site_shoppinghistory
                                      join b in db.site_shoppings on a.productid equals b.productid
                                      where a.shoppingid == shoppingID
                                      select new
                                      {
                                          a.productid,
                                          a.nums,
                                          a.title,
                                          a.points,
                                          a.totalpoints,
                                          a.buydate,
                                          a.name,
                                          a.mobile,
                                          a.address,
                                          b.img,
                                          b.desct,
                                          b.nump,
                                          b.ishot,
                                          b.exchanges,
                                          b.enabled,
                                          b.stock,
                                      }).FirstOrDefault();

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas_temp
                        }
                    }, timeFormat);

                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!", data = null });
            }

        }



    }
}