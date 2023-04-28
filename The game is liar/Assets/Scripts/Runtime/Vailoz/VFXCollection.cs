using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFX Collection", menuName = "RuntimeSet/VFX")]
public class VFXCollection : ScriptableObject
{
    [System.Serializable]
    public class VFXHolder
    {
        public EntityState type;
        public VFX vfx;
    }

    [SerializeField] private VFXHolder[] defaultItems;
    public Dictionary<EntityState, List<VFX>> items;

#if UNITY_EDITOR
    void OnEnable()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            items?.Clear();
            int count = System.Enum.GetValues(typeof(EntityState)).Length;
            items = new Dictionary<EntityState, List<VFX>>(count);
            for (int i = 0; i < count; i++)
                items.Add((EntityState)i, new List<VFX>(2));
            foreach (VFXHolder item in defaultItems)
                items[item.type].Add(item.vfx);
        }
    }
#endif
}

