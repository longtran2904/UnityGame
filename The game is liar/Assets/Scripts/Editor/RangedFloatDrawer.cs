using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RangedFloat))]
[CustomPropertyDrawer(typeof(RangedInt))]
[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class RangedFloatDrawer : PropertyDrawer
{
    private static MinMaxAttribute defaultRange = new MinMaxAttribute(0, 1);
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!(attribute is MinMaxAttribute limits))
            limits = defaultRange;
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
    }
}