using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Data
{
    /// <summary>实体（物品 / 场景对象）。Pinyin 为名字拼音，供拼音候选匹配（可缺省）。</summary>
    public class Entity
    {
        public string Id;
        public string Name;
        public string Pinyin;
        public string Room;
        public string[] Tags;
        public string[] State;
        public bool Pickable;
        public string Clue;

        public Entity(string id = "", string name = "", string room = "",
            string[] tags = null, string[] state = null, bool pickable = false, string clue = "",
            string pinyin = null)
        {
            Id = id;
            Name = name;
            Pinyin = pinyin;
            Room = room;
            Tags = tags ?? Array.Empty<string>();
            State = state ?? Array.Empty<string>();
            Pickable = pickable;
            Clue = clue;
        }

        public bool HasTag(string tag)
        {
            if (Tags == null || tag == null) return false;
            for (int i = 0; i < Tags.Length; i++) if (Tags[i] == tag) return true;
            return false;
        }
    }

    /// <summary>动词。AliasPinyin 与 Aliases 一一对应（可短于 Aliases）。</summary>
    public class Verb
    {
        public string Id;
        public string Name;
        public string Pinyin;
        public string[] Aliases;
        public string[] AliasPinyin;

        public Verb(string id = "", string name = "", string[] aliases = null,
            string pinyin = null, string[] aliasPinyin = null)
        {
            Id = id;
            Name = name;
            Pinyin = pinyin;
            Aliases = aliases ?? Array.Empty<string>();
            AliasPinyin = aliasPinyin ?? Array.Empty<string>();
        }

        public string AliasPinyinAt(int index)
        {
            if (AliasPinyin == null || index < 0 || index >= AliasPinyin.Length) return null;
            return AliasPinyin[index];
        }
    }

    /// <summary>全局配置（对应 JSON 的 config 段）。</summary>
    public sealed class GameConfig
    {
        public float InkMax = 60f;
        public float InkCostPerWord = 1f;
        public float GrowthRatePerSec = 0.5f;
        public float GrowthThreshold = 10f;
        public string DefaultFallback = "没有发生什么。也许换个说法？";
    }

    /// <summary>规则的目标匹配器（对应 rule.target）。null 表示该维度不约束。</summary>
    public sealed class RuleTarget
    {
        public string TargetId;       // target.id
        public string[] HasTags;      // target.has_tags：全部命中才算匹配
        public string[] HasState;     // target.has_state：全部命中才算匹配
        public bool? IsPickable;      // target.is_pickable

        public bool HasConstraints
        {
            get
            {
                return TargetId != null
                    || (HasTags != null && HasTags.Length > 0)
                    || (HasState != null && HasState.Length > 0)
                    || IsPickable.HasValue;
            }
        }
    }

    /// <summary>规则前置（对应 rule.pre）。null 表示该维度不约束。</summary>
    public sealed class RulePrecondition
    {
        public string[] PlayerHas;    // pre.player_has：全部持有才算满足
        public string Flag;           // pre.flag：该 flag 已设置才算满足
        public string Password;       // pre.password：需与 GameState.PendingPassword 相同

        public bool IsEmpty
        {
            get
            {
                return (PlayerHas == null || PlayerHas.Length == 0)
                    && Flag == null && Password == null;
            }
        }
    }

    /// <summary>规则效果（对应 rule.effects[] 里的一个对象，多个字段可同时出现）。</summary>
    public sealed class Effect
    {
        public bool RevealClue;                 // reveal_clue
        public bool AddToInventory;             // add_to_inventory
        public string SetFlag;                  // set_flag
        public string SpawnItem;                // spawn_item
        public string[] TargetStateAdd;         // target_state_add
        public string PlayerItemStateTarget;    // player_item_state_add（目标物品 id）
        public string[] PlayerItemStateAdd;     // state（配合 player_item_state_add）
        public bool TriggerGrowth;              // trigger_growth

        public bool IsEmpty
        {
            get
            {
                return !RevealClue && !AddToInventory && SetFlag == null && SpawnItem == null
                    && (TargetStateAdd == null || TargetStateAdd.Length == 0)
                    && PlayerItemStateTarget == null
                    && (PlayerItemStateAdd == null || PlayerItemStateAdd.Length == 0)
                    && !TriggerGrowth;
            }
        }
    }

    /// <summary>一条搭配规则（对应 rules[] 的一项）。</summary>
    public sealed class Rule
    {
        public string Id;
        public string VerbId;
        public RuleTarget Target = new RuleTarget();
        public RulePrecondition Pre = new RulePrecondition();
        public Effect[] Effects = Array.Empty<Effect>();
        public string Feedback;
        public string FailFeedback;   // 可缺省；缺省时用通用兜底文案
    }

    /// <summary>结局（对应 endings[] 的一项）。</summary>
    public sealed class Ending
    {
        public string Id;
        public string[] Flags;
        public string Text;

        public Ending(string id = "", string[] flags = null, string text = "")
        {
            Id = id;
            Flags = flags ?? Array.Empty<string>();
            Text = text;
        }
    }

    public sealed class GameDatabase
    {
        public readonly GameConfig Config = new GameConfig();
        public readonly List<Entity> Entities = new List<Entity>();
        public readonly Dictionary<string, Entity> EntityById = new Dictionary<string, Entity>(StringComparer.Ordinal);
        public readonly List<Verb> Verbs = new List<Verb>();
        public readonly Dictionary<string, Verb> VerbById = new Dictionary<string, Verb>(StringComparer.Ordinal);
        public readonly List<Rule> Rules = new List<Rule>();
        public readonly Dictionary<string, List<Rule>> RulesByVerb = new Dictionary<string, List<Rule>>(StringComparer.Ordinal);
        public readonly List<Ending> Endings = new List<Ending>();
    }
}