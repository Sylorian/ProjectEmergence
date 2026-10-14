using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RegistryItem : MonoBehaviour
{
    //Unity编辑器内的写入会初始化覆盖，所以程序可以任意调复写方法，重置会自动初始化
    [Serializable] private struct BoolEntry   { public string key; public bool   value; } // 是否可被检查
    [Serializable] private struct StringEntry { public string key; public string value; } // 对象名字
    [Serializable] private struct FloatEntry  { public string key; public float  value; } // 对象序列

    [SerializeField] private string id; // 对象在字典内的唯一ID

    [Header("初始属性")] [SerializeField] private List<BoolEntry> boolProps = new()
    {
        new BoolEntry { key = "IsReady", value = true }, //当前状态是否可被交互
        new BoolEntry { key = "IsHighlight", value = false }, //当前状态是否高亮
        new BoolEntry { key = "IsShow", value = true }, //当前状态是否被渲染
        new BoolEntry { key = "IsPlay", value = false }, //当前状态是否播放动画
        new BoolEntry { key = "IsAudio", value = false }, //当前状态是否播放音效
        new BoolEntry { key = "IsBagpack", value = false} //当前状态是否存在背包
    };
    [SerializeField] private List<StringEntry> stringProps = new()
    {
        new StringEntry { key = "Name", value = null}, //对象名
        new StringEntry { key = "Info", value = null}, //对象描述
        new StringEntry { key = "Clue", value = null}, //对象线索
        new StringEntry { key = "PrivateLabel", value = null},//对象自有标签
        new StringEntry { key = "PublicLabel", value = null}//对象公共标签
    };
    [SerializeField] private List<FloatEntry>  floatProps  = new()
    {
        new FloatEntry { key = "HowMany", value = 1},//对象存在数量
    };
    

    private void OnEnable() // 挂在空对象时，在对象被创立时，初始化时写入 //
    {
        var bag = new PropertyBag(); //bag规定好了属性在 PropertyBag内，是对象属性，安排了查询办法
        foreach (var e in boolProps)   bag.Set(e.key, e.value); // 键值对写入字典
        foreach (var e in stringProps) bag.Set(e.key, e.value);
        foreach (var e in floatProps)  bag.Set(e.key, e.value);
        
        //注册
        
        ScenePropertyRegistry.Register(id, bag , this.gameObject); //数据表和对象表同时存入
        Debug.Log(this.id + "已注册");
        /*
foreach (var pair in ScenePropertyRegistry.ErgodicGet(this.id))
{
    Debug.Log(pair);
*/
        //控制台日志返回
    }

    private void OnDisable() => ScenePropertyRegistry.Unregister(id); // 对象销毁时从字典删除
}