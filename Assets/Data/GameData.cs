using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameDataEntity_entities
{
    /// <summary>
    /// 物品的数字编号
    /// </summary>
    public int id;
    /// <summary>
    /// 物品的中文名称
    /// </summary>
    public string name;
    /// <summary>
    /// 物品中文名称的拼音
    /// </summary>
    public string pinyin;
    /// <summary>
    /// 物品所在房间的中文名称
    /// </summary>
    public string room;
    /// <summary>
    /// 物品初始持有的标签
    /// </summary>
    public List<string> tags;
    /// <summary>
    /// 查看物品时显示的文案数组
    /// </summary>
    public string[] clues;
}

[Serializable]
public class GameDataEntity_verbs
{
    /// <summary>
    /// 动词的数字编号，同义动词采用相同编号
    /// </summary>
    public int id;
    /// <summary>
    /// 动词的中文名称
    /// </summary>
    public string name;
    /// <summary>
    /// 动词中文的拼音
    /// </summary>
    public string pinyin;
}

[Serializable]
public class GameDataEntity_rules
{
    /// <summary>
    /// 动词的编号，与verbs表中的编号一致
    /// </summary>
    public int verb_id;
    /// <summary>
    /// 对象物品的编号，与entities表中的编号一致
    /// </summary>
    public int target_id;
    /// <summary>
    /// 动作目标物品需要持有的标签。没有则留空。
    /// </summary>
    public string[] require_tags;
    /// <summary>
    /// 要移除的标签。没有则留空给。
    /// </summary>
    public string[] remove_tags;
    /// <summary>
    /// 增加的标签。没有则留空。
    /// </summary>
    public string[] add_tags;
    /// <summary>
    /// 动作所需物品编号，即玩家背包中必须持有的物品。没有则留空。
    /// </summary>
    public int require_inventory_item;
    /// <summary>
    /// 要求背包中的该物品带有此标签。没有则留空。
    /// </summary>
    public string require_inventory_item_tag;
    /// <summary>
    /// 前置全局flag，满足才检定成功。没有则留空。
    /// </summary>
    public string require_flag;
    /// <summary>
    /// 成功后设置的全局flag。没有则留空。
    /// </summary>
    public string set_flag;
    /// <summary>
    /// 成功后生成/掉落的物品。没有则留空。
    /// </summary>
    public string spawn_item;
    /// <summary>
    /// 动作产生的除改变标签以外的效果的英文名，采用蛇形命名法snake_case。没有则留空。
    /// </summary>
    public string effect;
    /// <summary>
    /// 检定成功后打印在打字机上的文案
    /// </summary>
    public string feedback;
    /// <summary>
    /// 物品符合目标，但标签不符合条件时打印在打字机上的文案
    /// </summary>
    public string feedback_fail_tag;
    /// <summary>
    /// 物品符合目标，标签也符合目标，但背包物品持有条件不满足时打印的文案
    /// </summary>
    public string feedback_fail_inventory;
    /// <summary>
    /// 前置全局flag条件不满足时打印的文案。没有则留空。
    /// </summary>
    public string feedback_fail_flag;
}

[ExcelAsset]
public class GameData : ScriptableObject, ISerializationCallbackReceiver
{
    public List<GameDataEntity_entities> entities = new();
    public Dictionary<int, GameDataEntity_entities> entitiesDict;
    public List<GameDataEntity_verbs> verbs = new();
    public List<GameDataEntity_rules> rules = new();

    public void OnBeforeSerialize()
    {
        // Implement any logic needed before serialization
    }

    public void OnAfterDeserialize()
    {
        // Implement any logic needed after deserialization
        entitiesDict = new();
        foreach (GameDataEntity_entities item in entities)
        {
            var key = item.id;
            if (entitiesDict.ContainsKey(key))
            {
                Debug.LogError($"Duplicate key found in entitiesDict (script: GameData): {key}. Each key must be unique.");
                continue; // Skip adding this item to the dictionary
            }
            entitiesDict[key] = item;
        }
    }
}
