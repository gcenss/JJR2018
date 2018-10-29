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
using Dapper;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace jjr2018.Controllers
{
    public class UserController : jjrbasicController
    {

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 用户信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string UserData()
        {
            try
            {
                var houseclick = conn.Query(@"select isnull(SUM(clicknum),0)num from statist_house where houseid in (select saleid from house_sale_search_wuxi where userid=@userid)", new { userid = User.userid });
                var housetopclick = conn.Query(@"select isnull(SUM(clicknum),0)num from statist_house where houseid in (select saleid from house_sale_search_wuxi where istop =1 and userid=@userid)", new { userid = User.userid });
                var todaytophouse = conn.Query(@"select isnull(count(0),0)num  from house_sale_search_wuxi where DateDiff(dd,updatetime,getdate())=0 and istop =1 and userid=@userid", new { userid = User.userid });
                return JsonConvert.SerializeObject(
                                          new repmsg
                                          {
                                              state = 1,
                                              msg = "用户信息",
                                              data =
                                                      new
                                                      {
                                                          username = User.user_member.username,
                                                          userid = User.user_member.userid,
                                                          mobile = User.user_details.mobile,
                                                          //mobile_zs = string.IsNullOrEmpty(User.user_details.BindNum) ? User.user_details.mobile_zs : User.user_details.BindNum,
                                                          mobile_zs =  User.user_details.mobile_zs,
                                                          yjrate = User.user_member.yjrate,
                                                          servicetype = User.user_member.servicetype,
                                                          photoname = User.user_details.photoname,
                                                          gradeid = User.user_details.gradeid,
                                                          shangquanval = User.user_details.shangquanval,
                                                          origin = User.user_details.origin,
                                                          ebtotalnum = User.user_member.ebtotalnum,
                                                          allscore = User.user_details.scoretotal,
                                                          realname = User.user_details.realname,
                                                          searchtitle = User.searchtitle,
                                                          viliditystart = Convert.ToDateTime(User.user_member.viliditystart).ToShortDateString(),
                                                          vilidityend = Convert.ToDateTime(User.user_member.vilidityend).ToShortDateString(),
                                                          Remark = User.user_details.remark,
                                                          Remarkxuanyan = User.user_details.remark_xuanyan,
                                                          knowarea = User.user_details.know_area,
                                                          knowvillage = User.user_details.know_village,
                                                          refusenum = User.user_member.refusenum,
                                                          reftotalnum = User.user_member.reftotalnum,
                                                          houseclick = houseclick,
                                                          housetopclick = housetopclick,
                                                          housetotalnum = User.user_member.housetotalnum,
                                                          houseusenum = User.user_member.houseusenum,
                                                          syhousenum = User.user_member.housetotalnum - User.user_member.houseusenum,
                                                          Days = (Convert.ToDateTime(User.user_member.vilidityend) - Convert.ToDateTime(User.user_member.viliditystart)).TotalDays,
                                                          syDays = (Convert.ToDateTime(User.user_member.vilidityend) - DateTime.Now).TotalDays,
                                                          iszhongshan = string.IsNullOrEmpty(User.user_member.deptpath) ? -1 : User.user_member.deptpath.IndexOf("0,439"),
                                                          silvertotal = User.user_details.silvertotal,
                                                          IsSignIn = User.IsSignIn,
                                                          todaytophouse = todaytophouse,
                                                          roleid = User.user_member.roleid,
                                                          RCToken = gettoken(User.userid.ToString(), User.user_member.username),
                                                          shangquantext = getsahngquan(User.user_details.shangquanval),
                                                          scoretotal = User.user_details.scoretotal,
                                                          deptID = User.user_member.deptid > 0 ? User.user_member.deptid : 0,
                                                          deptname = getcompany(User.user_member.deptid > 0 ? int.Parse(User.user_member.deptid.ToString()):0),
                                                          BindNum=User.user_details.BindNum
                                                      }
                                          });

            }
            catch (Exception e)
            {

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无数据，请稍后再试！" });
            }


        }
        //所在公司
        protected string getcompany(int  deptid)
        {
            string ss=string.Empty;
            using (shhouseEntities db = new shhouseEntities())
            {
                var dept = db.user_dept.Where(p=>p.deptid==deptid).FirstOrDefault();
                if(dept!=null)
                ss = dept.deptname;
            }
            return ss;
        }

        //商圈解析方法
        protected string getsahngquan(string shagnquan)
        {
           
            string sq = "暂无数据";
            if (!string.IsNullOrEmpty(shagnquan))
            {
                JArray jar = JArray.Parse(shagnquan);
                for (int i = 0; i < jar.Count; i++)
                {
                    JObject j_val = JObject.Parse(jar[i].ToString());
                    sq += "," + j_val["b"].ToString();
                }
                if (sq != "") sq = sq.Substring(1);
            }
            return sq;
        }

        protected string gettoken(string userid, string name)
        {
            try
            {
                string url = @"https://api.cn.ronghub.com/user/getToken.json";
                string appKey = "e0x9wycfe400q";
                string appSecret = "JfKdjMRJG1iBBW";
                string postData = string.Format("userId={0}&name={1}&portraitUri={2}", userid, name, "");
                byte[] data = Encoding.UTF8.GetBytes(postData);

                // Prepare web request...  
                HttpWebRequest wrequest = (HttpWebRequest)WebRequest.Create(url);

                wrequest.Method = "POST";
                wrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                wrequest.Headers.Add("appKey", appKey);
                wrequest.Headers.Add("appSecret", appSecret);
                wrequest.ContentLength = data.Length;
                Stream newStream = wrequest.GetRequestStream();

                // Send the data.  
                newStream.Write(data, 0, data.Length);
                newStream.Close();

                // Get response  
                HttpWebResponse myResponse = (HttpWebResponse)wrequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string token = reader.ReadToEnd();
                JObject jo = (JObject)JsonConvert.DeserializeObject(token);
                token = jo["token"].ToString();
                return token;
            }
            catch(Exception e) {
                return "";
            }
        }


        /// <summary>
        /// app用 用户e币
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string eb()
        {
            try
            {
                return JsonConvert.SerializeObject(
                                          new repmsg
                                          {
                                              state = 1,
                                              msg = "用户信息",
                                              data =
                                                      new
                                                      {
                                                          ebtotalnum = User.user_member.ebtotalnum
                                                      }
                                          });

            }
            catch (Exception e)
            {

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无数据，请稍后再试！" });
            }


        }




        /// <summary>
        /// 首页数据中心
        /// </summary>
        /// <param name="userid"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public string DataCenter(string dd="",string xx="saleadd",string sx="desc")
        {
            if (dd == "")
            {
                dd = DateTime.Now.ToShortDateString();
            }
            using (shhouseEntities db = new shhouseEntities())
            {
                
                try
                {
                    //经纪人
                    if (User.user_member.roleid == 4)
                    {
                        var persons = db.Database.DynamicSqlQuery(@"select a.userid,Case when (a.housetotalnum - a.houseusenum)<1 then 0 else (a.housetotalnum - a.houseusenum) end syfb,isnull(a.houseusenum,0)houseusenum, isnull(a.refusenum,0)refusenum, isnull(b.saleadd,0)saleadd,isnull(b.rentadd,0)rentadd, isnull(b.housetopnum,0)housetopnum, (isnull(c.num,0) + isnull(d.num,0))alltopnum,realname  from user_member a 
                                                     left join (select * from statist_day where DATEDIFF(DAY,addtime,GETDATE())=0)b on a.userid=b.userid
                                                     left join (select count(0)num, userid from house_sale_search_wuxi where istop = 1 group by userid)c on a.userid = c.userid
                                                     left join (select count(0)num, userid from house_rent_search_wuxi where istop = 1 group by userid)d on a.userid = d.userid
                                                     left join user_details e on a.userid=e.userid
                                                     where a.userid =@userid", new SqlParameter[] { new SqlParameter("@userid", User.userid) });
                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "数据中心", data = persons });
                    }
                    //店长
                    else if (User.user_member.roleid == 3)
                    {

                        var persons = db.Database.DynamicSqlQuery(@"select a.userid,a.username,Case when (a.housetotalnum - a.houseusenum)<1 then 0 else (a.housetotalnum - a.houseusenum) end syfb,isnull(a.houseusenum,0)houseusenum, isnull(b.salerefsnum+b.refsnum,0)refusenum, isnull(b.saleadd,0)saleadd,isnull(b.rentadd,0)rentadd, isnull(b.housetopnum,0)housetopnum, (isnull(c.num,0) + isnull(d.num,0))alltopnum ,realname, 
                                                      isnull(b.salerefsnum,0)salerefsnum,isnull(b.rentrefsnum,0)rentrefsnum,isnull(houseusenum,0)houseusenum,(reftotalnum-refusenum)syref,ebtotalnum from user_member a
                                                     left join user_details e on a.userid=e.userid
                                                     left join (select * from statist_day where CONVERT(date,addtime,11)='" + dd + "')b on a.userid=b.userid left join (select count(0)num, userid from house_sale_search_wuxi where istop = 1 group by userid)c on a.userid = c.userid left join (select count(0)num, userid from house_rent_search_wuxi where istop = 1 group by userid)d on a.userid = d.userid  where a.deptpath='" + User.user_member.deptpath + "," + User.user_member.deptid + "' and state=0 order by "+ xx+" "+sx);
                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "数据中心", data = persons });
                    }

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "数据中心", data = "暂无数据，请稍后再试！" });

                }
                catch(Exception e)
                {

                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无数据，请稍后再试！" });
                }
            }
        }

        /// <summary>
        /// 修改基本资料
        /// </summary>
        /// <param name="realname">真实姓名</param>
        /// <param name="shangquan">商圈</param>
        /// <param name="photoname">头像照片</param>
        /// <param name=""></param>
        /// <returns></returns>
        public string BasicData(string realname,string company,string photoname,string shangquan= "")
        {
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(shangquan))
            {
                sql = " delete  from  user_search_countyid_wuxi where  userid = " + User.userid;
                for (int i = 1; i < CharString.Intercept(',', shangquan); i++)
                    sql += string.Format(" if  not  exists(select  1  from  user_search_countyid_wuxi  where  userid = {0}  and  countyshangquan = {1})  insert  into  user_search_countyid_wuxi (userid, countyshangquan)values({0}, {1})",
                        User.userid, CharString.Intercept(',', shangquan, i));

            }
            else
                sql = " delete  from  user_search_countyid_wuxi where  userid = " + User.userid;
            sql = sql + " update  user_details  set  photoname=@photoname where  userid = @userid ";
          

            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {
                    var edituser = db.Database.ExecuteSqlCommand(sql,
                                new SqlParameter[]{
                                new SqlParameter("@userid", User.userid),
                                new SqlParameter("@mobile", User.user_details.mobile.ToString()), 
                                new SqlParameter("@photoname", photoname),
               
                                
                              });
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功" });
                }
                catch(Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！" });
                }
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldpassword">旧密码</param>
        /// <param name="newpassword">新密码</param>
        /// <param name=""></param>
        /// <returns></returns>
        public string ChangePassword(string oldpassword, string newpassword)
        {

            oldpassword = Utils.MD5(oldpassword);
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_member = db.user_member.Where(p => p.userid == User.userid && p.password == oldpassword).FirstOrDefault();
                    if (user_member == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "旧密码错误", data = "" });
                    }
                    else
                    {
                        var user_member1 = db.user_member.Find(User.userid);
                        user_member.password = Utils.MD5(newpassword);
                        db.SaveChanges();
                    }
                    //string path = System.Web.HttpContext.Current.Server.MapPath($"~/tokens/" + Utils.MD5(User.userid.ToString()) + ".json");
                    //Utils.DeleteFile(path);
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功" });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,请稍后再试！" });
                }
            }
        }

        /// <summary>
        /// 手机号码管理
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="yzm">验证码</param>
        /// <param name="password">密码</param>
        /// <param name="type">1 安全手机号码,2 展示手机号码</param>
        /// <param name=""></param>
        /// <returns></returns>
        public string phonemanage(string phone, string password, string yzm, int type = 1)
        {

            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Find(User.userid);
                    if (type == 1)
                    {
                        //var uservalidityrecord = db.Database.SqlQuery<int>(" select  top 1  eid  from  user_validityrecord  where userid = @userid  and  codetype = 2  and  checkcode = @checkcode  and  state = 0  and  validity > getdate()  order  by  addtime  desc ", new SqlParameter[] { new SqlParameter("@userid", User.userid), new SqlParameter("@checkcode", yzm) });
                        var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where userid = " + User.userid + "  and  codetype = 1  and  checkcode = '" + yzm + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                        if (uservalidityrecord == null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误！" });
                        }

                        uservalidityrecord.state = "1";
                        user_details.mobile = phone;
                        db.SaveChanges();

                        //string path = System.Web.HttpContext.Current.Server.MapPath($"~/tokens/" + Utils.MD5(User.userid.ToString()) + ".json");
                        //Utils.DeleteFile(path);

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功！" });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(password))
                        {
                            password = Utils.MD5(password);
                        }
                        var user_member = db.user_member.Where(p => p.userid == User.userid && p.password == password).FirstOrDefault();
                        if (user_member == null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "密码错误！" });
                        }
                        user_details.mobile_zs = phone;
                        db.SaveChanges();

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功" });
                    }
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！" });
                }
            }

        }

        /// <summary>
        /// 个人服务
        /// </summary>
        /// <param name="yjrate">佣金比例</param>
        /// <param name="servicetype">服务类型</param>
        /// <param name=""></param>
        /// <returns></returns>
        public string PersonalService(decimal yjrate, string servicetype)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_member = db.user_member.Find(User.userid);
                    user_member.yjrate = yjrate;
                    user_member.servicetype = servicetype;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功", data = "" });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试", data = "" });
                }
            }

        }

        /// <summary>
        /// 站内消息
        /// </summary>
        /// <param name="ID">文章ID</param>
        /// <param name="pageSize">条数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public string MessageList(int pageSize = 10, int pageIndex = 1, int ID = 0)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {
                    if (ID > 0)
                    {

                        var user_noteinfo = db.user_noteinfo.Where(p => p.eid == ID).FirstOrDefault();
                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "站内消息", data = user_noteinfo });
                    }
                    else
                    {
                        int allcount = db.user_noteinfo.Where(p => !string.IsNullOrEmpty(p.title)).Count();
                        var user_noteinfolist = (from a in db.user_noteinfo
                                                 join b in db.user_details on a.userid equals b.userid into dc
                                                 from dci in dc.DefaultIfEmpty()
                                                 where !string.IsNullOrEmpty(a.title)
                                                 select new
                                                 {
                                                     a.eid,
                                                     a.title,
                                                     a.addtime,
                                                     dci.realname
                                                 }).OrderByDescending(p => p.addtime).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                     
                        return JsonConvert.SerializeObject(
                            new repmsg
                            {
                                state = 1,
                                msg = "站内消息",
                                data = new
                                {
                                    user_noteinfolist,
                                    allcount
                                }

                            },timeFormat);
                    }
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }

        }

        /// <summary>
        /// 积分记录
        /// </summary>
        /// <param name="type">1 最近7天，2最近30天，3本月</param>
        /// <param name="times">开始日期</param>
        /// <param name="timee">结束日期</param>
        /// <param name="pageSize">条数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public string RecordList(int type, DateTime times, DateTime timee, int pageSize = 10, int pageIndex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_score_wuxi = new List<user_score_wuxi>();
                    var count = 0;
                    if (type == 1)
                    {

                        user_score_wuxi = db.user_score_wuxi.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 10 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        count = db.user_score_wuxi.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 10 && p.userid == User.userid).Count();
                    }
                    else if (type == 2)
                    {
                        user_score_wuxi = db.user_score_wuxi.Where(p => DateTime.Now.Subtract(Convert.ToDateTime(p.addtime)).Days <= 30 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                    }
                    else if (type == 3)
                    {
                        user_score_wuxi = db.user_score_wuxi.OrderByDescending(p => System.Data.Entity.DbFunctions.DiffMonths(p.addtime, DateTime.Now) == 0 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                    }
                    else
                    {
                        var user_score_wuxilist = (from a in db.user_score_wuxi
                                                   where a.userid == User.userid && (DbFunctions.TruncateTime(a.addtime) >= times.Date && DbFunctions.TruncateTime(a.addtime) <= timee.Date)
                                                   select new
                                                   {
                                                       a.eid,
                                                       a.userid,
                                                       addtime = a.addtime,
                                                       a.score,
                                                       a.obtaindirections,
                                                   }).OrderByDescending(p => p.addtime).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "积分记录", data = user_score_wuxilist }, timeFormat);
                    }
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "积分记录", data = new { userscore = user_score_wuxi, counts = count } }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!" });
                }

            }
        }

        /// <summary>
        /// 店铺设置
        /// </summary>
        /// <param name="remarkxuanyan">宣言</param>
        /// <param name="remark">详情</param>
        /// <param name="knowarea">最熟悉的区域</param>
        /// <param name="knowvillage">最熟悉的小区</param>
        /// <returns></returns>
        public string shopset(string remarkxuanyan,string remark,string knowarea, string knowvillage) {
            try
            {
                using (var db = new shhouseEntities())
                {
                    var userdetails = db.user_details.Where(p => p.userid ==User.userid).FirstOrDefault();
                    userdetails.remark_xuanyan = remarkxuanyan;
                    userdetails.remark = remark;
                    userdetails.know_area = knowarea;
                    userdetails.know_village = knowvillage;
                    db.SaveChanges();
                }
            }
            catch
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "操作失败，请稍侯再试！" });

            }
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功！" });
        }


        /// <summary>
        /// 小区专家
        /// </summary>
        /// <param name="history">history=0</param>
        /// <param name="pageSize">条数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public string villageexperts(int history=1, int pagesize = 20, int pageindex = 1)
        {
            string _where = string.Empty;
            try
            {
                using (var db = new shhouseEntities())
                {
                    if (history == 0)
                    {
                        string sqlvilg = @"select villageid from user_villagesteward where userid="+User.userid+"  group by villageid order by villageid";
                        var villageid = db.Database.SqlQuery<int>(sqlvilg).ToList();
                        if (villageid != null)
                        {
                            foreach (var p in villageid)
                            {
                                _where += p + ",";
                            }
                            _where = _where.TrimEnd(',');
                            _where = " where ID in(" + _where + ")";
                        }
                        else
                        {
                            _where = "1=2";
                        }
                       string  sql = $@"select ID,Name,salenum from(select ID,Name,salenum, ROW_NUMBER() over(order by ID desc) as rows from NewHouse { _where } )t
                                    where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";


                        DataTable dt = Utils.Query(shvillageconn, sql);
                        if (dt != null)
                        {
                            dt.Columns.Add("State", Type.GetType("System.String"));//向table里增加多一列  
                            dt.Columns.Add("time", Type.GetType("System.String"));//向table里增加多一列  
          
                            int RowsCount = dt.Rows.Count;
                            for (int j = 0; j < RowsCount; j++)//为该列增加相应的数值  
                            {
                                int ID = Convert.ToInt32(dt.Rows[j]["ID"].ToString());
                                string sta = getState(ID, User.userid);
                                string time= gettime(ID, User.userid);
                                dt.Rows[j]["State"] = sta;
                                dt.Rows[j]["time"] = time;
                            }

                        }

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区专家", data = new {
                            dt,
                            dt.Rows.Count

                        } });

                    }
                    else
                    {
                        string sqlvilg = @"select villageid from house_sale_search_wuxi where isdel=0 and state=0 and userid="+ User.userid + "  group by villageid order by villageid";
                        var villageid = db.Database.SqlQuery<int>(sqlvilg).ToList();
                        if (villageid != null)
                        {
                            foreach (var p in villageid)
                            {
                                _where += p + ",";
                            }
                            _where = _where.TrimEnd(',');
                            _where = " where ID in(" + _where + ")";
                        }
                        else
                        {
                            _where = "1=2";
                        }
                         string sql = $@"select ID,Name,salenum from(select ID,Name,salenum, ROW_NUMBER() over(order by ID desc) as rows from NewHouse { _where } )t
                                    where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";

                        DataTable dt = Utils.Query(shvillageconn, sql);
                        if (dt != null)
                        {
                            dt.Columns.Add("State", Type.GetType("System.String"));//向table里增加多一列  
                            dt.Columns.Add("count", Type.GetType("System.String"));//向table里增加多一列  
                            int RowsCount = dt.Rows.Count;
                            for (int j = 0; j < RowsCount; j++)//为该列增加相应的数值  
                            {
                                int ID = Convert.ToInt32(dt.Rows[j]["ID"].ToString());
                                string sta = getState(ID, User.userid);
                                dt.Rows[j]["State"] = sta;
                                dt.Rows[j]["count"] = db.house_sale_search_wuxi.Where(x=>x.userid==User.userid).Count();
                            }

                        }

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区专家", data = new {
                            dt,
                            dt.Rows.Count
                        } });
                    }
                }
            }
            catch(Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无数据，请稍侯再试！" });

            }           

        }


        /// <summary>
        /// 小区管家提交申请
        /// </summary>
        /// <param name="vID">ID</param>
        /// <param name="vname">vname</param>
        public string apply(int vID,string vname)
        {
            try
            {
                if (vID > 0)
                {
                    using (var db = new shhouseEntities())
                    {
                        string sql = string.Format(@"
                        if not exists(select 0 from user_villagesteward where villageid={1} and userid={0}) 
                        begin insert into user_villagesteward(userid,villageid,applyunixdate,cityid,username,villagename)values({0},{1},{2},{3},'{4}','{5}') select 1 end 
                        else select 2 ", User.userid, vID, Utils.GetUnixNum(DateTime.Now), 3, User.user_details.realname, vname);

                        string sRet = Utils.Query(shhouseconn, sql).Rows[0][0].ToString();

                        if (sRet == "1")
                            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "已成功申请小区管家！" });
                        else if (sRet == "2")
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "不能重复申请小区管家！" });
                        else
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "操作失败，请稍后失败！" });
                    }
                }
                else {

                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "操作失败，请稍后失败！" });
                }


            }
            catch(Exception e) {

                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "操作失败，请稍侯再试！" });
            }
        }


        /// <summary>
        /// 小区管家判断状态
        /// </summary>
        /// <param name="ID">小区ID</param>
        /// <param name="userid">userid</param>
        /// <returns></returns>
        protected string getState(int  ID,int Userid)
        {
            string result = string.Empty;
            int iUnixdate = Utils.GetUnixNum(CharString.DateConvert(DateTime.Now.ToString("yyyy-MM-dd")));
            using (var db = new shhouseEntities())
            {
                var uservillagesteward = db.user_villagesteward.Where(p => p.villageid == ID&&p.userid== Userid).FirstOrDefault();
                if (uservillagesteward == null)
                {
                    result = "未申请";
                }
                else if (uservillagesteward.isapply == 0)
                {
                    result = "未审核";
                }
                else if (uservillagesteward.isapply == 2)
                {

                    result = "驳回";
                }
                else if (uservillagesteward.endunixdate >= iUnixdate)
                {
                    result = "使用中";
                }
                else
                {
                    result = "已过期";
                }

                return result;
            }
       }


        /// <summary>
        /// 小区管家判断状态
        /// </summary>
        /// <param name="ID">小区ID</param>
        /// <param name="userid">userid</param>
        /// <returns></returns>
        protected string gettime(int ID, int Userid) {
            string result = string.Empty;
            using (var db = new shhouseEntities())
            {
                //, CONVERT(varchar(10),dateadd(S,beginunixdate + 8 * 3600,'1970-01-01 00:00:00'),120)+'至'+CONVERT(varchar(10),dateadd(S,endunixdate + 8 * 3600,'1970-01-01 00:00:00'),120) ;
                string sql = @"select CONVERT(varchar(10),dateadd(S,applyunixdate + 8 * 3600,'1970-01-01 00:00:00'),120) as applyunixdate  from user_villagesteward where userid=" + Userid + "and villageid="+ ID + " group by villageid,applyunixdate order by villageid";
                var villageid = db.Database.SqlQuery<string>(sql).ToList();
                if (villageid != null)
                {
                    foreach (var p in villageid)
                    {
                        result = p;
                    }
                }

            }


            return result;
        }

        /// <summary>
        /// 元宝兑换e币
        /// </summary>
        /// <param name="元宝数">ybnum</param>
        /// <returns></returns>
        public string ybToeb(int ybnum)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Find(User.userid);
                    var user_member = db.user_member.Find(User.userid);
                    if (ybnum > user_details.silvertotal)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "兑换元宝大于已拥有的数量!" });
                    }
                    user_details.silvertotal = user_details.silvertotal - ybnum;
                    user_member.ebtotalnum = user_member.ebtotalnum + (ybnum/100);
                    db.SaveChanges();

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换成功!" });
                }
                catch {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换失败!" });
                }
           }

        }

        /// <summary>
        /// 元宝兑换e币
        /// </summary>
        /// <param name="元宝数">ybnum</param>
        /// <returns></returns>
        public string APPybToeb(int ybnum)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Find(User.userid);
                    var user_member = db.user_member.Find(User.userid);
                    if (ybnum > user_details.silvertotal)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "兑换元宝大于已拥有的数量!" });
                    }
                    user_details.silvertotal = user_details.silvertotal - ybnum;
                    user_member.ebtotalnum = user_member.ebtotalnum + (ybnum / 100);
                    db.SaveChanges();

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换成功!",
                        data = new
                        {
                           ybtotal= user_details.silvertotal,
                           ebtotal = user_member.ebtotalnum
                        }
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换失败!" });
                }
            }

        }



        /// <summary>
        /// 签到
        /// </summary>
        /// <returns></returns>
        public string SignIn()
        {
            using (var db = new shhouseEntities())
            {
                DateTime sdt = DateTime.Now.Date;
                DateTime dt = DateTime.Now.Date.AddDays(1);

                var SignIn = db.SignIn.Where(x=>x.UserID==User.userid&&x.exe_date>= sdt&& x.exe_date <dt).FirstOrDefault();
                if (SignIn != null)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "今日已签过!" });
                }
                else {
                    var stephen = new SignIn
                    { 
                        UserID = User.userid,
                        exe_date = DateTime.Now
                    };
                    var user_score_wuxi = new user_score_wuxi
                    {
                        userid = User.userid,
                        addtime = DateTime.Now,
                        score=5,
                        obtaindirections= "签到积分"
                    };
                    var userdetails = db.user_details.Where(p => p.userid == User.userid).FirstOrDefault();
                    userdetails.scoretotal = userdetails.scoretotal + 5;
                    db.user_score_wuxi.Add(user_score_wuxi);
                    db.SignIn.Add(stephen);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "签到成功!" });
                } 
            }      
        }

        /// <summary>
        /// 拨打电话消费1e币
        /// </summary>
        /// <param name="类型">type=0关注  1 取消关注</param>
        /// <returns></returns>
        public string call(int eb=1)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Find(User.userid);
                    var user_member = db.user_member.Find(User.userid);
                    if (eb > user_member.ebtotalnum)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "e币余额不足!" });
                    }
                    user_member.ebtotalnum = user_member.ebtotalnum - eb;
                    db.SaveChanges();
                   
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "消费成功!",
                        data = new
                        {
                            ebtotal = user_member.ebtotalnum
                        }
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换失败!" });
                }
            }

        }

        /// <summary>
        /// 关注小区
        /// </summary>
        /// <returns></returns>
        public string VillageCollection(int VillageID,int type) {

            using (var db = new shhouseEntities())
            {
                try
                {
                    var VillageCollections = db.VillageCollection.Where(x => x.UserID == User.userid && x.VillageID == VillageID).FirstOrDefault();
                    if (type == 0)
                    {
                       
                        if (VillageCollections != null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "此小区已经关注过了!" });
                        }
                        var VillageCollection = new VillageCollection
                        {
                            UserID = User.userid,
                            VillageID = VillageID,
                            Addtime = DateTime.Now
                        };
                        db.VillageCollection.Add(VillageCollection);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.VillageCollection.Remove(VillageCollections);
                        db.SaveChanges();
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "成功!",
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 0, msg = "关注失败!" });
                }
            }

        }




    }
}