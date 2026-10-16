using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Data
{
    public class Entity
    {
        public string Id;
        public string Name;
        public string Room;
        public string[] Tags;
        public string[] State;
        public bool Pickable;
        public string Clue;

        public Entity(string id = "", string name = "", string room = "",
            string[] tags = null, string[] state = null, bool pickable = false, string clue = "")
        {
            Id = id;
            Name = name;
            Room = room;
            Tags = tags ?? Array.Empty<string>();
            State = state ?? Array.Empty<string>();
            Pickable = pickable;
            Clue = clue;
        }
    }

    public class Verb
    {
        public string Id;
        public string Name;
        public string[] Aliases;

        public Verb(string id = "", string name = "", string[] aliases = null)
        {
            Id = id;
            Name = name;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }

    public sealed class GameDatabase
    {
        public readonly List<Entity> Entities = new List<Entity>();
        public readonly Dictionary<string, Entity> EntityById = new Dictionary<string, Entity>(StringComparer.Ordinal);
        public readonly List<Verb> Verbs = new List<Verb>();
        public readonly Dictionary<string, Verb> VerbById = new Dictionary<string, Verb>(StringComparer.Ordinal);
    }
}