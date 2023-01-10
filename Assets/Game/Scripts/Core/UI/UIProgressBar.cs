using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressBar : UIElement
{
    public static UIProgressBar Instance;

    [Space]
    [SerializeField] private Text _currentLevelText;
    [SerializeField] private Text _nextLevelText;
    [Space]
    [SerializeField] private Image _fillingImage;

    private float _targetValue;

    public override void Awake()
    {
        base.Awake();

        Instance = this;

        GameManager.OnGameSceneLoaded += Reset;
    }

    private void LateUpdate()
    {
        _fillingImage.fillAmount = Mathf.Lerp(_fillingImage.fillAmount, _targetValue, 0.1f);
    }

    public void Initialize(int levelNumber)
    {
        _currentLevelText.text = levelNumber.ToString();
        _nextLevelText.text = (levelNumber + 1).ToString();
    }

    public void SetProgressValue(float value)
    {
        _targetValue = value;
    }

    private void Reset()
    {
        _targetValue = 0;

        _fillingImage.fillAmount = 0;
    }
}
