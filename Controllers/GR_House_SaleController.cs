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
using jjr2018.Entity.shhouse;

namespace jjr2018.Controllers
{
    public class GR_House_SaleController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 查询房源列表   http://192.168.1.223/GR_House_Sale/ListByUser
        /// </summary>
        /// <param name="housetype">1已发布，2置顶，3下架，4草稿</param>
        /// <param name="keyword"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ListByUser(int housetype = 1, string keyword = "", int pagesize = 20, int pageindex = 1)
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
                    where1.Add("a.istop=1 and a.topend>=getdate() and a.isdel=0");
                    break;
                case 3:
                    where1.Add("a.isdel=-1");
                    break;
                case 4:
                    where1.Add("a.isdel=-10");
                    break;
            }
            where1.Add($"a.userid={userid}");
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("a.titletag like @keyword");
                where2.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select saleid,titles,shangquan,room,hall,toilet,minarea,layer,totallayer,directions,minprice, 
customid,isdel,hitcount,Istop,Convert(varchar,topend,102) as topend,villageid,villagename,addtime,isaudit,smallpath,updatetime,DATEDIFF (DAY, GETDATE(),topend )syday,clicknum from (SELECT a.saleid, b.titles, b.shangquan, a.room, b.hall, b.toilet, a.minarea, b.layer, 
b.totallayer,a.directions,b.linkman,b.tel, a.minprice,a.Istop,a.topend,b.customid,a.isdel,a.hitcount,a.villageid,b.villagename,b.addtime,a.isaudit,b.smallpath,a.updatetime,d.clicknum,ROW_NUMBER() over(order by unixdate desc,a.saleid desc) as rows
FROM house_sale_search_wuxi a INNER JOIN house_sale_list_wuxi b ON a.saleid = b.saleid
LEFT JOIN (select houseid,ClickNum from statist_house where DateDiff(dd,createtime,getdate())=0 )d on d.houseid=a.saleid
INNER JOIN house_sale_detail_wuxi c on a.saleid = c.saleid { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize}
";
            string sql_c = $@"select count(1) from house_sale_search_wuxi a { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var datas = ent.Database.DynamicSqlQuery(sql, where2.Select(x => ((ICloneable)x).Clone()).ToArray());
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

                    var datas_fb = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where isdel=0 and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                   // var datas_zd = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where istop=1 and isdel=0 and topend>=getdate() and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();
                    var datas_xj = ent.Database.SqlQuery<int>("select count(1) from house_sale_search_wuxi a where isdel=-1 and labelstate<>9 and userid=" + userid, where2.Select(x => ((ICloneable)x).Clone()).ToArray()).First();



                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            sales = datas,
                            count = datas_c,
                            count_fb = datas_fb,
                          
                            count_xj = datas_xj
                        }
                    }, timeFormat);
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


        /// <summary>
        /// 房源详情  http://192.168.1.223/GR_House_Sale/Find
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string Find(int houseid)
        {
            //int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var info1 = ent.house_sale_search_wuxi.FirstOrDefault(p => p.saleid == houseid );
                    var info2 = ent.house_sale_list_wuxi.FirstOrDefault(p => p.saleid == houseid );
                    var info3 = ent.house_sale_detail_wuxi.FirstOrDefault(p => p.saleid == houseid );
                    var info4 = ent.house_sale_img_wuxi.Where(p => p.houseid == houseid && (p.pictypeid == 0 || p.pictypeid == null)).Select(p => p.imgurl).ToList();
                    var info5 = ent.house_sale_img_wuxi.Where(p => p.houseid == houseid && p.pictypeid == 1).Select(p => p.imgurl).ToList();
                    // 浏览量
                    int browse_house_sale_count = ent.favourite_house_village.Where(p => p.houseid == houseid && p.housetype == 7).Count();
                    //预约量
                    int favourite_house_sale_count = ent.favourite_house_village.Where(p => p.houseid == houseid && p.housetype == 1).Count();
                    // 收藏量
                    string sql_c = $@"select count(1) from  house_agent_sms   where housetype=1 and houseid='{houseid.ToString() }'";
                  //  var parms1 = new object[] { };
                    int order_house_sale_count = ent.Database.SqlQuery<int>(sql_c).First();




                  

                    if (info1 == null )
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到房源信息1"
                        });
                    }
                    if (info2 == null )
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到房源信息2"
                        });
                    }
                    if ( info3 == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到房源信息3"
                        });
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            houseid = houseid,
                            villageid = info1.villageid,
                            room = info1.room,
                            hall = info2.hall,
                            toilet = info2.toilet,
                            minprice = info1.minprice,
                            minarea = info1.minarea,
                            layer = info2.layer,
                            totallayer = info2.totallayer,
                            nature = info1.nature,
                            directions = info1.directions,
                            fitment = info1.fitment,
                            villagename = info2.villagename,
                            address = info2.address,
                            titles = info2.titles,
                            tags = info2.tags,
                            smallpath = info2.smallpath,
                            imgcount = info2.imgcount,
                            isyear5 = info1.isyear5,
                            customid = info2.customid,
                            condition = info3.condition,
                            remark = Utils.NoHTML(info3.remark),                         
                            infrastructure = info3.infrastructure,
                            imgurlht = info3.imgurlht,
                            shangquan = info2.shangquan,
                            county = info2.county,
                            lift = info2.lift,
                            buildyear = info2.buildyear,
                            isonly = info2.isonly,
                            yjrate = info2.yjrate,
                            imgs1 = string.Join(",", info4),
                            imgs2 = string.Join(",", info5),
                            isdel = info1.isdel,
                            linkname= info2.linkman,
                            tel=info2.tel,
                            browse_house_sale_count= browse_house_sale_count,
                            favourite_house_sale_count= favourite_house_sale_count,
                            order_house_sale_count = order_house_sale_count
                        }
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "数据异常"
                    });
                }
            }
        }


        /// <summary>
        ///  提交二手房房源 http://192.168.1.223/GR_House_Sale/Add
        /// </summary>
        /// <param name="villageid">小区ID</param>
        /// <param name="room">室</param>
        /// <param name="hall">厅</param>
        /// <param name="toilet">卫</param>
        /// <param name="minprice">价格</param>
        /// <param name="minarea">面积</param>
        /// <param name="layer">楼层</param>
        /// <param name="totallayer">总楼层</param>
        /// <param name="nature">房屋性质id</param>
        /// <param name="directions">朝向id</param>
        /// <param name="fitment">装修id</param>
        /// <param name="villagename">小区名称</param>
        /// <param name="address">小区地址</param>
        /// <param name="titles">标题</param>        
        /// <param name="smallpath">列表页缩略图/封面图</param>
        /// <param name="imgcount">图片总数</param>    
        /// <param name="shangquan">商圈名</param>
        /// <param name="county">区域名</param>  
        /// <param name="imgs1">房源图</param>       
        /// <param name="isdel">0上架，-1删除，-10草稿</param>
        ///  <param name="sourcefrom">--来源，0：PC端，1：APP</param>
        ///  <param name="linkman">联系人</param>
        /// <param name="tel">手机号码</param>
        ///  <param name="buildid">楼栋号</param>
        /// <param name="roomid">室号</param>
        /// <returns></returns>
        [HttpPost]
        public string Add(int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
           string smallpath, short imgcount, string shangquan, string county,  string remark,
             string imgs1,  short isdel, int sourcefrom, string linkman, string tel, string buildid, string roomid)
        {
            


            //if (string.IsNullOrEmpty(imgs1))
            //{
            //    return JsonConvert.SerializeObject(new repmsg
            //    {
            //        state = 0,
            //        msg = "请上传房源图片"
            //    });
            //}
            int userid = User.userid;
            string addip = Utils.GetRealIP();
            int _hid = 0;
            string keywords = User.keywords;
            //string titless = Common.Utils.replace1(titles, keywords);
            //string remarks = Common.Utils.replace1(remark, keywords);

            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter houseid = new ObjectParameter("houseid", typeof(int));
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));


                    int i = ent.House_Sale_Add_GR2018(userid, villageid, room, hall, toilet, minprice, minarea, layer, totallayer, nature,
                        directions, fitment, villagename, address, titles,  smallpath, imgcount, 
                        shangquan, county, remark, imgs1,  addip, isdel,  linkman,  tel, buildid, roomid,
                        sourcefrom, houseid, state, msg);
                    int _state = (int)state.Value;
                    if (_state == 1)
                    {
                        _hid = Convert.ToInt32(houseid.Value);
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = _state,
                            msg = (string)msg.Value,
                            data = (int)houseid.Value
                        });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = _state,
                            msg = (string)msg.Value
                        });
                    }

                    //List<string> imgs = new List<string>();
                    //imgs.AddRange(imgs1.Split(','));
                    //imgs.AddRange(imgs2.Split(','));
                    //房源发布后，去掉oss超时设置
                    //OssHelper.modifymeta(imgs);
                }
                catch (Exception e)
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
        /// 修改房源  http://192.168.1.223/GR_House_Sale/Edit
        /// </summary>
        ///<param name="houseid">房源id</param>
        /// <param name="villageid">小区ID</param>
        /// <param name="room">室</param>
        /// <param name="hall">厅</param>
        /// <param name="toilet">卫</param>
        /// <param name="minprice">价格</param>
        /// <param name="minarea">面积</param>
        /// <param name="layer">楼层</param>
        /// <param name="totallayer">总楼层</param>
        /// <param name="nature">房屋性质id</param>
        /// <param name="directions">朝向id</param>
        /// <param name="fitment">装修id</param>
        /// <param name="villagename">小区名称</param>
        /// <param name="address">小区地址</param>
        /// <param name="titles">标题</param>
        /// <param name="smallpath">列表页缩略图/封面图</param>
        /// <param name="imgcount">图片总数</param>       
        /// <param name="shangquan">商圈名</param>
        /// <param name="county">区域名</param>      
        /// <param name="imgs1">房源图</param>        
        /// <param name="isdel">0上架，-1删除，-10草稿</param>
        ///  <param name="linkman">联系人</param>
        ///   <param name="tel">手机号码</param> 
        ///   /// <param name="tel">手机号码</param>
        ///  <param name="buildid">楼栋号</param>
        /// <returns></returns>
        [HttpPost]
        public string Edit(int houseid, int villageid, int room, int hall, int toilet, double minprice, double minarea, int layer,
            int totallayer, short nature, short directions, short fitment, string villagename, string address, string titles,
            string smallpath, short imgcount,    string shangquan, string county, string remark, string imgs1,  short isdel, string linkman, string tel, string buildid, string roomid)
        {
      

            int userid = User.userid;
            string keywords = User.keywords;
            //string titless = Common.Utils.replace1(titles, keywords);
            //string remarks = Common.Utils.replace1(remark, keywords);

            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));

                    int i = ent.House_Sale_Edit_GR2018(houseid, userid, villageid, room, hall, toilet, minprice, minarea,
                        layer, totallayer, nature, directions, fitment, villagename, address, titles,  smallpath,
                        imgcount,  shangquan, county, remark,
                        imgs1,  isdel, linkman, tel, buildid, roomid, state, msg);

                    //List<string> imgs = new List<string>();
                    //imgs.AddRange(imgs1.Split(','));
                    //imgs.AddRange(imgs2.Split(','));
                    //房源发布后，去掉oss超时设置
                    //OssHelper.modifymeta(imgs);

                    int _state = (int)state.Value;
                    if (_state == 1)
                    {
                        //删除静态页面
                        //HttpHelper.Get($"http://localhost:11894/chushou/removesalefile.ashx?city=3&saleid={houseid}");
                    }

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }




        /// <summary>
        /// 修改房源价格  http://192.168.1.223/GR_House_Sale/Editminprice
        /// </summary>
        /// <param name="houseid"></param>
        /// <param name="minprice"></param>
        /// <returns></returns>
        [HttpPost]
        public string Editminprice(int houseid, double minprice)
        {


            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_sale_search_wuxi myhouse_sale_search_wuxi = ent.house_sale_search_wuxi.Find(houseid);
                    if (userid != myhouse_sale_search_wuxi.userid)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "非本人信息不能修改",
                            data = null
                        });
                    }
                    myhouse_sale_search_wuxi.minprice = minprice;
                    //计算均价
                    double minarea = Convert.ToDouble(myhouse_sale_search_wuxi.minarea.ToString());
                    double avgprice = 0;
                    if (minarea != 0)
                    {
                        avgprice = Math.Round(minprice / minarea, 2) * 10000;
                    }
                    myhouse_sale_search_wuxi.avgprice = avgprice;
                    int searchprice = 0;

                int intminprice = Convert.ToInt32(minprice);


                        string sql_c = "select  typeid from base_samtype where parentid = 13 and Convert(int, space1)<= '" + intminprice + "' and Convert(int, space2)>'" + intminprice + "'";
                        searchprice = ent.Database.SqlQuery<int>(sql_c).DefaultIfEmpty().First();
               

            
                    myhouse_sale_search_wuxi.searchprice = Convert.ToInt16(searchprice.ToString());
                    int isok = ent.SaveChanges();
                    if (isok > 0)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "修改成功",
                            data = houseid
                        }, timeFormat);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 2,
                            msg = "修改失败",
                            data = null
                        });
                    }
            }
                catch (Exception e)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = "提交失败，请稍后再试"
                });
            }
        }
        }




        /// <summary>
        /// 查询房源的置顶价格  http://192.168.1.223/GR_House_Sale/GetEB
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string GetEB(int houseid)
        {
            int ebx = 0;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    int? sqid = ent.house_sale_search_wuxi.Where(p => p.saleid == houseid).Select(p => p.shangquanid).First();
                    if (sqid.HasValue && sqid > 0)
                    {
                        int? eb = ent.base_area.Where(p => p.areaid == sqid.Value).Select(p => p.topEB).First();
                        if (eb.HasValue)
                        {
                            ebx = eb.Value;
                        }
                        else
                        {
                            ebx = 0;
                        }
                    }
                    else
                    {

                        ebx = 0;
                    }
                }
                catch
                {
                    ebx = 0;
                }
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 1,
                    msg = "",
                    data = ebx
                });
            }
        }


        /// <summary>
        /// 房源置顶  http://192.168.1.223/GR_House_Sale/Top
        /// </summary>
        /// <param name="houseid"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        [HttpGet]
        public string Top(int houseid, int days) 
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                ObjectParameter state = new ObjectParameter("state", typeof(int));
                ObjectParameter msg = new ObjectParameter("msg", typeof(string));
                try
                {
                    int i = ent.House_Sale_Top_GR2018(userid, houseid, days, state, msg);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });

                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }


        /// <summary>
        /// 设置房源上架  http://192.168.1.223/GR_House_Sale/On
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string On(int houseid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));

                    int i = ent.House_Sale_On_GR2018(userid, houseid, state, msg);


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });

                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败"
                    });
                }
            }
        }


        /// <summary>
        /// 设置房源下架 http://192.168.1.223/GR_House_Sale/Off
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string Off(int houseid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                //try
                //{
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));

                    int i = ent.House_Sale_Off_GR2018(userid, houseid, state, msg);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });
                //}
                //catch (Exception e)
                //{
                //    return JsonConvert.SerializeObject(new repmsg
                //    {
                //        state = 0,
                //        msg = "提交失败"
                //    });
                //}
            }
        }


        /// <summary>
        /// 手动刷新房源  http://192.168.1.223/GR_House_Sale/Refresh
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string Refresh(int houseid)
        {
            ObjectParameter state = new ObjectParameter("state", typeof(int));
            ObjectParameter msg = new ObjectParameter("msg", typeof(string));
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                //try
                //{
                    ent.House_Sale_Refresh_GR2018(userid, houseid, state, msg);

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });
                //}
                //catch (Exception e)
                //{
                //    return JsonConvert.SerializeObject(new repmsg
                //    {
                //        state = 0,
                //        msg = "提交失败，请稍后再试"
                //    });
                //}
            }
        }


        /// <summary>
        /// 彻底删除  http://192.168.1.223/GR_House_Sale/Delete
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        [HttpGet]
        public string Delete(int houseid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));
                    var ls = ent.House_Sale_Delete_GR2018(userid, houseid, state, msg);


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });

                    //OssHelper.delete(ls);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpGet]
        public string ListTempl(string keyword, int? type, int pageindex = 1, int pagesize = 20)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            int userid = User.userid;
            List<string> where1 = new List<string>();
            List<SqlParameter> where2 = new List<SqlParameter>();
            string _where = "";
            where1.Add($"userid={userid}");
            if (type.HasValue)
            {
                where1.Add($"ttype={type}");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                where1.Add("titletag like @keyword");
                where2.Add(new SqlParameter("", "%" + keyword + "%"));
            }
            _where = " where " + string.Join(" and ", where1.ToArray());

            string sql = $@"select templateid, ttype, tname, createtime,tcontent
