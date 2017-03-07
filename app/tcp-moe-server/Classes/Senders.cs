/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tcp_moe_server.Classes
{
    class Senders : Enums
    {
        private static Pack pack = new Pack();

        public static void Handshake(ServerClient client, byte[] publicKey, int version)
        {
            Program.Send(
                client,
                pack.Serialize(
                    (int)PacketHeader.Handshake,
                    publicKey,
                    version
                )
            );
        }

        public static void Error(ServerClient client, string message)
        {
            Program.Send(
                client,
                pack.Serialize(
                    (int)PacketHeader.Error,
                    message
                )
            );
        }

        public static void Authentication(ServerClient client, AuthResponse response, string username="", string rank="")
        {
            Program.Send(
                client,
                pack.Serialize(
                    (int)PacketHeader.Authentication,
                    (int)response,
                    username,
                    rank
                )
            );
        }

        public static void Products(ServerClient client, string json)
        {
            Program.Send(
                client,
                pack.Serialize(
                    (int)PacketHeader.Products,
                    json
                )
            );
        }

        public static void Load(ServerClient client, byte[] product)
        {
            Program.Send(
                client,
                pack.Serialize(
                    (int)PacketHeader.Load,
                    product
                )
            );
        }
    }
}
