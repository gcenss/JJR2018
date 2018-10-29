using jjr2018.Common;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jjr2018.Controllers
{
    public class accountController : jjrbasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
        //查询订单列表
        //        public string orders(int pagesize = 20, int pageindex = 1)
        //        {
        //            int userid = User.userid;
        //            string sql = $@"select setname,ordernum,createtime,totals,paytype from(
        //select setname,ordernum,createtime,totals,paytype,
        //ROW_NUMBER() over(order by orderid desc) as rows from user_order where userid={userid} and ispay=1) t
        //where rows>={ (pageindex - 1) * pagesize + 1 } and rows<={ pageindex * pagesize };
        //";
        //            string sql_c = $@"select count(1) from user_order where userid={userid} and ispay=1;";

        //            using (shhouseEntities ent = new shhouseEntities())
        //            {
        //                try
        //                {
        //                    var datas = ent.Database.DynamicSqlQuery(sql);
        //                    var datas_c = ent.Database.SqlQuery<int>(sql_c).First();
        //                    return JsonConvert.SerializeObject(new repmsg
        //                    {
        //                        state = 1,
        //                        msg = "",
        //                        data = new
        //                        {
        //                            items = datas,
        //                            count = datas_c
        //                        }
        //                    });
        //                }
        //                catch (Exception e)
        //                {
        //                    return JsonConvert.SerializeObject(new repmsg
        //                    {
        //                        state = 0,
        //                        msg = "没有找到订单数据"
        //                    });
        //                }
        //            }
        //        }

        //账单查询
        public string bills(int? subtype, DateTime? start, DateTime? end, int pagesize= 20, int pageindex=1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            string _where = "";
            
            where1.Add($"userid={userid}");
            if (subtype.HasValue)
            {
                where1.Add($"subtype={subtype}");
            }
            if (start.HasValue)
            {
                where1.Add($"createtime>='{start.Value.ToString("yyyy-MM-dd")}'");
            }
            if (end.HasValue)
            {
                where1.Add($"createtime<='{end.Value.AddDays(1).ToString("yyyy-MM-dd")}'");
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select billid,userid,billtype,notes,createtime,subtype,totals,extdata
from (SELECT *, ROW_NUMBER() over(order by billid desc) as rows FROM user_bill { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from user_bill { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql);
                    var datas_c = ent.Database.SqlQuery<int>(sql_c).First();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            items = datas,
                            count = datas_c
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "没有找到账单信息"
                    });
                }
            }
        }
    }
}