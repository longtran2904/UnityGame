using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class MinMaxAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public MinMaxAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public bool IsInRange(float value)
    {
        return value >= min && value <= max;
    }
}

/// <summary>
/// Attribute used to show or hide the Field depending on certain conditions
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowWhenAttribute : PropertyAttribute
{
	public readonly string conditionFieldName;
	public readonly object comparationValue;
	public readonly object[] comparationValueArray;

	/// <summary>
	/// Attribute used to show or hide the Field depending on certain conditions
	/// </summary>
	/// <param name="conditionFieldName">Name of the bool condition Field</param>
	public ShowWhenAttribute(string conditionFieldName)
	{
		this.conditionFieldName = conditionFieldName;
	}

	/// <summary>
	/// Attribute used to show or hide the Field depending on certain conditions
	/// </summary>
	/// <param name="conditionFieldName">Name of the Field to compare (bool, enum, int or float)</param>
	/// <param name="comparationValue">Value to compare</param>
	public ShowWhenAttribute(string conditionFieldName, object comparationValue = null)
	{
		this.conditionFieldName = conditionFieldName;
		this.comparationValue = comparationValue;
	}

	/// <summary>
	/// Attribute used to show or hide the Field depending on certain conditions
	/// </summary>
	/// <param name="conditionFieldName">Name of the Field to compare (bool, enum, int or float)</param>
	/// <param name="comparationValueArray">Array of values to compare (only for enums)</param>
	public ShowWhenAttribute(string conditionFieldName, object[] comparationValueArray = null)
	{
		this.conditionFieldName = conditionFieldName;
		this.comparationValueArray = comparationValueArray;
	}
}

public class FlagTogglesAttribute : PropertyAttribute
{
	public int columnCount;
	public int maxDisplayCount;

	public FlagTogglesAttribute(int columnCount = 1, int maxDisplayCount = 0)
	{
		this.columnCount = columnCount;
		this.maxDisplayCount = maxDisplayCount;
	}
}