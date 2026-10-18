using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Data
{
    /// <summary>
    /// 把 game_data_sample.json 这类格式的文本解析成 GameDatabase。
    /// 只用 Hyperplasia.Core 自带的 JsonParser，不引用 UnityEngine；字段缺省即视为无约束/无效果。
    /// </summary>
    public sealed class GameDataLoader
    {
        public GameDatabase Load(string jsonText)
        {
            return Load(JsonParser.Parse(jsonText));
        }

        public GameDatabase Load(JsonValue root)
        {
            var db = new GameDatabase();
            LoadConfig(root, db);
            LoadEntities(root, db);
            LoadVerbs(root, db);
            LoadRules(root, db);
            LoadEndings(root, db);
            return db;
        }

        private static void LoadConfig(JsonValue root, GameDatabase db)
        {
            JsonValue c = root["config"];
            if (!c.IsObject) return;
            db.Config.InkMax = c["ink_max"].AsFloat(db.Config.InkMax);
            db.Config.InkCostPerWord = c["ink_cost_per_word"].AsFloat(db.Config.InkCostPerWord);
            db.Config.GrowthRatePerSec = c["growth_rate_per_sec"].AsFloat(db.Config.GrowthRatePerSec);
            db.Config.GrowthThreshold = c["growth_threshold"].AsFloat(db.Config.GrowthThreshold);
            string fb = c["default_fallback"].AsNonEmptyString();
            if (fb != null) db.Config.DefaultFallback = fb;
        }

        private static void LoadEntities(JsonValue root, GameDatabase db)
        {
            foreach (JsonValue e in root["entities"].AsList())
            {
                string id = e["id"].AsNonEmptyString();
                if (id == null) continue;
                var ent = new Entity(
                    id,
                    e["name"].AsString(""),
                    e["room"].AsNonEmptyString(),
                    e["tags"].AsStringArray(),
                    e["state"].AsStringArray(),
                    e["pickable"].AsBool(),
                    e["clue"].AsString(""),
                    e["pinyin"].AsNonEmptyString());
                db.Entities.Add(ent);
                db.EntityById[ent.Id] = ent;
            }
        }

        private static void LoadVerbs(JsonValue root, GameDatabase db)
        {
            foreach (JsonValue v in root["verbs"].AsList())
            {
                string id = v["id"].AsNonEmptyString();
                if (id == null) continue;
                var verb = new Verb(
                    id,
                    v["name"].AsString(""),
                    v["aliases"].AsStringArray(),
                    v["pinyin"].AsNonEmptyString(),
                    v["alias_pinyin"].AsStringArray());
                db.Verbs.Add(verb);
                db.VerbById[verb.Id] = verb;
            }
        }

        private static void LoadRules(JsonValue root, GameDatabase db)
        {
            foreach (JsonValue r in root["rules"].AsList())
            {
                string id = r["id"].AsNonEmptyString();
                string verbId = r["verb"].AsNonEmptyString();
                if (id == null || verbId == null) continue;

                var rule = new Rule
                {
                    Id = id,
                    VerbId = verbId,
                    Target = ParseTarget(r["target"]),
                    Pre = ParsePre(r["pre"]),
                    Effects = ParseEffects(r["effects"]),
                    Feedback = r["feedback"].AsNonEmptyString(),
                    FailFeedback = r["fail_feedback"].AsNonEmptyString(),
                };
                db.Rules.Add(rule);

                List<Rule> list;
                if (!db.RulesByVerb.TryGetValue(verbId, out list))
                {
                    list = new List<Rule>();
                    db.RulesByVerb[verbId] = list;
                }
                list.Add(rule);
            }
        }

        private static RuleTarget ParseTarget(JsonValue t)
        {
            var target = new RuleTarget();
            if (!t.IsObject) return target;
            target.TargetId = t["id"].AsNonEmptyString();
            target.HasTags = t["has_tags"].AsStringArray();
            target.HasState = t["has_state"].AsStringArray();
            target.IsPickable = t["is_pickable"].AsBoolOrNull();
            return target;
        }

        private static RulePrecondition ParsePre(JsonValue p)
        {
            var pre = new RulePrecondition();
            if (!p.IsObject) return pre;
            pre.PlayerHas = p["player_has"].AsStringArray();
            pre.Flag = p["flag"].AsNonEmptyString();
            pre.Password = p["password"].AsNonEmptyString();
            return pre;
        }

        private static Effect[] ParseEffects(JsonValue effects)
        {
            List<JsonValue> items = effects.AsList();
            if (items == null || items.Count == 0) return Array.Empty<Effect>();
            var list = new List<Effect>(items.Count);
            for (int i = 0; i < items.Count; i++) list.Add(ParseEffect(items[i]));
            return list.ToArray();
        }

        private static Effect ParseEffect(JsonValue e)
        {
            var ef = new Effect();
            if (!e.IsObject) return ef;
            ef.RevealClue = e["reveal_clue"].AsBool();
            ef.AddToInventory = e["add_to_inventory"].AsBool();
            ef.SetFlag = e["set_flag"].AsNonEmptyString();
            ef.SpawnItem = e["spawn_item"].AsNonEmptyString();
            ef.TargetStateAdd = e["target_state_add"].AsStringArray();
            ef.PlayerItemStateTarget = e["player_item_state_add"].AsNonEmptyString();
            ef.PlayerItemStateAdd = e["state"].AsStringArray();
            ef.TriggerGrowth = e["trigger_growth"].AsBool();
            return ef;
        }

        private static void LoadEndings(JsonValue root, GameDatabase db)
        {
            foreach (JsonValue en in root["endings"].AsList())
            {
                string id = en["id"].AsNonEmptyString();
                if (id == null) continue;
                db.Endings.Add(new Ending(id, en["flags"].AsStringArray(), en["text"].AsString("")));
            }
        }
    }
}