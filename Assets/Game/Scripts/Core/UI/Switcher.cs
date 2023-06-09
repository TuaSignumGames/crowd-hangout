using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Switcher : MonoBehaviour
{
    public enum SwitcherValueType { Text, Color }

    public SwitcherValueType valueType;
    [Space]
    public Text textSource;
    public Image imageSource;
    [Space]
    public Image indicatorSource;
    public IndicatorSettings indicatorSettings;
    [Space]
    public UnityEvent<int> onValueChanged;

    //private Action<int> onValueChangedIndexed;

    private TextItemData[] textItems;
    private ColorItemData[] colorItems;

    private SwitcherIndicator[] indicators;

    private Color indicatorActiveColor;
    private Color indicatorPassiveColor;

    private float indicatorOffset;

    private int itemIndex;
    private int itemsCount;

    private bool isInitialized;

    public bool Initialized => isInitialized;

    public void Initialize(string[] items, int actualItemIndex = 0)
    {
        if (!isInitialized)
        {
            itemsCount = items.Length;

            textItems = new TextItemData[itemsCount];
            indicators = new SwitcherIndicator[itemsCount];

            indicatorOffset = indicatorSettings.distance * (itemsCount - 1) / 2f;

            indicatorPassiveColor = indicatorSource.color;
            indicatorActiveColor = new Color(indicatorPassiveColor.r, indicatorPassiveColor.g, indicatorPassiveColor.b, indicatorSettings.activeIndicatorOpacity);

            for (int i = 0; i < itemsCount; i++)
            {
                textItems[i] = new TextItemData(i == 0 ? textSource : Instantiate(textSource, textSource.transform.parent), items[i]);
                indicators[i] = new SwitcherIndicator(i == 0 ? indicatorSource : Instantiate(indicatorSource, indicatorSource.transform.parent), indicatorActiveColor, indicatorPassiveColor);

                indicators[i].image.transform.position += new Vector3(i * indicatorSettings.distance - indicatorOffset, 0, 0);
            }

            textSource.transform.parent.gameObject.SetActive(true);

            ActivateItem(actualItemIndex);

            isInitialized = true;
        }
    }

    public void Initialize(Color[] items, int actualItemIndex = 0)
    {
        if (!isInitialized)
        {
            itemsCount = items.Length;

            colorItems = new ColorItemData[itemsCount];
            indicators = new SwitcherIndicator[itemsCount];

            indicatorOffset = indicatorSettings.distance * (itemsCount - 1) / 2f;

            for (int i = 0; i < itemsCount; i++)
            {
                colorItems[i] = new ColorItemData(i == 0 ? imageSource : Instantiate(imageSource, imageSource.transform.parent), items[i]);

                indicatorPassiveColor = new Color(items[i].r, items[i].g, items[i].b, indicatorSource.color.a);
                indicatorActiveColor = new Color(items[i].r, items[i].g, items[i].b, indicatorSettings.activeIndicatorOpacity);

                indicators[i] = new SwitcherIndicator(i == 0 ? indicatorSource : Instantiate(indicatorSource, indicatorSource.transform.parent), indicatorActiveColor, indicatorPassiveColor);

                indicators[i].image.transform.position += new Vector3(i * indicatorSettings.distance - indicatorOffset, 0, 0);
            }

            imageSource.transform.parent.gameObject.SetActive(true);

            ActivateItem(actualItemIndex);

            isInitialized = true;
        }
    }

    public void Increment()
    {
        ActivateItem(itemIndex + 1 == itemsCount ? 0 : itemIndex + 1);
    }

    public void Decrement()
    {
        ActivateItem(itemIndex - 1 < 0 ? itemsCount - 1 : itemIndex - 1);
    }

    private void ActivateItem(int index)
    {
        itemIndex = index;

        for (int i = 0; i < itemsCount; i++)
        {
            if (valueType == SwitcherValueType.Text)
            {
                textItems[i].component.gameObject.SetActive(i == index);
            }
            if (valueType == SwitcherValueType.Color)
            {
                colorItems[i].component.gameObject.SetActive(i == index);
            }

            indicators[i].SetActive(i == index);
        }

        if (onValueChanged != null && isInitialized)
        {
            onValueChanged.Invoke(index);
        }
    }

    public struct TextItemData
    {
        public Text component;
        public string value;

        public TextItemData(Text component, string value)
        {
            this.component = component;
            this.value = value;

            component.text = value;
        }
    }

    public struct ColorItemData
    {
        public Image component;
        public Color value;

        public ColorItemData(Image component, Color value)
        {
            this.component = component;
            this.value = value;

            component.color = value;
        }
    }

    public class SwitcherIndicator
    {
        public Image image;
        public Color activeColor;
        public Color passiveColor;

        public SwitcherIndicator(Image image, Color activeColor, Color passiveColor)
        {
            this.image = image;
            this.activeColor = activeColor;
            this.passiveColor = passiveColor;
        }

        public void SetActive(bool isActive)
        {
            image.color = isActive ? activeColor : passiveColor;
        }
    }

    [Serializable]
    public struct IndicatorSettings
    {
        public float distance;
        [Range(0, 1f)]
        public float activeIndicatorOpacity;
    }
}
