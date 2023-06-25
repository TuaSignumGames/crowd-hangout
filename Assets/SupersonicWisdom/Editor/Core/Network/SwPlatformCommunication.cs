using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwPlatformCommunication
    {
        #region --- Public Methods ---

        public static Dictionary<string, string> CreateAuthorizationHeadersDictionary ()
        {
            return string.IsNullOrWhiteSpace(SwAccountUtils.AccountToken) ? new Dictionary<string, string>() : new Dictionary<string, string>
            {
                { "authorization", "Bearer " + SwAccountUtils.AccountToken }
            };
        }

        #endregion


        #region --- Inner Classes ---

        internal static class URLs
        {
            #region --- Constants ---
            
            private const string BASE = "https://partners.super-api.supersonic.com/v1/";
            
            private const string BASE_PARTNERS = BASE + "partners/";
            internal const string LOGIN = BASE_PARTNERS + "login";

            private const string BASE_WISDOM = BASE + "wisdom/";
            internal const string TITLES = BASE_WISDOM + "titles";
            internal const string CURRENT_STAGE_API = BASE_WISDOM + "current-stage";
            internal const string DOWNLOAD_WISDOM_PACKAGE = BASE_WISDOM + "download-package";
            internal const string WISDOM_PACKAGE_MANIFEST = BASE_WISDOM + "package-manifest";
            
            #endregion
        }

        #endregion
    }
}