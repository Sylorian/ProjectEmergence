using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Diagnostics;

namespace Hyperplasia.Core.Flow
{
    public enum ResolveOutcome
    {
        /// <summary>命中且规则生效。</summary>
        Succeed,
        /// <summary>目标命中但前置不满足。</summary>
        TagFail,
        /// <summary>目标命中但背包物品持有条件不满足。</summary>
        InventoryFail,
        /// <summary>目标命中但背包物品标签不满足。</summary>
        InventoryTagFail,
        /// <summary>没有任何规则的目标命中。</summary>
        NoMatch
    }

    public sealed class ResolveResult
    {
        public ResolveOutcome Outcome;
        public Rule Rule;
        public string Feedback;
        public readonly List<string> Applied = new List<string>();

        public bool Succeeded => Outcome == ResolveOutcome.Succeed;
    }

    /// <summary>
    /// 查搭配：Resolve(verbId, targetId) 在 rules[verbId] 里按顺序找第一条 target 命中的规则，
    /// 命中且前置满足 → 执行 effects + feedback；命中但前置不满足 → fail_feedback；都不命中 → default_fallback。
    /// </summary>
    public sealed partial class ResolveEngine
    {
        private readonly GameDatabase _db;

        public ResolveEngine(GameDatabase db)
        {
            _db = db ?? new GameDatabase();
        }

        public ResolveResult Resolve(int verbId, int entityId, GameSession session)
        {
            var result = new ResolveResult();

            if (!_db.VerbById.TryGetValue(verbId, out var v) || v == null)
            {
                result.Outcome = ResolveOutcome.NoMatch;
                result.Feedback = GameConfig.DEFAULT_FALLBACK;
                return result;
            }

            if (!_db.EntityById.TryGetValue(entityId, out var e) || e == null)
            {
                result.Outcome = ResolveOutcome.NoMatch;
                result.Feedback = GameConfig.DEFAULT_FALLBACK;
                return result;
            }

            if (!_db.RulesByVerb.TryGetValue(verbId, out List<Rule> rules) || rules.Count == 0)
            {
                result.Outcome = ResolveOutcome.NoMatch;
                result.Feedback = GameConfig.DEFAULT_FALLBACK;
                return result;
            }

            foreach (var r in rules)
            {
                if (r.TargetId != entityId) continue;
                result.Rule = r;
                result.Outcome = ResolveOutcome.TagFail;
                result.Feedback = r.FeedbackFailTag ?? GameConfig.DEFAULT_FALLBACK;
                if (!e.HasTags(r.RequireTags)) continue;
                result.Outcome = ResolveOutcome.InventoryFail;
                result.Feedback = r.FeedbackFailInventory ?? GameConfig.DEFAULT_FALLBACK;
                var hasInventoryItem = session.TryGetInventoryItem(r.RequireInventoryItem, out var ii);
                if (r.RequireInventoryItem != -1 && !hasInventoryItem) continue;
                result.Outcome = ResolveOutcome.InventoryTagFail;
                result.Feedback = r.FeedbackFailInventoryTag ?? GameConfig.DEFAULT_FALLBACK;
                if (!string.IsNullOrEmpty(r.RequireInventoryItemTag) &&
                    (ii == null || !ii.HasTag(r.RequireInventoryItemTag))) continue;
                result.Outcome = ResolveOutcome.Succeed;
                result.Feedback = r.Feedback ?? GameConfig.DEFAULT_FALLBACK;
                Apply(r);
                return result;
            }

            return result;
        }

        private void Apply(Rule rule)
        {
            if (!_db.EntityById.TryGetValue(rule.TargetId, out var e) || e == null) return;
            e.RemoveTags(rule.RemoveTags);
            e.AddTags(rule.AddTags);

            ExecuteEffect(rule);
        }

        private void ExecuteEffect(Rule rule)
        {
            switch (rule.Effect)
            {
                case "turn_on_lamp": TurnOnLamp(rule.TargetId); break;
                case "turn_off_lamp": TurnOffLamp(rule.TargetId); break;
                case "open_wardrobe": OpenWardrobe(rule.TargetId); break;
                case "close_wardrobe": CloseWardrobe(rule.TargetId); break;
                case "talk_to_owl": TalkToOwl(rule.TargetId); break;
                case "sit_on_sofa": SitOnSofa(rule.TargetId); break;
                case "open_storage": OpenStorage(rule.TargetId); break;
                case "break_vase": BreakVase(rule.TargetId); break;
                case "mix_cleaner_can": MixCleanerCan(rule.TargetId); break;
                case "poison_mom": PoisonMom(rule.TargetId); break;
                case "feed_mom": FeedMom(rule.TargetId); break;
                case "disassemble_vent": DisassembleVent(rule.TargetId); break;
                case "answer_phone": AnswerPhone(rule.TargetId); break;
                case "take_knife": TakeKnife(rule.TargetId); break;
                case "stab_mom": StabMom(rule.TargetId); break;
                case "open_door": OpenDoor(rule.TargetId); break;
                case "break_window": BreakWindow(rule.TargetId); break;
                case "escape_window": EscapeWindow(rule.TargetId); break;
                case "light_candle": LightCandle(rule.TargetId); break;
                case "ignite_table": IgniteTable(rule.TargetId); break;
                case "enter_storage_from_living_room": EnterStorageFromLivingRoom(rule.TargetId); break;
                case "enter_living_room_from_storage": EnterLivingRoomFromStorage(rule.TargetId); break;
            }
        }

        /// <summary>某动词当前可操作的目标实体（任一规则 target 命中），供状态 2 高亮用。</summary>
        public List<Entity> GetCandidateTargets(int verbId, GameSession state)
        {
            var list = new List<Entity>();
            if (!_db.RulesByVerb.TryGetValue(verbId, out List<Rule> rules)) return list;

            foreach (Entity e in _db.Entities)
            {
                foreach (var r in rules)
                {
                    if (r.TargetId == e.Id)
                    {
                        list.Add(e);
                    }
                }
            }
            return list;
        }
    }
}
