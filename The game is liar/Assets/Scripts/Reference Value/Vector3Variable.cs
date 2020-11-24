using UnityEngine;

[CreateAssetMenu(menuName = "Variable/Vector3")]
public class Vector3Variable : ScriptableObject
{
    public Vector3 value;
    [SerializeField] private Vector3 defaultValue;

    void OnEnable()
    {
        value = defaultValue;
    }
}
