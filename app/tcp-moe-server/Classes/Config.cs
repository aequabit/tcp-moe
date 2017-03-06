namespace tcp_moe_server.Classes
{
    public class Config
    {
        /**
         * Version of the server. Has to match the client's version.
         */
        public static int version = 1;

        /**
         * The server's port. Has to be open.
         */
        public static ushort port = 13370;

        /**
         * URL to the API.
         */
        public static string apiUrl = "http://127.0.0.1:8080";
    }
}
