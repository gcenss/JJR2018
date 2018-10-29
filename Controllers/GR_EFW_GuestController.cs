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
    public class GR_EFW_GuestController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 查询转介绍列表   http://192.168.1.223/GR_EFW_Guest/ListByUser
        /// </summary>
        /// <param name="guesttype">1 二手房 2租房 3买客 4租客</param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ListByUser(int guesttype = 1,  int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new shhouseEntities())
            {
                try
                {
                    var datas = new List<EFW_Guest>();
                    var datas_c = 0;
                    datas = db.EFW_Guest.Where(p =>  p.userid == User.userid&&p.guesttype == guesttype).OrderByDescending(p => p.exedate).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                    datas_c = db.EFW_Guest.Where(p => p.userid == User.userid && p.guesttype == guesttype).Count();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            efw_guest = datas, 
                            count = datas_c
                        }
                    }, timeFormat);                   
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }

        public string findByUser(int id )
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new shhouseEntities())
            {
                try
                {

                    var efw_guest = db.EFW_Guest.FirstOrDefault(p => p.id == id && p.userid == User.userid);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = efw_guest

                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }

        /// <summary>
        /// 添加转介绍   http://192.168.1.223/GR_EFW_Guest/Add
        /// </summary>
        /// <param name="guesttype">1 二手房 2租房 3买客 4租客</param>
        /// <param name="guestTel">联系电话</param>
        /// <param name="guestName">联系人</param>
        /// <param name="villageName">小区名称</param>
        /// <param name="price">价格</param>
        /// <param name="minprice">最低价格3买客 4租客</param>
        /// <param name="maxprice">最高价格3买客 4租客</param>
        /// <param name="des">描述</param>
        /// <returns></returns>
        [HttpPost]
        public string Add(int guesttype, string guestTel,string guestName,string  villageName, string price , decimal minprice, decimal maxprice,string des)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;

            if (userid != 0 && guestTel != "0" && guestName != string.Empty && villageName != string.Empty && guesttype != 0)
            {
                if (guesttype == 1 || guesttype == 2)//转介绍二手房租房
                {

                }
                else//转介绍买客租客
                {
                    price = minprice + "~" + maxprice;
                }
                using (var db = new shhouseEntities())
                {
                    if (db.Database.ExecuteSqlCommand("insert into  EFW_Guest (userid,guestTel,guestName,villagename,price,des,state,guesttype) values ('" + userid
                    + "','" + guestTel + "','" + guestName + "','" + villageName + "','" + price + "','" + des + "','0','" + guesttype + "')") > 0)
                    {
                        //dc = ServiceMethod.WriteError("C9994", errorCode.C9994);


                        //1 二手房 2租房 3买客 4租客
                        if (guesttype == 1)
                            appUserScore.ScoreAdd(userid.ToString(), userscore.suggestSale, "app转介绍售房");
                        else if (guesttype == 2)
                            appUserScore.ScoreAdd(userid.ToString(), userscore.suggestRent, "app转介绍租房");
                        else if (guesttype == 3)
                            appUserScore.ScoreAdd(userid.ToString(), userscore.suggestMans, "app转介买客");
                        else if (guesttype == 4)
                            appUserScore.ScoreAdd(userid.ToString(), userscore.suggestManr, "app转介租客");

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "提交成功", data = null });

                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "提交失败", data = null });
                                        }
                }
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "信息填写不完整", data = null });

            }

            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
        }

        /// <summary>
        /// 修改转介绍   http://192.168.1.223/GR_EFW_Guest/Edit
        /// </summary>
        /// <param name="id">转介绍id</param>
        /// <param name="guesttype">1 二手房 2租房 3买客 4租客</param>
        /// <param name="guestTel">联系电话</param>
        /// <param name="guestName">联系人</param>
        /// <param name="villageName">小区名称</param>
        /// <param name="price">价格</param>
        /// <param name="minprice">最低价格3买客 4租客</param>
        /// <param name="maxprice">最高价格3买客 4租客</param>
        /// <param name="des">描述</param>
        /// <returns></returns>
        [HttpPost]
        public string Edit(int id,int guesttype, string guestTel, string guestName, string villageName, string price, decimal minprice, decimal maxprice, string des)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;

            if (guesttype == 1 || guesttype == 2)//转介绍二手房租房
            {

            }
            else//转介绍买客租客
            {
                price = minprice + "~" + maxprice;
            }



            //if (minprice!=0 && maxprice!=0)           
            //{
            //    price = minprice + "~" + maxprice;
            //}
            string sql = string.Empty;
            sql += " update EFW_Guest set guestTel=@guestTel where id=@id and userid=@userid ";
            sql += " update EFW_Guest set guestName=@guestName where id=@id and userid=@userid ";
            sql += " update EFW_Guest set villageName=@villageName where id=@id and userid=@userid ";
            sql += " update EFW_Guest set price=@price where id=@id and userid=@userid ";
            sql += " update EFW_Guest set des=@des where id=@id and userid=@userid ";

            using (shhouseEntities db = new shhouseEntities())
            {
                //try
                //{
                    var edituser = db.Database.ExecuteSqlCommand(sql,
                                new SqlParameter[]{
                                     new SqlParameter("@id", id),
                                new SqlParameter("@userid", userid),
                                new SqlParameter("@guestTel", guestTel),
                                new SqlParameter("@guestName", guestName),
                                new SqlParameter("@villageName", villageName),
                                new SqlParameter("@price", price),
                                 new SqlParameter("@des", des)
                              });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功", data = null });
                //}
                //catch (Exception e)
                //{
                //    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！", data = null });
                //}
            }

        }
        /// <summary>
        /// 删除转介绍   http://192.168.1.223/GR_EFW_Guest/Delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Delete(int id)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            
            string sql = string.Empty;
            sql += " Delete EFW_Guest  where id=@id and userid=@userid ";

            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {
                    var edituser = db.Database.ExecuteSqlCommand(sql,
                                new SqlParameter[]{
                                     new SqlParameter("@id", id),
                                new SqlParameter("@userid", userid)
                              });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "删除成功", data = null });
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "删除失败，请稍后再试！", data = null });
                }
            }

        }
    }
}