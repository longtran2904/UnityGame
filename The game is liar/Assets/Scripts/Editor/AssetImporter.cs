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
    
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                                              string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith("Game_Weapons.csv"))
            {
                const string folderName = folder + "Weapon Stats/";
                EditorUtils.ReadAndProcessFile
                    (asset, ',', folderName, stats =>
                     {
                         WeaponStat stat = AssetDatabase.LoadAssetAtPath<WeaponStat>(folderName + stats[(int)WeaponStats.Name] + ".asset");
                         stat ??= ScriptableObject.CreateInstance<WeaponStat>();
                         bool[] isCorrect = new bool[(int)WeaponStats.StatCount];
                         
                         const int Name        = (int)WeaponStats.Name;
                         const int Damage      = (int)WeaponStats.Damage;
                         const int CritDamage  = (int)WeaponStats.CritDamage;
                         const int CritChance  = (int)WeaponStats.CritChance;
                         const int FireRate    = (int)WeaponStats.FireRate;
                         const int Mode        = (int)WeaponStats.Mode;
                         const int Ammo        = (int)WeaponStats.Ammo;
                         const int Knockback   = (int)WeaponStats.Knockback;
                         const int Price       = (int)WeaponStats.Price;
                         const int Standard    = (int)WeaponStats.Standard;
                         const int Active      = (int)WeaponStats.Active;
                         const int Perfect     = (int)WeaponStats.Perfect;
                         const int Failed      = (int)WeaponStats.Failed;
                         const int Description = (int)WeaponStats.Description;
                         
                         isCorrect[Name]        = (stat.weaponName = stats[Name]) != "";
                         isCorrect[Damage]      =  int.TryParse(stats[Damage], out stat.damage);
                         isCorrect[CritDamage]  =  int.TryParse(stats[CritDamage], out stat.critDamage);
                         isCorrect[CritChance]  = float.TryParse(stats[CritChance], out stat.critChance);
                         isCorrect[FireRate]    = float.TryParse(stats[FireRate], out stat.fireRate);
                         isCorrect[Mode]        = true; // We don't parse the fire mode anymore.
                         isCorrect[Ammo]        =  int.TryParse(stats[Ammo], out stat.ammo);
                         isCorrect[Knockback]   = float.TryParse(stats[Knockback], out stat.knockback);
                         isCorrect[Price]       =  int.TryParse(stats[Price], out stat.price);
                         isCorrect[Standard]    = float.TryParse(stats[Standard], out stat.standardReload);
                         isCorrect[Active]      = float.TryParse(stats[Active], out stat.activeReload);
                         isCorrect[Perfect]     = float.TryParse(stats[Perfect], out stat.perfectReload);
                         isCorrect[Failed]      = float.TryParse(stats[Failed], out stat.failedReload);
                         isCorrect[Description] = (stat.description = stats[Description]) != null;
                         
                         for (int i = 0; i < isCorrect.Length; i++)
                             if (!isCorrect[i] && stats[i] != "")
                                 Debug.LogWarning($"Can't convert {(WeaponStats)i} to {stats[i]} of {stats[(int)WeaponStats.Name]}");
                         
                         return stat;
                     }, weapon => weapon.weaponName);
            }
            else if (asset.EndsWith("Game_Dialogues.csv"))
            {
                string folderName = folder + "Dialogues/";
                EditorUtils.ReadAndProcessFile(asset, '*', folderName, data =>
                                               {
                                                   Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                                                   // (ID, speaker, and comma), (dialogues), (comma and responses)
                                                   Debug.Assert(data.Length == 3, data.Length);
                                                   
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
                                                       dialogue.responses[i] = new Response(response[i],
                                                                                            i + 1 < response.Length ? response[i + 1] : null);
                                                   }
                                                   
                                                   return dialogue;
                                               }, dialogue => dialogue.DialogueID);
            }
        }
    }
}
