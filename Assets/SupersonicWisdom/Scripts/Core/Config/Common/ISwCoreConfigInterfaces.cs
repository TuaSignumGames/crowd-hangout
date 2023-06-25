using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace SupersonicWisdomSDK
{
    internal interface ISwConfigManagerState
    {
        EConfigListenerType Timing { get; }
        EConfigStatus Status { get; }
        SwWebRequestError WebRequestError { get; }
    }
    
    /// <summary>
    ///     Remote config repository
    ///     Responsible for fetching and persisting remote config and ab test data
    /// </summary>
    internal interface ISwConfigManager : ISwConfigManagerState
    {
        #region --- Public Methods ---

        /// <summary>
        ///     Add listener for finishing resolving remote config
        ///     The name "onLoaded" is used here because it's eventually
        ///     exposed via `SupersonicWisdom.Api.AddOnLoadedListener`
        /// </summary>
        /// <param name="onLoadedCallback"></param>
        void AddOnLoadedListener(OnLoaded onLoadedCallback);

        /// <summary>
        ///     Did finish process of fetching remote config (success/fail)
        /// </summary>
        /// <returns></returns>
        bool DidResolve { get; }

        /// <summary>
        ///     Fetch the remote config
        ///     After fetch OnLoaded listeners should be called
        ///     Fetch cannot be called before Init
        /// </summary>
        /// <returns></returns>
        IEnumerator Fetch ();

        /// <summary>
        ///     Init the repository
        ///     This method must be called before Fetch to restore any persisted remote config
        /// </summary>
        /// <param name="swLocalConfigProviders"></param>
        void Init(ISwLocalConfigProvider[] swLocalConfigProviders);
        void RemoveOnLoadedListener(OnLoaded onLoadedCallback);
        SwCoreConfig Config { get; }

        #endregion
    }

    internal interface ISwCoreInternalConfig : ISwConfigAccessor
    {
        public Dictionary<string, object> DynamicConfig { get; }
        Dictionary<string, object> AsDictionary();
        SwAbConfig Ab  { get; }
    }


    /// <summary>
    ///     Config values accessor by keys
    ///     It gets the accessible dictionary and the ab config and determine which value to return per key
    /// </summary>
    [PublicAPI]
    public interface ISwConfigAccessor
    {
        #region --- Public Methods ---

        int GetValue(string key, int defaultVal);
        float GetValue(string key, float defaultVal);
        bool GetValue(string key, bool defaultVal);
        string GetValue(string key, string defaultVal);
        
        /// <summary>
        ///     Does key exist in config
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <returns></returns>
        bool HasConfigKey(string key);

        #endregion
    }
}