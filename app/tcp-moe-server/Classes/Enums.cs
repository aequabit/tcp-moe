/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

namespace tcp_moe_server.Classes
{
    public class Enums
    {
        /**
         * Packet headers. Used to detemine the content of a packet.
         */
        public enum PacketHeader : int
        {
            Handshake = 0,
            Error = 1,
            Authentication = 2,
            Products = 3,
            Load = 4
        }

        /**
         * Authentication responses.
         */
        public enum AuthResponse : int
        {
            Success = 0,
            UnknownUser = 1,
            InvalidPassword = 2,
            InvalidHwid = 3,
            Banned = 4,
            Unverified = 5,
            ServerError = 6
        }
    }
}
