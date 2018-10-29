using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using System.Reflection;
using jjr2018.Common;  


namespace jjr2018.Controllers
{
    public class GR_BasicController : Controller
    {
        private string shhouseconnstr = ConfigurationManager.ConnectionStrings["shhouseconn"].ConnectionString;
        private string shvillageconnstr = ConfigurationManager.ConnectionStrings["shvillageconn"].ConnectionString;
        public SqlConnection shhouseconn { get; private set; }
        public SqlConnection shvillageconn { get; private set; }
        public new UserData User { get; private set; }
        protected override void Initialize(RequestContext requestContext)
        {
            //requestContext.HttpContext.Request.Headers.Add("token", "4d24ffeeea279bd7726b89176283396f");
            if (!requestContext.HttpContext.Request.Headers.AllKeys.Contains("token"))
            {
                sessionerror(requestContext.HttpContext.Response);
            }
            else
            {
                string _token = requestContext.HttpContext.Request.Headers["token"];
                using (var db = new shhouseEntities())
                {
                    var myuser_logintoken = db.user_logintoken.FirstOrDefault(p => p.token == _token);
                    if (myuser_logintoken == null) //token 不存在
                    {
                        sessionerror(requestContext.HttpContext.Response);
                    }
                    else
                    {
                        try
                        {
                            if (User == null)
                            {
                                User = new UserData();
                                User.userid = Int32.Parse(myuser_logintoken.UserID.ToString());
                                User.Lastlogintime = Convert.ToDateTime(myuser_logintoken.loninTime);
                                UserData(User.userid);
                            }
                        }
                        catch (Exception e)
                        {
                            sessionerror(requestContext.HttpContext.Response);
                        }
                    }
                }
                shhouseconn = new SqlConnection(shhouseconnstr);
                shvillageconn = new SqlConnection(shvillageconnstr);
                base.Initialize(requestContext);
            }
        }
        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            try
            {
                shhouseconn.Close();
                shhouseconn.Dispose();
                shvillageconn.Close();
                shvillageconn.Dispose();
            }
            catch { }
            base.OnResultExecuted(filterContext);
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                shhouseconn.Close();
                shhouseconn.Dispose();
                shvillageconn.Close();
                shvillageconn.Dispose();
            }
            catch { }
            base.Dispose(disposing);
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            try
            {
                shhouseconn.Close();
                shhouseconn.Dispose();
                shvillageconn.Close();
                shvillageconn.Dispose();
            }
            catch { }
            //base.OnException(filterContext);
            HttpContext.Response.Clear();
            HttpContext.Response.Write(JsonConvert.SerializeObject(new repmsg
            {
                state = 0,
                msg = filterContext.Exception.Message
            }));
        }

        /// <summary>
        /// 会话不存在，前端跳转登录
        /// </summary>
        /// <param name="Response"></param>
        private void sessionerror(HttpResponseBase Response)
        {
            Response.Clear();
            Response.Write(JsonConvert.SerializeObject(new repmsg { state = (int)repmsgstate.nologin, msg = "请登录" }));
            Response.End();
        }


        /// <summary>
        /// 获取登录用户基本信息
        /// </summary>
        /// <param name="Response"></param>
        private void UserData(int userid)
        {
            if (userid > 0)
            {
                using (var db = new shhouseEntities())
                {
                    User.user_member = db.user_member.Where(p => p.userid == userid).FirstOrDefault();
                    User.user_details = db.user_details.Where(p => p.userid == userid).FirstOrDefault();
                    if (db.user_search_all_wuxi.Where(p => p.UserID == userid).Count() > 0)
                    {
                        User.searchtitle = getStit(db.user_search_all_wuxi.Where(p => p.UserID == userid).FirstOrDefault().searchTitle);

                    }
                    else
                        User.searchtitle = "";

                   // User.allscore = db.Database.SqlQuery<int>(@"select  isnull(sum(score),0)score from user_score_wuxi where userid =@userid", new SqlParameter[] { new SqlParameter("@userid", User.userid) }).FirstOrDefault();
                    User.IsSignIn = db.Database.SqlQuery<int>(@"select count(*) from SignIn where userid =@userid and DateDiff(dd,exe_date,getdate())=0", new SqlParameter[] { new SqlParameter("@userid", User.userid) }).FirstOrDefault();
                    User.RCToken = Utils.gettoken(userid.ToString(), User.user_member.username);
                    //UserData

                }
            }
        }


        protected string getStit(string sval)
        {
            string stit = "";
            string[] sT = sval.Split(',');
            try
            {
                if (!string.IsNullOrEmpty(sval) && sT.Length > 3)
                {
                    if (sT.Length > 4)
                        stit = sT[3].ToString() + "," + sT[5].ToString();
                    else
                        stit = sT[3].ToString();

                    stit = stit.Replace("\"b\":\"", "").Replace("\"d\":\"", "").Replace("\"", "").Replace("'b':'", "").Replace("'d':'", "").Replace("'", "");
                }
            }
            catch { Response.Write(sval + "    " + sT.Length); Response.End(); }
            return stit;
        }

    }
}