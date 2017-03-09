/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

﻿using System;
using System.Windows.Forms;

namespace tcp_moe_client.Classes
{
    public class UI
    {
        public class MsgBox {
            public static void Show(string body, string title="tcp-moe", MessageBoxIcon icon = MessageBoxIcon.None)
            {
                MessageBox.Show(body, title, MessageBoxButtons.OK, icon);
            }
        }
    }
}
