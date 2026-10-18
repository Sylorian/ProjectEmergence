using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注册除实体外的条目（动词和规则）
/// </summary>
public class RegistryEntry : MonoBehaviour
{
    [Serializable] private struct BoolEntry   { public string key; public bool   value; } // 是否可被检查
    [Serializable] private struct StringEntry { public string key; public string value; } // 对象名字
    [Serializable] private struct FloatEntry  { public string key; public float  value; } // 对象序列

    [Serializable] private struct RegistryEntryVerb
    {
        public string Id;
        public List<BoolEntry> boolEntries;
        public List<StringEntry> stringEntries;
        public List<FloatEntry> floatEntries;
    }

    [Serializable] private struct RegistryEntryRule
    {
        public string Id;
        public List<BoolEntry> boolEntries;
        public List<StringEntry> stringEntries;
        public List<FloatEntry> floatEntries;
    }

    [Header("动词注册表")]
    [SerializeField] private List<RegistryEntryVerb> _registryEntryVerbs;

    [Header("规则注册表")]
    [SerializeField] private List<RegistryEntryRule> _registryEntryRules;

    private void OnEnable()
    {
        foreach (var verb in _registryEntryVerbs)
        {
            var bag = new PropertyBag();
            foreach (var entry in verb.boolEntries) bag.Set(entry.key, entry.value);
            foreach (var entry in verb.stringEntries) bag.Set(entry.key, entry.value);
            foreach (var entry in verb.floatEntries) bag.Set(entry.key, entry.value);
            ScenePropertyRegistry.Register(verb.Id, bag);
        }

        foreach (var rule in _registryEntryRules)
        {
            var bag = new PropertyBag();
            foreach (var entry in rule.boolEntries) bag.Set(entry.key, entry.value);
            foreach (var entry in rule.stringEntries) bag.Set(entry.key, entry.value);
            foreach (var entry in rule.floatEntries) bag.Set(entry.key, entry.value);
            ScenePropertyRegistry.Register(rule.Id, bag);
        }
    }

    private void OnDisable()
    {
        foreach (var verb in _registryEntryVerbs)
        {
            ScenePropertyRegistry.Unregister(verb.Id);
        }

        foreach (var rule in _registryEntryRules)
        {
            ScenePropertyRegistry.Unregister(rule.Id);
        }
    }
}