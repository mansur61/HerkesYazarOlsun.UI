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

        public static string _mipApiPath;
        public static string MipApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_mipApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _mipApiPath;
                }
            }
            set { _mipApiPath = value; }
        }

        public static string _generalApiPath;
        public static string GeneralApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_generalApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _generalApiPath;
                }
            }
            set { _generalApiPath = value; }
        }
        public static string _maliIslemlerApiPath;
        public static string MaliIslemlerApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_maliIslemlerApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _maliIslemlerApiPath;
                }
            }
            set { _maliIslemlerApiPath = value; }
        }

        public static string _sevkFisiApiPath;
        public static string SevkFisiApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_sevkFisiApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _sevkFisiApiPath;
                }
            }
            set { _sevkFisiApiPath = value; }
        }

        public static string _webServisApiPath;
        public static string WebServisApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_webServisApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _webServisApiPath;
                }
            }
            set { _webServisApiPath = value; }
        }

        public static string _daimiNezaretciServisApiPath;
        public static string DaimiNezaretciServisApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_daimiNezaretciServisApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _daimiNezaretciServisApiPath;
                }
            }
            set { _daimiNezaretciServisApiPath = value; }
        }

        public static string _belgeApiPath;
        public static string BelgeApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_belgeApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _belgeApiPath;
                }
            }
            set { _belgeApiPath = value; }
        }

        public static string _mebaysApiPath;
        public static string MebaysApiPath
        {
            get
            {
                if (string.IsNullOrEmpty(_mebaysApiPath))
                {
                    return ApiPath;
                }
                else
                {
                    return _mebaysApiPath;
                }
            }
            set { _mebaysApiPath = value; }
        }

        public static string KantarFisExcelSablonPath { get; internal set; }
        public static string TurkiyeGovTrClientId { get; set; }
        public static string TurkiyeGovTrClientSecret { get; set; }
        public static string TurkiyeGovTrRedirectUri { get; set; }
        public static string UygulamaId { get; set; }
        public static string MobilImzaApiPath { get; set; }
    }
}
