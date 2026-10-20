using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Data
{
    /// <summary>实体（物品 / 场景对象）。Pinyin 为名字拼音，供拼音候选匹配（可缺省）。</summary>
    public class Entity
    {
        /// <summary>
        /// 物品的数字编号
        /// </summary>
        private int _id;
        public int Id => _id;
        /// <summary>
        /// 物品的中文名称
        /// </summary>
        private string _name;
        public string Name => _name;
        /// <summary>
        /// 物品中文名称的拼音
        /// </summary>
        private string _pinyin;
        public string Pinyin => _pinyin;
        /// <summary>
        /// 物品所在房间的中文名称
        /// </summary>
        private string _room;
        public string Room => _room;
        /// <summary>
        /// 物品初始持有的标签
        /// </summary>
        private List<string> _tags;
        public List<string> Tags => _tags;
        /// <summary>
        /// 查看物品时显示的文案
        /// </summary>
        private string[] _clues;
        public string[] Clues => _clues;
        
        public Entity(
            int id = 0,
            string name = "",
            string pinyin = null,
            string room = "",
            List<string> tags = null,
            string[] clues = null
        )
        {
            _id = id;
            _name = name;
            _pinyin = pinyin;
            _room = room;
            _tags = tags ?? new List<string>();
            _clues = clues ?? Array.Empty<string>();
        }

        public bool HasTag(string tag)
        {
            if (tag == "") return true;
            return _tags.Contains(tag);
        }

        public bool HasTags(string[] tags)
        {
            if (tags == null || tags.Length == 0) return true;
            foreach (var tag in tags)
            {
                if (!HasTag(tag)) return false;
            }
            return true;
        }

        public bool AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            if (_tags.Contains(tag)) return false;
            _tags.Add(tag);
            return true;
        }

        public bool AddTags(string[] tags)
        {
            if (tags == null || tags.Length == 0) return false;
            bool added = false;
            foreach (var tag in tags)
            {
                added |= AddTag(tag);
            }
            return added;
        }

        public bool RemoveTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            if (!_tags.Contains(tag)) return false;
            _tags.Remove(tag);
            return true;
        }

        public bool RemoveTags(string[] tags)
        {
            if (tags == null || tags.Length == 0) return false;
            bool removed = false;
            foreach (var tag in tags)
            {
                removed |= RemoveTag(tag);
            }
            return removed;
        }
    }

    /// <summary>动词。</summary>
    public class Verb
    {
        /// <summary>
        /// 动词的数字编号，同义动词采用相同编号
        /// </summary>
        private int _id;
        public int Id => _id;

        /// <summary>
        /// 动词的中文名称
        /// </summary>
        private string _name;
        public string Name => _name;

        /// <summary>
        /// 动词中文的拼音
        /// </summary>
        private string _pinyin;
        public string Pinyin => _pinyin;

        public Verb(
            int id = 0,
            string name = "",
            string pinyin = null
        )
        {
            _id = id;
            _name = name;
            _pinyin = pinyin;
        }
    }

    /// <summary>一条搭配规则（对应 rules[] 的一项）。</summary>
    public sealed class Rule
    {
    /// <summary>
    /// 动词的编号，与verbs表中的编号一致
    /// </summary>
    private int _verb_id;
    public int VerbId => _verb_id;
    /// <summary>
    /// 对象物品的编号，与entities表中的编号一致
    /// </summary>
    private int _target_id;
    public int TargetId => _target_id;
    /// <summary>
    /// 动作目标物品需要持有的标签。没有则留空。
    /// </summary>
    private string[] _require_tags;
    public string[] RequireTags => _require_tags;
    /// <summary>
    /// 要移除的标签。没有则留空给。
    /// </summary>
    private string[] _remove_tags;
    public string[] RemoveTags => _remove_tags;
    /// <summary>
    /// 增加的标签。没有则留空。
    /// </summary>
    private string[] _add_tags;
    public string[] AddTags => _add_tags;
    /// <summary>
    /// 动作所需物品编号，即玩家背包中必须持有的物品。没有则留空。
    /// </summary>
    private int _require_inventory_item;
    public int RequireInventoryItem => _require_inventory_item;
    /// <summary>
    /// 要求背包中的该物品带有此标签。没有则留空。
    /// </summary>
    private string _require_inventory_item_tag;
    public string RequireInventoryItemTag => _require_inventory_item_tag;
    /// <summary>
    /// 前置全局flag，满足才检定成功。没有则留空。
    /// </summary>
    private string _require_flag;
    public string RequireFlag => _require_flag;
    /// <summary>
    /// 成功后设置的全局flag。没有则留空。
    /// </summary>
    private string _set_flag;
    public string SetFlag => _set_flag;
    /// <summary>
    /// 成功后生成/掉落的物品。没有则留空。
    /// </summary>
    private string _spawn_item;
    public string SpawnItem => _spawn_item;
    /// <summary>
    /// 动作产生的除改变标签以外的效果的英文名，采用蛇形命名法snake_case。没有则留空。
    /// </summary>
    private string _effect;
    public string Effect => _effect;
    /// <summary>
    /// 检定成功后打印在打字机上的文案
    /// </summary>
    private string _feedback;
    public string Feedback => _feedback;
    /// <summary>
    /// 物品符合目标，但标签不符合条件时打印在打字机上的文案
    /// </summary>
    private string _feedback_fail_tag;
    public string FeedbackFailTag => _feedback_fail_tag;
    /// <summary>
    /// 物品符合目标，标签也符合目标，但背包物品持有条件不满足时打印的文案
    /// </summary>
    private string _feedback_fail_inventory;
    public string FeedbackFailInventory => _feedback_fail_inventory;
    /// <summary>
    /// 物品及其标签符合目标，手中持有所需物品，但持有物标签不符合要求时打印的文案
    /// </summary>
    private string _feedback_fail_inventory_tag;
    public string FeedbackFailInventoryTag => _feedback_fail_inventory_tag;
    /// <summary>
    /// 前置全局flag条件不满足时打印的文案。没有则留空。
    /// </summary>
    private string _feedback_fail_flag;
    public string FeedbackFailFlag => _feedback_fail_flag;

        public Rule(
            int verb_id,
            int target_id,
            string[] require_tags,
            string[] remove_tags,
            string[] add_tags,
            int require_inventory_item,
            string require_inventory_item_tag,
            string require_flag,
            string set_flag,
            string spawn_item,
            string effect,
            string feedback,
            string feedback_fail_tag,
            string feedback_fail_inventory,
            string feedback_fail_inventory_tag,
            string feedback_fail_flag
        )
        {
            _verb_id = verb_id;
            _target_id = target_id;
            _require_tags = require_tags;
            _remove_tags = remove_tags;
            _add_tags = add_tags;
            _require_inventory_item = require_inventory_item;
            _require_inventory_item_tag = require_inventory_item_tag;
            _require_flag = require_flag;
            _set_flag = set_flag;
            _spawn_item = spawn_item;
            _effect = effect;
            _feedback = feedback;
            _feedback_fail_tag = feedback_fail_tag;
            _feedback_fail_inventory = feedback_fail_inventory;
            _feedback_fail_inventory_tag = feedback_fail_inventory_tag;
            _feedback_fail_flag = feedback_fail_flag;
        }
    }

    public sealed class GameDatabase
    {
        public readonly GameConfig Config = new GameConfig();
        public readonly List<Entity> Entities = new List<Entity>();
        public readonly Dictionary<int, Entity> EntityById = new Dictionary<int, Entity>();
        public readonly List<Verb> Verbs = new List<Verb>();
        public readonly Dictionary<int, Verb> VerbById = new Dictionary<int, Verb>();
        public readonly List<Rule> Rules = new List<Rule>();
        public readonly Dictionary<int, List<Rule>> RulesByVerb = new Dictionary<int, List<Rule>>();

        public bool TryGetEntity(int id, out Entity entity) => EntityById.TryGetValue(id, out entity);
        public bool TryGetVerb(int id, out Verb verb) => VerbById.TryGetValue(id, out verb);
        public bool TryGetRule(int verbId, out List<Rule> rules) => RulesByVerb.TryGetValue(verbId, out rules);
        public bool RemoveEntity(int id)
        {
            if (EntityById.TryGetValue(id, out var entity))
            {
                EntityById.Remove(id);
                Entities.Remove(entity);
                return true;
            }
            return false;
        }
    }
}