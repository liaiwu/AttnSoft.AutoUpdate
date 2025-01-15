using AttnSoft.AutoUpdate;
using AttnSoft.AutoUpdate.Middlewares;
using System.Reflection;

namespace WinAppClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.label1.Text = "�������ݰ汾 V"+ myUpdateApp.UpContext.ClientVersion.ToString();
            this.label3.Text = "�ļ��汾 V" + Assembly.GetEntryAssembly().GetName().Version.ToString();
            myUpdateApp.UpContext.OnFindNewVersion += OnFindNewVersion;

        }
        private async Task OnFindNewVersion(ApplicationDelegate<UpdateContext> next, UpdateContext context)
        {
            ShowNewVersion(context);
            await next(context);
        }
        private void ShowNewVersion(UpdateContext context)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker deleg = delegate { ShowNewVersion(context); };
                this.BeginInvoke(deleg);
                return;
            }
            MessageBox.Show("�����°汾:"+ context.UpdateVersion.Version.ToString());
        }
        private void button1_Click(object sender, EventArgs e)
        {
            myUpdateApp.StartUpdate();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //��Ϊ�������ֶ��������
            if (CheckCompletionMiddleware.CheckIsCompletion())
            {
                MessageBox.Show("�������!");
            }
        }

    }
}
