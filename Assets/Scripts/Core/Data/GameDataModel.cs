namespace Hyperplasia.Core.Data
{
    public class EntityDef
    {
        public string Id;
        public string Name;
        public string Room;
        public string[] Tags;
        public string[] State;
        public bool Pickable;
        public string Clue;
    }

    public class VerbDef
    {
        public string Id;
        public string Name;
        public string[] Aliases;
    }
}