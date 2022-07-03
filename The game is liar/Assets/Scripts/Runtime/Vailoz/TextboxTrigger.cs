using UnityEngine;

public enum TextboxType
{
    None,
    Dialogue,
    Chest,
    Weapon,

    Count
}

public class TextboxTrigger : MonoBehaviour
{
    public TextboxType textboxType;
    public Vector2 textboxOffset;
    public UnityEngine.Events.UnityEvent trigger;

    [ShowWhen("textboxType", TextboxType.Dialogue)] public Dialogue[] dialogues;
    private int currentDialogue;

    [System.Serializable]
    public struct ChestData
    {
        public WeaponInventory inventory;
        [MinMax(0, 5)] public RangedInt dropIndex;
        [MinMax(0, 360)] public RangedFloat dropRotation;
        [MinMax(1, 20)] public RangedFloat dropForce;
    }
    [ShowWhen("textboxType", TextboxType.Chest)] public ChestData chestData;

    [HideInInspector] public Vector3 hitGroundPos; // This is for dropped weapon
    [HideInInspector] public Weapon weapon;

    public Dialogue GetRandomDialogue()
    {
        if (currentDialogue == dialogues.Length)
        {
            currentDialogue = 0;
            MathUtils.Shuffle(dialogues);
        }
        return dialogues[currentDialogue++];
    }

    public Vector2 GetRandomDir()
    {
        return MathUtils.MakeVector2(chestData.dropRotation.randomValue, chestData.dropForce.randomValue);
    }
}