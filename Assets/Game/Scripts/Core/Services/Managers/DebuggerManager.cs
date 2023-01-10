using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerManager : Service<DebuggerManager>
{
    public GameObject debuggerMenuButton;
    [Space]
    public RectTransform _viewTransform;
    public VerticalLayoutGroup _viewLayoutGroup;
    public UIIndicator _viewStateIndicator;
    [Space]
    public ContextWindow[] _debuggerContextWindows;
    [Space]
    public ProfilerSettings profilerSettings;
    public NetworkHubSettings networkHubSettings;
    public ConsoleSettings consoleSettings;

    private ProfilerHandler _profilerHandler;
    private NetworkHubHandler _networkHubHandler;
    private ConsoleHandler _consoleHandler;

    public ProfilerHandler Profiler => _profilerHandler;
    public ConsoleHandler Console => _consoleHandler;

    public override void Initialize()
    {
        _profilerHandler = new ProfilerHandler(profilerSettings);
        _networkHubHandler = new NetworkHubHandler(networkHubSettings);
        _consoleHandler = new ConsoleHandler(consoleSettings);

        StartCoroutine(ContextWindowsInitializingCoroutine(_consoleHandler.OnRectTransformsCalculated, base.Initialize));
    }

    private void Update()
    {
        if (_profilerHandler != null)
        {
            _profilerHandler.Iterate();
        }
    }

    public void RefreshView()
    {
        _viewLayoutGroup.enabled = false;
        _viewLayoutGroup.enabled = true;
    }

    public void SwitchView()
    {
        _viewTransform.gameObject.SetActive(!_viewTransform.gameObject.activeSelf);
        _viewStateIndicator.SetState(_viewTransform.gameObject.activeSelf ? IndicatorStateType.Maximized : IndicatorStateType.Minimized);
    }

    public float GetViewFreeHeight()
    {
        // TODO Calculate free space

        return 0;
    }

    private IEnumerator ContextWindowsInitializingCoroutine(Action preinitializationCallback = null, Action postinitializationCallback = null)
    {
        yield return null;

        if (preinitializationCallback != null)
        {
            preinitializationCallback();
        }

        for (int i = 0; i < _debuggerContextWindows.Length; i++)
        {
            _debuggerContextWindows[i].Initialize();
        }

        if (postinitializationCallback != null)
        {
            postinitializationCallback();
        }

        debuggerMenuButton.SetActive(true);
    }

    public class ProfilerHandler
    {
        private ProfilerSettings _settings;

        private float _fpsCalculationPeriod;

        private float _fpsPreviousOutputTime;
        private float _fpsFrameValue;
        private float _fpsValuesSum;
        private int _fpsValuesCount;
        private int _fpsOutputValue;

        public ProfilerHandler(ProfilerSettings settings)
        {
            _settings = settings;

            _fpsCalculationPeriod = _settings.fpsCalculationPeriod;
        }

        public void Iterate()
        {
            if (Time.time > _fpsPreviousOutputTime + _fpsCalculationPeriod)
            {
                _fpsOutputValue = (int)(_fpsValuesSum / _fpsValuesCount);

                _fpsValuesSum = 0;
                _fpsValuesCount = 0;

                _fpsPreviousOutputTime = Time.time;

                UpdateFpsText(_fpsOutputValue);
                UpdateStatusIndicator();
            }
            else
            {
                _fpsFrameValue = 1 / Time.unscaledDeltaTime;

                if (_fpsFrameValue > 0)
                {
                    _fpsValuesSum += _fpsFrameValue;
                    _fpsValuesCount++;
                }
            }
        }

        private void UpdateFpsText(int value)
        {
            _settings.fpsValueText.text = value.ToString();
        }

        private void UpdateStatusIndicator()
        {
            _settings.performanceStatusIndicator.SetState(_fpsOutputValue < _settings.lowFpsValue ? IndicatorStateType.Fail : (_fpsOutputValue < _settings.mediumFpsValue ?
                IndicatorStateType.Warning : IndicatorStateType.Success));
        }
    }

    public class NetworkHubHandler
    {
        private NetworkHubSettings _settings;

        public NetworkHubHandler(NetworkHubSettings settings)
        {
            _settings = settings;

            _settings.connectionButton.Initialize();

            NetworkHub.OnInitialized += SetButtonForConnection;
        }

        private void SetButtonForConnection()
        {
            _settings.connectionButton.SetEvent(() => RequestConnection(), _settings.connectionRequestTitle);

            if (NetworkHub.OnInitialized != null)
            {
                NetworkHub.OnInitialized = null;
            }
        }

        private void SetButtonForDisconnection()
        {
            _settings.connectionButton.SetEvent(() => RequestDisconnection(), _settings.disconnectionRequestTitle);
        }

        private void RequestConnection()
        {
            _settings.connectionButton.Interactable = false;

            _settings.connectionStatusIndicator.SetState(IndicatorStateType.Waiting);

            NetworkHub.Instance.Connect(_settings.serverAddressInputField.text, int.Parse(_settings.serverPortInputField.text), OnConnected, OnConnectionFailed);
        }

        private void RequestDisconnection()
        {
            _settings.connectionButton.Interactable = false;

            NetworkHub.Instance.Disconnect(OnDisconnected);
        }

        private void OnConnected()
        {
            SetButtonForDisconnection();

            _settings.connectionButton.Interactable = true;

            _settings.connectionStatusIndicator.SetState(IndicatorStateType.Success);
        }

        private void OnDisconnected()
        {
            SetButtonForConnection();

            _settings.connectionButton.Interactable = true;

            _settings.connectionStatusIndicator.SetState(IndicatorStateType.Neutral);
        }

        private void OnConnectionFailed()
        {
            _settings.connectionButton.Interactable = true;

            _settings.connectionStatusIndicator.SetState(IndicatorStateType.Fail);
        }
    }

    public class ConsoleHandler
    {
        private ConsoleSettings _settings;

        private Queue<LogMessageData> _logsQueue;

        private LogMessage _logMessage;
        private LogMessage _previousLogMessage;

        private Vector2 _defaultLogCellSize;
        private Vector2 _logViewportSize;

        private Color _defaultCellColor;

        private float _logMessageBottomCoordY;
        private float _logsContainerCeilCoordY;

        private int _instantiatedLogsCount;

        private int _totalLogsCount;
        private int _warningLogsCount;
        private int _errorLogsCount;

        public ConsoleHandler(ConsoleSettings settings)
        {
            _settings = settings;

            _logsQueue = new Queue<LogMessageData>();

            _logMessage = new LogMessage(_settings.logCell);

            _settings.contextWindow.OnMaximized += () => FitLogContainer(true);

            Application.logMessageReceived += AddLog;
        }

        public void OnRectTransformsCalculated()
        {
            _defaultLogCellSize = _logMessage.cellTransform.rect.size;
            _logViewportSize = _settings.logsViewportTransform.rect.size;

            _defaultCellColor = _settings.logCell.GetComponent<Image>().color;

            _settings.logsContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _logViewportSize.y);

            _settings.logsContainerTransform.anchorMin = new Vector2(0, 1);
            _settings.logsContainerTransform.anchorMax = new Vector2(1, 1);
        }

        private void AddLog(string text, string stackTrace, LogType logType)
        {
            _logsQueue.Enqueue(new LogMessageData(text, logType));

            _totalLogsCount++;

            switch (logType)
            {
                case LogType.Log:
                    _settings.neutralLogsCountIndicator.SetValue(_totalLogsCount);
                    break;

                case LogType.Warning:
                    _settings.warningLogsCountIndicator.SetValue(++_warningLogsCount);
                    break;

                case LogType.Error:
                    _settings.errorLogsCountIndicator.SetValue(++_errorLogsCount);
                    break;
            }

            while (Instance && _logsQueue.Count > 0)
            {
                InstantiateLogMessageToConsoleView(_logsQueue.Dequeue());
            }
        }

        private void InstantiateLogMessageToConsoleView(LogMessageData logMessageData)
        {
            if (_instantiatedLogsCount > 0)
            {
                _logMessage = new LogMessage(Instantiate(Instance.consoleSettings.logCell, _settings.logsContainerTransform));

                _logMessage.cellTransform.anchoredPosition = new Vector2(0, _previousLogMessage.cellTransform.anchoredPosition.y - _previousLogMessage.cellTransform.rect.height - _settings.logsSpacing);
            }

            _logMessage.logText.text = logMessageData.text;

            if (logMessageData.logType != LogType.Log)
            {
                Image logCellImage = _logMessage.cellTransform.GetComponent<Image>();

                switch (logMessageData.logType)
                {
                    case LogType.Warning:
                        logCellImage.color = Color.yellow.Transparent(_defaultCellColor.a);
                        break;

                    case LogType.Error:
                        logCellImage.color = Color.red.Transparent(_defaultCellColor.a);
                        break;
                }
            }

            _logMessage.cellTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Ceil((float)logMessageData.text.Length / _settings.charactersPerLine) * _defaultLogCellSize.y);

            FitLogContainer();

            _previousLogMessage = _logMessage;

            _instantiatedLogsCount++;

            _settings.contextWindow.UpdateIndicators();
        }

        private void FitLogContainer(bool forced = false)
        {
            _logMessageBottomCoordY = _logMessage.cellTransform.anchoredPosition.y - _logMessage.cellTransform.rect.height;

            if (-_logMessageBottomCoordY > _logViewportSize.y)
            {
                _settings.logsContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -_logMessageBottomCoordY);

                _logsContainerCeilCoordY = _settings.logsContainerTransform.rect.height - _settings.logsViewportTransform.rect.height;

                if (_settings.logsContainerTransform.anchoredPosition.y > _logsContainerCeilCoordY - (_previousLogMessage.cellTransform.rect.height + _settings.logsSpacing) * 2f || forced)
                {
                    _settings.logsContainerTransform.anchoredPosition = new Vector2(0, _logsContainerCeilCoordY);
                }
            }
        }

        private struct LogMessage
        {
            public Text logText;
            public RectTransform cellTransform;

            public LogMessage(GameObject instance)
            {
                logText = instance.GetComponentInChildren<Text>();
                cellTransform = instance.GetComponent<RectTransform>();
            }
        }

        private struct LogMessageData
        {
            public string text;
            public LogType logType;

            public LogMessageData(string text, LogType logType)
            {
                this.text = text;
                this.logType = logType;
            }
        }
    }

    [Serializable]
    public struct ProfilerSettings
    {
        public Text fpsValueText;
        [Space]
        public float fpsCalculationPeriod;
        [Space]
        public float mediumFpsValue;
        public float lowFpsValue;
        [Space]
        public UIIndicator performanceStatusIndicator;
    }

    [Serializable]
    public struct NetworkHubSettings
    {
        public InputField serverAddressInputField;
        public InputField serverPortInputField;
        [Space]
        public QuickButton connectionButton;
        public string connectionRequestTitle;
        public string disconnectionRequestTitle;
        [Space]
        public UIIndicator connectionStatusIndicator;
    }

    [Serializable]
    public struct ConsoleSettings
    {
        public ContextWindow contextWindow;
        [Space]
        public GameObject logCell;
        [Space]
        public RectTransform logsContainerTransform;
        public RectTransform logsViewportTransform;
        [Space]
        public float logsSpacing;
        public float viewFittingThreshold;
        [Space]
        public int charactersPerLine;
        [Space]
        public UIIndicator neutralLogsCountIndicator;
        public UIIndicator warningLogsCountIndicator;
        public UIIndicator errorLogsCountIndicator;
    }
}

