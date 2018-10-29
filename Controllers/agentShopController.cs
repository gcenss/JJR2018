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
using Dapper;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace jjr2018.Controllers
{
    public class agentShopController : Controller
    {
        /// <summary>
        /// 经纪人店铺信息
        /// </summary>
        /// <param name="agentID">经纪人ID</param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>    
        /// <returns></returns>
        public string ShopMsg(int agentID, int pagesize = 20, int pageindex = 1,int? loginuserid= null)
        {
           
            {
                try
                {
                    using (shhouseEntities ent = new shhouseEntities())
                    {
                        //经纪人信息
                        var persons = (from a in ent.user_member
                                       join b in ent.user_details on a.userid equals b.userid into dc
                                       from dci in dc.DefaultIfEmpty()
                                       join c in ent.user_validity on a.userid equals c.userid into ec
                                       from eci in ec.DefaultIfEmpty()
                                       where a.userid==agentID
                                       select new
                                       {
                                           a.userid,
                                           a.mobile,
                                           a.username,
                                           dci.origin,
                                           a.deptpath,
                                           dci.Salenum,
                                           dci.Rentnum,
                                           dci.Dknum,
                                           a.servicetype,
                                           dci.gradeid,
                                           dci.remark,
                                           dci.photoname,
                                           dci.know_area,
                                           dci.know_village, 
                                       }).FirstOrDefault();
             
                        //二手房
                        
                        int userid = int.Parse(agentID.ToString());
                        List<string> where1 = new List<string>();
                        List<SqlParameter> where2 = new List<SqlParameter>();
                        string _where = ""; 
                        where1.Add($"a.isdel=0 and a.userid={userid}");
                        _where = " where " + string.Join(" and ", where1.ToArray());
                        string sql = $@"select saleid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice,smallpath,ISNULL(sharenum,0)sharenum,housesharenum,
                                        customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villagename,addtime,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum,directionsvar,fitmentvar,tags from (SELECT a.saleid, b.titles, b.shangquan, a.room, b.hall, b.toilet,b.tags,a.minarea, b.layer,g.num sharenum,h.num as housesharenum,  
                                        b.totallayer, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,b.villagename,b.addtime,a.updatetime,d.clicknum,b.smallpath,directionsvar, fitmentvar,ROW_NUMBER() over(order by unixdate desc,a.saleid desc) as rows
                                        FROM house_sale_search_wuxi a INNER JOIN house_sale_list_wuxi b ON a.saleid = b.saleid
                                        LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.saleid
                                        INNER JOIN house_sale_detail_wuxi c on a.saleid = c.saleid 
                                        left join(select sum(num)num,ContentID from ShareLog  where type=2 group by ContentID)g on a.saleid=g.ContentID 
                                        left join(select sum(num)num,ContentID from ShareLog  where type=1 group by ContentID)h on a.saleid=h.ContentID{ _where }) t
                                        where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";

                        string sql_c = $@"select count(1) from house_sale_search_wuxi a { _where }";

                        var sale = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                        var salecount = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                        //租房
                        string sql2 = $@"select rentid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice,smallpath,ISNULL(sharenum,0)sharenum,housesharenum,
                                        customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villagename,addtime,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum,directionsvar,fitmentvar,tags from (SELECT a.rentid, b.titles, b.shangquan, a.room, b.hall, b.toilet,b.tags, a.minarea, b.layer,g.num sharenum,h.num as housesharenum,  
                                        b.totallayer, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,b.villagename,b.addtime,a.updatetime,d.clicknum,b.smallpath,directionsvar, fitmentvar,ROW_NUMBER() over(order by unixdate desc,a.rentid desc) as rows
                                        FROM house_rent_search_wuxi a INNER JOIN house_rent_list_wuxi b ON a.rentid = b.rentid
                                        LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.rentid
                                        INNER JOIN house_rent_detail_wuxi c on a.rentid = c.rentid 
                                        left join(select sum(num)num,ContentID from ShareLog  where type=2 group by ContentID)g on a.rentid=g.ContentID 
                                        left join(select sum(num)num,ContentID from ShareLog  where type=1 group by ContentID)h on a.rentid=h.ContentID{ _where }) t
                                        where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }";

                        string sql_c2 = $@"select count(1) from house_rent_search_wuxi a { _where }";

                        var rent = ent.Database.DynamicSqlQuery(sql2, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                        var rentcount = ent.Database.SqlQuery<int>(sql_c2, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

                        //是否关注
                        string sql3 = "select count(*) from AgentCollection where UserID=" + loginuserid + " and AgentID=" + agentID;
                        var isguanzhu = ent.Database.SqlQuery<int>(sql3, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "",
                            data = new
                            {
                                agentinfo= persons,
                                sales = sale,
                                salescount = salecount,
                                rents=rent,
                                rentscount= rentcount,
                                isgz= isguanzhu
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "没有找到房源信息"
                    });
                }
            }
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

        public string FocusOn(int userid, int agentid)
        {
            try
            {
                using (shhouseEntities ent = new shhouseEntities())
                {
                    var Single = ent.AgentCollection.Where(p => p.UserID == userid && p.AgentID == agentid).FirstOrDefault();
                    if (Single == null) {

                        var AgentCollection = new AgentCollection
                        {
                            UserID= userid,
                            AgentID= agentid,
                            AddTime=DateTime.Now
                        };
                        ent.AgentCollection.Add(AgentCollection);
                        ent.SaveChanges();
                    }

                }

                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "关注成功！" });
            }
            catch
            {
                return JsonConvert.SerializeObject(new repmsg { state = 0, msg = "暂无数据，请稍后再试！" });

            }
        }


        public string CancelAttention(int userid, int agentid)
        {
            try
            {
                using (shhouseEntities ent = new shhouseEntities())
                {
                    var Single = ent.AgentCollection.Where(p => p.UserID == userid && p.AgentID == agentid).FirstOrDefault();
                    if (Single != null)
                    {
                        ent.AgentCollection.Remove(Single);
                        ent.SaveChanges();
                    }
                }
                return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "关注成功！" });
            }
            catch
            {
                return JsonConvert.SerializeObject(new repmsg { state = 0, msg = "暂无数据，请稍后再试！" });

            }
        }

    }
}
