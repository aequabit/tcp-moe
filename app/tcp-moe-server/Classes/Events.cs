/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;

namespace tcp_moe_server.Classes
{
    public class Events
    {
        public static void Server_StateChanged(ServerListener sender, bool listening)
        {
            if (listening)
                Helper.Log("[server] started listening on *:{0}", Config.port);
            else
                Helper.Log("[server] stopped listening");
        }

        public static void Server_ExceptionThrown(ServerListener sender, Exception ex)
        {
            Helper.Log("[server] exception: {0}", ex.Message);
        }

        public static void Server_ClientWritePacket(ServerListener sender, ServerClient client, int size)
        {
            Helper.Log("[server] {0}: sent {1} bytes", client.EndPoint.ToString(), size);
        }

        public static void Server_ClientStateChanged(ServerListener sender, ServerClient client, bool connected)
        {
            Helper.Log("[server] {0}: {1}connected", client.EndPoint.ToString(), !connected ? "dis" : "");

            if (connected)
            {
                client.UserState = new UserData();
                Senders.Handshake(client, Program.GetPublicKey(), Config.version);
            }

            Console.Title = sender.Clients.Length + " clients | tcp-moe - Server";
        }

        public static void Server_ClientReadPacket(ServerListener sender, ServerClient client, byte[] data)
        {
            Helper.Log("[server] {0}: read {1} bytes", client.EndPoint.ToString(), data.Length);
            Handlers.Incoming(client, data);
        }

        public static void Server_ClientExceptionThrown(ServerListener sender, ServerClient client, Exception ex)
        {
            if (client.Connected)
                Helper.Log("[client] {0}: exception: {1}", client.EndPoint.ToString(), ex.Message);
        }
    }
}
