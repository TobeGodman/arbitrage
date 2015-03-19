using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.IO;
using Quote2015;
using Trade2015;

namespace arbitrage
{
    public partial class Form2 : Form
    {


        public Form2()
        {
            this.Hide();
            InitializeComponent();
        }

        private Quote _q;
        private Trade _t;
        private string urlsh000300 = "http://qt.gtimg.cn/q=sh000300";
        private string ifId1, ifId2, ifId3, ifId4;
        private int daysToExp1, daysToExp2, daysToExp3, daysToExp4;
        private double sh300, riskFreeRate, dividend1, dividend2, dividend3, dividend4, etfCost, ifCost, trkError, marginCost;
        private double abtPt1, abtPt2, abtPt3, abtPt4, reAbtPt1, reAbtPt2, reAbtPt3, reAbtPt4;

        
        private readonly Timer _timer = new Timer
        {
            Interval = 250,
        };





        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timer.Stop();


            if (_t != null && _t.IsLogin)
            {
                _t.ReqUserLogout();
            }
            if (_q != null && _q.IsLogin)
            {
                foreach (var v in _q.DicTick.Keys)
                {
                    _q.ReqUnSubscribeMarketData(v);
                }
                _q.ReqUserLogout();
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {

            //刷新价格
            if (_q != null)
            {
                MarketData tick1, tick2, tick3, tick4;
                
                //沪深300指数数据
                string url300Cont = GetGeneralContent(urlsh000300);
                string[] str300 = url300Cont.Split('~');
               
                if (Convert.ToDecimal(str300[3]) > Convert.ToDecimal(str300[4]))
                {
                    this.textBox3.ForeColor = Color.Red;
                    this.textBox2.ForeColor = Color.Red;
                    this.textBox4.ForeColor = Color.Red;
                }
                else if (Convert.ToDecimal(str300[3]) < Convert.ToDecimal(str300[4]))
                {
                    this.textBox3.ForeColor = Color.Green;
                    this.textBox2.ForeColor = Color.Green;
                    this.textBox4.ForeColor = Color.Green;
                }
                else
                {
                    this.textBox3.ForeColor = Color.Gold;
                    this.textBox2.ForeColor = Color.Gold;
                    this.textBox4.ForeColor = Color.Gold;
                }
                this.textBox1.Text = str300[1];
                this.textBox2.Text = str300[3];
                this.textBox3.Text = str300[31];
                this.textBox4.Text = str300[32] + "%";
                sh300 = Convert.ToDouble(str300[3]);
                //正套临界点
                abtPt1 = sh300 * Math.Exp(riskFreeRate * daysToExp1 / 365) - dividend1 + sh300 * (2 * etfCost + 2 * ifCost + trkError * Math.Sqrt(daysToExp1));
                abtPt2 = sh300 * Math.Exp(riskFreeRate * daysToExp2 / 365) - dividend2 + sh300 * (2 * etfCost + 2 * ifCost + trkError * Math.Sqrt(daysToExp2));
                abtPt3 = sh300 * Math.Exp(riskFreeRate * daysToExp3 / 365) - dividend3 + sh300 * (2 * etfCost + 2 * ifCost + trkError * Math.Sqrt(daysToExp3));
                abtPt4 = sh300 * Math.Exp(riskFreeRate * daysToExp4 / 365) - dividend4 + sh300 * (2 * etfCost + 2 * ifCost + trkError * Math.Sqrt(daysToExp4));
                this.textBox57.Text = abtPt1.ToString("0.00");
                this.textBox58.Text = abtPt2.ToString("0.00");
                this.textBox59.Text = abtPt3.ToString("0.00");
                this.textBox60.Text = abtPt4.ToString("0.00");
                //反套临界点
                reAbtPt1 = sh300 * Math.Exp(riskFreeRate * daysToExp1 / 365) - dividend1 - sh300 * (2 * etfCost + 2 * ifCost + marginCost * daysToExp1 / 365 + trkError * Math.Sqrt(daysToExp1));
                reAbtPt2 = sh300 * Math.Exp(riskFreeRate * daysToExp2 / 365) - dividend2 - sh300 * (2 * etfCost + 2 * ifCost + marginCost * daysToExp2 / 365 + trkError * Math.Sqrt(daysToExp2));
                reAbtPt3 = sh300 * Math.Exp(riskFreeRate * daysToExp3 / 365) - dividend3 - sh300 * (2 * etfCost + 2 * ifCost + marginCost * daysToExp3 / 365 + trkError * Math.Sqrt(daysToExp3));
                reAbtPt4 = sh300 * Math.Exp(riskFreeRate * daysToExp4 / 365) - dividend4 - sh300 * (2 * etfCost + 2 * ifCost + marginCost * daysToExp4 / 365 + trkError * Math.Sqrt(daysToExp4));
                this.textBox61.Text = reAbtPt1.ToString("0.00");
                this.textBox62.Text = reAbtPt2.ToString("0.00");
                this.textBox63.Text = reAbtPt3.ToString("0.00");
                this.textBox64.Text = reAbtPt4.ToString("0.00");

                
                //股指期货数据
                if (_q.DicTick.TryGetValue(ifId1, out tick1))
                {
                    if (tick1.LastPrice > tick1.PreSettlementPrice)
                    {
                        this.textBox13.ForeColor = Color.Red;
                        this.textBox12.ForeColor = Color.Red;
                        this.textBox14.ForeColor = Color.Red;
                        this.textBox5.ForeColor = Color.Red;
                    }
                    else if (tick1.LastPrice < tick1.PreSettlementPrice)
                    {
                        this.textBox13.ForeColor = Color.Green;
                        this.textBox12.ForeColor = Color.Green;
                        this.textBox14.ForeColor = Color.Green;
                        this.textBox5.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox13.ForeColor = Color.Gold;
                        this.textBox12.ForeColor = Color.Gold;
                        this.textBox14.ForeColor = Color.Gold;
                        this.textBox5.ForeColor = Color.Gold;
                    }
                    if (tick1.LastPrice > Convert.ToDouble(str300[3]))
                    {
                        this.textBox53.ForeColor = Color.Red;
                        this.textBox15.ForeColor = Color.Red;
                    }
                    else if (tick1.LastPrice < Convert.ToDouble(str300[3]))
                    {
                        this.textBox53.ForeColor = Color.Green;
                        this.textBox15.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox53.ForeColor = Color.Gold;
                        this.textBox15.ForeColor = Color.Gold;
                    }

                    this.textBox5.Text = this.textBox12.Text = ((decimal)tick1.LastPrice).ToString();
                    this.textBox16.Text = (tick1.BidVolume).ToString();
                    this.textBox17.Text = ((decimal)tick1.BidPrice).ToString();
                    this.textBox18.Text = ((decimal)tick1.AskPrice).ToString();
                    this.textBox19.Text = (tick1.AskVolume).ToString();
                    this.textBox13.Text = ((decimal)tick1.LastPrice - (decimal)tick1.PreSettlementPrice).ToString();
                    this.textBox14.Text = (((decimal)tick1.LastPrice - (decimal)tick1.PreSettlementPrice) * 100 / (decimal)tick1.PreSettlementPrice).ToString("0.00") + "%";
                    this.textBox53.Text = this.textBox15.Text = ((decimal)tick1.LastPrice - Convert.ToDecimal(str300[3])).ToString();
                    this.textBox20.Text = (tick1.Volume).ToString();
                    this.textBox6.Text = (tick1.UpdateTime).ToString();
                }
                if (_q.DicTick.TryGetValue(ifId2, out tick2))
                {
                   
                    if (tick2.LastPrice > tick2.PreSettlementPrice)
                    {
                        this.textBox23.ForeColor = Color.Red;
                        this.textBox22.ForeColor = Color.Red;
                        this.textBox24.ForeColor = Color.Red;
                        this.textBox10.ForeColor = Color.Red;
                    }
                    else if (tick2.LastPrice < tick2.PreSettlementPrice)
                    {
                        this.textBox23.ForeColor = Color.Green;
                        this.textBox22.ForeColor = Color.Green;
                        this.textBox24.ForeColor = Color.Green;
                        this.textBox10.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox23.ForeColor = Color.Gold;
                        this.textBox22.ForeColor = Color.Gold;
                        this.textBox24.ForeColor = Color.Gold;
                        this.textBox10.ForeColor = Color.Gold;
                    }
                    if (tick2.LastPrice > Convert.ToDouble(str300[3]))
                    {
                        this.textBox54.ForeColor = Color.Red;
                        this.textBox25.ForeColor = Color.Red;
                    }
                    else if (tick2.LastPrice < Convert.ToDouble(str300[3]))
                    {
                        this.textBox54.ForeColor = Color.Green;
                        this.textBox25.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox54.ForeColor = Color.Gold;
                        this.textBox25.ForeColor = Color.Gold;
                    }
                    this.textBox10.Text = this.textBox22.Text = ((decimal)tick2.LastPrice).ToString();
                    this.textBox26.Text = (tick2.BidVolume).ToString();
                    this.textBox27.Text = ((decimal)tick2.BidPrice).ToString();
                    this.textBox28.Text = ((decimal)tick2.AskPrice).ToString();
                    this.textBox29.Text = (tick2.AskVolume).ToString();
                    this.textBox23.Text = ((decimal)tick2.LastPrice - (decimal)tick2.PreSettlementPrice).ToString();
                    this.textBox24.Text = (((decimal)tick2.LastPrice - (decimal)tick2.PreSettlementPrice) * 100 / (decimal)tick2.PreSettlementPrice).ToString("0.00") + "%";
                    this.textBox54.Text = this.textBox25.Text = ((decimal)tick2.LastPrice - Convert.ToDecimal(str300[3])).ToString();
                    this.textBox30.Text = (tick2.Volume).ToString();
                    this.textBox7.Text = (tick2.UpdateTime).ToString();
                }
                if (_q.DicTick.TryGetValue(ifId3, out tick3))
                {
                    if (tick3.LastPrice > tick3.PreSettlementPrice)
                    {
                        this.textBox33.ForeColor = Color.Red;
                        this.textBox32.ForeColor = Color.Red;
                        this.textBox34.ForeColor = Color.Red;
                        this.textBox51.ForeColor = Color.Red;
                    }
                    else if (tick3.LastPrice < tick3.PreSettlementPrice)
                    {
                        this.textBox33.ForeColor = Color.Green;
                        this.textBox32.ForeColor = Color.Green;
                        this.textBox34.ForeColor = Color.Green;
                        this.textBox51.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox33.ForeColor = Color.Gold;
                        this.textBox32.ForeColor = Color.Gold;
                        this.textBox34.ForeColor = Color.Gold;
                        this.textBox51.ForeColor = Color.Gold;
                    }
                    if (tick3.LastPrice > Convert.ToDouble(str300[3]))
                    {
                        this.textBox55.ForeColor = Color.Red;
                        this.textBox35.ForeColor = Color.Red;
                    }
                    else if (tick3.LastPrice < Convert.ToDouble(str300[3]))
                    {
                        this.textBox55.ForeColor = Color.Green;
                        this.textBox35.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox55.ForeColor = Color.Gold;
                        this.textBox35.ForeColor = Color.Gold;
                    }
                    this.textBox51.Text = this.textBox32.Text = ((decimal)tick3.LastPrice).ToString();
                    this.textBox36.Text = (tick3.BidVolume).ToString();
                    this.textBox37.Text = ((decimal)tick3.BidPrice).ToString();
                    this.textBox38.Text = ((decimal)tick3.AskPrice).ToString();
                    this.textBox39.Text = (tick3.AskVolume).ToString();
                    this.textBox33.Text = ((decimal)tick3.LastPrice - (decimal)tick3.PreSettlementPrice).ToString();
                    this.textBox34.Text = (((decimal)tick3.LastPrice - (decimal)tick3.PreSettlementPrice) * 100 / (decimal)tick3.PreSettlementPrice).ToString("0.00") + "%";
                    this.textBox55.Text = this.textBox35.Text = ((decimal)tick3.LastPrice - Convert.ToDecimal(str300[3])).ToString();
                    this.textBox40.Text = (tick3.Volume).ToString();
                    this.textBox8.Text = (tick3.UpdateTime).ToString();
                }
                if (_q.DicTick.TryGetValue(ifId4, out tick4))
                {
                    if (tick4.LastPrice > tick4.PreSettlementPrice)
                    {
                        this.textBox43.ForeColor = Color.Red;
                        this.textBox42.ForeColor = Color.Red;
                        this.textBox44.ForeColor = Color.Red;
                        this.textBox52.ForeColor = Color.Red;
                    }
                    else if (tick4.LastPrice < tick4.PreSettlementPrice)
                    {
                        this.textBox43.ForeColor = Color.Green;
                        this.textBox42.ForeColor = Color.Green;
                        this.textBox44.ForeColor = Color.Green;
                        this.textBox52.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox43.ForeColor = Color.Gold;
                        this.textBox42.ForeColor = Color.Gold;
                        this.textBox44.ForeColor = Color.Gold;
                        this.textBox52.ForeColor = Color.Gold;
                    }
                    if (tick4.LastPrice > Convert.ToDouble(str300[3]))
                    {
                        this.textBox56.ForeColor = Color.Red;
                        this.textBox45.ForeColor = Color.Red;
                    }
                    else if (tick4.LastPrice < Convert.ToDouble(str300[3]))
                    {
                        this.textBox56.ForeColor = Color.Green;
                        this.textBox45.ForeColor = Color.Green;
                    }
                    else
                    {
                        this.textBox56.ForeColor = Color.Gold;
                        this.textBox45.ForeColor = Color.Gold;
                    }
                    this.textBox52.Text = this.textBox42.Text = ((decimal)tick4.LastPrice).ToString();
                    this.textBox46.Text = (tick4.BidVolume).ToString();
                    this.textBox47.Text = ((decimal)tick4.BidPrice).ToString();
                    this.textBox48.Text = ((decimal)tick4.AskPrice).ToString();
                    this.textBox49.Text = (tick4.AskVolume).ToString();
                    this.textBox43.Text = ((decimal)tick4.LastPrice - (decimal)tick4.PreSettlementPrice).ToString();
                    this.textBox44.Text = (((decimal)tick4.LastPrice - (decimal)tick4.PreSettlementPrice) * 100 / (decimal)tick4.PreSettlementPrice).ToString("0.00") + "%";
                    this.textBox56.Text = this.textBox45.Text = ((decimal)tick4.LastPrice - Convert.ToDecimal(str300[3])).ToString();
                    this.textBox50.Text = (tick4.Volume).ToString();
                    this.textBox9.Text = (tick4.UpdateTime).ToString();
                }
                if (tick1 != null && tick1.LastPrice > abtPt1)
                    this.label34.Text = "正套";
                else if (tick1 != null && tick1.LastPrice < reAbtPt1)
                    this.label34.Text = "反套";
                else
                    this.label34.Text = "无";

                if (tick2 != null && tick2.LastPrice > abtPt2)
                    this.label35.Text = "正套";
                else if (tick2 != null && tick2.LastPrice < reAbtPt2)
                    this.label35.Text = "反套";
                else
                    this.label35.Text = "无";

                if (tick3 != null && tick3.LastPrice > abtPt3)
                    this.label36.Text = "正套";
                else if (tick3 != null && tick3.LastPrice < reAbtPt3)
                    this.label36.Text = "反套";
                else
                    this.label36.Text = "无";

                if (tick4 != null && tick4.LastPrice > abtPt4)
                    this.label37.Text = "正套";
                else if (tick4 != null && tick4.LastPrice < reAbtPt4)
                    this.label37.Text = "反套";
                else
                    this.label37.Text = "无";

                this.button1.BackColor = Color.Gold;
                this.button1.ForeColor = Color.Black;
                this.button1.Text = "正在监控";
            }


        }



        void quote_OnRtnTick(object sender, TickEventArgs e)
        {

        }

        private static string GetGeneralContent(string strUrl)
        {
            string strMsg = string.Empty;
            try
            {
                WebRequest request = WebRequest.Create(strUrl);
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("gb2312"));

                strMsg = reader.ReadToEnd();

                reader.Close();
                reader.Dispose();
                response.Close();
            }
            catch
            { }
            return strMsg;
        }



