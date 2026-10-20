using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    /// <summary>运行时世界状态：背包 / 玩家房间 / 墨盒数量。</summary>
    public sealed class GameSession
    {
        public readonly GameDatabase Database;
        public readonly TyperEngine Typer;

        private readonly Dictionary<int, Entity> _inventory = new Dictionary<int, Entity>();

        private string _currentRoom;
        public string CurrentRoom { get { return _currentRoom; } set { _currentRoom = value; } }
        private float _ink;
        public float Ink { get { return _ink; } set { _ink = value; } }
        private readonly List<string> _history = new List<string>();

        /// <summary>初始实体状态取自 GameDatabase；startRoom 缺省取第一个实体的房间。</summary>
        public GameSession(GameDatabase db)
        {
            Database = db;
            Typer = new TyperEngine(db);

            CurrentRoom = GameConfig.START_ROOM;
        }

        // —— inventory ——
        public bool HasInventoryItem(int id) { return _inventory.ContainsKey(id); }
        public void AddInventoryItem(int id, Entity entity) { _inventory[id] = entity; }
        public void RemoveInventoryItem(int id) { _inventory.Remove(id); }
        public IEnumerable<int> Inventory { get { return _inventory.Keys; } }
    }
}