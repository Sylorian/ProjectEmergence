using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Bridge
{
    /// <summary>
    /// 把 game_data_sample.json 这类格式的文本解析成 GameDatabase。
    /// 只用 Hyperplasia.Core 自带的 JsonParser，不引用 UnityEngine；字段缺省即视为无约束/无效果。
    /// </summary>
    public sealed class GameDataLoader
    {
        public GameDatabase Load(GameData root)
        {
            var db = new GameDatabase();
            LoadConfig(db);
            LoadGameData(root, db);
            return db;
        }

        private static void LoadConfig(GameDatabase db)
        {
        }

        private static void LoadGameData(GameData root, GameDatabase db)
        {
            LoadEntities(root, db);
            LoadVerbs(root, db);
            LoadRules(root, db);
        }

        private static void LoadEntities(GameData root, GameDatabase db)
        {
            foreach (var e in root.entities)
            {
                var ent = new Entity(
                    e.id,
                    e.name,
                    e.pinyin,
                    e.room,
                    e.tags,
                    e.clues);
                db.Entities.Add(ent);
                db.EntityById[ent.Id] = ent;
            }
        }

        private static void LoadVerbs(GameData root, GameDatabase db)
        {
            foreach (var v in root.verbs)
            {
                var verb = new Verb(
                    v.id,
                    v.name,
                    v.pinyin);
                db.Verbs.Add(verb);
                db.VerbById[verb.Id] = verb;
            }
        }

        private static void LoadRules(GameData root, GameDatabase db)
        {
            foreach (var r in root.rules)
            {
                var rule = new Rule(
                    r.verb_id,
                    r.target_id,
                    r.require_tags,
                    r.remove_tags,
                    r.add_tags,
                    r.require_inventory_item,
                    r.require_inventory_item_tag,
                    r.require_flag,
                    r.set_flag,
                    r.spawn_item,
                    r.effect,
                    r.feedback,
                    r.feedback_fail_tag,
                    r.feedback_fail_inventory,
                    r.feedback_fail_inventory_tag,
                    r.feedback_fail_flag
                );
                db.Rules.Add(rule);
                if (!db.RulesByVerb.ContainsKey(rule.VerbId))
                {
                    db.RulesByVerb[rule.VerbId] = new List<Rule>();
                }
                db.RulesByVerb[rule.VerbId].Add(rule);
            }
        }
    }
}