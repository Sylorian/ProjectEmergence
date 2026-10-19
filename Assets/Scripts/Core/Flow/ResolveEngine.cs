using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    public enum ResolveOutcome
    {
        /// <summary>命中且规则生效。</summary>
        Succeed,
        /// <summary>目标命中但前置不满足。</summary>
        TagFail,
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
    public sealed class ResolveEngine
    {
        private readonly GameDatabase _db;

        public ResolveEngine(GameDatabase db)
        {
            _db = db ?? new GameDatabase();
        }

        public ResolveResult Resolve(int verbId, int entityId, GameState state)
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
                result.Feedback = r.FeedbackFail ?? GameConfig.DEFAULT_FALLBACK;
                if (e.HasTag(r.TargetTag) != r.TagExist) continue;
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
            e.RemoveTag(rule.TargetTag);
            e.AddTag(rule.TagChanged);

            ExecuteEffect(rule.Effect);
        }

        private void ExecuteEffect(string effect)
        {
            // TODO: Implement effect execution logic here.
        }

        /// <summary>某动词当前可操作的目标实体（任一规则 target 命中），供状态 2 高亮用。</summary>
        public List<Entity> GetCandidateTargets(int verbId, GameState state)
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
