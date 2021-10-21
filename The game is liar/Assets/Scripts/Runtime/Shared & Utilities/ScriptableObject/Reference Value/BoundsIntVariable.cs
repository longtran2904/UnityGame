using UnityEngine;

[CreateAssetMenu(menuName = "Variable/BoundsInt")]
public class BoundsIntVariable : ScriptableObject
{
    public BoundsInt value;
    [SerializeField] private BoundsInt defaultValue;

    void OnEnable()
    {
        value = defaultValue;
    }
}
