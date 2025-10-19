using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

[CustomPropertyDrawer(typeof(SpringData))]
public class SecondOrderDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);
        if (!property.isExpanded) return;
        
        Profiler.BeginSample("Second Order Drawer - Calculate Points");
        float f = property.FindPropertyRelative("f").floatValue;
        float z = property.FindPropertyRelative("z").floatValue;
        float r = property.FindPropertyRelative("r").floatValue;
        
        const float tMax = 4;
        float dy = 0, dt = Time.fixedDeltaTime;
        float[] x = new float[(int)(tMax / dt)];
        float[] y = new float[(int)(tMax / dt)];
        
        (float k1, float k2, float k3) = MathUtils.InitK123(f, z, r);
        float acc = Mathf.Abs(Physics2D.gravity.y);
        float maxSpeed = 20;
        float drag = acc / maxSpeed;
        float velocity = 0;
        SerializedProperty xArr = property.FindPropertyRelative("x");
        for (int i = 1; i < y.Length; i++)
        {
            velocity += (acc - drag * velocity) * dt;
            x[i] = i / (x.Length / (int)tMax) % 2;
            y[i] = MathUtils.SecondOrder(dt, k1, k2, k3, x[i], x[i - 1], y[i - 1], ref dy);
        }
        float yMin = Mathf.Min(0, Mathf.Min(y));
        float yMax = Mathf.Max(1, Mathf.Max(y));
        Profiler.EndSample();
        
        Profiler.BeginSample("Second Order Drawer - Draw Lines");
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(128));
        
        Vector2 originP = ToPoint(0, 0);
        Vector2 horMaxP = ToPoint(tMax, 0);
        Vector2 verMaxP = ToPoint(0, 1);
        Vector2 labelSize = Vector2.one * 12;
        
        GUI.Label(new Rect(originP + Vector2.left * labelSize.x, labelSize), "0");
        GUI.Label(new Rect(verMaxP + Vector2.left * labelSize.x + Vector2.down * labelSize.y / 2, labelSize), "1");
        GUI.Label(new Rect(horMaxP + Vector2.left * labelSize.x + Vector2.up * labelSize.y / 2, labelSize), tMax.ToString());
        
        GameUtils.BeginGL();
        GameUtils.DrawGLLine(originP, horMaxP);
        GameUtils.DrawGLLine(ToPoint(0, yMin), ToPoint(0, yMax));
        GameUtils.DrawGLLine(verMaxP, ToPoint(tMax, 1));
        
        if (yMax > 5)
        {
            GameUtils.DrawGLLine(ToPoint(0, 5), ToPoint(tMax, 5));
            GUI.Label(new Rect(ToPoint(0, 5) + Vector2.left * labelSize.x + Vector2.down * labelSize.y / 2, labelSize), "5");
        }
        
        for (int i = 1; i < x.Length; i++)
            GameUtils.DrawGLLine(ToPoint(dt * (i - 1), x[i - 1]), ToPoint(dt * i, x[i]), Color.green);
        for (int i = 1; i < y.Length; i++)
            GameUtils.DrawGLLine(ToPoint(dt * (i - 1), y[i - 1]), ToPoint(dt * i, y[i]), Color.cyan);
        
        GameUtils.EndGL();
        EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(32));
        Profiler.EndSample();
        
        Vector2 ToPoint(float t, float x)
        {
            return new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, t / tMax), Mathf.Lerp(rect.yMax, rect.yMin, (x - yMin) / (yMax - yMin)));
        }
    }
}

