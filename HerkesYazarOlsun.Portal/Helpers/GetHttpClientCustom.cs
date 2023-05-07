namespace HerkesYazarOlsun.Portal.Helpers
{
    public class GetHttpClientCustom
    {
        public static SocketsHttpHandler _socketsHttpHandler;

        public GetHttpClientCustom()
        {
            if (_socketsHttpHandler == null)
            {
                _socketsHttpHandler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    MaxConnectionsPerServer = 100
                };
            }
        }

        public HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient(_socketsHttpHandler);
            //client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }
    }
}
