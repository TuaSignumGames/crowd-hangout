namespace SupersonicWisdomSDK
{
    internal class SwLoader : ISwLoader
    {
        #region --- Events ---

        public event OnBlockingLoaderVisibilityChanged OnBlockingLoaderVisibilityChangedEvent;

        #endregion


        #region --- Members ---

        private readonly SwCoreNativeAdapter _coreNativeAdapter;

        #endregion


        #region --- Properties ---

        public bool IsVisible { get; private set; }

        #endregion


        #region --- Construction ---

        public SwLoader(SwCoreNativeAdapter wisdomCoreNativeAdapter)
        {
            _coreNativeAdapter = wisdomCoreNativeAdapter;
        }

        #endregion


        #region --- Public Methods ---

        public bool Hide ()
        {
            if (!_coreNativeAdapter.ToggleBlockingLoader(false)) return false;
            IsVisible = false;

            OnBlockingLoaderVisibilityChangedEvent?.Invoke(IsVisible);

            return true;
        }

        public bool Show ()
        {
            if (!_coreNativeAdapter.ToggleBlockingLoader(true)) return false;
            IsVisible = true;

            OnBlockingLoaderVisibilityChangedEvent?.Invoke(IsVisible);

            return true;
        }

        #endregion
    }
}