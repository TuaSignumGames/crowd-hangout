using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawConditionAttribute : PropertyAttribute
{
    public string otherPropertiesArrayName;
    public string otherPropertyName;
    public object comparisonValue;

    public ComparisonType comparisonType;
    public DisablingType disablingType;

    public DrawConditionAttribute(string otherPropertyName, object comparisonValue, ComparisonType comparisonType, DisablingType disablingType = DisablingType.Hide)
    {
        this.otherPropertyName = otherPropertyName;
        this.comparisonValue = comparisonValue;
        this.comparisonType = comparisonType;
        this.disablingType = disablingType;
    }

    public DrawConditionAttribute(string otherPropertiesArrayName, string otherPropertyName, object comparisonValue, ComparisonType comparisonType, DisablingType disablingType = DisablingType.Hide)
    {
        this.otherPropertiesArrayName = otherPropertiesArrayName;
        this.otherPropertyName = otherPropertyName;
        this.comparisonValue = comparisonValue;
        this.comparisonType = comparisonType;
        this.disablingType = disablingType;
    }
}
