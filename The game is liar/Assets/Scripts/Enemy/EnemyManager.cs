//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;

//class EnemyManager : MonoBehaviour
//{
//    List<Enemy> enemies = new List<Enemy>();
//    Dictionary<RuleType, EnemyRule[]> ruleTable = new Dictionary<RuleType, EnemyRule[]>();

//    private void Start()
//    {
//        foreach (var obj in GameObject.FindGameObjectsWithTag("Enemy"))
//            enemies.Add(obj.GetComponent<Enemy>());
//        foreach (var type in ruleTable.Keys)
//        {
//            ruleTable[type] = ruleTable[type].OrderBy(x => x.priority).ThenBy(x => x.numberOfTags).ToArray();
//        }
//    }

//    private void Update()
//    {
//        /*foreach (var enemy in enemies)
//        {
//            foreach (var type in ruleTable.Keys)
//            {
//                if (enemy.type == type)
//                {
//                    foreach (EnemyRule rule in ruleTable[type])
//                    {
//                        if (rule.CheckTags(enemy))
//                        {
//                            rule.GetState().Execute(enemy);
//                            break;
//                        }
//                    }
//                    break;
//                }
//            }
//        }*/
//    }
//}