[CustomPropertyDrawer(typeof(Property<>))]
public class FlagTogglesDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int columnCount = 2;
        FlagTogglesAttribute flagAttribute = GetAttribute<FlagTogglesAttribute>(fieldInfo);
        if (flagAttribute != null && flagAttribute.columnCount > 0)
            columnCount = flagAttribute.columnCount;
        float result = GetEnumNames().Length / (float)columnCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        return result;
    }
    
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        Profiler.BeginSample("FlagDrawer - OnGui");
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
        
        SerializedProperty flags = _property.FindPropertyRelative("properties");
        flags.longValue = (long)DrawToggles(_position, (ulong)flags.longValue, _label, columnCount, enumLength, enumNames);
        Profiler.EndSample();
    }
    
    string[] GetEnumNames()
    {
        // EX: normal Property`1[[EntityProperty    , Vailoz, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
        // EX: array  Property`1[[Entity+VFXProperty, Vailoz, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]][]
        string fullName = fieldInfo.FieldType.FullName;
        string[] names = fullName.Split('[', ']');
        Debug.Assert(names.Length == (fieldInfo.FieldType.IsArray ? 7 : 5), GameUtils.GetAllString(names, fullName + " " + fieldInfo.Name + ": "));
        string[] result = System.Type.GetType(names[2]).GetEnumNames();
        return result;
    }
    
    static T GetAttribute<T>(System.Reflection.FieldInfo fieldInfo) where T : System.Attribute
    {
        object[] attributes = fieldInfo.GetCustomAttributes(typeof(FlagTogglesAttribute), true);
        T result = null;
        foreach (var att in attributes)
        {
            result = att as T;
            if (result != null)
                break;
        }
        return result;
    }
    
    static ulong DrawToggles(Rect _position, ulong flags, GUIContent _label, int columnCount, int enumLength, string[] enumNames)
    {
        Profiler.BeginSample("FlagDrawer - DrawToggles");
        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);
        bool[] buttonPressed = new bool[enumLength];
        
        for (int i = 0; i < buttonPressed.Length; i++)
        {
            float buttonWidth = (_position.width - EditorGUIUtility.labelWidth) / columnCount;
            float buttonHeight = _position.height / Mathf.Ceil(buttonPressed.Length * 1.0f / columnCount);
            Rect buttonPos = new Rect(_position.x + EditorGUIUtility.labelWidth + buttonWidth * (i % columnCount),
                                      _position.y + (Mathf.Floor(i / columnCount) * buttonHeight), buttonWidth, buttonHeight);
            buttonPressed[i] = GUI.Toggle(buttonPos, MathUtils.HasFlag(flags, i), enumNames[i], "Button");
        }
        
        for (int i = 0; i < buttonPressed.Length; ++i)
            flags = MathUtils.SetFlag(flags, i, buttonPressed[i]);
        Profiler.EndSample();
        
        return flags;
    }
}

[CustomPropertyDrawer(typeof(IntReference))]
public class IntReferenceDrawer : PropertyDrawer
{
    /// <summary>
    /// Options to display in the popup to select constant or variable.
    /// </summary>
    private readonly string[] popupOptions =
    { "Use Constant", "Use Variable" };
    
    /// <summary> Cached style to use to draw the popup button. </summary>
    private GUIStyle popupStyle;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (popupStyle == null)
            popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };
        
        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);
        EditorGUI.BeginChangeCheck();
        
        // Get properties
        SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
        SerializedProperty constantValue = property.FindPropertyRelative("constantValue");
        SerializedProperty variable = property.FindPropertyRelative("variable");
        
        // Calculate rect for configuration button
        Rect buttonRect = new Rect(position);
        buttonRect.yMin += popupStyle.margin.top;
        buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
        position.xMin = buttonRect.xMax;
        
        // Store old indent level and set it to 0, the PrefixLabel takes care of it
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);
        useConstant.boolValue = result == 0;
        EditorGUI.PropertyField(position, useConstant.boolValue ? constantValue : variable, GUIContent.none);
        
        if (EditorGUI.EndChangeCheck())
            property.serializedObject.ApplyModifiedProperties();
        
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(Optional<>))]
public class OptionalPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"));
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProperty = property.FindPropertyRelative("value");
        var enabledProperty = property.FindPropertyRelative("enabled");
        
        EditorGUI.BeginProperty(position, label, property);
        position.width -= 24;
        
        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
        EditorGUI.PropertyField(position, valueProperty, label, true);
        EditorGUI.EndDisabledGroup();
        
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        position.x += position.width + 24;
        position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
        position.x -= position.width;
        EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
        
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(RangedFloat))]
[CustomPropertyDrawer(typeof(RangedInt))]
[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class RangedFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Profiler.BeginSample("MinMax - OnGUI");
        if (!(attribute is MinMaxAttribute limits))
            limits = new MinMaxAttribute(0, 1);
        var minProp = property.FindPropertyRelative("min");
        var maxProp = property.FindPropertyRelative("max");
        bool isRangedInt = minProp.propertyType == SerializedPropertyType.Integer;
        float min = isRangedInt ? minProp.intValue : minProp.floatValue;
        float max = isRangedInt ? maxProp.intValue : maxProp.floatValue;
        string format = isRangedInt ? null : "0.00";
        label.text += "(" + min.ToString(format) + ", " + max.ToString(format) + ")";
        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(position, label, ref min, ref max, limits.min, limits.max);
        if (EditorGUI.EndChangeCheck() || !limits.IsInRange(min) || !limits.IsInRange(max))
        {
            float minValue = Mathf.Clamp(min, limits.min, limits.max);
            float maxValue = Mathf.Clamp(max, limits.min, limits.max);
            if (isRangedInt)
            {
                minProp.intValue = (int)minValue;
                maxProp.intValue = (int)maxValue;
            }
            else
            {
                minProp.floatValue = minValue;
                maxProp.floatValue = maxValue;
            }
        }
        Profiler.EndSample();
    }
}