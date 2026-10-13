using System.Collections;              // 非泛型 IEnumerator
using System.Collections.Generic;      // Dictionary、IEnumerable<>、KeyValuePair<>

public class PropertyBag:IEnumerable<KeyValuePair<string, object>>
{
    private readonly Dictionary<string, object> values = new();
    
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        => values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => values.GetEnumerator();

    public void Set<T>(string key, T value) => values[key] = value; // T是参数类型，意思是可以自动判断value的类型然后存入

    public T Get<T>(string key, T defaultValue = default) // T是返回值的类型，因为这个方法没有声明void所以需要对返回值规定，T的意思是自动判断返回值的数据类型
    {
        if (values.TryGetValue(key, out var v) && v is T t) //out var v 是对这个字典查找的key，如果存在，赋值给v，var可以自动判断key的参数类型，&&短路运算做类型写入（V->T）
            return t;
        return defaultValue;   // 类型不匹配或不存在时返回默认值
    }

    public bool Has(string key) => values.ContainsKey(key); // 提供Has方法查找字典中是否包含所查元素

    public IEnumerable<string> Keys => values.Keys;//遍历所有键返回
}