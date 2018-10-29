using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using jjr2018.Common;
using jjr2018.Entity.shhouse;
using jjr2018.Models;
using jjr2018.WxPayAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode.Codec;


namespace jjr2018.Controllers
{
    public class WxPayController : jjrbasicController
    {
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="paytype"></param>
        /// <returns></returns>
        public string createOrder(int setid, int paytype=2)
        {
            int userid = User.userid;
            //创建订单
            using (shhouseEntities ent = new shhouseEntities())
            {
                ObjectParameter ordernum = new ObjectParameter("ordernum", typeof(string));
                ObjectParameter setname = new ObjectParameter("setname", typeof(string));
                ObjectParameter state = new ObjectParameter("state", typeof(int));
                ObjectParameter totals = new ObjectParameter("totals", typeof(int));
                ObjectParameter msg = new ObjectParameter("msg", typeof(string));
                ent.order_create_jjr2018(userid, DateTime.Now, DateTime.Now.AddHours(2), setid, 1, paytype, ordernum, setname, totals, state, msg);
                int _state = (int)state.Value;
                if (_state == 1)
                { 
                     try
                        {
                            //微信支付
                            NativePay nativePay = new NativePay();
                            WxPayData data= nativePay.GetPayMsg(setid.ToString(), (string)setname.Value, (string)ordernum.Value, (int)totals.Value);
                            WxPayData ww = new WxPayData();
                            ww.SetValue("appid", APPconfig.APPID);
                            ww.SetValue("noncestr", data.GetValue("nonce_str"));
                            ww.SetValue("package", "Sign=WXPay");
                            ww.SetValue("partnerid", APPconfig.MCHID);
                            ww.SetValue("prepayid", data.GetValue("prepay_id"));
                            string timestamp = WxPayApi.GenerateTimeStamp();
                            ww.SetValue("timestamp", timestamp);
                            ww.SetValue("sign", data.GetValue("sign"));
                            string sign = ww.MakeSign();

                           if (data==null)
                            {
                                return JsonConvert.SerializeObject(new repmsg
                                {
                                    state = 0,
                                    msg = "订单创建失败"
                                });
                            }

                        return JsonConvert.SerializeObject(new repmsg
                        {
                            state = 1,
                            msg = "",
                            data = new
                            {
                                tradeno = (string)ordernum.Value,
                                setname = (string)setname.Value,
                                total = (int)totals.Value,
                                json = data.GetValue("result_code"),
                                Sign = sign,
                                prepay_id= data.GetValue("prepay_id"),
                                nonce_str = data.GetValue("nonce_str"),
                                partnerid = APPconfig.MCHID,
                                timestamp= timestamp
                            }
                            });
                        }
                        catch (Exception e)
                        {
                            return JsonConvert.SerializeObject(new repmsg
                            {
                                state = 0,
                                msg = e.Message
                            });
                        }
                }
                else
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