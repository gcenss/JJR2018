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
using jjr2018.Entity.efwask;


namespace jjr2018.Controllers
{
    public class GR_EXP_TopicController : GR_BasicController
    {
        IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();


        /// <summary>
        /// 我的提问列表   http://192.168.1.223/GR_EXP_Topic/ListByUser
        /// </summary>        
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ListByUser(int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {

                    var datas_c = 0;
                    var datas = (from a in db.EXP_Topic
                                 where a.UserID == User.userid && a.isDel == "0"
                                 select new
                                 {
                                     a.ID,
                                     a.UserID,
                                     a.ETID,
                                     a.M_ETID,
                                     a.ETName,
                                     a.M_ETName,
                                     a.Title,
                                     a.TColor,
                                     a.Content,
                                     a.Author,
                                     a.IP,
                                     a.Img,
                                     a.ReplyNum,
                                     a.UpdateTime,
                                     a.isDel,
                                     a.Commend,
                                     a.City,
                                     EXP_PostNumber = db.EXP_Post.Where(p => p.TopicID == a.ID && p.isDel == "0").Count(),
                                     EXP_likeTopic1Number = db.EXP_likeTopic.Where(p => p.topicid == a.ID && p.type == 1).Count(),
                                     EXP_likeTopic2Number = db.EXP_likeTopic.Where(p => p.topicid == a.ID && p.type == 2).Count()
                                 }).OrderByDescending(p => p.ID).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();

                    datas_c = db.EXP_Topic.Where(p => p.UserID == User.userid).Count();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "获取成功",
                        data = new
                        {
                            exp_topic = datas,
                            count = datas_c
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }


        /// <summary>
        /// 我关注的问题 http://192.168.1.223/GR_EXP_Topic/ListFavouriteByUser
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string ListFavouriteByUser(int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss"; //EXP_likeTopic
            using (var db = new efwaskEntities())
            {
                try
                {

                    var datas_c = 0;
                    //var datas = (from a in db.EXP_Topic where a.isDel == "0" && (from b in db.EXP_likeTopic where b.type == 2 && b.userid == User.userid select b.topicid).Contains(a.ID) select a).OrderByDescending(p => p.UpdateTime).Skip(pagesize * (pageindex - 1)).Take(pagesize); ;


                    var datas = (from a in db.EXP_Topic
                                 where a.isDel == "0" && (from b in db.EXP_likeTopic where b.type == 2 && b.userid == User.userid select b.topicid).Contains(a.ID)
                                 select new
                                 {
                                     a.ID,
                                     a.UserID,
                                     a.ETID,
                                     a.M_ETID,
                                     a.ETName,
                                     a.M_ETName,
                                     a.Title,
                                     a.TColor,
                                     a.Content,
                                     a.Author,
                                     a.IP,
                                     a.Img,
                                     a.ReplyNum,
                                     a.UpdateTime,
                                     a.isDel,
                                     a.Commend,
                                     a.City,
                                     EXP_PostNumber = db.EXP_Post.Where(p => p.TopicID == a.ID && p.isDel == "0").Count(),
                                     EXP_likeTopic1Number = db.EXP_likeTopic.Where(p => p.topicid == a.ID && p.type == 1).Count(),
                                     EXP_likeTopic2Number = db.EXP_likeTopic.Where(p => p.topicid == a.ID && p.type == 2).Count()
                                 }).OrderByDescending(p => p.ID).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();

                    datas_c = (from a in db.EXP_Topic where a.isDel == "0" && (from b in db.EXP_likeTopic where b.type == 2 && b.userid == User.userid select b.topicid).Contains(a.ID) select a).Count();

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "获取成功",
                        data = new
                        {
                            exp_topic = datas,
                            count = datas_c
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }




        /// <summary>
        /// 问题详情  http://192.168.1.223/GR_EXP_Topic/Find
        /// </summary>
        /// <param name="houseid"></param>
        /// <returns></returns>
        public string Find(int id)
        {
            int userid = User.userid;
            using (var ent = new efwaskEntities())
            {
                try
                {
                    var info1 = ent.EXP_Topic.FirstOrDefault(p => p.ID == id );
                    if (info1 == null)
                    {
                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 0,
                            msg = "没有找到问题"
                        });
                    }
                    var info2 = ent.EXP_TopicImg.Where(p => p.topicid == info1.Img).Select(p => p.imgurl).ToList();
                    int  EXP_PostNumber = ent.EXP_Post.Where(p => p.TopicID == id && p.isDel == "0").Count();
                    int EXP_likeTopic1Number = ent.EXP_likeTopic.Where(p => p.topicid == id && p.type == 1).Count();
                    int EXP_likeTopic2Number = ent.EXP_likeTopic.Where(p => p.topicid == id && p.type == 2).Count();

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = new
                        {
                            ID= info1.ID,
                            UserID = info1.UserID,
                            ETID = info1.ETID,
                            M_ETID = info1.M_ETID,
                            ETName = info1.ETName,
                            M_ETName = info1.M_ETName,
                            Title = info1.Title,
                            TColor = info1.TColor,
                            Content = info1.Content,
                            Author = info1.Author,
                            IP = info1.IP,
                            Img = info1.Img,
                            ReplyNum = info1.ReplyNum,
                            UpdateTime = info1.UpdateTime,
                            isDel = info1.isDel,
                            Commend = info1.Commend,
                            City= info1.City,
                            imgs1 = string.Join(",", info2),
                            EXP_PostNumber,
                            EXP_likeTopic1Number,
                            EXP_likeTopic2Number
                        }
                    });
                }
                catch
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
        /// 问题评论列表 http://192.168.1.223/GR_EXP_Topic/EXP_PostListByTopicID
        /// </summary>
        /// <param name="TopicID">问题id</param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public string EXP_PostListByTopicID(int TopicID, int pagesize = 20, int pageindex = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {
                    var datas_temp = new List<EXP_Post>();
                    var datas_c = 0;



                    datas_temp = db.EXP_Post.Where(p => p.TopicID == TopicID && p.isDel == "0").OrderByDescending(p => p.ReplyTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList();
                    datas_c = db.EXP_Post.Where(p => p.TopicID == TopicID && p.isDel == "0").Count();

                    //  int totaluser_noteinfo = db.Database.SqlQuery<int>(@"select isnull(count(0),0)num  from user_noteinfo where  userid=@userid", new SqlParameter[] { new SqlParameter("@userid", User.userid) }).First();

                    Dictionary<string, object>[] datas = new Dictionary<string, object>[datas_temp.Count()];
                    int i = 0;
                    foreach (var temp in datas_temp)
                    {
                        datas[i] = new Dictionary<string, object>();


                        datas[i].Add("ID", temp.ID);
                        datas[i].Add("UserID", temp.UserID);
                        datas[i].Add("TopicID", temp.TopicID);

                        datas[i].Add("Replyer", temp.Replyer);
                        datas[i].Add("RContent", temp.RContent);
                        datas[i].Add("ReplyTime", temp.ReplyTime);
                        datas[i].Add("Neutral", temp.Neutral);
                        datas[i].Add("Support", temp.Support);
                        datas[i].Add("Opposed", temp.Opposed);
                        datas[i].Add("ComIP", temp.ComIP);
                        datas[i].Add("CaiNa", temp.CaiNa);
                        datas[i].Add("isDel", temp.isDel);
                        datas[i].Add("City", temp.City);


                       int userid = Convert.ToInt32(temp.UserID);
                        string username = "";
                        //string zongdianval = "";
                        //string mendianval = "";
                        string photoname = "";
                        int roleid=0;

                        if (userid > 0)
                        {
                            using (var dbshhouse = new shhouseEntities()) 
                            {
                                var user_member = dbshhouse.user_member.FirstOrDefault(p => p.userid == userid);
                                username = user_member.username;
                                roleid = Convert.ToInt32(user_member.roleid);

                                var user_details = dbshhouse.user_details.FirstOrDefault(p => p.userid == userid);
                                photoname = user_details.photoname;

                                //string deptpath = user_member.deptpath;
                                //if (!string.IsNullOrEmpty(deptpath))
                                //{
                                //    string[] sArray = deptpath.Split(',');
                                //    if (sArray.Length >= 2)
                                //    {
                                //        int deptid = Convert.ToInt32(sArray[1]);
                                //        var user_dept_zongdian = dbshhouse.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                //        zongdianval = user_dept_zongdian.deptname;
                                //    }
                                //    if (sArray.Length >= 3)
                                //    {
                                //        int deptid = Convert.ToInt32(sArray[2]);
                                //        var user_dept_mendian = dbshhouse.user_dept.FirstOrDefault(p => p.deptid == deptid);
                                //        mendianval = user_dept_mendian.deptname;
                                //    }
                                //}
                            }
                        }


                        datas[i].Add("username", username);
                        datas[i].Add("photoname", photoname);
                        datas[i].Add("roleid", roleid);


                        i++;
                    }





                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "获取成功",
                        data = new
                        {
                            exp_post = datas,
                            count = datas_c
                        }
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }

        /// <summary>
        /// 评论数量 http://192.168.1.223/GR_EXP_Topic/EXP_PostNumberByTopicID
        /// </summary>
        /// <param name="TopicID"></param>  
        /// <returns></returns>
        public string EXP_PostNumberByTopicID(int TopicID)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {
                    var datas_c = 0;
                    datas_c = db.EXP_Post.Where(p => p.TopicID == TopicID && p.isDel == "0").Count();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "获取成功",
                        data = datas_c
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }

        /// <summary>
        /// 问题点赞和收藏数量 http://192.168.1.223/GR_EXP_Topic/EXP_likeTopicNumberByTopicID
        /// </summary>
        /// <param name="TopicID"></param>
        /// <param name="type">1点赞，2收藏</param>
        /// <returns></returns>
        public string EXP_likeTopicNumberByTopicID(int TopicID, int type = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {
                    var datas_c = 0;
                    datas_c = db.EXP_likeTopic.Where(p => p.topicid == TopicID && p.type == type).Count();
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "获取成功",
                        data = datas_c
                    }, timeFormat);
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }



        /// <summary>
        /// 提问  http://192.168.1.223/GR_EXP_Topic/EXP_PostAdd
        /// </summary>
        /// <param name="TopicID"></param>
        /// <param name="Content"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        [HttpPost]
        public string EXP_TopicAdd(int M_ETID, string M_ETName, string Title , string Content ,int ETID, string ETName,string albumArr)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string guid = Guid.NewGuid().ToString();
            using (var db = new efwaskEntities())
            {
                try
                {
                    var exp_topic = new EXP_Topic
                    {
                        UserID = User.userid,
                                                M_ETID= M_ETID,//类别
                        M_ETName= M_ETName,//类别名称
                        Title= Title,
                        Content= Content,
                        Author= User.user_member.username,
                        Img= guid,
                        UpdateTime = DateTime.Now,
                    
                        Commend= "0",

                        ETID = ETID,
                        ETName= ETName,

                        isDel = "0",
                        City = "无锡"
                    };
                    db.EXP_Topic.Add(exp_topic);                 

                    //添加图片
                    if (albumArr != null && albumArr.Length > 0)
                    {
                        string[] imgArr = albumArr.Split('|');


                        for (int i = 0; i < imgArr.Length; i++)
                        {
                            var exp_topicimg = new EXP_TopicImg
                            {
                                topicid= guid,
                                imgurl = imgArr[i]
                            };
                            db.EXP_TopicImg.Add(exp_topicimg);
                        }
                    }
                    db.SaveChanges();

                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "回答成功!", data = null });

                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }


        /// <summary>
        /// 添加评论  http://192.168.1.223/GR_EXP_Topic/EXP_PostAdd
        /// </summary>
        /// <param name="TopicID"></param>
        /// <param name="Content"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns> 
        [HttpPost]
        public string EXP_PostAdd(int TopicID, string Content)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {

                    EXP_Post myexp_post = new EXP_Post();
                    myexp_post.UserID = User.userid;
                    myexp_post.TopicID = TopicID;
                    myexp_post.Replyer = User.user_details.username;
                    myexp_post.RContent = Content;
                    myexp_post.ReplyTime = DateTime.Now;
                    myexp_post.ReplyerIP = Utils.GetRealIP();
                    myexp_post.CaiNa = "0";
                    myexp_post.isDel = "0";
                    myexp_post.City = "无锡";
                    
                    db.EXP_Post.Add(myexp_post);
                    db.SaveChanges();
                    int ID = myexp_post.ID;
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "回答成功!", data = ID });

                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }



