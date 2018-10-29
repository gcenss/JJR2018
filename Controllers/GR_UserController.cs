using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using jjr2018.Models;
using jjr2018.Entity.shhouse;
using System.Data.SqlClient;
using jjr2018.Common;
using System.Data;
using System.Data.Entity;
using Newtonsoft.Json.Converters;
using jjr2018.Entity.shvillage;
using jjr2018.Entity.efwnewhouse;

namespace jjr2018.Controllers
{
    public class GR_UserController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 用户基础信息 http://192.168.1.223/GR_User/UserData
        /// </summary>
        /// <returns></returns>
        public string UserData()
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            try
            {
                //用户房源点击数量、
                using (shhouseEntities db = new shhouseEntities())
                {
                    //var houseclick = db.Database.DynamicSqlQuery(@"select isnull(SUM(clicknum),0)num from statist_house where houseid in (select saleid from house_sale_search_wuxi where userid=@userid)", new SqlParameter[] { new SqlParameter("@userid", User.userid) });
                    //var housetopclick = db.Database.DynamicSqlQuery(@"select isnull(SUM(clicknum),0)num from statist_house where houseid in (select saleid from house_sale_search_wuxi where istop =1 and userid=@userid)", new SqlParameter[] { new SqlParameter("@userid", User.userid) });
                    //var todaytophouse = db.Database.DynamicSqlQuery(@"select isnull(count(0),0)num  from house_sale_search_wuxi where DateDiff(dd,updatetime,getdate())=0 and istop =1 and userid=@userid", new SqlParameter[] { new SqlParameter("@userid", User.userid) });
                    int totaluser_noteinfo = db.Database.SqlQuery<int>(@"select isnull(count(0),0)num  from user_noteinfo where  userid=@userid", new SqlParameter[] { new SqlParameter("@userid", User.userid) }).First();

                    //关注数量
                    // 二手房
                    int Favourite_house_sale_count = (from a in db.favourite_house_village
                                                     join b in db.house_sale_list_wuxi on a.houseid equals b.saleid into ab
                                                      from abi in ab
                                                      join c in db.house_sale_search_wuxi on abi.saleid equals c.saleid into abc
                                                      from abci in abc
                                                      where a.userid == User.userid && a.housetype == 1
                                                      select new { a.id }).Count();
                    //新房
                    int Favourite_NewHouse_count = 0;
                    using (var dbefwnewhouse = new efwnewhouseEntities())
                    {
                        int[] NewHouseID = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 6).Select(x => x.houseid.Value).ToArray();
                        Favourite_NewHouse_count = (from a in dbefwnewhouse.NewHouse where NewHouseID.Any(x => x == a.ID) select new { a.ID }).Count();
                    }

                    //租房
                    int Favourite_house_rent_count = (from a in db.favourite_house_village
                                                      join b in db.house_rent_list_wuxi on a.houseid equals b.rentid into ab
                                                      from abi in ab
                                                      join c in db.house_rent_search_wuxi on abi.rentid equals c.rentid into abc
                                                      from abci in abc
                                                      where a.userid == User.userid && a.housetype == 2
                                                      select new { a.id }).Count();
                    //小区
                    int Favourite_Community_count = 0;
                    using (var dbshvillage = new shvillageEntities())
                    {
                        int[] NewHouseID = db.favourite_house_village.Where(p => p.userid == User.userid && p.housetype == 3).Select(x => x.houseid.Value).ToArray();
                        Favourite_Community_count = (from a in dbshvillage.NewHouse where NewHouseID.Any(x => x == a.ID) select new { a.ID }).Count();
                    }

                    //转介绍 1 二手房 2租房 3买客 4租客
                    // 出售房
                    int EFW_Guest_house_sale_count = db.EFW_Guest.Where(p => p.userid == User.userid && p.guesttype == 1).Count();
                    //出租房
                    int EFW_Guest_house_rent_count = db.EFW_Guest.Where(p => p.userid == User.userid && p.guesttype == 2).Count();
                    //求购
                    int EFW_Guest_sellcustom_count = db.EFW_Guest.Where(p => p.userid == User.userid && p.guesttype == 3).Count();
                    //求租
                    int EFW_Guest_rentcustom_count = db.EFW_Guest.Where(p => p.userid == User.userid && p.guesttype == 4).Count();


                    DateTime dtdy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    int totaluser_signIn_dy = db.SignIn.Where(x => x.UserID == User.userid && x.exe_date >= dtdy).Count();

                    return JsonConvert.SerializeObject(
                                              new repmsg
                                              {
                                                  state = 1,
                                                  msg = "用户信息",
                                                  data =
                                                          new
                                                          {
                                                              username = User.user_member.username,//用户名
                                                              userid = User.user_member.userid,//用户id
                                                              mobile = User.user_details.mobile,//手机号
                                                              //mobile_zs = string.IsNullOrEmpty(User.user_details.BindNum) ? User.user_details.mobile_zs : User.user_details.BindNum,//有隐号用隐号 没隐号显示展示号
                                                              mobile_zs =User.user_details.mobile_zs,
                                                              ////yjrate = User.user_member.yjrate,//佣金比例
                                                              ////servicetype = User.user_member.servicetype,//服务类型 
                                                              photoname = User.user_details.photoname,//头像
                                                              gradeid = User.user_details.gradeid,//等级
                                                              //shangquanval = User.user_details.shangquanval, //商圈
                                                              ////origin = User.user_details.origin, //上级门店或总店
                                                              ebtotalnum = User.user_member.ebtotalnum,//总e币数量
                                                              allscore = User.user_details.scoretotal,//j积分
                                                              realname = User.user_details.realname,//真实姓名
                                                              searchtitle = User.searchtitle,//搜索字段
                                                              //viliditystart = Convert.ToDateTime(User.user_member.viliditystart).ToShortDateString(),//用户开始有效期
                                                              //vilidityend = Convert.ToDateTime(User.user_member.vilidityend).ToShortDateString(),//用户结束有效期
                                                              remark = User.user_details.remark,//简介
                                                              remarkxuanyan = User.user_details.remark_xuanyan,//简介——宣言
                                                              //knowarea = User.user_details.know_area,//熟悉区域
                                                              //knowvillage = User.user_details.know_village,//熟悉等级
                                                              //refusenum = User.user_member.refusenum,//刷新使用数量
                                                              //reftotalnum = User.user_member.reftotalnum,//刷新总数量
                                                              ////houseclick = houseclick,//房源点击率
                                                              ////housetopclick = housetopclick,////房源点击率
                                                              //housetotalnum = User.user_member.housetotalnum,//房源总数量
                                                              //houseusenum = User.user_member.houseusenum,//房源已使用数量
                                                              //syhousenum = User.user_member.housetotalnum - User.user_member.houseusenum,
                                                              //Days = (Convert.ToDateTime(User.user_member.vilidityend) - Convert.ToDateTime(User.user_member.viliditystart)).TotalDays,//服务期
                                                              //syDays = (Convert.ToDateTime(User.user_member.vilidityend) - DateTime.Now).TotalDays,//还有多少天到期
                                                              //iszhongshan = string.IsNullOrEmpty(User.user_member.deptpath) ? -1 : User.user_member.deptpath.IndexOf("0,439"),//关联部门ID
                                                              silvertotal = User.user_details.silvertotal,//元宝
                                                              issignIn = User.IsSignIn,//签到
                                                              //todaytophouse = todaytophouse,
                                                              roleid = User.user_member.roleid,//用户类型
                                                              idnumber = User.user_details.idnumber,//身份证号码
                                                              cardtype = User.user_details.CardType,//证件类型
                                                              sexuality = User.user_details.sexuality,//性别 
                                                              birthday = User.user_details.birthday,//性别 
                                                              totaluser_note = totaluser_noteinfo, //站内信息数
                                                              RCToken = User.RCToken,
                                                              totaluser_signIn_dy = totaluser_signIn_dy,//本月签到数量
                                                              Favourite_house_sale_count, //关注出售数量
                                                              Favourite_NewHouse_count, //关注新房数量
                                                              Favourite_house_rent_count, //关注出租数量
                                                              Favourite_Community_count,//关注小区数量
                                                              EFW_Guest_house_sale_count,//转介绍出售数量
                                                              EFW_Guest_house_rent_count,//转介绍出租数量
                                                              EFW_Guest_sellcustom_count,//转介绍求购数量
                                                              EFW_Guest_rentcustom_count, //转介绍求租数量
                                                              NickName = User.user_member.NickName
                                
                                                          }
                                              }, timeFormat);

                }
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无数据，请稍后再试！", data = null });
            }
        }

        ///// <summary>
        ///// 修改基本资料  账号 手机号码 正式姓名 姓名 生日 证件类型 身份证号
        ///// </summary>
        ///// <param name="username">账号</param>
        ///// <param name="mobile">手机号码</param>
        ///// <param name="realname">真实姓名</param>
        ///// <param name="sexuality">性别</param>
        ///// <param name="birth">生日</param>
        ///// <param name="cardtype">证件类型</param>
        ///// <param name="idnumber">身份证号</param>
        ///// <returns></returns>
        //public string UserModify(string username, string mobile, string realname, string sexuality, string birth , string cardtype , string idnumber )
        //{
        //    string sql = string.Empty;
        //         sql += " update user_details set username=@username where userid=@userid ";
        //         sql += " update user_details set mobile=@mobile where userid=@userid ";
        //         sql += " update user_details set realname=@realname where userid=@userid ";
        //         sql += " update user_details set sexuality=@sexuality where userid=@userid ";
        //         sql += " update user_details set birth=@birth where userid=@userid ";
        //         sql += " update user_details set cardtype=@cardtype where userid=@userid ";
        //         sql += " update user_details set idnumber=@idnumber where userid=@userid ";    
        //    using (shhouseEntities db = new shhouseEntities())
        //    {
        //        try
        //        {
        //            var edituser = db.Database.ExecuteSqlCommand(sql,
        //                        new SqlParameter[]{
        //                        new SqlParameter("@userid", User.userid),
        //                        new SqlParameter("@username", username),
        //                        new SqlParameter("@mobile", mobile),
        //                        new SqlParameter("@realname", realname),
        //                        new SqlParameter("@sexuality", sexuality),
        //                        new SqlParameter("@birth", birth),
        //                        new SqlParameter("@cardtype", cardtype),
        //                         new SqlParameter("@idnumber", idnumber)
        //                      });
        //            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功" });
        //        }
        //        catch (Exception e)
        //        {
        //            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试！" });
        //        }
        //    }
        //}

        /// <summary>
        /// 修改基本资料  http://192.168.1.223/GR_User/UserModify
        /// </summary>  
        /// <param name="realname">真实姓名</param>
        /// <param name="sexuality">性别</param>
        /// <param name="birth">生日</param>
        /// <param name="cardtype">证件类型</param>
        /// <param name="idnumber">身份证号</param>
        /// <returns></returns>
        public string UserModify(string realname = null, int? sexuality = null, string photoname =null, string birthday=null, string cardtype = null, string idnumber = null,string NickName=null)
        {

          
            using (shhouseEntities db = new shhouseEntities())
            {

                user_details myuser_details = db.user_details.Find(User.userid);
                user_member myuser_member = db.user_member.Find(User.userid);
                bool isModify = false;

                if (realname != null&& myuser_details.realname != realname)
                {
                    myuser_details.realname = realname;
                    isModify = true;
                }
                if (sexuality != null&& myuser_details.sexuality != sexuality)
                {
                    myuser_details.sexuality = sexuality;
                    isModify = true;
                }
                if (photoname != null&& myuser_details.photoname != photoname)
                {
                    myuser_details.photoname = photoname;
                    isModify = true;
                }
                if (birthday != null&& myuser_details.birthday != Convert.ToDateTime(birthday))
                {
                    myuser_details.birthday =Convert.ToDateTime( birthday);
                    isModify = true;
                }
                if (cardtype != null && myuser_details.CardType != cardtype)
                {
                    myuser_details.CardType = cardtype;
                    isModify = true;
                }
                if (idnumber != null&& myuser_details.idnumber != idnumber)
                {
                    myuser_details.idnumber = idnumber;
                    isModify = true;
                }

                if (NickName != null&& myuser_member.NickName != NickName)
                {
                    myuser_member.NickName = NickName;
                    isModify = true;
                }

                if (isModify == false)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "提交成功，您信息未做任何调整",
                        data = null
                    }, timeFormat);
                }
                int isok = db.SaveChanges();
                if (isok > 0)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "修改成功",
                        data = null
                    }, timeFormat);
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 2,
                        msg = "修改失败" ,
                        data = null
                    });
                }

            }
        }

        /// <summary>
        /// 修改密码 http://192.168.1.223/GR_User/ChangePassword
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
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "旧密码错误", data = null });
                    }
                    else
                    {
                        var user_member1 = db.user_member.Find(User.userid);
                        user_member1.password = Utils.MD5(newpassword);
                        db.SaveChanges();
                    }
                    //string path = System.Web.HttpContext.Current.Server.MapPath($"~/tokens/" + Utils.MD5(User.userid.ToString()) + ".json");
                    //Utils.DeleteFile(path); //修改密码后需要重新登录
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功", data = null });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败,请稍后再试！", data = null });
                }
            }
        }

        /// <summary>
        /// 修改手机号码  http://192.168.1.223/GR_User/ChangeMobile
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="yzm">验证码</param>
        /// <param name="password">密码</param>
        /// <param name="type">1 安全手机号码 (安全手机需要审核验证码),2 展示手机号码（需要验证密码）</param>
        /// <param name=""></param>
        /// <returns></returns>
        public string ChangeMobile(string phone, string password, string yzm, int type = 1)
        {             
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_details = db.user_details.Find(User.userid);
                    if (type == 1)
                    {

                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = " select top 1 * from  user_validityrecord  where userid = " + User.userid + "  and  codetype = 1  and  checkcode = '" + yzm + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc " });


                        //var uservalidityrecord = db.Database.SqlQuery<int>(" select  top 1  eid  from  user_validityrecord  where userid = @userid  and  codetype = 2  and  checkcode = @checkcode  and  state = 0  and  validity > getdate()  order  by  addtime  desc ", new SqlParameter[] { new SqlParameter("@userid", User.userid), new SqlParameter("@checkcode", yzm) });
                        var uservalidityrecord = db.user_validityrecord.SqlQuery(" select top 1 * from  user_validityrecord  where userid = " + User.userid + "  and  codetype = 1  and  checkcode = '" + yzm + "'  and  state = 0  and  validity > getdate()  order  by  validity  desc ").FirstOrDefault();
                        if (uservalidityrecord == null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "验证码错误！" });
                        }
                        var user_detailsmobile = db.user_validityrecord.SqlQuery(" select top 1 * from  user_details  where mobile = '"+ phone + "'   ").FirstOrDefault();
                        if (user_detailsmobile == null)
                        {
                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "手机号码已经存在不可以修改！" });
                        }
                        uservalidityrecord.state = "1";
                        user_details.mobile = phone;
                        db.SaveChanges();

                        string path = System.Web.HttpContext.Current.Server.MapPath($"~/tokens/" + Utils.MD5(User.userid.ToString()) + ".json");
                        Utils.DeleteFile(path);

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

        //积分商品列表 缺少
        
        
        
        /// <summary>
        /// 站内消息 http://192.168.1.223/GR_User/MessageList  
        /// </summary>
        /// <param name="infotype">信息类型,0:默认。1：站内公告 但是本人对他有怀疑 (0 为站长发的 1为用户发的 ) </param>
        /// <param name="pagesize">条数</param>
        /// <param name="pageindex">页码</param>
        /// <param name="id">具体那一条</param>
        /// <returns></returns>
        public string MessageList(int infotype=0, int pagesize = 10, int pageindex = 1, int id = 0)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            using (var db = new shhouseEntities())
            {
                try
                {
                    if (id > 0)
                    {

                        var user_noteinfo = db.user_noteinfo.Where(p => p.eid == id &&p.isshow==1).FirstOrDefault();
                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "站内消息", data = user_noteinfo });
                    }
                    else
                    {
                        if (infotype == 0)
                        {
                            int allcount = db.user_noteinfo.Where(p => p.infotype == infotype && p.isshow == 1).Count();
                            var user_noteinfolist = db.user_noteinfo.Where(p =>  p.infotype == infotype && p.isshow == 1).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

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

                                }, timeFormat);
                        }
                        else {
                            int allcount = db.user_noteinfo.Where(p => p.userid == User.userid && p.infotype == infotype && p.isshow == 1).Count();
                            var user_noteinfolist = db.user_noteinfo.Where(p => p.userid == User.userid && p.infotype == infotype && p.isshow == 1).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize);

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

                                }, timeFormat);

                        }


               
                    }
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无消息，请稍后再试!" });
                }
            }

        }



        public string MessageAdd(string title, string content, int isshow = 1)
        {

            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            DateTime time = DateTime.Now;
            using (shhouseEntities db = new shhouseEntities())
            {
                try
                {
                    user_noteinfo myuser_noteinfo = new user_noteinfo();
                    myuser_noteinfo.userid = userid;
                    myuser_noteinfo.infotype = 1;
                    myuser_noteinfo.title = title;
                    myuser_noteinfo.content = content;
                    myuser_noteinfo.addtime = time;
                    myuser_noteinfo.isshow = isshow;
                    myuser_noteinfo.city = 3;
                    db.user_noteinfo.Add(myuser_noteinfo);
                    db.SaveChanges();
                    int eid = myuser_noteinfo.eid;

                    if (eid > 0)
                    {

                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "反馈成功",
                            data = eid
                        }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "反馈失败",
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
        /// 积分记录  http://192.168.1.223/GR_User/RecordList
        /// </summary>
        /// <param name="type">1 最近7天，2最近30天，3本月</param>
        /// <param name="times">开始日期</param>
        /// <param name="timee">结束日期</param>
        /// <param name="pagesize">条数</param>
        /// <param name="pageindex">页码</param>
        /// <returns></returns>
        public string RecordList(int type, DateTime times, DateTime timee, int pagesize = 10, int pageindex = 1)
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
                        user_score_wuxi = db.user_score_wuxi.Where(p => DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 7 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                        count = db.user_score_wuxi.Where(p => DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 7 && p.userid == User.userid).Count();
                    }
                    else if (type == 2)
                    {
                        user_score_wuxi = db.user_score_wuxi.Where(p => DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 30 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                        count = db.user_score_wuxi.Where(p => DbFunctions.DiffDays(p.addtime, DateTime.Now) <= 30 && p.userid == User.userid).Count();
                    }
                    else if (type == 3)
                    {
                        user_score_wuxi = db.user_score_wuxi.Where(p => DbFunctions.DiffMonths(p.addtime, DateTime.Now) == 0 && p.userid == User.userid).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                        count = db.user_score_wuxi.Where(p => DbFunctions.DiffMonths(p.addtime, DateTime.Now) == 0 && p.userid == User.userid).Count();
                    }
                    else
                    {
                        user_score_wuxi = db.user_score_wuxi.Where(p => p.userid == User.userid && (DbFunctions.TruncateTime(p.addtime) >= times.Date && DbFunctions.TruncateTime(p.addtime) <= timee.Date)).OrderByDescending(p => p.addtime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                        count = db.user_score_wuxi.Where(p => p.userid == User.userid && (DbFunctions.TruncateTime(p.addtime) >= times.Date && DbFunctions.TruncateTime(p.addtime) <= timee.Date)).Count();
                    }
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "积分记录", data = new { userscore = user_score_wuxi, counts = count } }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data =null});
                    }

            }
        }
        
        ///// <summary>
        ///// 元宝兑换e币
        ///// </summary>
        ///// <param name="元宝数">ybnum</param>
        ///// <returns></returns>
        //public string ybToeb(int ybnum)
        //{
        //    using (var db = new shhouseEntities())
        //    {
        //        try
        //        {
        //            var user_details = db.user_details.Find(User.userid);
        //            var user_member = db.user_member.Find(User.userid);
        //            if (ybnum > user_details.silvertotal)
        //            {
        //                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "兑换元宝大于已拥有的数量!" , data =null});
        //                }
        //            user_details.silvertotal = user_details.silvertotal - ybnum;
        //            user_member.ebtotalnum = user_member.ebtotalnum + (ybnum / 100);
        //            db.SaveChanges();

        //            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换成功!", data = null });
        //        }
        //        catch
        //        {
        //            return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "兑换失败!", data = null });
        //        }
        //    }

        //}

        /// <summary>
        /// 签到 http://192.168.1.223/GR_User/SignIn
        /// </summary>
        /// <returns></returns>
        public string SignIn()
        {
            using (var db = new shhouseEntities())
            {
                DateTime sdt = DateTime.Now.Date;
                DateTime dt = DateTime.Now.Date.AddDays(1);

                var SignIn = db.SignIn.Where(x => x.UserID == User.userid && x.exe_date >= sdt && x.exe_date < dt).FirstOrDefault();
                if (SignIn != null)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "今日已签过!", data = null });
                }
                else
                {
                    var stephen = new SignIn
                    {
                        UserID = User.userid,
                        exe_date = DateTime.Now
                    };
                    var user_score_wuxi = new user_score_wuxi
                    {
                        userid = User.userid,
                        addtime = DateTime.Now,
                        score = 2,
                        obtaindirections = "签到积分"
                    };
                    db.user_score_wuxi.Add(user_score_wuxi);
                    db.SignIn.Add(stephen);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "签到成功!", data = null });
                }
            }
        }




        /// <summary>
        /// 签到 http://192.168.1.223/GR_User/SignIn
        /// </summary>
        /// <returns></returns>
        public string SignIn_ListByM() 
        {
            using (var db = new shhouseEntities())
            {
                DateTime sdt =  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                int allcount = db.SignIn.Where(x => x.UserID == User.userid && x.exe_date > sdt).Count();
                var SignIn_List = db.SignIn.Where(x => x.UserID == User.userid && x.exe_date > sdt).OrderByDescending(p => p.ID);
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "签到情况",
                    data = new
                    {
                        SignIn_List,
                        allcount
                    }
                }, timeFormat);

              
            }
        }
    }
}