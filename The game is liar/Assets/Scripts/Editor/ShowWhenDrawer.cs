using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowWhenAttribute))]
public class ShowWhenDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool showField = ShouldShowField(property, out bool hasError, out string errorMessage);

        if (hasError)
        {
            ShowError(position, label, errorMessage);
            return;
        }

        if (showField)
            EditorGUI.PropertyField(position, property, label, true);
    }

    bool ShouldShowField(SerializedProperty property, out bool hasError, out string errorMessage)
    {
        bool showField = true;

        ShowWhenAttribute attribute = (ShowWhenAttribute)this.attribute;
        hasError = false;
        errorMessage = "";

        //SerializedProperty conditionField = property.serializedObject.FindProperty(attribute.conditionFieldName);

        SerializedProperty conditionField = property.FindSiblingProperty(attribute.conditionFieldName);

        // We check that exist a Field with the parameter name
        if (conditionField == null)
        {
            hasError = true;
            errorMessage = "Error getting the condition Field. Check the name.";
            return true;// Errors should be displayed
        }

        switch (conditionField.propertyType)
        {
            case SerializedPropertyType.Boolean:
                try
                {
                    bool comparationValue = attribute.comparationValue == null || (bool)attribute.comparationValue;
                    showField = conditionField.boolValue == comparationValue;
                }
                catch
                {
                    hasError = true;
                    errorMessage = "Invalid comparation Value Type";
                    return true;// Errors should be displayed
                }
                break;
            case SerializedPropertyType.Enum:
                object paramEnum = attribute.comparationValue;
                object[] paramEnumArray = attribute.comparationValueArray;

                if (paramEnum == null && paramEnumArray == null)
                {
                    hasError = true;
                    errorMessage = "The comparation enum value is null";
                    return true;// Errors should be displayed
                }
                else if (IsEnum(paramEnum))
                {
                    if (!CheckSameEnumType(new[] { paramEnum.GetType() }, property.serializedObject.targetObject.GetType(), conditionField.propertyPath))
                    {
                        hasError = true;
                        errorMessage = "Enum Types doesn't match";
                        return true;// Errors should be displayed
                    }
                    else
                    {
                        string enumValue = Enum.GetValues(paramEnum.GetType()).GetValue(conditionField.enumValueIndex).ToString();
                        if (paramEnum.ToString() != enumValue)
                            showField = false;
                        else
                            showField = true;
                    }
                }
                else if (IsEnum(paramEnumArray))
                {
                    if (!CheckSameEnumType(paramEnumArray.Select(x => x.GetType()), property.serializedObject.targetObject.GetType(), conditionField.propertyPath))
                    {
                        hasError = true;
                        errorMessage = "Enum Types doesn't match";
                        return true;// Errors should be displayed
                    }
                    else
                    {
                        string enumValue = Enum.GetValues(paramEnumArray[0].GetType()).GetValue(conditionField.enumValueIndex).ToString();
                        if (paramEnumArray.All(x => x.ToString() != enumValue))
                            showField = false;
                        else
                            showField = true;
                    }
                }
                /*else if (paramEnum.GetType() == typeof(string))
                {
                    string notEnum = paramEnum.ToString().Remove(0, 1);
                    if (string.IsNullOrWhiteSpace(notEnum))
                    {
                        hasError = true;
                        errorMessage = "The comparation enum value is empty, whitespace, or null";
                        return true;
                    }
                    string enumValue = Enum.GetValues(conditionField.GetTheActualType()).GetValue(conditionField.enumValueIndex).ToString();
                    if (enumValue == notEnum)
                        showField = false;
                    else
                        showField = true;
                }*/
                else
                {
                    hasError = true;
                    errorMessage = "The comparation enum value is not an enum";
                    return true;// Errors should be displayed
                }
                break;
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Float:
                string stringValue;
                bool error = false;

                float conditionValue = 0;
                if (conditionField.propertyType == SerializedPropertyType.Integer)
                    conditionValue = conditionField.intValue;
                else if (conditionField.propertyType == SerializedPropertyType.Float)
                    conditionValue = conditionField.floatValue;

                try
                {
                    stringValue = (string)attribute.comparationValue;
                }
                catch
                {
                    hasError = true;
                    errorMessage = "Invalid comparation Value Type";
                    return true;// Errors should be displayed
                }

                if (stringValue.StartsWith("=="))
                {
                    float? value = GetValue(stringValue, "==");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue == value;
                }
                else if (stringValue.StartsWith("!="))
                {
                    float? value = GetValue(stringValue, "!=");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue != value;
                }
                else if (stringValue.StartsWith("<="))
                {
                    float? value = GetValue(stringValue, "<=");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue <= value;
                }
                else if (stringValue.StartsWith(">="))
                {
                    float? value = GetValue(stringValue, ">=");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue >= value;
                }
                else if (stringValue.StartsWith("<"))
                {
                    float? value = GetValue(stringValue, "<");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue < value;
                }
                else if (stringValue.StartsWith(">"))
                {
                    float? value = GetValue(stringValue, ">");
                    if (value == null)
                        error = true;
                    else
                        showField = conditionValue > value;
                }

                if (error)
                {
                    hasError = true;
                    errorMessage = "Invalid comparation instruction for Int or float value";
                    return true;// Errors should be displayed
                }
                break;
            default:
                hasError = true;
                errorMessage = "This type has not supported.";
                return true;// Errors should be displayed
        }

        return showField;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldShowField(property, out bool error, out string errorMessage))
            return EditorGUI.GetPropertyHeight(property);

        return 0.0f;
        //else
        //    return -EditorGUIUtility.standardVerticalSpacing;
    }

    /// <summary>
    /// Return if the object is enum and not null
    /// </summary>
    private static bool IsEnum(object obj)
    {
        return obj != null && obj.GetType().IsEnum;
    }

    /// <summary>
    /// Return if all the objects are enums and not null
    /// </summary>
    private static bool IsEnum(object[] obj)
    {
        return obj != null && obj.All(o => o.GetType().IsEnum);
    }

    /// <summary>
    /// Check if the field with name "fieldName" has the same class as the "checkTypes" classes through reflection
    /// </summary>
    private static bool CheckSameEnumType(IEnumerable<Type> checkTypes, Type classType, string fieldName)
    {
        Type currentFieldType;
        string[] fields = fieldName.Split('.');
        if (fields.Length > 1)
        {
            currentFieldType = classType.GetField(fields[0]).FieldType;
            for (int i = 1; i < fields.Length; i++)
            {
                if (currentFieldType.IsArray)
                {
                    currentFieldType = currentFieldType.GetElementType();// GetFields()[fieldIdx];

                    i += 1;// The fieldNames for array will containt Array.data[0] so we need to skip two
                }
                else
                    currentFieldType = currentFieldType.GetField(fields[i]).FieldType;
            }
        }
        else
            currentFieldType = classType.GetField(fieldName).FieldType;

        if (currentFieldType != null)
            return checkTypes.All(x => x == currentFieldType);

        return false;
    }

    private void ShowError(Rect position, GUIContent label, string errorText)
    {
        EditorGUI.LabelField(position, label, new GUIContent(errorText));
    }

    /// <summary>
    /// Return the float value in the content string removing the remove string
    /// </summary>
    private static float? GetValue(string content, string remove)
    {
        string removed = content.Replace(remove, "");
        try
        {
            return float.Parse(removed);
        }
        catch
        {
            return null;
        }
    }
}