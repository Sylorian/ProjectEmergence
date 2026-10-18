using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    public enum ResolveOutcome
    {
        /// <summary>命中且规则生效。</summary>
        Applied,
        /// <summary>目标命中但前置不满足。</summary>
        PreFailed,
        /// <summary>没有任何规则的目标命中。</summary>
        NoMatch
    }

    public sealed class ResolveResult
    {
        public ResolveOutcome Outcome;
        public Rule Rule;
        public string Feedback;
        public readonly List<string> Applied = new List<string>();

        public bool Succeeded { get { return Outcome == ResolveOutcome.Applied; } }
    }

    /// <summary>
    /// 查搭配：Resolve(verbId, targetId) 在 rules[verbId] 里按顺序找第一条 target 命中的规则，
    /// 命中且前置满足 → 执行 effects + feedback；命中但前置不满足 → fail_feedback；都不命中 → default_fallback。
    /// </summary>
    public sealed class ResolveEngine
    {
        private const string DefaultPreFail = "还差些什么，现在做不到。";
        private readonly GameDatabase _db;

        public ResolveEngine(GameDatabase db)
        {
            _db = db ?? new GameDatabase();
        }

        public ResolveResult Resolve(string verbId, string targetId, GameState state)
        {
            var result = new ResolveResult();

            List<Rule> rules;
            if (verbId == null || !_db.RulesByVerb.TryGetValue(verbId, out rules) || rules.Count == 0)
            {
                result.Outcome = ResolveOutcome.NoMatch;
                result.Feedback = _db.Config.DefaultFallback;
                return result;
            }

            Rule preFailed = null;
            for (int i = 0; i < rules.Count; i++)
            {
                Rule r = rules[i];
                if (!TargetMatches(r.Target, targetId, state)) continue;

                if (PreSatisfied(r.Pre, state))
                {
                    result.Outcome = ResolveOutcome.Applied;
                    result.Rule = r;
                    result.Feedback = r.Feedback ?? _db.Config.DefaultFallback;
                    Apply(r, targetId, state, result.Applied);
                    return result;
                }

                if (preFailed == null) preFailed = r; // 只记住第一条「目标命中但前置不满足」
            }

            if (preFailed != null)
            {
                result.Outcome = ResolveOutcome.PreFailed;
                result.Rule = preFailed;
                result.Feedback = preFailed.FailFeedback ?? DefaultPreFail;
                return result;
            }

            result.Outcome = ResolveOutcome.NoMatch;
            result.Feedback = _db.Config.DefaultFallback;
            return result;
        }

        /// <summary>目标是否命中：给了约束的维度必须全部满足；完全无约束则视为「任意目标」。</summary>
        public bool TargetMatches(RuleTarget t, string targetId, GameState state)
        {
            if (t == null) return true;

            Entity e;
            _db.EntityById.TryGetValue(targetId ?? string.Empty, out e);

            if (t.TargetId != null && t.TargetId != targetId) return false;
            if (t.IsPickable.HasValue && (e == null || e.Pickable != t.IsPickable.Value)) return false;

            if (t.HasTags != null)
            {
                for (int i = 0; i < t.HasTags.Length; i++)
                    if (e == null || !e.HasTag(t.HasTags[i])) return false;
            }

            if (t.HasState != null)
            {
                for (int i = 0; i < t.HasState.Length; i++)
                    if (!state.EntityHasState(targetId, t.HasState[i])) return false;
            }

            return true;
        }

        public bool PreSatisfied(RulePrecondition p, GameState state)
        {
            if (p == null || p.IsEmpty) return true;

            if (p.PlayerHas != null)
            {
                for (int i = 0; i < p.PlayerHas.Length; i++)
                    if (!state.HasItem(p.PlayerHas[i])) return false;
            }

            if (p.Flag != null && !state.HasFlag(p.Flag)) return false;
            if (p.Password != null && p.Password != state.PendingPassword) return false;

            return true;
        }

        private void Apply(Rule rule, string targetId, GameState state, List<string> log)
        {
            Effect[] effects = rule.Effects;
            if (effects == null) return;

            for (int i = 0; i < effects.Length; i++)
            {
                Effect ef = effects[i];
                if (ef == null) continue;

                if (ef.RevealClue) { state.RevealClue(targetId); log.Add("reveal_clue:" + targetId); }
                if (ef.AddToInventory) { state.AddItem(targetId); log.Add("add_to_inventory:" + targetId); }
                if (ef.SetFlag != null) { state.SetFlag(ef.SetFlag); log.Add("set_flag:" + ef.SetFlag); }
                if (ef.SpawnItem != null) { state.MarkSpawned(ef.SpawnItem); log.Add("spawn_item:" + ef.SpawnItem); }

                if (ef.TargetStateAdd != null)
                {
                    for (int j = 0; j < ef.TargetStateAdd.Length; j++)
                    {
                        state.AddEntityState(targetId, ef.TargetStateAdd[j]);
                        log.Add("target_state_add:" + targetId + "=" + ef.TargetStateAdd[j]);
                    }
                }

                if (ef.PlayerItemStateTarget != null && ef.PlayerItemStateAdd != null)
                {
                    for (int j = 0; j < ef.PlayerItemStateAdd.Length; j++)
                    {
                        state.AddEntityState(ef.PlayerItemStateTarget, ef.PlayerItemStateAdd[j]);
                        log.Add("player_item_state_add:" + ef.PlayerItemStateTarget + "=" + ef.PlayerItemStateAdd[j]);
                    }
                }

                if (ef.TriggerGrowth) { state.TriggerGrowth(); log.Add("trigger_growth"); }
            }
        }

        /// <summary>某动词当前可操作的目标实体（任一规则 target 命中），供状态 2 高亮用。</summary>
        public List<Entity> GetCandidateTargets(string verbId, GameState state)
        {
            var list = new List<Entity>();
            List<Rule> rules;
            if (verbId == null || !_db.RulesByVerb.TryGetValue(verbId, out rules)) return list;

            foreach (Entity e in _db.Entities)
            {
                for (int i = 0; i < rules.Count; i++)
                {
                    if (TargetMatches(rules[i].Target, e.Id, state))
                    {
                        list.Add(e);
                        break;
                    }
                }
            }
            return list;
        }
    }
}
