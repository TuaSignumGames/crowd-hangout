using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DrawConditionAttribute))]
public class DrawConditionPropertyDrawer : PropertyDrawer
{
    private DrawConditionAttribute _drawConditionAttribute;

    private object _otherPropertyValue;

    private float _propertyHeight;

    private int _propertiesArraySize;

    private bool _isArrayProperty;
    private bool _isPropertyVisible;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _propertyHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _drawConditionAttribute = (DrawConditionAttribute)attribute;

        _isArrayProperty = _drawConditionAttribute.otherPropertiesArrayName != string.Empty;

        if (_isArrayProperty)
        {
            _propertiesArraySize = property.serializedObject.FindProperty(_drawConditionAttribute.otherPropertiesArrayName).arraySize;

            for (int i = 0; i < _propertiesArraySize; i++)
            {
                UpdateField(position, property, property.serializedObject.FindProperty($"{_drawConditionAttribute.otherPropertiesArrayName}.Array.data[{i}].{_drawConditionAttribute.otherPropertyName}"), label);
            }

            Debug.Log(" --- ");
        }
        else
        {
            UpdateField(position, property, property.serializedObject.FindProperty(_drawConditionAttribute.otherPropertyName), label);
        }
    }

    private void UpdateField(Rect position, SerializedProperty property, SerializedProperty otherProperty, GUIContent label)
    {
        _otherPropertyValue = GetPropertyValue(otherProperty);

        _isPropertyVisible = _otherPropertyValue.CompareTo(_drawConditionAttribute.comparisonValue) == _drawConditionAttribute.comparisonType;

        Debug.Log($"{property.displayName}: {_otherPropertyValue.CompareTo(_drawConditionAttribute.comparisonValue)}");

        _propertyHeight = base.GetPropertyHeight(property, label);

        if (_isPropertyVisible)
        {
            EditorGUI.PropertyField(position, property, label);
        }
        else
        {
            if (_drawConditionAttribute.disablingType == DisablingType.ReadOnly)
            {
                GUI.enabled = false;

                EditorGUI.PropertyField(position, property, label);

                GUI.enabled = true;
            }
            else
            {
                _propertyHeight = 0;
            }
        }
    }

    private object GetPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue;

            case SerializedPropertyType.Float:
                return property.floatValue;

            case SerializedPropertyType.String:
                return property.stringValue;

            case SerializedPropertyType.Enum:
                return property.enumValueIndex;

            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue;

            default:
                return new object();
        }
    }
}
