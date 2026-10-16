using System.Linq;

namespace Hyperplasia.Core.Data
{
    public sealed class GameDataLoader
    {
        public GameDatabase Load(string jsonText)
        {
            var root = JsonParser.Parse(jsonText);
            return Load(root);
        }

        public GameDatabase Load(JsonValue root)
        {
            var db = new GameDatabase();

            LoadEntities(root, db);
            LoadVerbs(root, db);

            return db;
        }

        private void LoadEntities(JsonValue root, GameDatabase db)
        {
            JsonValue entities = root["entities"];
            foreach (JsonValue e in entities.AsList())
            {
                var ent = new Entity(
                    e["id"].AsString(),
                    e["name"].AsString(),
                    e["room"].AsString(),
                    e["tags"].AsList().Select(t => t.AsString()).ToArray(),
                    e["state"].AsList().Select(s => s.AsString()).ToArray(),
                    e["pickable"].AsBool(),
                    e["clue"].AsString()
                );
                db.Entities.Add(ent);
                db.EntityById[ent.Id] = ent;
            }
        }

        public void LoadVerbs(JsonValue root, GameDatabase db)
        {
            JsonValue verbs = root["verbs"];
            foreach (JsonValue v in verbs.AsList())
            {
                var verb = new Verb(
                    v["id"].AsString(),
                    v["name"].AsString(),
                    v["aliases"].AsList().Select(a => a.AsString()).ToArray()
                );
                db.Verbs.Add(verb);
                db.VerbById[verb.Id] = verb;
            }
        }
    }
}