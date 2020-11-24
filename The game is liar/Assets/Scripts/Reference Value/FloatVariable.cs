using UnityEngine;

[CreateAssetMenu(menuName = "Variable/Float")]
public class FloatVariable : ScriptableObject
{
    public float value;
    [SerializeField] private float defaultValue;

    private void OnEnable()
    {
        value = defaultValue;
    }
}
