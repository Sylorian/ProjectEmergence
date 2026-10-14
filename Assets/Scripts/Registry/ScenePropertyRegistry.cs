using System.Collections.Generic;
using UnityEngine;

public static class ScenePropertyRegistry
{
    private static readonly Dictionary<string, PropertyBag> _bags = new(); // 初始化创建静态注册表，挂在一个空对象上做初始化处理。此表存数据
    private static readonly Dictionary<string, GameObject> _instance = new(); //初始化创建对象引用表，此表存对象引用用于映射

    public static void Register(string id , PropertyBag bag , GameObject instance) // 并行注册方法，注入字典的是这个PropertyBag对象，属性是规定好的
    {
        if (string.IsNullOrEmpty(id)) return; // 防止重复写入
        ScenePropertyRegistry._bags[id] = bag;
        ScenePropertyRegistry._instance[id] = instance; // 同 ID 覆盖
    }

    public static void Unregister(string id) // 并行注销方法
    {
        ScenePropertyRegistry._bags.Remove(id);
        ScenePropertyRegistry._instance.Remove(id);
    } 

    //对象引用对外方法
    public static T GetInstance<T>(string id) where T : Component
        => ScenePropertyRegistry._instance.TryGetValue(id, out var go) && go ? go.GetComponent<T>() : null;
    
    public static bool TryGet(string id, out PropertyBag bag)// 查询某对象是否为注册对象，true则返回这个对象的bag，false则非法。
        => ScenePropertyRegistry._bags.TryGetValue(id, out bag);

    // —— 便捷读写 ——

    public static T Get<T>(string id, string key, T defaultValue = default)
        => ScenePropertyRegistry._bags.TryGetValue(id, out var bag) ? bag.Get(key, defaultValue) : defaultValue;// 直接按照id在字典内查询对象包内的值

    public static void Set<T>(string id, string key, T value)// 直接按照id修改字典内对象包内的值
    {
        if (ScenePropertyRegistry._bags.TryGetValue(id, out var bag))
            bag.Set(key, value);
        else
            Debug.LogWarning($"[Registry] 未找到对象: {id}");
    }

    public static IEnumerable<KeyValuePair<string, object>> ErgodicGet(string id)
    {
        if (ScenePropertyRegistry.TryGet(id, out PropertyBag bag))
        {
            foreach (KeyValuePair<string, object> kvp in bag)
            {
                yield return kvp;
            }
        }
        
        
        /* 提供对象属性包的遍历取出方法
         foreach (var pair in ScenePropertyRegistry.ErgodicGet("0001"))
            {
                Debug.Log(pair);
            }
         */
    }

    public static string FindIdByName(string name)
    {
        foreach (var kvp in ScenePropertyRegistry._bags)
        {
            string id = kvp.Key;
            PropertyBag bag = kvp.Value;

            // 假设 PropertyBag 有 Get<string> 方法
            string currentName = bag.Get<string>("Name");

            if (currentName == name)
            {
                return id;
            }
        }
        Debug.Log(name + "的 ID 是空的");
        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => _bags.Clear(); // 动态释放
}