        private void Form2_Load(object sender, EventArgs e)
        {

            Form1 fLogin = new Form1();
            if (fLogin.ShowDialog(this) == DialogResult.Cancel)
            {
                this.Close();
                return;
            }

            _t = fLogin.trade;
            _q = fLogin.quote;

            if (_q != null)
                _q.OnRtnTick += quote_OnRtnTick;

            fLogin.Close();




        }



        private void button1_Click(object sender, EventArgs e)
        {

            DateTime today = DateTime.Now;

            if (_t == null)
            {
                MessageBox.Show("获取行情信息失败，请重新打开本软件");
                this.Close();
                return;
            }
            else
            {
                string[] allId = _t.DicInstrumentField.Keys.ToArray();
                List<string> ifId = new List<string>();
                for (int j = 0; j < allId.Length; j++)
                {
                    if (allId[j].Contains("IF"))
                    {
                        ifId.Add(allId[j]);
                    }

                }
                if (ifId.Count() == 4)
                {

                    ifId.Sort();
                    this.label8.Text = this.textBox11.Text = ifId[0];
                    ifId1 = ifId[0];
                    this.label9.Text = this.textBox21.Text = ifId[1];
                    ifId2 = ifId[1];
                    this.label10.Text = this.textBox31.Text = ifId[2];
                    ifId3 = ifId[2];
                    this.label14.Text = this.textBox41.Text = ifId[3];
                    ifId4 = ifId[3];
                }



                if (_q != null && _q.IsLogin)
                {
                    _q.ReqSubscribeMarketData(ifId1);
                    _q.ReqSubscribeMarketData(ifId2);
                    _q.ReqSubscribeMarketData(ifId3);
                    _q.ReqSubscribeMarketData(ifId4);
                       
                }
                else
                {
                    MessageBox.Show("获取数据失败");
                }


                InstrumentField instField1, instField2, instField3, instField4;
                if (ifId.Count() == 4)
                {
                    if (_t.DicInstrumentField.TryGetValue(ifId[0].ToString(), out instField1))
                    {
                        this.textBox65.Text = instField1.ExpireDate;
                        DateTime ifExpDay1 = DateTime.ParseExact(instField1.ExpireDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        TimeSpan span1 = ifExpDay1.Subtract(today);
                        daysToExp1 = Convert.ToInt32(span1.Days) + 1;

                    }
                    if (_t.DicInstrumentField.TryGetValue(ifId[1].ToString(), out instField2))
                    {
                        this.textBox66.Text = instField2.ExpireDate;
                        DateTime ifExpDay2 = DateTime.ParseExact(instField2.ExpireDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        TimeSpan span2 = ifExpDay2.Subtract(today);
                        daysToExp2 = Convert.ToInt32(span2.Days) + 1;
                    }
                    if (_t.DicInstrumentField.TryGetValue(ifId[2].ToString(), out instField3))
                    {
                        this.textBox67.Text = instField3.ExpireDate;
                        DateTime ifExpDay3 = DateTime.ParseExact(instField3.ExpireDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        TimeSpan span3 = ifExpDay3.Subtract(today);
                        daysToExp3 = Convert.ToInt32(span3.Days) + 1;
                    }
                    if (_t.DicInstrumentField.TryGetValue(ifId[3].ToString(), out instField4))
                    {
                        this.textBox68.Text = instField4.ExpireDate;
                        DateTime ifExpDay4 = DateTime.ParseExact(instField4.ExpireDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        TimeSpan span4 = ifExpDay4.Subtract(today);
                        daysToExp4 = Convert.ToInt32(span4.Days) + 1;
                    }
                }
                riskFreeRate = (double)(numericUpDown1.Value / 100);
                etfCost = (double)(numericUpDown2.Value / 100);
                ifCost = (double)(numericUpDown3.Value / 100);
                trkError = (double)(numericUpDown4.Value / 100);
                marginCost = (double)(numericUpDown5.Value / 100);
                dividend1 = (double)numericUpDown6.Value;
                dividend2 = (double)numericUpDown7.Value;
                dividend3 = (double)numericUpDown8.Value;
                dividend4 = (double)numericUpDown9.Value;

                this.numericUpDown1.Enabled = false;
                this.numericUpDown2.Enabled = false;
                this.numericUpDown3.Enabled = false;
                this.numericUpDown4.Enabled = false;
                this.numericUpDown5.Enabled = false;
                this.numericUpDown6.Enabled = false;
                this.numericUpDown7.Enabled = false;
                this.numericUpDown8.Enabled = false;
                this.numericUpDown9.Enabled = false;


                _timer.Tick += _timer_Tick;
                _timer.Start();
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.button1.BackColor = Color.Black;
            this.button1.ForeColor = Color.White;
            this.button1.Text = "开始监控";
            _timer.Stop();
            this.numericUpDown1.Enabled = true;
            this.numericUpDown2.Enabled = true;
            this.numericUpDown3.Enabled = true;
            this.numericUpDown4.Enabled = true;
            this.numericUpDown5.Enabled = true;
            this.numericUpDown6.Enabled = true;
            this.numericUpDown7.Enabled = true;
            this.numericUpDown8.Enabled = true;
            this.numericUpDown9.Enabled = true;
        }




    }


}
