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
using EFW.Common;

namespace jjr2018.Controllers
{
    public class addAgentController : jjrbasicController
    {

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 门店下对应经纪人列表
        /// </summary>
        /// <param name="deptid">门店id</param>
        /// <param name="pagesize">条数</param>
        /// <param name="pageindex">页数</param>
        /// <returns></returns>
        public string Listjjr(int? deptid= null, int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";
             where1.Add($" a.state=0 and a.city=3 and a.roleid=4 and a.deptpath+ ',' like  '%,{deptid},%'");
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select * from (select a.userid,a.username,a.viliditystart,a.vilidityend ,b.realname,b.mobile,b.bindnum,erpgh,b.photoname,b.gradeid,b.origin,b.know_area,b.know_village,ROW_NUMBER() over(order by a.userid desc) as rows from user_member a  
                            left  join  user_details  b  on  a.userid = b.userid  
                            left  join  user_roles  c  on  a.roleid = c.roleid   
                            left  join  user_search_all_wuxi m  on  a.userid = m.userid
                            left  join  (select  userid, agentnum, storenum  from  statist_total) f  on  f.userid = a.userid 
                            left  join  user_dept g  on  a.deptid=g.deptid { _where }) t
                            where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";
            string sql_c = $@"select count(1) from user_member a { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    //var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    DataTable datas = DBHelperSQL.Query(sql);
                    for (int i = 0; i < datas.Rows.Count; i++) {
                        datas.Rows[i]["know_area"] = Utils.chuli(datas.Rows[i]["know_area"]);
                    }
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

                    
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas,
                            count = datas_c,
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,

                        msg = "没有找到经纪人信息"
                    });
                }
            }
        }

        /// <summary>
        /// 添加经纪人
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号</param>
        /// <param name="realname">真实姓名</param>
        /// <param name="erpgh">erp工号</param>
        /// <param name="shopid">门店ID</param>
        /// <returns></returns>
        public string Addjjr(string username= null, string password = null, string mobile = null, string realname = null, string erpgh=null, int? shopid = null)
        {
            List<SqlParameter> parms = new List<SqlParameter>()
            {
                new SqlParameter("@username",  SqlDbType.VarChar) { Value = username},
                new SqlParameter("@password",  SqlDbType.VarChar) { Value = Utils.MD5(password), Size = 50 },
                new SqlParameter("@mobile",  SqlDbType.VarChar) { Value = mobile},
                new SqlParameter("@realname",  SqlDbType.VarChar) { Value = realname},
                new SqlParameter("@city",  SqlDbType.Int) { Value = 3 },
                new SqlParameter("@area",  SqlDbType.Int) { Value = 0 },
                new SqlParameter("@addip",  SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@erpgh",  SqlDbType.VarChar) { Value = erpgh},
                new SqlParameter("@deptid",  SqlDbType.Int) { Value = shopid },
                new SqlParameter("@reftotalnum",  SqlDbType.Int) { Value = 0 },
                new SqlParameter("@ebtotalnum",  SqlDbType.Int) { Value = 0 },
                new SqlParameter("@housetotalnum",  SqlDbType.Int) { Value = 5 },
                new SqlParameter("@ismobilelock",  SqlDbType.Int) { Value = 0 },

                new SqlParameter("@userid",  SqlDbType.Int) { Direction = ParameterDirection.Output },
                new SqlParameter("@state",  SqlDbType.Int) { Direction = ParameterDirection.Output },
                new SqlParameter("@msg",  SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output }
            };
            try
            {
                DBHelperSQL.ExecuteFunc("user_addbroker_jjr2018", parms);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "提交失败"});
            }

            int _state = (int)parms.First(p => p.ParameterName == "@state").Value;
            string _msg = (string)parms.First(p => p.ParameterName == "@msg").Value;
           
            if (_state == 1)
            {
                int _useid = (int)parms.First(p => p.ParameterName == "@userid").Value;
                //经纪人添加成功后添加年度套餐
                List<SqlParameter> parms1 = new List<SqlParameter>()
                    {
                        new SqlParameter("@userid", SqlDbType.Int) { Value =  _useid},
                        new SqlParameter("@setid", SqlDbType.Int) { Value = 4},
                        new SqlParameter("@setsnum", SqlDbType.Int) { Value = 1},
                        new SqlParameter("@state", SqlDbType.Int) { Direction = ParameterDirection.Output },
                        new SqlParameter("@msg", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output }
                    };
                try
                {
                    DBHelperSQL.ExecuteFunc("order_free_jjr2018", parms1);
                    int state = Convert.ToInt32(parms1.First(p => p.ParameterName == "@state").Value);
                    string msg = Convert.ToString(parms1.First(p => p.ParameterName == "@msg").Value);
                    SMS.SendSMS_New(mobile, "恭喜您已开通e房网经纪人账号，账号("+ username + ")，密码("+ password + ")。登陆地址：http://dwz.cn/rPTLC45W【e房网】");
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "添加成功" });
                }
                catch (Exception ex)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "添加失败" });
                }
                
            }
            else {
                return JsonConvert.SerializeObject(new repmsg { state = 2, msg = _msg });
            }

        }


        /// <summary>
        /// 更新经纪人接口
        /// </summary>
        /// <param name="userid">经纪人ID </param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机</param>
        /// <param name="realname">真实姓名</param>
        /// <param name="erpgh">erp工号</param>
        /// <returns></returns>
        public string Editjjr(int?userid=null,string username = null, string password = null, string mobile = null, string realname = null, string erpgh = null)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_member = db.user_member.Find(userid);
                    user_member.username = username;
                    user_member.password = Utils.MD5(password);
                    user_member.erpgh = erpgh;
                    var user_details = db.user_details.Find(userid);
                    user_details.realname = realname;
                    user_details.mobile = mobile;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "修改成功"});
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "修改失败，请稍后再试"});
                }
            }
        }


        /// <summary>
        /// 删除经纪人
        /// </summary>
        /// <param name="userid">经纪人ID </param>
        /// <returns></returns>
        public string Deletejjr(int? userid = null)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_member = db.user_member.Find(userid);
                    user_member.state = -2;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "删除成功" });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "删除失败，请稍后再试" });
                }
            }
        }


        /// <summary>
        /// 经纪人详情
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string Detailjjr(int? userid = null)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    var user_member = db.user_member.Find(userid);
                    var user_detail = db.user_details.Find(userid);
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            data1= user_member,
                            data2 = user_detail
                        }
                    }, timeFormat);
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "删除失败，请稍后再试" });
                }
            }
        }

    }
}