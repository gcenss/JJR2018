using jjr2018.Common;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jjr2018.Controllers
{
    /// <summary>
    /// 使用帮助
    /// </summary>
    public class helpinfoController : Controller
    {
        public string ListCategory()
        {
            //CTE递归查询
            string sql = @"with tt as
                        (
                            select classid,classname,parentid,orderid,isshow, 1 as depth from help_classify where classid=1
                            union all
                            select A.classid,A.classname,A.parentid,A.orderid,A.isshow, b.depth+1 as depth from help_classify A inner join
                            tt B on B.classid=A.parentid 
                        )
                        select * from tt where isshow=0 order by depth, orderid
                        ";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql);
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = datas
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "暂无帮助信息"
                    });
                }
            }
        }

        public string List(int? id)
        {
            int _id = 1;
            if(id.HasValue)
            {
                _id = id.Value;
            }
            string sql = $@"with tt as
(
select classid,parentid,isshow from help_classify where classid={_id}
union all
select A.classid,A.parentid,A.isshow from help_classify A inner join tt B on A.parentid =B.classid
)

select a.* from help_news a inner join tt on a.classid=tt.classid where isdel=0
                        ";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql);
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = datas
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "暂无帮助信息"
                    });
                }
            }
        }
    }
}