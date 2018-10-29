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
    public class GR_House_Agent_SmsController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 预约看房二手房 http://192.168.1.223/GR_House_Agent_Sms/Order_house_sale_list    
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


                    //string sql = $@"select * from (
                    //                   SELECT      a.saleid,a.address,a.addtime as saleaddtime,a.county,a.directionsvar,a.fitmentvar,a.layer,a.linkman,a.naturevar,a.shangquan,a.smallpath,a.tel,
                    //                            a.titles,a.totallayer,a.villagename,a.hall,a.toilet,a.tags,b.minprice,b.minarea,b.room,b.labelstate,b.avgprice,c.housetype,c.addtime,  c.OrderUserid, 
                    //                            ROW_NUMBER() over(order by c.addtime desc) as rows
                    //                   FROM        house_sale_search_wuxi b  JOIN   house_sale_list_wuxi a ON b.saleid = a.saleid  JOIN house_agent_sms c ON a.saleid = c.houseid 
                    //                   where housetype=1 and c.OrderUserid='{ User.userid.ToString() }') t
                    //where t.rows >={ (pageindex - 1) * pagesize + 1 } and t.rows <={ pageindex * pagesize }";
                    //string sql_c = $@"select count(1) from  house_sale_search_wuxi b  JOIN house_sale_list_wuxi a ON b.saleid = a.saleid  JOIN  house_agent_sms c ON a.saleid = c.houseid 
                    //               where housetype=1 and c.OrderUserid='{ User.userid.ToString() }'";
                     //        var parms1 = new object[] { };
                    //        var datas = ent.Database.DynamicSqlQuery(sql, parms1);
                    //        var datas_c = ent.Database.SqlQuery<int>(sql_c, parms1).First();
  


                    int datas_c = (from a in db.house_agent_sms
                                   join b in db.house_sale_list_wuxi on a.houseid equals b.saleid 
                                   join c in db.house_sale_search_wuxi on b.saleid equals c.saleid
                                   where a.OrderUserid == User.userid && a.housetype == 1
                                   select new { a.id }).Count();

                    var datas_temp = (from a in db.house_agent_sms
                                      join b in db.house_sale_list_wuxi on a.houseid equals b.saleid
                                      join c in db.house_sale_search_wuxi on b.saleid equals c.saleid 

                                      where a.OrderUserid == User.userid && a.housetype == 1
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
                        int userid = Convert.ToInt32(temp.userid);
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
                                   where a.OrderUserid == User.userid && a.housetype == 2
                                   select new { a.id }).Count();

                var datas_temp = (from a in db.house_agent_sms
                                  join b in db.house_rent_list_wuxi on a.houseid equals b.rentid 
                                  join c in db.house_rent_search_wuxi on b.rentid equals c.rentid 
                                  where a.OrderUserid == User.userid && a.housetype == 2
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
                        int userid = Convert.ToInt32(temp.userid);
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






        //添加预约

        public string OrderViewAdd(int houseid, int housetype, string content)
        {
            try
            {
                //return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "9|"+ User.userid + "|9", data = null });

                if (housetype == 1)
                {
                    string sTitle = "预约看房--出售";
                    string sHouseType = housetype.ToString();
                    content = "用户:" + User.user_details.realname + " 手机：" + User.user_details.mobile + " 房源编号：S" + houseid + "<br /><br />留言内容：" + content;


                    string villagename = "";
                    string room = "";
                    string hall = "";
                    string toilet = "";
                    string minprice = "";

                    string khmobile = User.user_details.mobile;
                    string jjrmobile = "";
                    string jjruserid = "";
                    string jjrrealname = "";


                    using (var db = new shhouseEntities())
                    {

                        var info1 = db.house_sale_search_wuxi.FirstOrDefault(p => p.saleid == houseid);
                        if (info1 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });
                        var info2 = db.house_sale_list_wuxi.FirstOrDefault(p => p.saleid == houseid);
                        if (info2 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });
                        var info3 = db.user_details.FirstOrDefault(p => p.userid == info1.userid);
                        if (info3 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });

                        villagename = info2.villagename;
                        room = info1.room.ToString();
                        hall = info2.hall.ToString();
                        toilet = info2.toilet.ToString();
                        minprice = info1.minprice.ToString();

                        jjrmobile = info3.mobile;
                        jjruserid = info3.userid.ToString();
                        jjrrealname = info3.realname;


                        //添加
                        string sql = "insert into user_send (msgtitle,msgcontent,userid,username,msgreciveusername,origin,city) values ('" + sTitle + "',@content,'" + User.userid + "','" + User.user_details.realname + "','" + jjrrealname + "',1,3)" +
                        " insert into user_recive (msgreciveuser,msgid) select top 1 " + jjruserid + ",msgid from user_send where msgcontent = @content order by addtime desc ";
                        int insertuser_recive = db.Database.ExecuteSqlCommand(sql, new SqlParameter[] { new SqlParameter("@content", content) });


                        if (insertuser_recive > 0)
                        {
                            //您于 2018-07-04 13:37  收到一条预约看房留言，请注意查收。【e房网】

                            //给客户发信息  尊敬的用户，您已成功预约看房（世茂首府/2室2厅/165.00万），e房网顾问会尽快与您联系，请保持电话畅通
                            string kContent = string.Format("尊敬的用户，您已成功预约看房（{0}/{1}室{2}厅/{3}万），e房网顾问会尽快与您联系，请保持电话畅通！【e房网】", villagename, room, hall, minprice);
                            string kReturn = SMS.SendSMS_New(khmobile, kContent);
                            




                            //给经纪人发信息 您的房源（世茂首府/2室2厅/165.00万）已被约看，请尽快进入e房网手机端查看并联系客户
                            string sContent = string.Format("您的房源（{0}/{1}室{2}厅/{3}万）已被约看，请尽快进入e房网手机端查看并联系客户！【e房网】", villagename, room, hall, minprice);
                            string sReturn = SMS.SendSMS_New(jjrmobile, sContent);
                     

                            sql = "insert into house_agent_sms (userid,houseid,housetype,addtime,remark,status,OrderUserid) values (" + jjruserid + "," + houseid + "," + sHouseType + ",getdate(),'" + sContent + "','" + sReturn + "','" + User.userid + "')";
                            int inserthouse_agent_sms = db.Database.ExecuteSqlCommand(sql);


                            if (inserthouse_agent_sms > 0)
                            {
                                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "预约成功", data = null });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                        }

                    }
                }

                if (housetype == 2)
                {
                    string sTitle = "预约看房--出租";
                    string sHouseType = housetype.ToString();
                    content = "用户:" + User.user_details.realname + " 手机：" + User.user_details.mobile + " 房源编号：R" + houseid + "<br /><br />留言内容：" + content;

                    string villagename = "";
                    string room = "";
                    string hall = "";
                    string toilet = "";
                    string minprice = "";

                    string khmobile = User.user_details.mobile;
                    string jjrmobile = "";
                    string jjruserid = "";
                    string jjrrealname = "";


                    using (var db = new shhouseEntities())
                    {
                        var info1 = db.house_rent_search_wuxi.FirstOrDefault(p => p.rentid == houseid);
                        if (info1 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });
                        var info2 = db.house_rent_list_wuxi.FirstOrDefault(p => p.rentid == houseid);
                        if (info2 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });
                        var info3 = db.user_details.FirstOrDefault(p => p.userid == info1.userid);
                        if (info3 == null) return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });

                        villagename = info2.villagename;
                        room = info1.room.ToString();
                        hall = info2.hall.ToString();
                        toilet = info2.toilet.ToString();
                        minprice = info1.minprice.ToString();

                        jjrmobile = info3.mobile;
                        jjruserid = info3.userid.ToString();
                        jjrrealname = info3.realname;


                        string sql = "insert into user_send (msgtitle,msgcontent,userid,username,msgreciveusername,origin,city) values ('" + sTitle + "',@content,'" + User.userid + "','" + User.user_details.realname + "','" + jjrrealname + "',1,3)" +
                        " insert into user_recive (msgreciveuser,msgid) select top 1 " + jjruserid + ",msgid from user_send where msgcontent = @content order by addtime desc";
                        int insertuser_recive = db.Database.ExecuteSqlCommand(sql, new SqlParameter[] { new SqlParameter("@content", content) });


                        if (insertuser_recive > 0)
                        {
                            ////给经纪人发信息 您的房源（世茂首府/2室2厅/165.00万）已被约看，请尽快进入e房网手机端查看并联系客户
                            //string sContent = string.Format("您的房源（{0}/{1}室{2}厅/{3}元/月）已被约看，请尽快进入e房网手机端查看并联系客户", villagename, room, hall, minprice);
                            //string sReturn = SMS.SendSMS_New(jjrmobile, sContent);





                            ////给客户发信息  尊敬的用户，您已成功预约看房（世茂首府/2室2厅/165.00万），e房网顾问会尽快与您联系，请保持电话畅通
                            //string kContent = string.Format("尊敬的用户，您已成功预约看房（{0}/{1}室{2}厅/{3}元/月），e房网顾问会尽快与您联系，请保持电话畅通！", villagename, room, hall, minprice);
                            //string kReturn = SMS.SendSMS_New(khmobile, kContent);

                            //给客户发信息  尊敬的用户，您已成功预约看房（世茂首府/2室2厅/165.00万），e房网顾问会尽快与您联系，请保持电话畅通
                            string kContent = string.Format("尊敬的用户，您已成功预约看房（{0}/{1}室{2}厅/{3}元/月），e房网顾问会尽快与您联系，请保持电话畅通！【e房网】", villagename, room, hall, minprice);
                            string kReturn = SMS.SendSMS_New(khmobile, kContent);





                            //给经纪人发信息 您的房源（世茂首府/2室2厅/165.00万）已被约看，请尽快进入e房网手机端查看并联系客户
                            string sContent = string.Format("您的房源（{0}/{1}室{2}厅/{3}元/月）已被约看，请尽快进入e房网手机端查看并联系客户！【e房网】", villagename, room, hall, minprice);
                            string sReturn = SMS.SendSMS_New(jjrmobile, sContent);




                            sql = "insert into house_agent_sms (userid,houseid,housetype,addtime,remark,status,OrderUserid) values (" + jjruserid + "," + houseid + "," + sHouseType + ",getdate(),'" + sContent + "','" + sReturn + "','" + User.userid + "')";

                            int inserthouse_agent_sms = db.Database.ExecuteSqlCommand(sql);
                            if (inserthouse_agent_sms > 0)
                            {
                                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "预约成功", data = null });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                        }

                    }
                }

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "类型错误", data = null });
            }
            catch
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
        }



        public string OrderViewAdd2(int houseid, int housetype, string content)
        {
            try
            {
                string sTitle = housetype == 1 ? "预约看房--出售" : "预约看房--出租";
                string sUid = "";
                string sUname = "";
                string sMobile = "";
                string sHouseType = housetype.ToString();
                content = "用户:" + User.user_details.realname + " 手机：" + User.user_details.mobile + " 房源编号：S" + houseid + "<br /><br />留言内容：" + content;
                using (var db = new shhouseEntities())
                {
                    if (housetype == 1)
                    {
                        //通过房源找用户信息
                        var persons = (from a in db.house_sale_search_wuxi
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       where a.saleid == houseid
                                       select new
                                       {
                                           dci.userid,
                                           dci.mobile,
                                           dci.realname
                                       }).FirstOrDefault();

                        if (persons != null)
                        {
                            sUid = persons.userid.ToString();
                            sUname = persons.realname;
                            sMobile = persons.mobile.ToString();
                        }
                    }
                    if (housetype == 2)
                    {
                        var persons = (from a in db.house_rent_search_wuxi
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       where a.rentid == houseid
                                       select new
                                       {
                                           dci.userid,
                                           dci.mobile,
                                           dci.realname
                                       }).FirstOrDefault();

                        if (persons != null)
                        {
                            sUid = persons.userid.ToString();
                            sUname = persons.realname;
                            sMobile = persons.mobile.ToString();
                        }
                    }
                    if (string.IsNullOrEmpty(sUid))
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "房源不存在。", data = null });
                    }

                    user_send myuser_send = new user_send();
                    myuser_send.msgtitle = sTitle;
                    myuser_send.msgcontent = content;
                    myuser_send.userid = User.userid;
                    myuser_send.username = User.user_details.realname;
                    myuser_send.msgreciveusername = sUname;
                    myuser_send.origin = 1;
                    myuser_send.city = 3;
                    myuser_send.isdel = 0;
                    myuser_send.issystem = 0;
                    myuser_send.addtime = DateTime.Now;
                    db.user_send.Add(myuser_send);
                    db.SaveChanges();
                    int msgid = myuser_send.msgid;

                    //return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "2房源"+ msgid + "|"+ myuser_send.msgid + "不存在。", data = null });

                    user_recive myuser_recive = new user_recive();
                    myuser_recive.msgreciveuser = int.Parse(sUid);
                    myuser_recive.msgid = msgid;
                    myuser_recive.isread = 0;
                    myuser_recive.isdel = 0;
                    db.user_recive.Add(myuser_recive);
                    int insertuser_recive = db.SaveChanges();

                    if (insertuser_recive > 0)
                    {
                        string sContent = "您于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + " 收到一条预约看房留言，请注意查收。【e房网】";
                        string sReturn = SMS.SendSMS_New(sMobile, sContent);

                        house_agent_sms myhouse_agent_sms = new house_agent_sms();
                        myhouse_agent_sms.userid = int.Parse(sUid);
                        myhouse_agent_sms.houseid = houseid;
                        myhouse_agent_sms.housetype = Convert.ToInt16(sHouseType);
                        myhouse_agent_sms.addtime = DateTime.Now;
                        myhouse_agent_sms.remark = sContent;
                        myhouse_agent_sms.status = sReturn;
                        myhouse_agent_sms.OrderUserid = User.userid;

                        db.house_agent_sms.Add(myhouse_agent_sms);
                        int inserthouse_agent_sms = db.SaveChanges();
                        if (inserthouse_agent_sms > 0)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "预约成功", data = null });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
                    }
                }     
            }
            catch
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
            }
        }
    }
}