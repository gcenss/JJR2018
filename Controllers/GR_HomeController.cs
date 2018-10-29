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

using System.Text;

namespace jjr2018.Controllers
{
    public class GR_HomeController : Controller
    {
        //private string shhouseconnstr = ConfigurationManager.ConnectionStrings["shhouseconn"].ConnectionString;
        //private string shvillageconnstr = ConfigurationManager.ConnectionStrings["shvillageconn"].ConnectionString;
        //public SqlConnection shvillageconn { get; private set; }
        //public SqlConnection shhouseconn { get; private set; }

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        /// <summary>
        /// 个人会员登录 密码登录 http://192.168.1.223/GR_Home/login
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public string Login(string username, string password)
        {
            if (DateTime.Now.Hour != 1)
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string sRoleid = "", sCity = "", sName = "", sState = "", sDeptpath = "", passwords = "", sBegintime = "", sEndtime = "", sLastlogintime = "", sSeriestime = "", sUserid = "", sMobile = "";
                    passwords = Utils.MD5(password);
                    using (var db = new shhouseEntities())
                    {
                        var persons = (from a in db.user_member
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       join c in db.user_validity on a.userid equals c.userid into ec
                                       from eci in ec.DefaultIfEmpty()
                                       where (a.username == username || dci.mobile == username) && a.password == passwords && a.password!=""
                                       //&& a.roleid == 5
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
                                           a.mobile
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
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码输入错误。", data = null });
                        }
                    }
                    if (sState == "-1")//锁定
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被冻结。", data = null });
                    else if (sState == "-2")
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被删除。", data = null });



                    //非个人会员
                    if (sRoleid != "5")
                    {
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
                    }

                    DateTime sdt = DateTime.Now.Date;
                    DateTime ndt = DateTime.Now.Date.AddDays(1);
                



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

                        //RCToken=  Utils.gettoken(sUserid, user_member.username);

                        int userid = int.Parse(sUserid);
                        //var user_score = db.user_score_wuxi.Where(x => x.userid == userid && x.addtime >= sdt && x.addtime < ndt && x.obtaindirections == "登录积分").FirstOrDefault();
                        //if (user_score == null)
                        //{
                        //    var user_score_wuxi = new user_score_wuxi
                        //    {
                        //        userid = int.Parse(sUserid),
                        //        addtime = DateTime.Now,
                        //        score = userscore.sign,//登陆积分
                        //        obtaindirections = "登录积分"
                        //    };
                        //    db.user_score_wuxi.Add(user_score_wuxi);
                        //}
                        db.SaveChanges();
                        db.Database.ExecuteSqlCommand("UPDATE user_search_all_wuxi set  LastLoginTime = (datediff(S,'1970-01-01 00:00:00', getdate()) - 8 * 3600)  where  userid =@sUserid", new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });
                    }
                    //写入token
                    //string token = Utils.MD5(sUserid);
                    //string tokenjson = JsonConvert.SerializeObject(new { userid = sUserid, Lastlogintime = sLastlogintime });
                    //System.IO.File.WriteAllText(Server.MapPath($"~/tokens/{token}.json"), tokenjson);

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
                            roleid = sRoleid
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
        /// 个人会员登录 手机号码+验证码 http://192.168.1.223/GR_Home/LoginBySmscode
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <returns></returns>
        public string LoginBySmscode(string mobile, string smscode)
        {
            if (DateTime.Now.Hour != 1)
            {
                if (!string.IsNullOrEmpty(mobile) && !string.IsNullOrEmpty(smscode))
                {
                    using (var db = new shhouseEntities())
                    {
                        //先判断验证码正确不正确
                        var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where Mobile = " + mobile
                        + "  and  codetype = 1  and  checkcode = '" + smscode + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();

                        if (uservalidityrecord == null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误！", data = null });
                        }
                        uservalidityrecord.state = "1";
                        db.SaveChanges();

                        //把这条验证码保存为已经使用过 

                        decimal demobile = Convert.ToDecimal(mobile);

                        var user_details = db.user_member.Where(x => x.mobile == demobile).FirstOrDefault();
                        if (user_details == null)
                        {
                             return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户不存在！", data = null });
                            //用户不存在  就注册一个用户名 并提交             
                            //if (!Utils.IsSafeSqlString(mobile))
                            //{
                            //    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您输入的手机号码包含不安全的字符,因此无法提交!", data = null });
                            //}           
             
                                //var intuser = db.Database.ExecuteSqlCommand(User_Common.GR_adduserbymobile_sql,
                                //         new SqlParameter[]{
                                //new SqlParameter("@parentid", "0"),
                                //new SqlParameter("@parentpath", ""),
                                //new SqlParameter("@mobile", mobile.Trim()),
                                //new SqlParameter("@telfirst", ""),
                                //new SqlParameter("@tel", ""),
                                //new SqlParameter("@linkman", ""),
                                //new SqlParameter("@area", ""),
                                //new SqlParameter("@city", 3),
                                //new SqlParameter("@storetotal", "0"),
                                //new SqlParameter("@housetotal", "5"),
                                //new SqlParameter("@agenttotal", "0"),
                                //new SqlParameter("@refamount", "7"),
                                //new SqlParameter("@userid2", "0"),
                                ////new SqlParameter("@username", Utils.CheckData(username.Trim())),
                                ////new SqlParameter("@realname", Utils.CheckData(realname.Trim())),
                                //new SqlParameter("@password", ""),
                                //new SqlParameter("@addip", Utils.GetRealIP()),
                                //new SqlParameter("@roleid", 5), //为五的时候为个人会员
                                //new SqlParameter("@origin", ""),
                                //new SqlParameter("@originpath", ""),
                                //new SqlParameter("@ismobilelock", "0"),
                                //new SqlParameter("@validity_begintime", "1990-1-1"),
                                //new SqlParameter("@validity_endtime", "1990-1-1"),
                                //new SqlParameter("@deptname", ""),
                                //new SqlParameter("@company", ""),
                                //new SqlParameter("@address", "")
                                //              });   
                        }

                    }


                    string sRoleid = "", sCity = "", sName = "", sState = "", sDeptpath = "", passwords = "", sBegintime = "", sEndtime = "", sLastlogintime = "", sSeriestime = "", sUserid = "", sMobile = "";
                    //passwords = Utils.MD5(password);
                    using (var db = new shhouseEntities())
                    {
                        var persons = (from a in db.user_member
                                       join b in db.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       join c in db.user_validity on a.userid equals c.userid into ec
                                       from eci in ec.DefaultIfEmpty()
                                       where (dci.mobile == mobile)
                                       //&& a.roleid == 5
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
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户不存在。", data = null });
                        }
                    }
                    if (sState == "-1")//锁定
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被冻结。", data = null });
                    else if (sState == "-2")
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "抱歉，您的账户已被删除。", data = null });


                    //非个人会员
                    if (sRoleid != "5")
                    {
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
                    }





                    DateTime sdt = DateTime.Now.Date;
                    DateTime ndt = DateTime.Now.Date.AddDays(1);

                   
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

                        int userid = int.Parse(sUserid);
                        //var user_score = db.user_score_wuxi.Where(x => x.userid == userid && x.addtime >= sdt && x.addtime < ndt && x.obtaindirections == "登录积分").FirstOrDefault();
                        //if (user_score == null)
                        //{
                        //    var user_score_wuxi = new user_score_wuxi
                        //    {
                        //        userid = int.Parse(sUserid),
                        //        addtime = DateTime.Now,
                        //        score = userscore.sign,//登陆积分
                        //        obtaindirections = "登录积分"
                        //    };
                        //    db.user_score_wuxi.Add(user_score_wuxi);
                        //}
                        db.SaveChanges();
                        db.Database.ExecuteSqlCommand("UPDATE user_search_all_wuxi set  LastLoginTime = (datediff(S,'1970-01-01 00:00:00', getdate()) - 8 * 3600)  where  userid =@sUserid", new SqlParameter[] { new SqlParameter("@sUserid", sUserid) });
                    }
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


                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "登录成功",
                        data = new
                        {
                            token = token,
                            roleid = sRoleid
                        }
                    });

                }
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "手机号或验证码不能为空", data = null });
            }
            else
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "凌晨 1:00 - 2:00 数据维护中...", data = null });
            }
        }
        
        /// <summary>
        /// 注册账号 http://192.168.1.223/GR_Home/Regist
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号码</param>
        /// <param name="realname">真实姓名</param>
        /// <param name="smscode">验证码</param>
        /// <returns></returns>
        public string Regist(string username, string password, string mobile, string realname, string smscode )
        {

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码为空,提交失败!", data = null });
            }
            if (!Utils.IsSafeSqlString(username))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您输入的用户名包含不安全的字符,因此无法提交!", data = null });
            }
            if (!Utils.IsSafeSqlString(mobile))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您输入的手机号码包含不安全的字符,因此无法提交!", data = null });
            }
            if (User_Common.IsHaveUsername(username.Trim()) > 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您所输入的用户名已被使用过, 请输入其他的用户名!", data = null });
            }
            if (User_Common.IsHaveMobile(mobile.Trim()) > 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您所输入的手机号码已被使用过, 请输入其他的手机号码!", data = null });
            }   



            using (var db = new shhouseEntities())
            {
               
                var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where Mobile = " + mobile + "  and  codetype = 1  and  checkcode = '" + smscode + "'" +
                    "  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                if (uservalidityrecord == null)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误或已经失效请重新获取！", data = null });
                }
                uservalidityrecord.state = "1";
                db.SaveChanges();


                var intuser = db.Database.ExecuteSqlCommand(User_Common.GR_adduser_sql,
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
                                new SqlParameter("@roleid", 5), //为五的时候为个人会员
                                new SqlParameter("@origin", ""),
                                new SqlParameter("@originpath", ""),
                                new SqlParameter("@ismobilelock", "0"),
                                new SqlParameter("@validity_begintime", "1990-1-1"),
                                new SqlParameter("@validity_endtime", "1990-1-1"),
                                new SqlParameter("@deptname", ""),
                                new SqlParameter("@company", ""),
                                new SqlParameter("@address", "")
                              });

                if (intuser.ToString() == "-99")
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名重复，请重新输入!", data = null });
                else if (CharString.IntConvert(intuser) > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "注册成功!", data = null });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "注册失败，网络异常!", data = null });
                }
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
        }


        /// <summary>
        /// 注册账号 http://192.168.1.223/GR_Home/RegistByMobile
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号码</param>
        /// <param name="realname">真实姓名</param>
        /// <param name="smscode">验证码</param>
        /// <returns></returns>
        public string RegistByMobile( string password, string mobile, string smscode)
        {

            if ( string.IsNullOrEmpty(password))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "用户名或密码为空,提交失败!", data = null });
            } 
            if (!Utils.IsSafeSqlString(mobile))
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您输入的手机号码包含不安全的字符,因此无法提交!", data = null });
            }
            if (User_Common.IsHaveMobile(mobile.Trim()) > 0)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "您所输入的手机号码已被使用过, 请输入其他的手机号码!", data = null });
            }

            using (var db = new shhouseEntities())
            {
                var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where Mobile = " + mobile + "  and  codetype = 1  and  checkcode = '" + smscode + "'" +
                    "  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                if (uservalidityrecord == null)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误或已经失效请重新获取！", data = null });
                }
                uservalidityrecord.state = "1";
                db.SaveChanges();
                var intuser = db.Database.ExecuteSqlCommand(User_Common.GR_adduserbymobile_sql,
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
                                //new SqlParameter("@username", Utils.CheckData(username.Trim())),
                                //new SqlParameter("@realname", Utils.CheckData(realname.Trim())),
                                new SqlParameter("@password", Utils.MD5(password.Trim())),
                                new SqlParameter("@addip", Utils.GetRealIP()),
                                new SqlParameter("@roleid", 5), //为五的时候为个人会员
                                new SqlParameter("@origin", ""),
                                new SqlParameter("@originpath", ""),
                                new SqlParameter("@ismobilelock", "0"),
                                new SqlParameter("@validity_begintime", "1990-1-1"),
                                new SqlParameter("@validity_endtime", "1990-1-1"),
                                new SqlParameter("@deptname", ""),
                                new SqlParameter("@company", ""),
                                new SqlParameter("@address", "")
                              });

                if (intuser.ToString() == "-99")
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "手机号码重复，请重新输入!", data = null });
                else if (CharString.IntConvert(intuser) > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "注册成功!", data = null });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "注册失败，网络异常!", data = null });
                }
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常", data = null });
        }




        /// <summary>
        /// 发送验证短信验证码 http://192.168.1.223/GR_Home/SendSmsCode
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public string SendSmsCode(string mobile)
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
                    else
                    {
                        sID = "0";
                        sEmail = null;
                        sMobile = mobile;
                        flg = true;

                    }
                }
                if (flg)
                {
                    Random random = new Random();
                    string sCode = random.Next(111111, 1000000).ToString();
                    if (SMS.SendSMS_New(sMobile, "验证码：" + sCode + "  【e房网】") != "100")
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "短信发送失败，请稍侯再试", data = null });
                    }
                    else
                    {
                        //添加到数据库
                        using (var db = new shhouseEntities())
                        {
                            var user_validityrecord = new user_validityrecord()
                            {    
                                userid = int.Parse(sID),//用户id                          
                                validity = DateTime.Now.AddHours(24),//有效期
                                checkcode = sCode,//验证码
                                codetype = 1,//类型 发短信为1 发邮件为2 这里默认为1
                                state = "0",//是否有效 状态
                                body = "",//描述
                                Mobile= mobile//新增用户手机号码
                            };
                            db.user_validityrecord.Add(user_validityrecord);
                            db.SaveChanges();
                        }
                    }

                }
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "发送成功", data = null });
            }
            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "发送失败", data = null });
        }

        /// <summary>
        /// 根据手机号重设新密码 http://192.168.1.223/GR_Home/FindPassword
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smscode"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        public string FindPassword(string mobile, string smscode, string newpassword)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Where(x => x.mobile == mobile).FirstOrDefault();
                    var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where userid = " + user_details.userid + "  and  codetype = 1  and  checkcode = '" + smscode + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                    if (uservalidityrecord == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误！", data = null });
                    }
                    uservalidityrecord.state = "1";
                    var user_member = db.user_member.Find(user_details.userid);
                    user_member.password = Utils.MD5(newpassword);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功！", data = null });
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！", data = null });
                }
            }
        }
    }
}