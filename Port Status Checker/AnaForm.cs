using Port_Status_Checker.Properties;
using System;
using System.Windows.Forms;

namespace Port_Status_Checker
{
    public partial class AnaForm : Form
    {
        public AnaForm()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Panel1.Controls.Clear();
            // PortCheckForm1'i oluşturun ve panele ekleyin
            PortCheckForm1 PortCheckForm1 = new PortCheckForm1();
            PortCheckForm1.TopLevel = false;
            PortCheckForm1.FormBorderStyle = FormBorderStyle.None;
            PortCheckForm1.Dock = DockStyle.Fill;
            guna2Panel1.Controls.Add(PortCheckForm1);
            PortCheckForm1.Show();
        }

        private void AnaForm_Load(object sender, EventArgs e)
        {
            guna2Panel1.Controls.Clear();
            // PortCheckForm1'i oluşturun ve panele ekleyin
            PortCheckForm1 PortCheckForm1 = new PortCheckForm1();
            PortCheckForm1.TopLevel = false;
            PortCheckForm1.FormBorderStyle = FormBorderStyle.None;
            PortCheckForm1.Dock = DockStyle.Fill;
            guna2Panel1.Controls.Add(PortCheckForm1);
            PortCheckForm1.Show();
        }

        

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Panel1.Controls.Clear();
            // PortCheckForm1'i oluşturun ve panele ekleyin
            Log Log= new Log();
            Log.TopLevel = false;
            Log.FormBorderStyle = FormBorderStyle.None;
            Log.Dock = DockStyle.Fill;
            guna2Panel1.Controls.Add(Log);
            Log.Show();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Panel1.Controls.Clear();
            // PortCheckForm1'i oluşturun ve panele ekleyin
            AboutForm AboutForm = new AboutForm();
            AboutForm.TopLevel = false;
            AboutForm.FormBorderStyle = FormBorderStyle.None;
            AboutForm.Dock = DockStyle.Fill;
            guna2Panel1.Controls.Add(AboutForm);
            AboutForm.Show();
        }
    }
}