        /// <summary>
        /// 添加点赞和收藏  http://192.168.1.223/GR_EXP_Topic/EXP_likeTopicAdd
        /// </summary>
        /// <param name="TopicID"></param>
        /// <param name="type">1点赞，2收藏</param>

        /// <returns></returns>
        public string EXP_likeTopicAdd(int TopicID, int type = 1)
        {
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            using (var db = new efwaskEntities())
            {
                try
                {
                    //判断有么有重复点赞或者收藏
                    int totalexp_liketopic = db.Database.SqlQuery<int>(@"select isnull(count(0),0)num  from EXP_likeTopic where  userid=@userid and TopicID=@TopicID and type=@type ", 
                        new SqlParameter[] { new SqlParameter("@userid", User.userid),
                        new SqlParameter("@TopicID",TopicID),
                        new SqlParameter("@type", type)
                        }).First();

                    if (totalexp_liketopic > 0)
                    {                        
                        return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "操作已成功，请勿重复提交!", data = null });
                    }

                   var exp_liketopic = new EXP_likeTopic
                    {
                        username = User.user_details.username,
                        topicid = TopicID,
                        type = type,
                        userid = User.userid,
                    };
                    db.EXP_likeTopic.Add(exp_liketopic);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new repmsg { state = 1, msg = "操作成功!", data = null });
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new repmsg { state = 2, msg = "暂无记录，请稍后再试!", data = null });
                }
            }
        }
    }
}