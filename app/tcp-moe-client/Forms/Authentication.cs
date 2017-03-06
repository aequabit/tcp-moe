using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using tcp_moe_client.Classes;

namespace tcp_moe_client.Forms
{
    public partial class Authentication : Form
    {
        public bool terminate = true;

        private Worker worker;

        public Authentication(Worker worker)
        {
            this.worker = worker;

            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Senders.Authentication(this, txtUsername.Text.Trim(), txtPassword.Text, Local.HWID);
        }

        public void Unlock()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Enabled = true;
            });
        }

        private void frmAuthentication_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(170, 0, 0), 16), 0, 0, this.Bounds.Right, 0);
        }

        private void Authentication_Load(object sender, EventArgs e)
        {
        }

        private void Authentication_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (terminate)
                worker.Shutdown();
        }

        public void Kill()
        {
            terminate = false;
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }

        private void Credentials_TextChanged(object sender, EventArgs e)
        {
            if (txtUsername.Text.Length > 0 && txtPassword.Text.Length > 0)
                btnLogin.Enabled = true;
            else
                btnLogin.Enabled = false;
        }
    }
}
