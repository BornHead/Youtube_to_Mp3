using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq; 
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoLibrary;
using MediaToolkit;
using System.Net;
using System.Threading;

namespace 유튜브tomp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            var cookiefolder_path_ = Application.StartupPath + @"\download";
            if (Directory.Exists(cookiefolder_path_) == false)
            {
                Directory.CreateDirectory(cookiefolder_path_);
            }

            InitializeComponent();
        }

        //┏====================================================================================================================================================┒
        //│    ↓  ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓    폼드래그 이동   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓   ↓ │

        private Point mousePoint; // 현재 마우스 포인터의 좌표저장 변수 선언

        // 마우스 누를때 현재 마우스 좌표를 저장한다 
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            mousePoint = new Point(e.X, e.Y); //현재 마우스 좌표 저장
        }

        // 마우스 왼쪽 버튼을 누르고 움직이면 폼을 이동시킨다
        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) //마우스 왼쪽 클릭 시에만 실행
            {
                //폼의 위치를 드래그중인 마우스의 좌표로 이동 
                Location = new Point(Left - (mousePoint.X - e.X), Top - (mousePoint.Y - e.Y));
            }
        }

        //┏====================================================================================================================================================┒
        //│    ↑  ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑    폼드래그 이동   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑   ↑ │

        private async void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            foreach (var url in guna2TextBox1.Text.Split('\n'))
            {
                var url_now = url.Replace("\r","");
                if (url_now != "")
                {
                    //https://youtu.be/dly3BvtmXbA
                    var v = string.Empty;
                    Regex regex = new Regex(@"\.be/(.+)");
                    foreach (Match t in regex.Matches(url_now))
                    {
                        v = t.Groups[1].Value;
                    }
                    //https://x.cec.cx/check.php?

                    guna2ProgressBar1.Minimum = 0;
                    guna2ProgressBar1.Maximum = 100;

                    guna2ProgressBar1.Value = 0;

                retry:
                    // Title
                    WebRequest GetTitle = HttpWebRequest.Create(url_now);

                    
                    var youtube = YouTube.Default;
                    var video = await youtube.GetVideoAsync(url_now);
                    var fullname = video.FullName.Replace(".mp4", "");

                    Thread.Sleep(100);
                    try
                    {
                        File.WriteAllBytes(Application.StartupPath + @"\download" + @"\" + fullname, await video.GetBytesAsync());

                        var inputFile = new MediaToolkit.Model.MediaFile { Filename = Application.StartupPath + @"\download" + @"\" + fullname };
                        var outputFile = new MediaToolkit.Model.MediaFile { Filename = $"{Application.StartupPath + @"\download" + @"\" + fullname}.mp3" };
                        using (var enging = new Engine())
                        {
                            enging.GetMetadata(inputFile);
                            enging.Convert(inputFile, outputFile);
                        }

                        File.Delete(Application.StartupPath + @"\download" + @"\" + fullname);

                        guna2ProgressBar1.Value = 100;
                    }
                    catch
                    {
                        fullname = Regex.Replace(fullname, @"[^a-zA-Z0-9가-힣]", "", RegexOptions.Singleline);
                        goto retry;
                    }
                    int index = guna2TextBox1.Text.IndexOf(Environment.NewLine);
                    CSafesettext(guna2TextBox1, guna2TextBox1.Text.Remove(0, index + 1));
                }
            }

            MessageBox.Show("완료양");
        }
        delegate void CrossThreadSafesettext(Control ctl,string text);
        static private void CSafesettext(Control ctl, string text)
        {
            if (ctl.InvokeRequired)
                ctl.Invoke(new CrossThreadSafesettext(CSafesettext), ctl,text);
            else
            {
                ctl.Text = text;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Button1.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }
        class Donation
        {
            public string 노래제목 { get; set; }   // Id 라는 속성
        }
        void data_binding()
        {
            try
            {
                // 컬렉션 리스트를 만든다
                List<Donation> donationList = new List<Donation>();

                DirectoryInfo di = new DirectoryInfo(Application.StartupPath + @"\download");
                //CSafeDGVadd(listView1);
                foreach (var t in di.GetFiles().Select((value, index) => (value, index)))
                {
                    var ext = new List<string> { "mp3" };
                    var myFiles = Directory
                        .EnumerateFiles(Application.StartupPath + @"\download", "*.*", SearchOption.AllDirectories)
                        .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

                    // 컬럼들로 Donation 객체를 만든다
                    Donation d = new Donation();
                    d.노래제목 = di.GetFiles()[t.index].Name.Replace(".mp3", "");

                    // Donation 리스트에 Donation 객체를 추가한다
                    donationList.Add(d);
                    //CSafelistadd(listView1, di.GetFiles()[t.index].Name, di.GetFiles()[t.index].LastWriteTime.ToString("yy년MM월dd일"));
                }
                // DataGridView의 DataSource 속성에 Donation 리스트 (컬렉션)을 지정하여
                // 데이타 바인딩을 수행한다
                if (donationList.Count > 0)
                    CSafeDGVdatasrc(guna2DataGridView1, donationList);
                else
                    CSafeDGVdatasrc(guna2DataGridView1, donationList);
            }
            catch { }
        }
        delegate void CrossThreadSafeDGVdatasrc(Guna2DataGridView ctl, dynamic t);
        static private void CSafeDGVdatasrc(Guna2DataGridView ctl, dynamic t)
        {
            if (ctl.InvokeRequired)
            {
                ctl.Invoke(new CrossThreadSafeDGVdatasrc(CSafeDGVdatasrc), ctl, t);
            }
            else
            {
                try
                {
                    var scrollPosition = 0;
                    var cell = 0;
                    try
                    {
                        scrollPosition = ctl.FirstDisplayedScrollingRowIndex;
                        cell = ctl.CurrentCell.RowIndex;
                    }
                    catch { }

                    ctl.DataSource = t;
                    //...reload 
                    ctl.CurrentCell = ctl.Rows[cell].Cells[0];
                    ctl.FirstDisplayedScrollingRowIndex = scrollPosition;
                }
                catch
                { }
            }
        }
        delegate void CrossThreadSafeDGVadd(Guna2DataGridView ctl, bool status, string[] t0 = null, string t1 = "", string t2 = "", string t3 = "", string t4 = "", string t5 = "", string t6 = "", string t7 = "", string t8 = "", string t9 = "", string t10 = "");
        static private void CSafeDGVadd(Guna2DataGridView ctl, bool status, string[] t0 = null, string t1 = "", string t2 = "", string t3 = "", string t4 = "", string t5 = "", string t6 = "", string t7 = "", string t8 = "", string t9 = "", string t10 = "")
        {
            if (ctl.InvokeRequired)
            {
                ctl.Invoke(new CrossThreadSafeDGVadd(CSafeDGVadd), ctl, status, t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
            }
            else
            {
                if (status == true)
                {
                    ctl.Rows.Add(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
                    //ctl.FirstDisplayedScrollingRowIndex = ctl.Rows.Count - 1;
                }
                else
                {
                    ctl.Rows.Add(t0);
                    //ctl.FirstDisplayedScrollingRowIndex = ctl.Rows.Count - 1;
                }
            }
        }
        delegate void CrossThreadSafeDGVclear(Guna2DataGridView ctl);
        static private void CSafeDGVclear(Guna2DataGridView ctl)
        {
            if (ctl.InvokeRequired)
                ctl.Invoke(new CrossThreadSafeDGVclear(CSafeDGVclear), ctl);
            else
            {
                ctl.Rows.Clear();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            data_binding();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            guna2Button1.Enabled = true;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            int index = guna2TextBox1.Text.IndexOf(Environment.NewLine); 
            guna2TextBox1.Text = guna2TextBox1.Text.Remove(0, index + 1);
        }
    }
}
