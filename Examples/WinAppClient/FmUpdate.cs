using AttnSoft.AutoUpdate;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;


namespace WinAppClient
{
    public partial class FmUpdate : Form
    {
        Label label1 = new Label();
        Label label2 = new Label();
        RichTextBox richTextBox1=new RichTextBox();
        ProgressBar progressBar1 = new ProgressBar();
        UpdateContext updateContext;
        ApplicationDelegate<UpdateContext> next;

        float Scaling = 1;
        public FmUpdate()
        {
            InitializeComponent();
            Scaling= GetScreenScalingFactor();

            Stream? fsOpen = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinAppClient.AttnSoft.Update.png");
            if(fsOpen != null)
                this.pictureBox2.Image = System.Drawing.Bitmap.FromStream(fsOpen);

        }
        private float GetScreenScalingFactor()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                float dpiX = g.DpiX;
                return dpiX / 96;
            }
        }

        private void ShowDownloadInfo()
        {
            this.panel1.SuspendLayout();
            this.panel1.Controls.Clear();
            // 
            // progressBar1
            // 
            progressBar1.Name = "progressBar1";
            progressBar1.Height = 15;
            progressBar1.Width = this.panel1.Width;
            progressBar1.TabIndex = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Dock = DockStyle.Bottom;
            
            // 
            // label1
            // 
            label1.AutoSize = false;
            label1.Width = this.Width / 4;
            label1.Name = "label1";
            label1.Text = "正在下载... ";
            label1.Dock = DockStyle.Left;
            // 
            // label2
            // 
            label2.AutoSize = false;
            label2.Name = "label2";
            label2.Width = this.Width/4;
            label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            label2.Text = "0%";
            label2.Dock = DockStyle.Right;

            this.Height = (int)(this.Height*0.45);
        
            this.panel1.Controls.Add(label1);
            this.panel1.Controls.Add(label2);
            this.panel1.Controls.Add(progressBar1);
            this.btnSkipVersion.Visible=this.btnUpdate.Visible = false;

            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
        }
        public void ShowUpdateInfo(ApplicationDelegate<UpdateContext> nextUpdate,UpdateContext context) 
        {
            this.updateContext = context;
            this.next = nextUpdate;

            this.panel1.SuspendLayout();
            this.panel1.Controls.Clear();
            // 
            // label1
            // 
            label1.AutoSize = false;
            label1.Height = (int)(label1.Height*1.1* Scaling);
            label1.Name = "label1";
            label1.Text = $"当前版本:V{updateContext.ClientVersion}";
            label1.Dock = DockStyle.Top;

            // 
            // label2
            // 
            label2.AutoSize = false;
            label2.Height = (int)(label2.Height * 1.1* Scaling);
            label2.Name = "label2";
            label2.Text = $"最新版本:V{updateContext.UpdateVersion?.Version}";
            label2.Dock = DockStyle.Top;

            if (!string.IsNullOrEmpty(updateContext.UpdateVersion?.Desc))
            {
                richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
                richTextBox1.Name = "richTextBox1";
                richTextBox1.BorderStyle = BorderStyle.FixedSingle;
                richTextBox1.Text = updateContext.UpdateVersion?.Desc;

                Label label3 = new Label();
                label3.AutoSize = false;
                label3.Height = (int)(label3.Height * 1.2* Scaling);
                label3.Name = "label3";
                label3.Text = "发版说明";
                label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                label3.Dock = DockStyle.Top;
                this.panel1.Controls.Add(richTextBox1);
                this.panel1.Controls.Add(label3);
                //this.Height = 400;
            }
            this.btnSkipVersion.Visible = updateContext.UpdateVersion.CanSkip;
            this.panel1.Controls.Add(label2);
            this.panel1.Controls.Add(label1);

            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.Show();
            if (updateContext.UpdateVersion.IsForcibly)
            {
                btnUpdate_Click(null,null);
            }
        }

        int lastReported = 0;
        int threshold = 5; // 当进度增加至少1%时更新一次UI

        public void ChangProgress(int value)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker deleg = delegate { ChangProgress(value); };
                this.Invoke(deleg);
                return;
            }
            //// 当进度达到阈值或任务完成时更新进度
            if (value - lastReported >= threshold || value == 100)
            {
                lastReported = value;
                this.progressBar1.Value = value;
                this.label2.Text = $"{value}%";
                this.Refresh();
                Application.DoEvents();
                System.Threading.Thread.Sleep(300);//为了演示效果，这里停顿一下
            }

        }
        public void DownloadComplet()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker deleg = delegate { DownloadComplet(); };
                this.Invoke(deleg);
                return;
            }
            this.label1.Text = "解压安装...";
            this.label2.Text = "100%";
            this.progressBar1.Value = 100;
            this.Refresh();
            Application.DoEvents();
            System.Threading.Thread.Sleep(500);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ShowDownloadInfo();
            Application.DoEvents();
            System.Threading.Thread.Sleep(200);
            if (next != null)
            {
                next.Invoke(updateContext);
            }
        }

        private void btnSkipVersion_Click(object sender, EventArgs e)
        {
            updateContext.SkipVersion();
            this.Hide();
        }
    }
}
