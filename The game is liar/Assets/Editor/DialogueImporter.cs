using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueImporter : AssetPostprocessor
{
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var asset in importedAssets)
        {
            if (asset.EndsWith("Game_Dialogues.csv"))
            {
                using (FileStream stream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        int count = 0;
                        while (true)
                        {
                            string lineData = reader.ReadLine();
                            if (lineData == null)
                            {
                                break;
                            }
                            if (count == 0)
                            {
                                count++;
                                continue;
                            }

                            // Read the csv file with ',' inside ""
                            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                            int i = 0;
                            // Ex: ID,Speaker,"bla bla bla, senteces.;Hello","Yes:12;No:15"   --->   ID,Speaker, - bla bla bla, sentences.;Hello - Yes:12;No:15
                            foreach (var data in lineData.Split('*'))
                            {
                                Debug.Log(data + " " + i);
                                if (data == "," || data == "")
                                {
                                    continue;
                                }
                                if (i == 0)
                                {
                                    string[] temp = data.Split(','); // ID,Speaker, ---> ID - Speaker
                                    dialogue.DialogueID = temp[0];
                                    dialogue.speaker = temp[1];
                                }
                                else if (i == 1)
                                {
                                    dialogue.dialogues = data.Split(';'); // bla bla bla, sentences.;Hello ---> bla bla bla, sentences. - Hello
                                    foreach (var item in data.Split(';'))
                                    {
                                        Debug.Log(item);
                                    }
                                }
                                else
                                {
                                    List<Response> responses = new List<Response>();
                                    foreach (var textAndID in data.Split(';')) // Yes:12;No:15 ---> Yes:12 - No:15
                                    {
                                        string[] temp = textAndID.Split(':'); // Yes:12 ---> Yes - 12
                                        responses.Add(new Response(temp[0], temp[1]));
                                    }
                                    dialogue.responses = responses.ToArray();
                                }
                                i++;
                            }
                            Dialogue.AddDialogue(dialogue);
                            string path = "Assets/Dialogues/" + dialogue.DialogueID + ".asset";
                            AssetDatabase.CreateAsset(dialogue, path);
                            Selection.activeObject = dialogue;
                        }
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Debug.Log("Done creating at Assets/Dialogues!");
            }
        }
    }
}
