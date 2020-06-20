using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Enemies)), CanEditMultipleObjects]
public class CustomEnemyEditor : Editor
{
    public SerializedProperty
        enemyType_Prop,
        hitEffect_Prop,
        bullet_Prop,
        shootRange_Prop,
        timeBtwShots_Prop,
        shootPos_Prop,
        rotOffset_Prop;

    private void OnEnable()
    {
        enemyType_Prop = serializedObject.FindProperty("enemyType");
        hitEffect_Prop = serializedObject.FindProperty("hitEffect");
        bullet_Prop = serializedObject.FindProperty("bullet");
        shootRange_Prop = serializedObject.FindProperty("shootRange");
        timeBtwShots_Prop = serializedObject.FindProperty("timeBtwShots");
        shootPos_Prop = serializedObject.FindProperty("shootPos");
        rotOffset_Prop = serializedObject.FindProperty("rotOffset");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Enemies enemies = (Enemies)target;

        serializedObject.Update();

        enemies.enemyType = (EnemyType)EditorGUILayout.EnumPopup("EnemyType", enemies.enemyType);

        switch (enemies.enemyType)
        {
            case EnemyType.Turret:
                EditorGUILayout.PropertyField(bullet_Prop, new GUIContent("Bullet"));
                EditorGUILayout.PropertyField(hitEffect_Prop, new GUIContent("Hit Effect"));
                EditorGUILayout.PropertyField(shootRange_Prop, new GUIContent("Attack Range"));
                EditorGUILayout.PropertyField(timeBtwShots_Prop, new GUIContent("Time Between Shots"));
                EditorGUILayout.PropertyField(shootPos_Prop, new GUIContent("Shoot Position"));
                EditorGUILayout.PropertyField(rotOffset_Prop, new GUIContent("Rotation Offset"));
                break;
            case EnemyType.Jelly:
                EditorGUILayout.PropertyField(bullet_Prop, new GUIContent("Bullet"));
                EditorGUILayout.PropertyField(hitEffect_Prop, new GUIContent("Hit Effect"));
                EditorGUILayout.PropertyField(timeBtwShots_Prop, new GUIContent("Time Between Shots"));
                EditorGUILayout.PropertyField(shootPos_Prop, new GUIContent("Shoot Position"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
