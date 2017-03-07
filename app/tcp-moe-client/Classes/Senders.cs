/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

﻿using System;

namespace tcp_moe_client.Classes
{
    public class Senders : Enums
    {
        private static Pack pack = new Pack();

        public static void Handshake(byte[] keys)
        {
            Worker.instance.Send(
                pack.Serialize(
                    (int)PacketHeader.Handshake,
                    keys
                )
            );
        }

        public static void Authentication(Forms.Authentication auth, string username, string password, string hwid)
        {
            Worker.instance.Send(
                pack.Serialize(
                    (int)PacketHeader.Authentication,
                    username,
                    password,
                    hwid
                )
            );
        }

        public static void Products()
        {
            Worker.instance.Send(
                pack.Serialize(
                    (int)PacketHeader.Products
                )
            );
        }

        public static void Load(string product)
        {
            Worker.instance.Send(
                pack.Serialize(
                    (int)PacketHeader.Load,
                    product
                )
            );
        }
    }
}
