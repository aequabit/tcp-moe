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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcp_moe_client.Forms
{
    public partial class Debug : Form
    {
        public Debug()
        {
            InitializeComponent();
        }

        private void tmrReload_Tick(object sender, EventArgs e)
        {
            lstLog.DataSource = null;
            lstLog.Items.Clear();
            lstLog.DataSource = Classes.Debug.GetLog();

        }
    }
}
