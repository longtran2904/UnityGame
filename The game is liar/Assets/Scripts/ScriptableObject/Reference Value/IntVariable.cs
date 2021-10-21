using UnityEngine;

[CreateAssetMenu(menuName = "Variable/Int")]
public class IntVariable : ScriptableObject
{
    public int value;
    [SerializeField] private int defaultValue;

    private void OnEnable()
    {
        value = defaultValue;
    }
}
