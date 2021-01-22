using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Configuration;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Client
{
    public partial class Client : Form
    {
        SpeechSynthesizer s = new SpeechSynthesizer();
        Choices list = new Choices();
        SpeechRecognitionEngine rec = new SpeechRecognitionEngine();
        bool wake = true;

        String[] greetings = new String[3] { "hello", "Hello, I am Elijah", "客户端启动成功" };

        public String greet_action()
        {
            Random r = new Random();
            return greetings[r.Next(3)];
        }
        string index = "C:\\Users\\13099\\Desktop\\sspu\\C#\\Service\\Client\\Client\\bin\\htmlview\\index.html";
        UTF8Encoding utf8 = new UTF8Encoding();
        string json = "";
        string question = "";
        string text = "";
        int userInsertFlag = 0;
        int robotInsertFlag = 0;
        static string receiveData;
        public Client()
        {
            list.Add(new string[]
            {
                 "现在时间","你的名字叫什么", "what is today","打开火狐",
                "sleep","wake", "hay, 杠杠", "最小化" , "最大化" ,"还原",
                "讲个笑话","杠杠","测试","清屏","连接",
            });
            Grammar gr = new Grammar(new GrammarBuilder(list));
            s.SelectVoiceByHints(VoiceGender.Female);
            s.Speak(greet_action());
            try
            {
                rec.RequestRecognizerUpdate();
                rec.LoadGrammarAsync(gr);
                rec.SetInputToDefaultAudioDevice();
                rec.RecognizeAsync(RecognizeMode.Multiple);
                rec.SpeechRecognized += rec_SpeechRecognized;

            }
            catch
            {
                return;
            }
            InitializeComponent();
            this.webBrowser1.Navigate(index);
            clearMsg();
        }
        public void say(string h)
        {
            s.Speak(h);
            ShowMsg(h);
            insertRobotMsg(h);
            //textBox1.AppendText(h + "\n");

        }
        public static void killprog(string s)
        {

            System.Diagnostics.Process[] procs = null;

            try
            {
                procs = Process.GetProcessesByName(s);
                Process Prog = procs[0];

                if (!Prog.HasExited)
                {
                    Prog.Kill();
                }

            }
            finally
            {
                if (procs != null)
                {
                    foreach (Process p in procs)
                    {
                        p.Dispose();
                    }
                }
            }
            procs = null;
        }

        String[] jokes = new String[4]
        {
            "从前有一个人叫小明，小明没听见。",
            "苍蝇没事的时候为什么要搓搓手，搓搓脚？开饭之前先洗手…",
            "我一个人要乘出租车，司机问我：你们两个要去哪里？",
            "到医院来了几趟，总觉得这里才应该叫检查院。"
        };
        public String tell_jokes()
        {
            Random rr = new Random();
            return jokes[rr.Next(4)];

        }
        private void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            string r = e.Result.Text;
            ShowMsg(r);
            insertRobotMsg(r);
            //textBox2.AppendText(r + "\n");
            if (r == "hay, 杠杠")
            {
                wake = true;
            }
            if (r == "wake")
            {
                wake = true;
            }
            if (r == "sleep")
            {
                wake = false;
            }

            if (wake == true)
            {

                if (r == "讲个笑话")
                {
                    say(tell_jokes());
                }

                if (r == "杠杠")
                {
                    say("I am listening !");
                }
                try
                {
                    if (r == "测试")
                    {
                        say("测试发送信息！");
                        string str = "0这条测试信息由语音控制发出！";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);//将str存放到名为buffer的数组中
                        socketSend.Send(buffer);

                    }
                }
                catch
                {
                    ShowMsg("连接失败，检查服务端是否开启");
                    insertRobotMsg("连接失败，检查服务端是否开启");
                }

                if (r == "最小化")
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                if (r == "还原")
                {
                    this.WindowState = FormWindowState.Normal;
                }
                
                if (r == "最大化")
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                if (r == "清屏")
                {
                    //textBox3.Text = null;
                    clearMsg();
                }
                if (r == "你的名字叫什么")
                {
                    say("I am EDITH");
                }
                if (r == "现在时间")
                {
                    say(DateTime.Now.ToString("h m tt"));
                }
                if (r == "what is today")
                {
                    say(DateTime.Now.ToString("M/d/yyyy"));
                }
                if (r == "连接")
                {
                    linkto();
                }
                if (r == "打开火狐")
                {
                    say("正在打开火狐浏览器,请稍候！"); 
                    Process.Start(@"C:\black\firefox\Mozilla Firefox\firefox.exe");
                    //Process.Start("http://www.google.co.in");
                }

            }

        }

        Socket socketSend;
        private void button1_Click(object sender, EventArgs e)//连接的按钮
        {
            try
            {
                //创建负责通信的Socket
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(textBox1.Text);//获取服务器的ip地址
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
                socketSend.Connect(point);
                ShowMsg("连接成功");
                insertRobotMsg("连接成功");

                //开启一个新的线程不停的接收服务端发来的消息
                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start();
            }
            catch
            {
                ShowMsg("连接失败，检查服务端是否开启");
                insertRobotMsg("连接失败，检查服务端是否开启");
            }
        }
        private void linkto()//连接的按钮
        {
            try
            {
                //创建负责通信的Socket
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(textBox1.Text);//获取服务器的ip地址
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
                socketSend.Connect(point);
                ShowMsg("连接成功");
                insertRobotMsg("连接成功");

                //开启一个新的线程不停的接收服务端发来的消息
                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start();
            }
            catch
            {
                ShowMsg("连接失败，检查服务端是否开启");
                insertRobotMsg("连接失败，检查服务端是否开启");
            }
        }
        void ShowMsg(string str)
        {
            string time = DateTime.Now.ToString();
            //textBox3.AppendText(time + "\r\n");
            //textBox3.AppendText(str + "\r\n\r\n");
        }

        void Recive()
        {
            while(true)//客户端不断的接收服务端传来的消息
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    int r = socketSend.Receive(buffer);
                    //实际接收到的有效字节数
                    if (r == 0)//若服务端关闭收到的数据为0，则停止接收
                    {
                        break;
                    }
                    int n = buffer[0];
                    if(n==0)//表示发送的是文字消息
                    {
                        string s = Encoding.UTF8.GetString(buffer, 1, r-1);//从第二个元素开始解码，第一个数据是标记位
                        ShowMsg(socketSend.RemoteEndPoint + ":" + s);
                        insertRobotMsg(s);
                        getAnswer(s, 2);
                    }
                    else if(n==1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();//保留对话框
                        sfd.InitialDirectory = @"C:\Users\13099\Desktop\";
                        sfd.Title = "请选择要保存的文件";
                        sfd.Filter = "所有文件|*.*";
                        sfd.ShowDialog(this);//弹出保存路径的对话框
                        string path = sfd.FileName;//拿到要保存的路径
                        using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }
                        MessageBox.Show("保存成功");
                        insertRobotMsg("已成功接收文件！");
                    }
                    else if(n==2)
                    {
                        ZD();
                    }

                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 选择要发送的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void choicefile_Click(object sender, EventArgs e)//选择文件的按钮
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"F:\计算机相关\C#\大作业";//设置初始目录
                ofd.Title = "请选择要发送的文件";
                ofd.Filter = "所有文件|*.*";
                ofd.ShowDialog();

                textBox5.Text = ofd.FileName;//将文件名显示在文本框中
            }
            catch
            {

            }

        }

        private void button4_Click(object sender, EventArgs e)//发送文件的按钮
        {
            //获得要发送文件的路径
            string path = textBox5.Text;
            try
            {
                using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))//运用文件流来传送文件
                {
                    byte[] buffer = new byte[1024 * 1024 * 5];
                    int r = fsRead.Read(buffer, 0, buffer.Length);
                    List<byte> list = new List<byte>();
                    list.Add(1);
                    list.AddRange(buffer);
                    byte[] newBuffer = list.ToArray();
                    //dicSocket[cboUsers.SelectedItem.ToString()].Send(newBuffer, 0, r + 1, SocketFlags.None);
                    socketSend.Send(newBuffer, 0, r + 1, SocketFlags.None);
                }
            }
            catch
            {
                ShowMsg("发送异常，请检查是否已选择文件或已经连上服务器！！！");
                insertRobotMsg("发送异常，请检查是否已选择文件或已经连上服务器！！！");
            }
            textBox5.Text = null;
        }

        void ZD()//震动的方法：两个点的坐标来回变换
        {
            for(int i =0;i<500;i++)
            {
                this.Location = new Point(200, 200);
                this.Location = new Point(200, 280);
            }
        }

        /// <summary>
        /// 客户端给服务器发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>//连接的按钮
        private void button2_Click(object sender, EventArgs e)
        {
            string str = textBox4.Text.Trim();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);//将str存放到名为buffer的数组中
            try
            {
                socketSend.Send(buffer);
                ShowMsg("自己" + ":" + str);
                insertUserMsg(str);
                getAnswer(str,1);
            }
            catch
            {
                ShowMsg("请先选择一个客户端进行连接！！！");
                insertUserMsg("请先选择一个客户端进行连接！！！");
            }
            
            textBox4.Text = null; //发送完消息后，清除输入框
        }

        public void clearMsg()
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(index);
            var node = doc.DocumentNode.SelectSingleNode("//body//div/ul");
            node.RemoveAllChildren();
            doc.Save(index);
            webBrowser1.Refresh();
        }

        public String getAnswer(string question,int x)
        {
            string url = "http://127.0.0.1:8000/api/v1/message/message/";
            //string postData = "key=" + appid + "&info=" + question;
            String user = null;
            if (x == 1)
            {
                user = "&user=1";
            }
            else
            {
                user = "&user=2";
            }
            string postData = "message=" + question + user;
            try
            {
                byte[] byteResquest = Encoding.GetEncoding("utf-8").GetBytes(postData);
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "Post";
                request.KeepAlive = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteResquest.Length;
                StreamWriter sw = new StreamWriter(request.GetRequestStream());
                sw.Write(postData);
                sw.Flush();
                WebResponse response = request.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader sr = new StreamReader(s, Encoding.GetEncoding("utf-8"));
                json = sr.ReadToEnd();
            }
            catch
            {
                json = "";
            }
            return json;
        }
        public String onGethismess()
        {
            string url = "http://127.0.0.1:8000/api/v1/message/message/";
            //string postData = '';
            try
            {
                //请求的地址
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://127.0.0.1:8000/api/v1/message/message/?size=9999");
                //请求方法
                req.Method = "GET";

                //req.ContentType = "application/json";
                //请求的超时时间    10秒还没出来就超时
                req.Timeout = 10000;
                //接收响应的结果
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                //for (var i =0;i< response.data.length)
                //接收HTTP响应的数据流
                using (Stream resStream = response.GetResponseStream())
                {
                    //把数据流存入StreamReadr,选用编码格式
                    using (StreamReader reader = new StreamReader(resStream, Encoding.UTF8))
                    {
                        //通过ReadToEnd()把整个HTTP响应作为一个字符串取回，也可以通过 StreamReader.ReadLine()方法逐行取回HTTP响应的内容。
                        string responseContent = reader.ReadToEnd().ToString();
                        //var items = JsonConvert.DeserializeObject<StatTemplateStateDto[]>(responseContent);
                        var item = reader.ReadToEnd();

                        //for (var i =0;i< reader.ReadToEnd().data.length)
                        insertUserMsg(responseContent); //测试成功
                        //丢给浏览器
                        //Response.Write(responseContent);
                        //关闭数据流
                        reader.Close();

                    }
                }
                //断开响应连接
                response.Close();
            }
            catch
            {
                json = "";
            }
            return json;
        }
        public class StatTemplateStateDto
        {
            /// <summary>
            /// 映射标识
            /// </summary>
            public virtual String Id { get; set; }

            /// <summary>
            /// 名称
            /// </summary>
            public virtual String Message { get; set; }

            /// <summary>
            /// 单位
            /// </summary>
            public virtual String User { get; set; }

            /// <summary>
            /// 最小值
            /// </summary>

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;//不检查新线程访问主线程
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //textBox3.Text = null;
            clearMsg();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        public void insertRobotMsg(string text)
        {
            string t = text;
            if (t == "")
            {
                return;
            }
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(index, utf8);
                var node = doc.DocumentNode.SelectSingleNode("//body//div//ul");
                var robotNode = "<li class = 'robot'><div><span>" + t + "</span></div></li>";
                HtmlNode newRobotMsg = HtmlNode.CreateNode(robotNode);
                node.AppendChild(newRobotMsg);
                doc.Save(index, utf8);
                webBrowser1.Refresh();

                System.Windows.Forms.Application.DoEvents();
                webBrowser1.Document.Window.ScrollTo(webBrowser1.Document.Body.ScrollTop, webBrowser1.Document.Body.ScrollRectangle.Bottom);
                robotInsertFlag = 1;
            }
            catch
            {
                robotInsertFlag = 0;
            }
        }

        public void insertUserMsg(string question)
        {
            string q = question;
            if (q == "")
            {
                return;
            }
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(index, utf8);
                var node = doc.DocumentNode.SelectSingleNode("//body//div//ul");
                HtmlNode newUserMsg = HtmlNode.CreateNode("<li class='user'><div>" + q + "</div></li>" + "\n");
                node.AppendChild(newUserMsg);
                doc.Save(index, utf8);
                webBrowser1.Refresh();
                System.Windows.Forms.Application.DoEvents();
                webBrowser1.Document.Window.ScrollTo(webBrowser1.Document.Body.ScrollTop, webBrowser1.Document.Body.ScrollRectangle.Bottom);
                userInsertFlag = 1;
            }
            catch
            {
                userInsertFlag = 0;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            onGethismess();
        }
    }
}
