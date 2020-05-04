using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemiesMovement)), CanEditMultipleObjects]
public class CustomEnemyControllorEditor : Editor
{
    public SerializedProperty
    radius_Prop,
    whatIsGround_Prop,
    distanceToExplode_Prop,
    explodeRange_Prop,
    timeToExplode_Prop,
    distanceToChase_Prop,
    timeBtwFlash_Prop,
    flashTime_Prop,
    triggerMaterial_Prop,
    attackRange_Prop,
    rayLength_Prop;

    private void OnEnable()
    {
        radius_Prop = serializedObject.FindProperty("radius");
        whatIsGround_Prop = serializedObject.FindProperty("whatIsGround");
        distanceToExplode_Prop = serializedObject.FindProperty("distanceToExplode");
        explodeRange_Prop = serializedObject.FindProperty("explodeRange");
        timeToExplode_Prop = serializedObject.FindProperty("timeToExplode");
        distanceToChase_Prop = serializedObject.FindProperty("distanceToChase");
        timeBtwFlash_Prop = serializedObject.FindProperty("timeBtwFlash");
        flashTime_Prop = serializedObject.FindProperty("flashTime");
        triggerMaterial_Prop = serializedObject.FindProperty("triggerMaterial");
        attackRange_Prop = serializedObject.FindProperty("attackRange");
        rayLength_Prop = serializedObject.FindProperty("rayLength");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EnemiesMovement movement = (EnemiesMovement)target;

        movement.enemyType = (EnemyType)EditorGUILayout.EnumPopup("EnemyType", movement.enemyType);

        switch (movement.enemyType)
        {
            case EnemyType.Maggot:
                EditorGUILayout.PropertyField(radius_Prop, new GUIContent("Radius"));
                EditorGUILayout.PropertyField(whatIsGround_Prop, new GUIContent("What is ground"));
                break;
            case EnemyType.Bat:
                EditorGUILayout.PropertyField(distanceToExplode_Prop, new GUIContent("Distance to explode"));
                EditorGUILayout.PropertyField(explodeRange_Prop, new GUIContent("Explode range"));
                EditorGUILayout.PropertyField(timeToExplode_Prop, new GUIContent("Time to explode"));
                EditorGUILayout.PropertyField(distanceToChase_Prop, new GUIContent("Distance to chase"));
                EditorGUILayout.PropertyField(timeBtwFlash_Prop, new GUIContent("Time between flashes"));
                EditorGUILayout.PropertyField(flashTime_Prop, new GUIContent("Flash time"));
                EditorGUILayout.PropertyField(triggerMaterial_Prop, new GUIContent("Trigger Material"));
                break;
            case EnemyType.Alien:
                EditorGUILayout.PropertyField(rayLength_Prop, new GUIContent("Wall Check Length"));
                EditorGUILayout.PropertyField(attackRange_Prop, new GUIContent("Shoot Range"));
                break;
            case EnemyType.Jelly:
                EditorGUILayout.PropertyField(attackRange_Prop, new GUIContent("Attack range"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
