/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

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
    public partial class Loader : Form
    {
        public bool terminate = true;

        private Worker worker;

        private List<Product> products = new List<Product>();

        public Product selected;

        public Loader(Worker worker)
        {
            this.worker = worker;

            InitializeComponent();
        }

        private void tmrLoad_Tick(object sender, EventArgs e)
        {
            if (prgLoad.Value < 100)
                prgLoad.Value++;
            else
                tmrLoad.Stop();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            tmrLoad.Enabled = true;
            this.Enabled = false;

            selected = products[lstCheatList.SelectedIndex];
            Senders.Load(lstCheatList.SelectedItem.ToString());
        }

        private void frmLoader_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(170, 0, 0), 16), 0, 0, this.Bounds.Right, 0);
        }

        private void frmLoader_Load(object sender, EventArgs e)
        {
            lblUser.Text = lblUser.Text.Replace("{{ USERNAME }}", Session.Username);
            lblRank.Text = lblRank.Text.Replace("{{ RANK }}", Session.Rank);
            Senders.Products();
        }

        private void lstCheatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Product selected = products[lstCheatList.SelectedIndex];
            lblInfo.Text = selected.description + Environment.NewLine + "Expiry: " + selected.expiry;
            btnLoad.Enabled = true;
        }

        public void InjectionFinished()
        {
            this.Invoke((MethodInvoker)delegate
            {
                UI.MsgBox.Info("Successfully loaded " + lstCheatList.SelectedItem.ToString() + "!", "Injection successful");
                Worker.instance.Shutdown();
            });
        }

        public void AddProduct(Product product)
        {
            this.Invoke((MethodInvoker)delegate
            {
                products.Add(product);
                lstCheatList.Items.Add(product.name);
            });
        }

        private void Loader_FormClosing(object sender, FormClosingEventArgs e)
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
    }
}
