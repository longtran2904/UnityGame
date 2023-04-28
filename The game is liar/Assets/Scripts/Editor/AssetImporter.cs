using UnityEngine;
using UnityEditor;

public class AssetImporter : AssetPostprocessor
{
    public const string folder = "Assets/Files/";
    
    private enum WeaponStats
    {
        Name, Damage, CritDamage, CritChance, FireRate, Mode, Ammo, Knockback, Price, Standard, Active, Perfect, Failed, Description,
        StatCount
    }
    
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith("Game_Weapons.csv"))
            {
                const string folderName = folder + "Weapon Stats/";
                EditorUtils.ReadAndProcessFile(asset, ',', folderName, stats =>
                                               {
                                                   WeaponStat weaponStat = AssetDatabase.LoadAssetAtPath<WeaponStat>(folderName + stats[(int)WeaponStats.Name] + ".asset");
                                                   weaponStat ??= ScriptableObject.CreateInstance<WeaponStat>();
                                                   bool[] isCorrect = new bool[(int)WeaponStats.StatCount];
                                                   
                                                   isCorrect[(int)WeaponStats.Name] = (weaponStat.weaponName = stats[(int)WeaponStats.Name]) != "";
                                                   isCorrect[(int)WeaponStats.Damage] = int.TryParse(stats[(int)WeaponStats.Damage], out weaponStat.damage);
                                                   isCorrect[(int)WeaponStats.CritDamage] = int.TryParse(stats[(int)WeaponStats.CritDamage], out weaponStat.critDamage);
                                                   isCorrect[(int)WeaponStats.CritChance] = float.TryParse(stats[(int)WeaponStats.CritChance], out weaponStat.critChance);
                                                   isCorrect[(int)WeaponStats.FireRate] = float.TryParse(stats[(int)WeaponStats.FireRate], out weaponStat.fireRate);
                                                   isCorrect[(int)WeaponStats.Mode] = true; // We don't parse the fire mode anymore.
                                                   isCorrect[(int)WeaponStats.Ammo] = int.TryParse(stats[(int)WeaponStats.Ammo], out weaponStat.ammo);
                                                   isCorrect[(int)WeaponStats.Knockback] = float.TryParse(stats[(int)WeaponStats.Knockback], out weaponStat.knockback);
                                                   isCorrect[(int)WeaponStats.Price] = int.TryParse(stats[(int)WeaponStats.Price], out weaponStat.price);
                                                   isCorrect[(int)WeaponStats.Standard] = float.TryParse(stats[(int)WeaponStats.Standard], out weaponStat.standardReload);
                                                   isCorrect[(int)WeaponStats.Active] = float.TryParse(stats[(int)WeaponStats.Active], out weaponStat.activeReload);
                                                   isCorrect[(int)WeaponStats.Perfect] = float.TryParse(stats[(int)WeaponStats.Perfect], out weaponStat.perfectReload);
                                                   isCorrect[(int)WeaponStats.Failed] = float.TryParse(stats[(int)WeaponStats.Failed], out weaponStat.failedReload);
                                                   isCorrect[(int)WeaponStats.Description] = (weaponStat.description = stats[(int)WeaponStats.Description]) != null;
                                                   
                                                   for (int i = 0; i < isCorrect.Length; i++)
                                                       if (!isCorrect[i] && stats[i] != "")
                                                       Debug.LogWarning($"Can't convert {(WeaponStats)i} to {stats[i]} of {stats[(int)WeaponStats.Name]}");
                                                   
                                                   return weaponStat;
                                               }, weapon => weapon.weaponName);
            }
            else if (asset.EndsWith("Game_Dialogues.csv"))
            {
                string folderName = folder + "Dialogues/";
                EditorUtils.ReadAndProcessFile(asset, '*', folderName, data =>
                                               {
                                                   Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                                                   Debug.Assert(data.Length == 3, data.Length); // (ID, speaker, and comma), (dialogues), (comma and responses)
                                                   
                                                   string[] header = data[0].Split(',');
                                                   Debug.Assert(header.Length == 3, GameUtils.GetAllString(header));
                                                   dialogue.DialogueID = header[0];
                                                   dialogue.speaker = header[1];
                                                   
                                                   string[] sentences = data[1].Split(';');
                                                   dialogue.dialogues = sentences;
                                                   
                                                   string[] responses = data[2].Split(',')[1].Split(';');
                                                   dialogue.responses = new Response[responses.Length];
                                                   for (int i = 0; i < responses.Length; i++)
                                                   {
                                                       string[] response = responses[i].Split(':');
                                                       dialogue.responses[i] = new Response(response[i], i + 1 < response.Length ? response[i + 1] : null);
                                                   }
                                                   
                                                   return dialogue;
                                               }, dialogue => dialogue.DialogueID);
            }
        }
    }
}
