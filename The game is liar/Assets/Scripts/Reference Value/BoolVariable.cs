using UnityEngine;

[CreateAssetMenu(menuName = "Variable/Bool")]
public class BoolVariable : ScriptableObject
{
    public bool value;
    [SerializeField] private bool defaultValue;

    void OnEnable()
    {
        value = defaultValue;
    }
}
