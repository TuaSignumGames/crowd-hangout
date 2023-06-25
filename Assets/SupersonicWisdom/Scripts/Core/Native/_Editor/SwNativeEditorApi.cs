using System.Collections;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal class SwNativeEditorApi : SwNativeApi
    {
        #region --- Constants ---

        private const string IS_AVAILABLE = "isAvailable";

        #endregion


        #region --- Members ---

        private OnConnectivityStatusChanged _connectivityCallback;

        private OnWebResponse _webResponseCallback;

        #endregion
        
        // Todo: Will be used as part of Native Simulator feature
        public static SwNativeEditorApi Instance { get; private set; }


        #region --- Construction ---

        public SwNativeEditorApi(SwNativeBridge nativeBridge) : base(nativeBridge)
        {
            Instance = this;
        }

        #endregion


        #region --- Public Methods ---

        public override void AddSessionEndedCallback(OnSessionEnded callback)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | AddSessionEndedCallback");
        }

        public override void AddSessionStartedCallback(OnSessionStarted callback)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | AddSessionStartedCallback");
        }

        public override void Destroy()
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | Destroy");
        }

        public override string GetAdvertisingId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
        
        public override string GetAppInstallSource()
        {
            return "";
        }

        public override string GetConnectionStatus()
        {
            SwInfra.Logger.Log($"SwNativeEditorApi | {nameof(GetConnectionStatus)}");

            return "{\"" + IS_AVAILABLE + "\":true}";
        }

        public override string GetOrganizationAdvertisingId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public override IEnumerator Init(SwNativeConfig configuration)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | Init | {configuration}");

            // Uncomment below line in case you want to toggle change connection status
            // SwInfra.CoroutineService.StartCoroutine(ChangeConnectionStatusRepeating());
            
            yield break;
        }

        // Todo: Will be used as part of Native Simulator feature
        public static void TurnOnConnection()
        {
            Instance?._connectivityCallback?.Invoke( "{\"" + IS_AVAILABLE + "\":true}");
        }
        
        // Todo: Will be used as part of Native Simulator feature
        public static void TurnOffInternet()
        {
            Instance?._connectivityCallback?.Invoke( "{\"" + IS_AVAILABLE + "\":false}");
        }

        public override void InitializeSession(SwEventMetadataDto metadata)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | InitializeSession | Uuid: {metadata.uuid}, CustomInstallationId: {metadata.swInstallationId}");
        }

        public override bool IsSupported()
        {
            return false;
        }

        public override void RemoveSessionEndedCallback(OnSessionEnded callback)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | RemoveSessionEndedCallback");
        }

        public override void RemoveSessionStartedCallback(OnSessionStarted callback)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | RemoveSessionStartedCallback");
        }

        public override void SendRequest(string requestJsonString)
        {
            var request = JsonUtility.FromJson<SwRemoteRequest>(requestJsonString);
            SwInfra.CoroutineService.StartCoroutine(SendRequestRoutine(request.key, request.url, request.headers, request.body));
        }

        public override void AddServerCallbacks(OnWebResponse callback)
        {
            _webResponseCallback = callback;
        }

        public override void RemoveServerCallbacks(OnWebResponse callback)
        {
            _webResponseCallback = callback;
        }

        public override bool ToggleBlockingLoader(bool shouldPresent)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | ToggleBlockingLoader(" + shouldPresent + ") - returning `false`");

            return false;
        }

        public override void RequestRateUsPopup()
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | RequestRateUsPopup");
        }

        public override void AddConnectivityCallbacks(OnConnectivityStatusChanged callback)
        {
            _connectivityCallback = callback;
        }

        public override void RemoveConnectivityCallbacks(OnConnectivityStatusChanged callback)
        {
            SwInfra.Logger.Log("SwNativeEditorApi | RemoveConnectivityCallbacks");
        }

        public override void UpdateMetadata(SwEventMetadataDto metadata)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | UpdateMetadata | Uuid: {metadata.uuid}, CustomInstallationId: {metadata.swInstallationId}");
        }

        public override void UpdateWisdomConfiguration(SwNativeConfig configuration)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | UpdateWisdomConfiguration | configuration: {configuration}");
        }

        #endregion


        #region --- Private Methods ---

        public override void ClearDelegates()
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | clearDelegates");
        }

        public override void RemoveAllSessionCallbacks()
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | RemoveAllSessionCallbacks");
        }

        public override void TrackEvent(string eventName, string customsJson, string extraJson)
        {
            SwInfra.Logger.Log($"{nameof(SwNativeEditorApi)} | TrackEvent | {eventName} | {customsJson} | {extraJson}");
        }

        private IEnumerator ChangeConnectionStatusRepeating()
        {
            var isAvailable = true;

            while (true)
            {
                yield return new WaitForSeconds(5);

                isAvailable = !isAvailable;
                _connectivityCallback?.Invoke(isAvailable ? "{\"" + IS_AVAILABLE + "\":true}" : "{\"" + IS_AVAILABLE + "\":false}");
            }
        }

        private IEnumerator SendRequestRoutine(string key, string url, string headers, string body)
        {
            var client = new SwUnityWebRequestClient();

            yield return new WaitForSeconds(1);

            var response = new SwWebResponse
            {
                key = key,
            };

            yield return SwInfra.CoroutineService.StartCoroutine(client.Post(url, body, response, 10, null, true));

            if (response.DidSucceed)
            {
                if (response.data != null)
                {
                    _webResponseCallback?.Invoke(JsonUtility.ToJson(response));
                }
                else
                {
                    SwInfra.Logger.LogError("Skipping callback");
                }
            }
            else
            {
                _webResponseCallback?.Invoke(JsonUtility.ToJson(response));
            }
        }

        #endregion
    }
}