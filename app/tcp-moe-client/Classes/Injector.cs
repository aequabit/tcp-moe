using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tcp_moe_client.Classes
{
    public class Injector
    {
        public static void Inject(string process, byte[] data)
        {
            // implement your own injection method
            //   process = i.e. csgo.exe
            //   data = bytes of .dll

            // call this when the injection is finished
            Worker.instance.loader.InjectionFinished();
        }
    }
}
