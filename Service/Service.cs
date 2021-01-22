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
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Service
{
    public partial class Service : Form
    {
        string index = "C:\\Users\\13099\\Desktop\\sspu\\C#\\Service\\Service\\htmlview\\index.html";
        UTF8Encoding utf8 = new UTF8Encoding();
       
        int userInsertFlag = 0;
        int robotInsertFlag = 0;
        public Service()
        {
            InitializeComponent();
            this.webBrowser1.Navigate(index);
        }

        private void button6_Click(object sender, EventArgs e)//震动按钮
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            try
            {
                dicSocket[cboUsers.SelectedItem.ToString()].Send(buffer);//获得下拉框里的ip和端口号
            }
            catch
            {
                ShowMsg("请先选择一个客户端进行连接！！！");
                insertRobotMsg("请先选择一个客户端进行连接！！！");
            }

        }

        private void button1_Click(object sender, EventArgs e)//后退按钮
        {
            Application.Exit();
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
                    dicSocket[cboUsers.SelectedItem.ToString()].Send(newBuffer, 0, r + 1, SocketFlags.None);
                }
            }
            catch
            {
                ShowMsg("发送异常，请检查是否已选择文件或已经连上客户端！！！");
                insertRobotMsg("发送异常，请检查是否已选择文件或已经连上客户端！！！");
            }
            textBox5.Text = null;
        }

        /// <summary>
        /// 选择要发送的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)//选择文件的按钮
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

        private void button2_Click(object sender, EventArgs e)//开始监听按钮
        {
            try
            {
                //点击开始监听时，在服务器端创建一个负责监听ip地址跟端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//TCP需要建立连接
                IPAddress ip = IPAddress.Any;
                //创建端口号对象
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
                //监听
                socketWatch.Bind(point);
                ShowMsg("监听成功");
                insertRobotMsg("监听成功");
                socketWatch.Listen(10);

                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);
            }
            catch
            { }
        }

        Socket socketSend;
        //等待客户端的连接 并且创建与之通信用的Socket
        void Listen(object o)
        {
            Socket socketWatch = o as Socket;
            //等待客户端的连接 并且创建一个负责通信的Socket
            while(true)//连续连接多个客户端
            {
                try
                {
                    //负责跟客户端通信的Socket
                    socketSend = socketWatch.Accept();
                    //将远程连接的客户端的ip地址和socket存入集合中
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    Console.WriteLine("test");
                    //将远程连接的客户端的ip地址和端口号存放到下拉框中
                    cboUsers.Items.Add(socketSend.RemoteEndPoint.ToString());
                    //IP连接成功
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");
                    insertRobotMsg("连接成功");
                    //开启一个新的线程不停的接收客户端发送的消息
                    Thread th = new Thread(Recive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch
                {

                }
            }
        }

        void ShowMsg(string str)//展示文本的方法
        {
            string time = DateTime.Now.ToString();
            //textBox3.AppendText(time + "\r\n");
           // textBox3.AppendText(str + "\r\n\r\n");
        }

        //将远程连接的客户端的ip地址和socket存入集合中
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();

        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Recive(object o)//接收消息的方法
        {
            Socket socketSend = o as Socket;
            while (true)
            {
                try
                {
                    //客户端连接成功后，服务器应该接受客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 2];//接收完成之后放到大小为2M的数组里面
                                                              //实际接收到的有效字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    int n = buffer[0];
                    if (n == 1)
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
                        ShowMsg("已成功接收文件！");
                        insertRobotMsg("已成功接收文件！");
                        MessageBox.Show("保存成功");
                        
                    }
                    else
                    {
                        string str = Encoding.UTF8.GetString(buffer, 0, r);
                        //string str = Encoding.UTF8.GetString(buffer, 1, r - 1);//从第二个元素开始解码，第一个数据是标记位
                        ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                        insertRobotMsg(str);
                    }

                } 
                catch
                {

                }
            }
        }


        /// <summary>
        /// 服务器给客户端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)//发送信息按钮
        {
            string str = textBox4.Text;
            ShowMsg("自己" + ":" + str);
            insertUserMsg(str);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            List<byte> list = new List<byte>();
            list.Add(0);
            list.AddRange(buffer);
            //将泛型集合转换为数组
            byte[] newBuffer = list.ToArray();
            //获得用户在下拉框中选中的ip地址
            try
            {
                string ip = cboUsers.SelectedItem.ToString();
                dicSocket[ip].Send(newBuffer);
            }
            catch
            {
                ShowMsg("请先选择一个客户端进行连接！！！");
                insertRobotMsg("请先选择一个客户端进行连接！！！");
            }

            
            //socketSend.Send(buffer);
            textBox4.Text = null;
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

        public void clearMsg()
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(index);
            var node = doc.DocumentNode.SelectSingleNode("//body//div/ul");
            node.RemoveAllChildren();
            doc.Save(index);
            webBrowser1.Refresh();
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //textBox3.Text = null;
            clearMsg();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
