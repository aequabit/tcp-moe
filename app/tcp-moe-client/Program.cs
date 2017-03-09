/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Reflection;
using System.Windows.Forms;
using tcp_moe_client.Classes;

namespace tcp_moe_client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /**
             * Check if the loader is already running.
             */
            bool result;
            var mutex = new System.Threading.Mutex(true, "tcp-moe-client", out result);
            if (!result)
            {
                UI.MsgBox.Show("Only one instance of tcp-moe can be running at a time.", "Only one instance", MessageBoxIcon.Error);
                return;
            }

            /**
             * Load the embedded assemblies.
             */
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            EmbeddedAssembly.Load("tcp_moe_client.Assemblies.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var worker = new Classes.Worker();
            Application.Run(worker.Satisfy());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
