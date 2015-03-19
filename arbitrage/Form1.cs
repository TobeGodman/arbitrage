using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using Quote2015;
using Trade2015;

namespace arbitrage
{
    public partial class Form1 : Form
    {



        internal Quote quote;
        internal Trade trade;
        private string[] lines = { "国都期货-联通", "国都期货-电信" ,"测试"};

        public Form1()
        {
            InitializeComponent();
        }



       

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                ShowMsg("请填写账号和密码");
            }
            else if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                ShowMsg("请填写密码");
            }
            else
            {
                ShowMsg("登录中...");
                if (this.comboBox1.SelectedIndex == 0)
                {
                    quote = new Quote("ctp_quote_proxy.dll")
                    {
                        //Server = "tcp://asp-sim2-md1.financial-trading-platform.com:26213",
                        //Broker = "2030",
                        Server = "tcp://140.206.102.218:41213",
                        Broker = "9050",
                        Investor = this.textBox1.Text,
                        Password = this.textBox2.Text,
                    };
                    quote.OnFrontConnected += quote_OnFrontConnected;
                    quote.OnRspUserLogin += quote_OnRspUserLogin;

                    trade = new Trade("ctp_trade_proxy.dll")
                    {
                        //Server = "tcp://asp-sim2-front1.financial-trading-platform.com:26205",
                        //Broker = "2030",
                        Server = "tcp://140.206.102.218:41205",
                        Broker = "9050",
                        Investor = this.textBox1.Text,
                        Password = this.textBox2.Text,
                    };
                    trade.OnFrontConnected += trade_OnFrontConnected;
                    trade.OnRspUserLogin += trade_OnRspUserLogin;
                    trade.ReqConnect();
                }
                else if (this.comboBox1.SelectedIndex == 1)
                {
                    quote = new Quote("ctp_quote_proxy.dll")
                    {
                        //Server = "tcp://222.66.235.70:61213",
                        //Broker = "1026",
                        Server = "tcp://180.166.45.98:41213",
                        Broker = "9050",
                        Investor = this.textBox1.Text,
                        Password = this.textBox2.Text,
                    };
                    quote.OnFrontConnected += quote_OnFrontConnected;
                    quote.OnRspUserLogin += quote_OnRspUserLogin;

                    trade = new Trade("ctp_trade_proxy.dll")
                    {
                        //Server = "tcp://222.66.235.70:61205",
                        //Broker = "1026",
                        Server = "tcp://180.166.45.98:41205",
                        Broker = "9050",
                        Investor = this.textBox1.Text,
                        Password = this.textBox2.Text,
                    };
                    trade.OnFrontConnected += trade_OnFrontConnected;
                    trade.OnRspUserLogin += trade_OnRspUserLogin;
                    trade.ReqConnect();
                }
                else
                {
                    quote = new Quote("ctp_quote_proxy.dll")
                    {
                        Server = "tcp://140.206.102.69:41213",
                        Broker = "7090",
                        //Server = "tcp://140.206.102.218:41213",
                        //Broker = "9050",
                        //Investor = this.textBox1.Text,
                        //Password = this.textBox2.Text,
                    };
                    quote.OnFrontConnected += quote_OnFrontConnected;
                    quote.OnRspUserLogin += quote_OnRspUserLogin;

                    trade = new Trade("ctp_trade_proxy.dll")
                    {
                        Server = "tcp://asp-sim2-front1.financial-trading-platform.com:26205",
                        Broker = "2030",
                        //Server = "tcp://140.206.102.218:41205",
                        //Broker = "9050",
                        Investor = this.textBox1.Text,
                        Password = this.textBox2.Text,
                    };
                    trade.OnFrontConnected += trade_OnFrontConnected;
                    trade.OnRspUserLogin += trade_OnRspUserLogin;
                    trade.ReqConnect();
                }
            }

        }

        

        void trade_OnRspUserLogin(object sender, Trade2015.IntEventArgs e)
        {
            if (e.Value == 0)
            {
                ShowMsg("登录成功");
                Thread.Sleep(1500);
                //交易登录成功后,登录行情
                if (quote == null)
                    LoginSuccess();
                else
                    quote.ReqConnect();
                
            }
            else
            {
                ShowMsg("登录错误:"+e.Value.ToString());
                trade.ReqUserLogout();
                trade = null;
                quote = null;
            }
        }

        void trade_OnFrontConnected(object sender, EventArgs e)
        {
            ((Trade)sender).ReqUserLogin();
        }


        void quote_OnRspUserLogin(object sender, Quote2015.IntEventArgs e)
        {
            LoginSuccess();
        }

        void quote_OnFrontConnected(object sender, EventArgs e)
        {
            ((Quote)sender).ReqUserLogin();
        }

        //登录成功
        void LoginSuccess()
        {
            trade.OnFrontConnected -= trade_OnFrontConnected;
            trade.OnRspUserLogin -= trade_OnRspUserLogin;
            if (quote != null)
            {
                quote.OnFrontConnected -= quote_OnFrontConnected;
                quote.OnRspUserLogin -= quote_OnRspUserLogin;
            }
            this.Invoke(new Action(() =>
            {
                this.DialogResult = DialogResult.OK;
            }));
        }




        void ShowMsg(string pMsg)
        {
            this.Invoke(new Action(() =>
            {
                this.label3.Text = DateTime.Now.ToString("HH:mm:ss") + "|" + pMsg;
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string line in lines)
                this.comboBox1.Items.Add(line);
            this.comboBox1.SelectedIndex = 0;
        }

    }
}
