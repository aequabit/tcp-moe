/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InjectionLibrary;
using JLibrary.PortableExecutable;

namespace tcp_moe_client.Classes
{
    public class Injector
    {
        public static void Inject(string process, byte[] data)
        {
            //   process = i.e. csgo.exe
            //   data = bytes of .dll

            Process[] processes = Process.GetProcesses(process);

            // Create Portable Executable
            var dll = new PortableExecutable(data);
            // Create our Mapper
            var mapper = new ManualMap();
            // Inject it with our Mapper
            mapper.Inject(dll, processes[0].Id);

            // call this when the injection is finished
            Worker.instance.loader.InjectionFinished();
        }
    }
}
