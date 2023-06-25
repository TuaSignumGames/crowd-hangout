using SupersonicWisdomSDK.Editor;

namespace SupersonicWisdomSDK
{
    internal static class SwNativeApiFactory
    {
        #region --- Public Methods ---

        public static ISwNativeApi GetInstance()
        {
            if (SwUtils.IsRunningOnAndroid())
            {
                return new SwNativeAndroidApi(new SwNativeAndroidBridge());
            }

            if (SwUtils.IsRunningOnIos())
            {
                return new SwNativeIosApi(new SwNativeIosBridge());
            }

            if (SwUtils.IsRunningOnEditor())
            {
                return new SwNativeEditorApi(null);
            }

            return new SwNativeUnsupportedApi(null);
        }

        #endregion
    }
}