from (SELECT templateid, ttype, tname, createtime,tcontent, 
ROW_NUMBER() over(order by templateid desc) as rows
FROM house_template { _where }) t
where t.rows>={ (pageindex - 1) * pagesize + 1 } and t.rows<={ pageindex * pagesize }
";
            string sql_c = $@"select count(1) from house_template { _where }";
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var parms1 = new object[] { };
                    if (where2.Count > 0)
                    {
                        parms1 = where2.ToArray();
                    }
                    var datas = ent.Database.DynamicSqlQuery(sql, parms1);
                    var datas_c = ent.Database.SqlQuery<int>(sql_c, parms1).First();
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
                        msg = "没有找到模板信息"
                    });
                }
            }
        }

        [HttpPost]
        public string AddTempl(int ttype, string tname, string tcontent)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tt = new house_template()
                    {
                        ttype = ttype,
                        tname = tname,
                        tcontent = tcontent,
                        userid = userid,
                        createtime = DateTime.Now
                    };
                    ent.house_template.Add(tt);
                    ent.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "提交成功"
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpPost]
        public string EditTempl(int templateid, int ttype, string tname, string tcontent)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        tm.ttype = ttype;
                        tm.tname = tname;
                        tm.tcontent = tcontent;
                        ent.SaveChanges();
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "提交成功"
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpGet]
        public string DeleteTempl(int templateid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        ent.house_template.Remove(tm);
                        ent.SaveChanges();

                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "提交成功"
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }

        [HttpGet]
        public string FindTempl(int templateid)
        {
            int userid = User.userid;
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    house_template tm = ent.house_template.FirstOrDefault(p => p.templateid == templateid && p.userid == userid);
                    if (tm == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到模板"
                        });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "",
                            data = tm
                        });
                    }
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "提交失败，请稍后再试"
                    });
                }
            }
        }
    }

}