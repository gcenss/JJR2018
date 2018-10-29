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
using Newtonsoft.Json.Converters;

namespace jjr2018.Controllers
{
    public class HomeController : Controller
    {
        private string shhouseconnstr = ConfigurationManager.ConnectionStrings["shhouseconn"].ConnectionString;
        private string shvillageconnstr = ConfigurationManager.ConnectionStrings["shvillageconn"].ConnectionString;
        public SqlConnection shvillageconn { get; private set; }
        public SqlConnection shhouseconn { get; private set; }

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string login(string username, string password)
        {
            string sValue = "";
            if (DateTime.Now.Hour != 1)
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {

                    string sRoleid = "", sCity = "", sName = "", sState = "", sSerieslogin = "", sDeptpath = "", passwords = "",
                           sBegintime = "", sEndtime = "", sLastlogintime = "", sSeriestime = "", sUserid = "", sPermition = "", sMobile = "",roleid="";
                    passwords = Utils.MD5(password);
                    using (var db = new shhouseEntities())
                    {

                        var persons = (from a in db.user_member
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       join c in db.user_validity on a.userid equals c.userid into ec
                                       from eci in ec.DefaultIfEmpty()
                                       where (a.username == username|| dci.mobile==username) && a.password == passwords && (a.roleid==3||a.roleid==4)
                                       select new
                                       {
                                           a.userid,
                                           a.username,
                                           a.roleid,
                                           a.city,
                                           a.deptpath,
                                           a.state,
                                           a.serieslogin,
                                           eci.begintime,
                                           eci.endtime,
                                           a.lastlogintime,
                                           a.seriestime,
                                           a.mobile,
                                       }).FirstOrDefault();

                        if (persons != null)
                        {
                            sUserid = persons.userid.ToString();
                            sName = persons.username;
                            sRoleid = persons.roleid.ToString();
                            sCity = persons.city.ToString();
                            sDeptpath = persons.deptpath;
                            sState = persons.state.ToString();
  
                            sBegintime = persons.begintime.ToString();      //开始时间
                            sEndtime = persons.endtime.ToString();          //截止时间
                            sLastlogintime = persons.lastlogintime.ToString();
                            sSeriestime = persons.seriestime.ToString();
                            sMobile = persons.mobile.ToString();
                            roleid = persons.roleid.ToString();
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码输入错误。", data = null });
                        }

                    }
                    if (sState == "-1")//锁定
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被冻结，请联系您的主管或管理员。", data = null });
                    else if (sState == "-2")
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被删除，请联系您的主管或管理员", data = null });


                    if (!string.IsNullOrEmpty(sEndtime) && sBegintime != sEndtime)
                    {
                        if (sRoleid == "4" && !string.IsNullOrEmpty(sDeptpath))
                        {
                            sDeptpath = "," + sDeptpath;
                            sDeptpath = sDeptpath.Replace(",0,", "");
                            using (var db = new shhouseEntities())
                            {
                                var user_validity = db.user_validity.SqlQuery("select * from user_validity where userid = (select top 1 userid from user_member where deptid in("+ sDeptpath + "))").FirstOrDefault();
                                if (user_validity != null)
                                {
                                    sEndtime = user_validity.endtime.ToString();
                                }
                            }
                        }

                        if (Convert.ToDateTime(CharString.DateConvert(sEndtime)) < Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您的账户服务期已截止，请去续费充值!", data = null });
                    }

                    DateTime sdt = DateTime.Now.Date;
                    DateTime dt = DateTime.Now.Date.AddDays(1);
                    //更新用户表
                    using (var db = new shhouseEntities())
                    {
                        var user_member = db.user_member.Find(int.Parse(sUserid));
                        user_member.lastlogintime = user_member.logintime;
                        user_member.lastloginip = user_member.loginip;
                        user_member.logincount = user_member.logincount + 1;
                        user_member.logintime = DateTime.Now;
                        user_member.loginip = Utils.GetRealIP();

                        var user_details = db.user_details.Find(int.Parse(sUserid));
                        user_details.logintimenum = Utils.GetUnixNum(DateTime.Now);
                        user_details.logintime = DateTime.Now;

                        db.SaveChanges();
                        db.Database.ExecuteSqlCommand("UPDATE user_search_all_wuxi set  LastLoginTime = (datediff(S,'1970-01-01 00:00:00', getdate()) - 8 * 3600)  where  userid =@sUserid" , new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });

                    }


                    ////写入token
                    //string token = Utils.MD5(sUserid);
                    //string tokenjson=JsonConvert.SerializeObject(new { userid = sUserid, Lastlogintime = sLastlogintime});
                    //System.IO.File.WriteAllText(Server.MapPath($"~/tokens/{token}.json"), tokenjson);
                    //return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "登录成功", data = token });


                    string token = Utils.MD5(sUserid);
                    using (var db = new shhouseEntities())
                    {
                        //写入token                       
                        db.Database.ExecuteSqlCommand("delete from user_logintoken where  userid = @sUserid", new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });
                        user_logintoken myuser_Logintoken = new user_logintoken();
                        myuser_Logintoken.UserID = int.Parse(sUserid);
                        myuser_Logintoken.token = token;
                        myuser_Logintoken.loninTime = DateTime.Now;
                        db.user_logintoken.Add(myuser_Logintoken);
                        db.SaveChanges();
                    }
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "登录成功", data = token });

