// https://web.archive.org/web/20200210153438/https://www.sharkbombs.com/2015/02/17/unity-editor-enum-flags-as-toggle-buttons/

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Property<>))]
public class FlagTogglesDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int columnCount = 2;
        FlagTogglesAttribute flagAttribute = GetAttribute<FlagTogglesAttribute>(fieldInfo);
        if (flagAttribute != null && flagAttribute.columnCount > 0)
            columnCount = flagAttribute.columnCount;
        return GetEnumNames().Length / (float)columnCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        int columnCount = 2;
        string[] enumNames = GetEnumNames();
        int enumLength = enumNames.Length;
        if (enumNames[enumLength - 1] == "Count")
            --enumLength;

        FlagTogglesAttribute flagAttribute = GetAttribute<FlagTogglesAttribute>(fieldInfo);
        if (flagAttribute != null)
        {
            if (flagAttribute.maxDisplayCount > 0)
                enumLength = flagAttribute.maxDisplayCount;
            if (flagAttribute.columnCount > 0)
                columnCount = flagAttribute.columnCount;
        }

        SerializedProperty serializedArray = _property.FindPropertyRelative("serializedEnumNames");
        SerializedProperty arrayProp = _property.FindPropertyRelative("properties");

        arrayProp.arraySize = (enumLength + 63) / 64;
        for (int i = 0; i < arrayProp.arraySize; i++)
            DrawToggles(_position, arrayProp.GetArrayElementAtIndex(i), _label, columnCount, i * 64, enumLength, enumNames, serializedArray);

        serializedArray.arraySize = enumNames.Length;
        for (int i = 0; i < serializedArray.arraySize; i++)
            serializedArray.GetArrayElementAtIndex(i).stringValue = enumNames[i];
    }

    string[] GetEnumNames()
    {
        // Property`1[[EntityStatProperty, Vailoz, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
        return fieldInfo.FieldType.Assembly.GetType(fieldInfo.FieldType.FullName.Split('[')[2].Split(',')[0]).GetEnumNames();
    }

    static T GetAttribute<T>(System.Reflection.FieldInfo fieldInfo) where T : System.Attribute
    {
        object[] attributes = fieldInfo.GetCustomAttributes(typeof(FlagTogglesAttribute), true);
        foreach (var att in attributes)
        {
            T flagAttribute;
            if ((flagAttribute = att as T) != null)
                return flagAttribute;
        }
        return null;
    }

    static void DrawToggles(Rect _position, SerializedProperty _property, GUIContent _label,
        int columnCount, int startIndex, int length, string[] enumNames, SerializedProperty serializedNameArray)
    {
        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);
        bool[] buttonPressed = new bool[length];
        float buttonWidth = (_position.width - EditorGUIUtility.labelWidth) / columnCount;
        float buttonHeight = _position.height / Mathf.Ceil(buttonPressed.Length * 1.0f / columnCount);

        for (int i = 0; i < buttonPressed.Length; i++)
        {
            bool currentValue = MathUtils.HasFlag(_property.longValue, i);
            int arrayIndex = i + startIndex;
            string currentName = enumNames[arrayIndex];
            if (arrayIndex >= serializedNameArray.arraySize || currentName != serializedNameArray.GetArrayElementAtIndex(arrayIndex).stringValue)
            {
                currentValue = false;
                for (int newIndex = 0; newIndex < serializedNameArray.arraySize; newIndex++)
                    if (serializedNameArray.GetArrayElementAtIndex(newIndex).stringValue == currentName)
                        currentValue = MathUtils.HasFlag(_property.longValue, newIndex);
            }
            buttonPressed[i] = currentValue;
            Rect buttonPos = new Rect(_position.x + EditorGUIUtility.labelWidth + buttonWidth * (i % columnCount),
                _position.y + (Mathf.Floor(i / columnCount) * buttonHeight), buttonWidth, buttonHeight);
            buttonPressed[i] = GUI.Toggle(buttonPos, buttonPressed[i], currentName, "Button");
        }

        for (int i = 0; i < buttonPressed.Length; ++i)
            if (buttonPressed[i])
                _property.longValue |= (1L << (i % 64));
            else
                _property.longValue &= ~(1L << (i % 64));
    }
}