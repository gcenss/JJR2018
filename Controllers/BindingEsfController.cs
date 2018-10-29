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
    public class BindingEsfController : jjrbasicController
    {

        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();

        /// <summary>
        /// 查询房源列表
        /// </summary>
        /// <param name="housetype">1已发布，2置顶，3下架，4草稿</param>
        /// <param name="villagename"></param>
        /// <param name="searchprice">价格</param>
        /// <param name="searcharea">面积</param>
        /// <param name="countyid">区域</param>
        /// <param name="agentID">经纪人ID</param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string RealHouseList(int housetype = 1,int? countyid=null,int? pricestart=null,int? priceend=null, int? searcharea=null,int? agentID=null, string villagename = "",int? room=null, string saleid= null, int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";
            switch (housetype)
            {
                case 1:
                    where1.Add("a.isdel=0");
                    break;
                case 2:
                    where1.Add("a.istop=1 and a.isdel=0 and a.topend>=getdate()");
                    break;
                case 3:
                    where1.Add("a.isdel=-1");
                    break;
                case 4:
                    where1.Add("a.isdel=-10");
                    break;
            }
            if (countyid!=null)       where1.Add($" countyid={countyid}");
            if (pricestart != null)  where1.Add($" minprice >={pricestart}");
            if (priceend != null)  where1.Add($" minprice <={priceend}");
            if (searcharea != null)   where1.Add($" searcharea={searcharea}");
            if (room != null) where1.Add($" room={room}");
            if (agentID!=null)        where1.Add($"a.saleid in(select houseid from BringCustomer where Userid={userid} and IsSelf=1)");
            else                      where1.Add($"a.saleid not in(select houseid from BringCustomer where Userid={userid} and IsSelf=1)");

            where1.Add($"a.userid !={userid}");
            where1.Add("a.labelstate=9");
            if (!string.IsNullOrEmpty(villagename))
            {
                where1.Add("b.villagename like @villagename");
                where2.Add(new SqlParameter("@villagename", "%" + villagename + "%"));
            }
            if (!string.IsNullOrEmpty(saleid))
            {
                where1.Add("a.saleid like @saleid");
                where2.Add(new SqlParameter("@saleid", "%" + saleid + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select saleid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,minprice,smallpath,ISNULL(sharenum,0)sharenum,housesharenum,
customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villagename,addtime,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum,directionsvar,fitmentvar from (SELECT a.saleid, b.titles, b.shangquan, a.room, b.hall, b.toilet, a.minarea, b.layer,g.num sharenum,h.num as housesharenum,  
b.totallayer, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,b.villagename,b.addtime,a.updatetime,d.clicknum,b.smallpath,directionsvar, fitmentvar,ROW_NUMBER() over(order by unixdate desc,a.saleid desc) as rows
FROM house_sale_search_wuxi a INNER JOIN house_sale_list_wuxi b ON a.saleid = b.saleid
LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.saleid
INNER JOIN house_sale_detail_wuxi c on a.saleid = c.saleid 
left join(select sum(num)num,ContentID from ShareLog  where type=2 group by ContentID)g on a.saleid=g.ContentID 
left join(select sum(num)num,ContentID from ShareLog  where type=1 group by ContentID)h on a.saleid=h.ContentID{ _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from house_sale_search_wuxi a left join house_sale_list_wuxi b on a.saleid=b.saleid { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

                  
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
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,

                        msg = "没有找到真房源信息"
                    });
                }
            }
        }

        /// <summary>
        /// 经纪人绑定真房源
        /// </summary>
        /// <param name="HouseIDs">房源ID字符串,以逗号隔开</param>
        /// <returns></returns>
        public string BindHouse(string  HouseIDs)
        {
            using (var db = new shhouseEntities())
            {
                try
                {
                    if (HouseIDs != null)
                    {


                        if (HouseIDs != null && HouseIDs.Length > 0)
                        {
                            DynamicParameters dp = new DynamicParameters();
                            string[] HouseID = HouseIDs.Split(',');
                            for (int i = 0; i < HouseID.Length; i++)
                            {
                                //判断权限,扣除相应的用量

                                dp.Add("@userid", User.userid);
                                dp.Add("@houseid", int.Parse(HouseID[i]));
                                dp.Add("@state", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                dp.Add("@msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                int c = conn.Execute("bindesf", param: dp, commandType: CommandType.StoredProcedure);

                                //var BringCustomer = new BringCustomer
                                //{
                                //    HouseID =int.Parse(HouseID[i]),
                                //    Userid = User.userid,
                                //    Bringnum=0,
                                //    IsSelf=1
                                //};
                                //db.BringCustomer.Add(BringCustomer);
                                //db.SaveChanges();
                            }

                            int s = dp.Get<int>("@state");

                            if (s == 1)
                            {
                                return JsonConvert.SerializeObject(new repmsg
                                {
                                    state = s,
                                    msg = dp.Get<string>("@msg"),
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new repmsg
                                {
                                    state = 2,
                                    msg = dp.Get<string>("@msg")
                                });
                            }

                        }
                        else {

                            return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常,请稍后在试！" });

                        }
                       

                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常,请稍后在试！" });
                    }
                }
                catch(Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常,请稍后在试！" });
                }
            }
          
        }

        /// <summary>
        /// 取消关联
        /// </summary>
        /// <param name="HouseIDs">房源ID字符串,以逗号隔开</param>
        /// <returns></returns>
        public string Cancelbinding(string HouseIDs = null) {

            using (var db = new shhouseEntities())
            {
                try
                {
                    if (HouseIDs != null)
                    {
                        if (HouseIDs != null && HouseIDs.Length > 0)
                        {
                            string[] HouseID = HouseIDs.Split(',');

                            for (int i = 0; i < HouseID.Length; i++)
                            {
                                int iHouseID = int.Parse(HouseID[i]);
                                var BringCustomer = db.BringCustomer.Where(m => m.HouseID == iHouseID&&m.Userid==User.userid&&m.IsSelf==1).FirstOrDefault();
                                var usermember = db.user_member.Where(m => m.userid == User.userid).FirstOrDefault();
                                if (BringCustomer != null)
                                {
                                    db.BringCustomer.Remove(BringCustomer);
                                    usermember.houseusenum = usermember.houseusenum - 1;
                                }
                            }
                        }
                        db.SaveChanges();

                        return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "取消关联成功！" });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "参数么的传！" });
                    }
                }
                catch(Exception E)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "网络异常,请稍后在试！" });
                }
            }

        }


    }
}