                    //return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "登录成功",
                    //    data = new
                    //    {
                    //        token = token,
                    //        roleid = roleid
                    //    }
                    // });

                }
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码不能为空", data = null });
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "凌晨 1:00 - 2:00 数据维护中...", data = null });
            }
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string APPlogin(string username, string password)
        {


            string sValue = "";
            if (DateTime.Now.Hour != 1)
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {

                    string sRoleid = "", sCity = "", sName = "", sState = "", sSerieslogin = "", sDeptpath = "", passwords = "",
                           sBegintime = "", sEndtime = "", sLastlogintime = "", sSeriestime = "", sUserid = "", sPermition = "", sMobile = "", roleid = "";
                    passwords = Utils.MD5(password);
                    using (var db = new shhouseEntities())
                    {

                        var persons = (from a in db.user_member
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       join c in db.user_validity on a.userid equals c.userid into ec
                                       from eci in ec.DefaultIfEmpty()
                                       where (a.username == username || dci.mobile == username) && a.password == passwords 
                                       select new
                                       {
                                           a.userid,
                                           a.username,
                                           a.roleid,
                                           a.city,
                                           a.deptpath,
                                           a.state,
                                           a.serieslogin,
                                           eci.begintime,
                                           eci.endtime,
                                           a.lastlogintime,
                                           a.seriestime,
                                           a.mobile,
                                       }).FirstOrDefault();

                        if (persons != null)
                        {
                            sUserid = persons.userid.ToString();
                            sName = persons.username;
                            sRoleid = persons.roleid.ToString();
                            sCity = persons.city.ToString();
                            sDeptpath = persons.deptpath;
                            sState = persons.state.ToString();

                            sBegintime = persons.begintime.ToString();      //开始时间
                            sEndtime = persons.endtime.ToString();          //截止时间
                            sLastlogintime = persons.lastlogintime.ToString();
                            sSeriestime = persons.seriestime.ToString();
                            sMobile = persons.mobile.ToString();
                            roleid = persons.roleid.ToString();
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码输入错误。", data = null });
                        }

                    }
                    if (sState == "-1")//锁定
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被冻结，请联系您的主管或管理员。", data = null });
                    else if (sState == "-2")
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被删除，请联系您的主管或管理员", data = null });


                    if (!string.IsNullOrEmpty(sEndtime) && sBegintime != sEndtime)
                    {
                        if (sRoleid == "4" && !string.IsNullOrEmpty(sDeptpath))
                        {
                            sDeptpath = "," + sDeptpath;
                            sDeptpath = sDeptpath.Replace(",0,", "");
                            using (var db = new shhouseEntities())
                            {
                                var user_validity = db.user_validity.SqlQuery("select * from user_validity where userid = (select top 1 userid from user_member where deptid in(" + sDeptpath + "))").FirstOrDefault();
                                if (user_validity != null)
                                {
                                    sEndtime = user_validity.endtime.ToString();
                                }
                            }
                        }

                        if (Convert.ToDateTime(CharString.DateConvert(sEndtime)) < Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您的账户服务期已截止，请去续费充值!", data = null });
                    }

                    DateTime sdt = DateTime.Now.Date;
                    DateTime dt = DateTime.Now.Date.AddDays(1);
                    //更新用户表
                    using (var db = new shhouseEntities())
                    {
                        var user_member = db.user_member.Find(int.Parse(sUserid));
                        user_member.lastlogintime = user_member.logintime;
                        user_member.lastloginip = user_member.loginip;
                        user_member.logincount = user_member.logincount + 1;
                        user_member.logintime = DateTime.Now;
                        user_member.loginip = Utils.GetRealIP();

                        var user_details = db.user_details.Find(int.Parse(sUserid));
                        user_details.logintimenum = Utils.GetUnixNum(DateTime.Now);
                        user_details.logintime = DateTime.Now;

                        db.SaveChanges();
                        db.Database.ExecuteSqlCommand("UPDATE user_search_all_wuxi set  LastLoginTime = (datediff(S,'1970-01-01 00:00:00', getdate()) - 8 * 3600)  where  userid =@sUserid", new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });

                    }


                    ////写入token
                    //string token = Utils.MD5(sUserid);
                    //string tokenjson = JsonConvert.SerializeObject(new { userid = sUserid, Lastlogintime = sLastlogintime });
                    //System.IO.File.WriteAllText(Server.MapPath($"~/tokens/{token}.json"), tokenjson);
                    ////return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "登录成功", data = token });
                    //return JsonConvert.SerializeObject(new repmsg
                    //{
                    //    state = 1,
                    //    msg = "登录成功",
                    //    data = new
                    //    {
                    //        token = token,
                    //        roleid = roleid
                    //    }
                    //});

                    string token = Utils.MD5(sUserid);
                    using (var db = new shhouseEntities())
                    {
                        //写入token                       
                        db.Database.ExecuteSqlCommand("delete from user_logintoken where  userid = @sUserid", new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });
                        user_logintoken myuser_Logintoken = new user_logintoken();
                        myuser_Logintoken.UserID = int.Parse(sUserid);
                        myuser_Logintoken.token = token;
                        myuser_Logintoken.loninTime = DateTime.Now;
                        db.user_logintoken.Add(myuser_Logintoken);
                        db.SaveChanges();
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "登录成功",
                        data = new
                        {
                            token = token,
                            roleid = roleid
                        }
                    });


                }
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码不能为空", data = null });
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "凌晨 1:00 - 2:00 数据维护中...", data = null });
            }
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string regist(string username, string password,string mobile,string realname, string smscode )
        {
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码为空,因此无法提交!", data = null });
            }
            if (!Utils.IsSafeSqlString(username))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您输入的用户名包含不安全的字符,因此无法提交!", data = null });
            }

            if (getJ(username.Trim()) > 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您所输入的用户名已被使用过, 请输入其他的用户名!", data = null });
            }
            if (getM(mobile.Trim()) > 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您所输入的手机号码已被使用过, 请输入其他的手机号码!", data = null });
            }



            using (var db = new shhouseEntities())
            {
                var intuser=db.Database.ExecuteSqlCommand(adduser,
                         new SqlParameter[]{
                                new SqlParameter("@parentid", "0"),
                                new SqlParameter("@parentpath", ""),
                                new SqlParameter("@mobile", mobile.Trim()),
                                new SqlParameter("@telfirst", ""),
                                new SqlParameter("@tel", ""),
                                new SqlParameter("@linkman", ""),
                                new SqlParameter("@area", ""),
                                new SqlParameter("@city", 3),
                                new SqlParameter("@storetotal", "0"),
                                new SqlParameter("@housetotal", "5"),
                                new SqlParameter("@agenttotal", "0"),
                                new SqlParameter("@refamount", "7"),                    
                                new SqlParameter("@userid2", "0"),
                                new SqlParameter("@username", Utils.CheckData(username.Trim())),
                                new SqlParameter("@realname", Utils.CheckData(realname.Trim())),
                                new SqlParameter("@password", Utils.MD5(password.Trim())),
                                new SqlParameter("@addip", Utils.GetRealIP()),
                                new SqlParameter("@roleid", 4),
                                new SqlParameter("@origin", ""),
                                new SqlParameter("@originpath", ""),
                                new SqlParameter("@ismobilelock", "0"),
                                new SqlParameter("@validity_begintime", "1990-1-1"),
                                new SqlParameter("@validity_endtime", "1990-1-1")
                              });

                if (intuser.ToString() == "-99")
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名重复，请重新输入!", data = null });
                else if (CharString.IntConvert(intuser) > 0)
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "注册成功!", data = null });
            }
            return "";
        }

        /// <summary>
        /// 新增用户  1总店 、2独立总店 、3门店 、4经纪人 、 5个人   当前为门店时，给 @parentid 赋值    
        /// </summary>
        private static string adduser
        {
            get
            {
                return " declare  @deptid  int, @deptpath  varchar(30), @zm  varchar(5), @fF  int, @sS  varchar(30)  "
                    + " if  exists( select  *  from  user_member  where  username = @username)  select  -99   "
                    + " else  begin "
                    + " if  @roleid = 1  begin  set  @fF = 0  set  @sS = '0,'  set  @deptpath = '0'  end  "
                    + " if  @roleid = 2  begin  set  @fF = 0  set  @sS = '0'  set  @deptpath = '0'  end  "
                    + " else  if  @roleid = 3  and  @parentid > 0  begin  "
                    + "    set  @fF = @parentid  set  @sS = @parentpath  set  @deptpath = @parentpath  "
                    + "    update  statist_total  set  housenum = housenum + @housetotal, storenum = storenum + 1, agentnum = agentnum + @agenttotal  where  userid = @userid2"
                    + " end  "       //当添加门店时，同时改变总店的端口统计数 

                    + " else  if  @roleid = 4  and  @parentid > 0  begin  "
                    + "    set  @deptpath = (select  parentpath  from  user_dept  where  deptid = @parentid)   set  @deptpath = (@deptpath + ',' + cast(@parentid  as  varchar(10)))  set  @deptid = 0  "
                    + "    update  statist_total  set  agentnum = agentnum + 1, housenum = housenum + @housetotal  where  userid = @userid2"
                    + " end     "    //当添加经纪人时，(parentid 上级门店ID)
                    + " else  set  @deptid = '0'   "

                    + " if  @roleid = 1  or  @roleid = 2  or  @roleid = 3  begin  insert  into  user_dept(deptname, parentid, parentpath, telfirst, telsecond, mobile, company, linkman, address, area, city) values(@deptname, @fF, @sS, @telfirst, @tel, @mobile, @company, @linkman, @address, @area, @city)  set  @deptid = cast(SCOPE_IDENTITY()  as  varchar)  end "
                    + " set  @zm = (select  firstletter  from  base_area  where  areaid = @city)"
                    + " insert  into  user_member (username, password, addip, deptid, deptpath, roleid, area, city)  values(@username, @password, @addip, @deptid, @deptpath, @roleid, @area, @city)  "
                    + " declare  @userid_identity  int  set  @userid_identity = (select  scope_identity())"
                    + " insert  into  user_details(userid, username, realname, mobile, telfirst, tel, islockmobile, originpath, origin) values(@userid_identity, @username, @realname, @mobile, @telfirst, @tel, @ismobilelock, @originpath, @origin)  "
                    + " insert  into  user_verified(userid)  values(@userid_identity)  "

                    + " insert  into  user_search_all_wuxi (UserID, HasImg, RealAudit, searchTitle, StarLevel, LastLoginTime)values(@userid_identity, 0, 0, @realname + ',' + @mobile + ',' + @originpath, 1, 0)  "

                    + " insert  into  user_validity(userid, begintime, endtime)  values(@userid_identity, @validity_begintime, @validity_endtime)  "
                    + " insert  into  statist_total(userid, housetotal, storetotal, agenttotal, refamount, city)values(@userid_identity, @housetotal, @storetotal, @agenttotal, @refamount, @city)  " //用户可发布房源数
                    + " select  @userid_identity  end ";
            }
        }

        /// <summary>
        /// 注册账号所用方法1
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private int getJ(string sName)
        {
            int iA = 0;
            using (var db = new shhouseEntities())
            {
                var user_member = db.user_member.Where(p => p.username == sName);
                //var user_member = db.user_member.SqlQuery(" select  userid  from  user_member  where  username = '@username' ", new SqlParameter[] { new SqlParameter("@username", sName) });
                foreach (var p in user_member)
                {
                    var sss = p.username;
                    var sss1 = p.userid;
                }
                    iA = user_member.Count();
            }
            return iA;
        }

        /// <summary>
        /// 注册账号所用方法2
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private int getM(string sName)
        {
            int iA = 0;
            using (var db = new shhouseEntities())
            {
                var user_details = db.user_details.Where(p => p.mobile == sName);
               // var user_details = db.user_details.SqlQuery(" select  userid  from  user_details  where  mobile = @mobile ", new SqlParameter[] { new SqlParameter("@mobile", sName) });
                iA = user_details.Count();
            }
            return iA;
        }


        /// <summary>
        /// 发送手机短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public string sendsmscode(string mobile)
        {

            bool flg = false;
            string sID = "", sEmail = "", sMobile = "";
            if (!string.IsNullOrEmpty(mobile))
            {
                //查询用户信息
                using (var db = new shhouseEntities())
                {
                    var persons = db.user_details.Where(x => x.mobile == mobile).FirstOrDefault();
                    //var persons = (from u in db.user_details join b in db.user_member on u.userid equals b.userid where b.mobile == mobile select u).FirstOrDefault();
                    if (persons != null)
                    {
                        sID = persons.userid.ToString();
                        sEmail = persons.email;
                        sMobile = persons.mobile;
                        flg = true;
                    }
                }
                if (flg)
                {
                    string sCode = Utils.MD5(sID + Utils.GetUnixNum(DateTime.Now));
                    sCode = Utils.GetRandom(6);
                    if (SMS.SendSMS_New(sMobile, "验证码：" + sCode + "  【e房网】") != "100")
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "短信发送失败，请稍侯再试", data = null });
                    }
                    else
                    {
                        insert_validityrecord(sID, 24, "", sCode, 1);
                    }

                }
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "发送成功", data = null });
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "发送失败", data = null });
        }

        /// <summary>
        /// 发送手机短信所用方法
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        private void insert_validityrecord(string sID, int iHours, string sBody, string sCode, int iType)
        {
            try
            {
                using (var db = new shhouseEntities())
                {
                    var user_validityrecord = new user_validityrecord()
                    {
                        userid = int.Parse(sID),
                        validity = DateTime.Now.AddHours(24),
                        checkcode = sCode,
                        codetype = iType,
                        state = "0",
                        body = ""

                    };
                    db.user_validityrecord.Add(user_validityrecord);
                    db.SaveChanges();
                }
            }
            catch
            {

            }
        }



        /// <summary>
        /// 首页右边列信息（公告 1，违规公告 2，问答 3）
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string RightMsg(int type)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd";
            using (var db = new shhouseEntities())
            {
                if (type == 1)
                {
                    var user_noteinfos = db.user_noteinfo.OrderByDescending(p => p.addtime).Take(5);
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "公告", data = user_noteinfos }, timeFormat);
                }
                else if (type == 2)
                {
                    var user_noteinfos = db.user_noteinfo.OrderByDescending(p => p.addtime).Take(5);
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "违规公告", data = user_noteinfos});
                }
                else
                {
                    var user_noteinfos = db.user_noteinfo.OrderByDescending(p => p.addtime).Take(5);
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "问答", data = user_noteinfos });
                }
            }
        }

        /// <summary>
        /// 基础数据
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string BaseData() {
            using (var db = new shhouseEntities())
            {
                var base_samtype = db.base_samtype.OrderBy(p => p.parentid).OrderBy(p=>p.orderid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "基础数据", data = base_samtype });
            }
       }

        /// <summary>
        /// 房源标签
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string houseflag()
        {
            using (var db = new shhouseEntities())
            {
                var house_tags = db.house_tags.OrderByDescending(p=>p.parentid);
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "房源标签", data = house_tags });
            }
        }

        /// <summary>
        /// 小区查询
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //public string XiaoQu2(string name)
        //{
        //    shvillageconn = new SqlConnection(shvillageconnstr);
        //    string sql = "select top 10 Name,ID,Address,SectionID,quyu from NewHouse where cityid=3";
        //    if (!string.IsNullOrEmpty(name))
        //    {

        //        sql += " and Name like'%" + name + "%'";
        //    }
        //    DataTable dt = Utils.Query(shvillageconn, sql);
        //    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区", data = dt });
        //}


        /// <summary>
        /// 小区查询2
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string XiaoQu(string name)
        {
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "小区", data = Utils.getHouseInfoKey(System.Web.HttpUtility.UrlDecode(name)) });
        }

        /// <summary>
        /// 基础数据
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string BaseArea()
        {
            shhouseconn = new SqlConnection(shhouseconnstr);
            string sql = "select areaid,areaname,parentid from base_area where parentid in(select areaid from base_area where parentid=3)";
            DataTable dt = Utils.Query(shhouseconn, sql);
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "商圈", data = dt });
        }

        /// <summary>
        /// 基础数据根据ID查询
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string BaseArea2(int ID)
        {
            shhouseconn = new SqlConnection(shhouseconnstr);
            string sql = "select areaid,areaname,parentid from base_area where parentid in(select areaid from base_area where parentid=3) and areaid="+ID;
            DataTable dt = Utils.Query(shhouseconn, sql);
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "商圈", data = dt });
        }

        /// <summary>
        /// 根据手机号重设新密码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        public string findpassword(string mobile, string smscode, string newpassword)
        {

            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Where(x => x.mobile == mobile).FirstOrDefault();
                    var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where userid = " + user_details.userid + "  and  codetype = 1  and  checkcode = '" + smscode + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                    if (uservalidityrecord == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误！" });
                    }
                    uservalidityrecord.state = "1";
                    var user_member = db.user_member.Find(user_details.userid);
                    user_member.password = Utils.MD5(newpassword);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功！" });


                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！" });
                }
            }
        }


        /// <summary>
        /// 用户信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string getuserinfo(int userid)
        {
            shhouseconn = new SqlConnection(shhouseconnstr);
            string sql = "select a.username,photoname,b.realname,a.roleid,b.mobile,b.origin,know_village,know_area from user_member a left join user_details b on  a.userid=b.userid where a.userid=" + userid;
            DataTable dt = Utils.Query(shhouseconn, sql);
            for (int i = 0; i < dt.Rows.Count; i++) { dt.Rows[i]["know_area"] = Utils.chuli(dt.Rows[i]["know_area"]); }
            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "用户信息", data = dt });
        }


    }
}