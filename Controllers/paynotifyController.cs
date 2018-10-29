using jjr2018.Entity.shhouse;
using jjr2018.WxPayAPI;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace jjr2018.Controllers
{
    public class paynotifyController : Controller
    {

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <returns></returns>
        public string wx()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            string postStr = Encoding.UTF8.GetString(b);

            //解析xml
            WxPayData notifyData = new WxPayData();
            try
            {
                notifyData.FromXml(postStr);
            }
            catch (WxPayException ex)
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                return res.ToXml();
            }

            //检查xml数据
            if (!notifyData.IsSet("transaction_id") || !notifyData.IsSet("total_fee") || !notifyData.IsSet("out_trade_no"))
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "缺少参数");
                return res.ToXml();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            string out_trade_no = notifyData.GetValue("out_trade_no").ToString();

            int total_fee = Convert.ToInt32(notifyData.GetValue("total_fee"));

            //保存日志文件
            System.IO.File.WriteAllText(Server.MapPath($"~/wxpaylog/{out_trade_no}.txt"), postStr);


            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                return res.ToXml();
            }

            //执行存储过程-发货
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter payresult = new ObjectParameter("payresult", typeof(int));
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));

                    ent.order_pay_jjr2018(out_trade_no, 2, total_fee, payresult, state, msg);

                    int _state = (int)state.Value;
                    if (_state == 1)
                    {
                        WxPayData res = new WxPayData();
                        res.SetValue("return_code", "SUCCESS");
                        res.SetValue("return_msg", "OK");
                        return res.ToXml();
                    }
                    else
                    {
                        WxPayData res = new WxPayData();
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "订单处理失败");
                        return res.ToXml();
                    }
                }
                catch(Exception e)
                {
                    WxPayData res = new WxPayData();
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg",e.Message);
                    return res.ToXml();

                }
            }
        }


        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <returns></returns>
        public string APPwx()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            string postStr = Encoding.UTF8.GetString(b);

            //解析xml
            WxPayData notifyData = new WxPayData();
            try
            {
                notifyData.FromXml(postStr);
            }
            catch (WxPayException ex)
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                return res.ToXml();
            }

            //检查xml数据
            if (!notifyData.IsSet("transaction_id") || !notifyData.IsSet("total_fee") || !notifyData.IsSet("out_trade_no"))
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "缺少参数");
                return res.ToXml();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            string out_trade_no = notifyData.GetValue("out_trade_no").ToString();

            int total_fee = Convert.ToInt32(notifyData.GetValue("total_fee"));

            //保存日志文件
            System.IO.File.WriteAllText(Server.MapPath($"~/wxpaylog/{out_trade_no}.txt"), postStr);


            //查询订单，判断订单真实性
            if (!APPQueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                return res.ToXml();
            }

            //执行存储过程-发货
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter payresult = new ObjectParameter("payresult", typeof(int));
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));

                    ent.order_pay_jjr2018(out_trade_no, 2, total_fee, payresult, state, msg);

                    int _state = (int)state.Value;
                    if (_state == 1)
                    {
                        WxPayData res = new WxPayData();
                        res.SetValue("return_code", "SUCCESS");
                        res.SetValue("return_msg", "OK");
                        return res.ToXml();
                    }
                    else
                    {
                        WxPayData res = new WxPayData();
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "订单处理失败");
                        return res.ToXml();
                    }
                }
                catch (Exception e)
                {
                    WxPayData res = new WxPayData();
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", e.Message);
                    return res.ToXml();

                }
            }
        }


        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool APPQueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.APPOrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}