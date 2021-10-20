using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

public class WeaponImporter : AssetPostprocessor
{
    private enum WeaponStats
    {
        Name, Damage, CritDamage, CritChance, FireRate, Ammo, Knockback, Price, Standard, Active, Perfect, Failed, Description,
        StatCount
    }

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith("Game_Weapons.csv"))
            {
                bool endOfFile = false;
                List<string[]> data = new List<string[]>();
                try
                {
                    using (FileStream stream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                        {
                            while (!endOfFile)
                            {
                                string fileData = reader.ReadLine();
                                if (fileData == null)
                                {
                                    endOfFile = true;
                                    break;
                                }
                                data.Add(fileData.Split(','));
                            }
                        }
                    }

                    Dictionary<string, bool[]> debug = new Dictionary<string, bool[]>(); // For debug if a cell is empty

                    foreach (var stats in data)
                    {
                        if (data.IndexOf(stats) == 0)
                        {
                            continue;
                        }

                        bool[] isCorrect = new bool[(int)WeaponStats.StatCount];

                        WeaponStat weaponStat = ScriptableObject.CreateInstance<WeaponStat>();
                        weaponStat.weaponName = stats[(int)WeaponStats.Name];
                        isCorrect[0]  = int.TryParse(stats[(int)WeaponStats.Damage], out weaponStat.damage);
                        isCorrect[1]  = int.TryParse(stats[(int)WeaponStats.CritDamage], out weaponStat.critDamage);
                        isCorrect[2]  = float.TryParse(stats[(int)WeaponStats.CritChance], out weaponStat.critChance);
                        isCorrect[3]  = float.TryParse(stats[(int)WeaponStats.FireRate], out weaponStat.fireRate);
                        isCorrect[4]  = int.TryParse(stats[(int)WeaponStats.Ammo], out weaponStat.ammo);
                        isCorrect[5]  = float.TryParse(stats[(int)WeaponStats.Knockback], out weaponStat.knockback);
                        isCorrect[6]  = int.TryParse(stats[(int)WeaponStats.Price], out weaponStat.price);
                        isCorrect[7]  = float.TryParse(stats[(int)WeaponStats.Standard], out weaponStat.standardReload);
                        isCorrect[8]  = float.TryParse(stats[(int)WeaponStats.Active], out weaponStat.activeReload);
                        isCorrect[9] = float.TryParse(stats[(int)WeaponStats.Perfect], out weaponStat.perfectReload);
                        isCorrect[10] = float.TryParse(stats[(int)WeaponStats.Failed], out weaponStat.failedReload);
                        weaponStat.description = stats[(int)WeaponStats.Description];

                        debug.Add(weaponStat.weaponName, isCorrect);

                        // Create and refresh the assets
                        string path = "Assets/Weapon Stats/" + weaponStat.weaponName + ".asset";
                        AssetDatabase.CreateAsset(weaponStat, path);
                        InternalDebug.Log($"Done creating at {path}!");
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.FocusProjectWindow();

                    // Warning if there is an emty cell
                    foreach (var stats in debug)
                    {
                        for (int i = 0; i < stats.Value.Length; i++)
                        {
                            if (!stats.Value[i])
                            {
                                InternalDebug.LogWarning($"Can't convert at element {i} of {stats.Key}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalDebug.LogWarning($"The file {asset} could not be read!");
                    InternalDebug.LogError(e);
                    throw;
                }
            }
        }
    }
}
