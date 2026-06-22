namespace HerkesYazarOlsun.Portal.Helpers
{
    public class AppSettings
    {
        public static string SecretKey { get; set; }
        public static string ApiPath { get; set; }

        public static string _servisApiPath;
        public static string ServisApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_servisApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _servisApiPath;
                }
            }
            set { _servisApiPath = value; }
        }








    }
}
