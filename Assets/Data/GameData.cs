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
    /// 查看物品时显示的文案
    /// </summary>
    public string clue;
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
    /// 动作目标物品需要持有的标签，用|连接。没有则留空。
    /// </summary>
    public string target_tag;
    /// <summary>
    /// 检定标签的目标存在状态，符合则检定成功，不符合则检定失败。没有则留空。
    /// </summary>
    public bool tag_exist;
    /// <summary>
    /// 标签的改动内容，即将原来的标签改为什么。没有则留空。
    /// </summary>
    public string tag_changed;
    /// <summary>
    /// 动作产生的除改变标签以外的效果的英文名，采用蛇形命名法`snake_case`。没有则留空。
    /// </summary>
    public string effect;
    /// <summary>
    /// 检定成功后打印在打字机上的文案
    /// </summary>
    public string feedback;
    /// <summary>
    /// 物品符合目标，但标签不符合条件时打印在打字机上的文案
    /// </summary>
    public string feedback_fail;
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
