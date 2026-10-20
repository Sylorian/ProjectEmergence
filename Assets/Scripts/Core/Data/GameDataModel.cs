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
        private string _clue;
        public string Clue => _clue;
        
        public Entity(
            int id = 0,
            string name = "",
            string pinyin = null,
            string room = "",
            List<string> tags = null,
            string clue = ""
        )
        {
            _id = id;
            _name = name;
            _pinyin = pinyin;
            _room = room;
            _tags = tags ?? new List<string>();
            _clue = clue;
        }

        public bool HasTag(string tag)
        {
            if (tag == "") return true;
            return _tags.Contains(tag);
        }

        public bool AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            if (_tags.Contains(tag)) return false;
            _tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            if (!_tags.Contains(tag)) return false;
            _tags.Remove(tag);
            return true;
        }
    }

    /// <summary>动词。AliasPinyin 与 Aliases 一一对应（可短于 Aliases）。</summary>
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
        private string _target_tag;
        public string TargetTag => _target_tag;
        /// <summary>
        /// 检定标签的目标存在状态，符合则检定成功，不符合则检定失败。没有则留空。
        /// </summary>
        private bool _tag_exist;
        public bool TagExist => _tag_exist;
        /// <summary>
        /// 标签的改动内容，即将原来的标签改为什么。没有则留空。
        /// </summary>
        private string _tag_changed;
        public string TagChanged => _tag_changed;
        /// <summary>
        /// 动作所需物品，即玩家背包中必须持有的物品。没有则留空。
        /// </summary>
        private string _required_item;
        public string RequiredItem => _required_item;
        /// <summary>
        /// 动作产生的除改变标签以外的效果的英文名，采用蛇形命名法`snake_case`。没有则留空。
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
        private string _feedback_fail;
        public string FeedbackFail => _feedback_fail;

        public Rule(
            int verb_id = 0,
            int target_id = 0,
            string target_tag = null,
            bool tag_exist = false,
            string tag_changed = null,
            string required_item = null,
            string effect = null,
            string feedback = null,
            string feedback_fail = null
        )
        {
            _verb_id = verb_id;
            _target_id = target_id;
            _target_tag = target_tag;
            _tag_exist = tag_exist;
            _tag_changed = tag_changed;
            _required_item = required_item;
            _effect = effect;
            _feedback = feedback;
            _feedback_fail = feedback_fail;
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