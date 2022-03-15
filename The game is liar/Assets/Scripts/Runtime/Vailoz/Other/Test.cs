using UnityEditor;
using UnityEngine;
using System.IO;

public class Test : MonoBehaviour
{    
    void LoadAllFilesAtDirectory<T>(string dirPath, string searchPattern, SearchOption option, System.Action<T> func) where T : Object
    {
        string[] paths = Directory.GetFiles(Application.dataPath + dirPath, searchPattern, SearchOption.TopDirectoryOnly);
        if ((paths == null) || (paths.Length == 0) || (func == null))
            return;

        T[] result = new T[paths.Length];
        int i = 0;
        foreach (string path in paths)
        {
            string assetPath = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');
            result[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (result == null)
                continue;

            func(result[i]);
            EditorUtility.SetDirty(result[i]);

            i++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [EasyButtons.Button]
    void ChangeAllWeaponsScale()
    {
        string[] paths = Directory.GetFiles(Application.dataPath + "/Prefabs/Weapons", "*.prefab", SearchOption.TopDirectoryOnly);
        foreach (string path in paths)
        {
            string assetPath = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');
            Weapon weapon = AssetDatabase.LoadAssetAtPath<Weapon>(assetPath);
            if (weapon)
            {
                weapon.posOffset *= 6.25f;
                EditorUtility.SetDirty(weapon);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [EasyButtons.Button]
    void ChangeAllWeaponsMaterial(Material mat)
    {
        LoadAllFilesAtDirectory<Weapon>("/Prefabs/Weapons", "*.prefab", SearchOption.TopDirectoryOnly, weapon => weapon.GetComponent<SpriteRenderer>().material = mat);
    }
}
