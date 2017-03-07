/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace tcp_moe_client.Classes
{
    class Debug
    {
        private static List<string> entries = new List<string>();

        public static void Log(string format, params object[] args)
        {
            entries.Add(String.Format("[{0}] {1}", DateTime.Now.ToString("dd.MM.yyyy, HH:mm:ss"), String.Format(format, args)));
        }

        public static List<string> GetLog()
        {
            return entries;
        }
    }
}
