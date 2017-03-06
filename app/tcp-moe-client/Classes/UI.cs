using System;
using System.Windows.Forms;

namespace tcp_moe_client.Classes
{
    public class UI
    {
        public class MsgBox {
            public static void Info(string body, string title="tcp-moe")
            {
                MessageBox.Show(body, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            public static void Warning(string body, string title = "tcp-moe")
            {
                MessageBox.Show(body, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            public static void Error(string body, string title = "tcp-moe")
            {
                MessageBox.Show(body, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
