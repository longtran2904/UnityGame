using UnityEditor;

public static class SerializedPropertyExt
{
    public static SerializedProperty GetParent(this SerializedProperty aProperty)
    {
        var path = aProperty.propertyPath;
        int i = path.LastIndexOf('.');
        if (i < 0)
            return null;
        return aProperty.serializedObject.FindProperty(path.Substring(0, i));
    }

    public static SerializedProperty FindSiblingProperty(this SerializedProperty aProperty, string aPath)
    {
        var parent = aProperty.GetParent();
        if (parent == null)
            return aProperty.serializedObject.FindProperty(aPath);
        return parent.FindPropertyRelative(aPath) ?? aProperty.serializedObject.FindProperty(aPath);
    }
}
