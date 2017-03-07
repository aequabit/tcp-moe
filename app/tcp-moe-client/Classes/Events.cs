/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;

namespace tcp_moe_client.Classes
{
    public class Events
    {
        public static void Client_WritePacket(UserClient sender, int size)
        {
            Debug.Log("[client] sent {0} bytes", size);
        }

        public static void Client_StateChanged(UserClient sender, bool connected)
        {
            Debug.Log("[client] {0}connected", !connected ? "dis" : "");
        }

        public static void Client_ReadPacket(UserClient sender, byte[] data)
        {
            Debug.Log("[client] read {0} bytes", data.Length);
            Handlers.Incoming(data);
        }

        public static void Client_ExceptionThrown(UserClient sender, Exception ex)
        {
            Debug.Log("[client] exception: {0}", ex.Message);
            // TODO: add server reporting

            if (!Worker.instance.ClientConnected() && Worker.instance.running)
            {
                UI.MsgBox.Error("Can't connect to server.", "Connection failure");
                Worker.instance.Shutdown();
            }
        }
    }
}
