#if SW_STAGE_STAGE1_OR_ABOVE
using System.Collections.Generic;

#if SW_STAGE_STAGE1_OR_ABOVE

namespace SupersonicWisdomSDK
{
    internal class SwStage1ConfigManager : SwCoreConfigManager
    {
        #region --- Members ---

        private readonly SwDeepLinkHandler _deepLinkHandler;

        private List<ISwStage1ConfigListener> _listeners;

        #endregion


        #region --- Construction ---

        public SwStage1ConfigManager(ISwSettings settings, SwCoreUserData coreUserData, SwCoreTracker tracker, SwStage1NativeAdapter swStage1NativeAdapter, SwDeepLinkHandler deepLinkHandler) : base(settings, coreUserData, tracker, swStage1NativeAdapter)
        {
            _deepLinkHandler = deepLinkHandler;
        }

        #endregion


        #region --- Public Methods ---

        public virtual void SetupListeners(List<ISwStage1ConfigListener> listeners)
        {
            _listeners = listeners;
        }

        #endregion


        #region --- Private Methods ---

        protected override SwRemoteConfigRequestPayload CreatePayload()
        {
            var payload = base.CreatePayload();
            payload.testId = SwInfra.KeyValueStore.GetString(SwStage1DeepLinkConstants.TestIdStorageKey);

            return payload;
        }

        protected override void OnConfigReady()
        {
            base.OnConfigReady();

            TryLoadDeepLinkConfig();
        }
        
         // todo: Remove in 7.4
        protected override void AddStaticPredefinedKeys()
        {
            base.AddStaticPredefinedKeys();
            
            var allKeys = new HashSet<string>()
            {
                "appsFlyerDomain",
            };

            _predefinedKeys.UnionWith(allKeys);
        }

        private void TryLoadDeepLinkConfig()
        {
            Config.DynamicConfig.SwMerge(true, SwConfigUtils.ResolveDeepLinkConfig(_deepLinkHandler.DeepLinkParams));
        }

        #endregion
    }
}
#endif
#endif