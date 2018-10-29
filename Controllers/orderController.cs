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
    public class orderController : jjrbasicController
    {
        /// <summary>
        /// 套餐查询
        /// </summary>
        /// <param name="settype"></param>
        /// <returns></returns>
        public string findsets(int settype)
        {
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    var data = ent.user_set.Where(p => p.settype == settype).ToList();

                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "",
                        data = data
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "查找失败"
                    });
                }
            }
        }


        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="paytype"></param>
        /// <returns></returns>
        public string createOrder(int setid, int paytype)
        {
            if (paytype > 2)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = "支付方式不正确"
                });
            }
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
                if(_state == 1)
                {
                    if(paytype == 1)
                    {
                        IAopClient client = new DefaultAopClient(
                            "https://openapi.alipay.com/gateway.do", 
                            "APPID", 
                            "APP_PRIVATE_KEY", 
                            "json", 
                            "1.0", 
                            "RSA2", 
                            "ALIPAY_PUBLIC_KEY", 
                            "UTF-8", 
                            false
                            );
                        //支付宝支付
                        AlipayTradePagePayModel model = new AlipayTradePagePayModel
                        {
                            Body = (string)setname.Value,
                            Subject = (string)setname.Value,
                            TotalAmount = Math.Round((decimal)totals.Value / 100m,2).ToString(),
                            OutTradeNo = (string)ordernum.Value,
                            ProductCode = "FAST_INSTANT_TRADE_PAY"
                        };
                        
                        AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
                        // 设置同步回调地址
                        request.SetReturnUrl("");
                        // 设置异步通知接收地址
                        request.SetNotifyUrl("");
                        // 将业务model载入到request
                        request.SetBizModel(model);
                        
                        try
                        {
                            AlipayTradePagePayResponse response = client.SdkExecute(request);
                            //支付宝支付地址
                            string url = "https://openapi.alipay.com/gateway.do?" + response.Body;
                            //Response.Write(response.Body);

                            return JsonConvert.SerializeObject(new repmsg
                            {
                                state = 1,
                                msg = "",
                                data = new
                                {
                                    tradeno = (string)ordernum.Value,
                                    total = (int)totals.Value,
                                    paytype = paytype,
                                    url = url
                                }
                            });
                        }
                        catch (Exception exp)
                        {
                            return JsonConvert.SerializeObject(new repmsg
                            {
                                state = 0,
                                msg = exp.Message
                            });
                        }
                    }
                    else if (paytype == 2)
                    {
                        try
                        {
                            //微信支付
                            NativePay nativePay = new NativePay();

                            string url = nativePay.GetPayUrl(setid.ToString(), (string)setname.Value, (string)ordernum.Value, (int)totals.Value);

                            if (string.IsNullOrEmpty(url))
                            {
                                return JsonConvert.SerializeObject(new repmsg
                                {
                                    state = 0,
                                    msg = "订单创建失败"
                                });
                            }

                            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                            qrCodeEncoder.QRCodeVersion = 0;
                            qrCodeEncoder.QRCodeScale = 8;

                            //将字符串生成二维码图片
                            Bitmap image = qrCodeEncoder.Encode(url, Encoding.Default);
                            string _root = Server.MapPath($"~/wxpaycode/{userid}/");
                            if (!System.IO.Directory.Exists(_root))
                            {
                                System.IO.Directory.CreateDirectory(_root);
                            }
                            string _filename = Guid.NewGuid().ToString().Replace("-", "") + ".jpg";
                            image.Save(_root + _filename, System.Drawing.Imaging.ImageFormat.Jpeg);

                            return JsonConvert.SerializeObject(new repmsg
                            {
                                state = 1,
                                msg = "",
                                data = new
                                {
                                    tradeno = (string)ordernum.Value,
                                    setname = (string)setname.Value,
                                    total = (int)totals.Value,
                                    paytype = paytype,
                                    qrimg = $"/wxpaycode/{userid}/{_filename}"
                                }
                            });
                        }
                        catch(Exception e)
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
                        return "";
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

        /// <summary>
        /// 检查订单是否支付成功
        /// </summary>
        /// <param name="ordernum"></param>
        /// <returns></returns>
        public string checkorder(string ordernum)
        {
            Thread.Sleep(1000);
            if (string.IsNullOrEmpty(ordernum) || ordernum.Length != 20)
            {
                return JsonConvert.SerializeObject(new repmsg
                {
                    state = 0,
                    msg = "查询失败，订单号不正确"
                });
            }
            int orderid = Convert.ToInt32(ordernum.Substring(8, 10));
            int userid = User.userid;
            //查询订单支付状态
            string sql = $"select ispay from user_order WITH(XLOCK) where orderid={orderid}";
            using (shhouseEntities ent = new shhouseEntities())
            {
                var datas = ent.Database.SqlQuery<int?>(sql).FirstOrDefault();
                if (datas.HasValue && datas.Value == 1)
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 1,
                        msg = "此订单已支付成功"
                    });
                }
                else
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "未成功支付"
                    });
                }
            }
        }

        /// <summary>
        /// e币兑换刷新量
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public string exchangerefresh(int setid, int num)
        {
            int userid = User.userid;
            
            using (shhouseEntities ent = new shhouseEntities())
            {
                try
                {
                    ObjectParameter state = new ObjectParameter("state", typeof(int));
                    ObjectParameter msg = new ObjectParameter("msg", typeof(string));
                    ent.exchange_e2refresh(userid, setid, num, state, msg);


                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = (int)state.Value,
                        msg = (string)msg.Value
                    });
                }
                catch
                {
                    return JsonConvert.SerializeObject(new repmsg
                    {
                        state = 0,
                        msg = "兑换失败，请稍后再试"
                    });
                }
            }
        }
    }
}