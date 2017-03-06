namespace tcp_moe_client.Classes
{
    public class Config
    {
        /**
         * Version of the client. Has to match the server's version.
         */
        public static int version = 1;

        /**
         * Server network credentials.
         */
        public static string host = "127.0.0.1";
        public static ushort port = 13370;
    }
}
