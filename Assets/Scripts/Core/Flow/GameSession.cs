using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    /// <summary>运行时世界状态：背包 / 玩家房间 / 墨盒数量。</summary>
    public sealed class GameSession
    {
        public readonly GameDatabase Database;
        public readonly TyperEngine TyperEngine;

        private readonly Dictionary<int, Entity> _inventory = new Dictionary<int, Entity>();

        private string _currentRoom;
        public string CurrentRoom { get { return _currentRoom; } set { _currentRoom = value; } }
        private float _ink;
        public float Ink { get { return _ink; } set { _ink = value; } }

        /// <summary>初始实体状态取自 GameDatabase；startRoom 缺省取第一个实体的房间。</summary>
        public GameSession(GameDatabase db)
        {
            Database = db;
            TyperEngine = new TyperEngine(db);

            CurrentRoom = GameConfig.START_ROOM;
        }

        // —— inventory ——
        public bool CollectToInventory(int id)
        {
            if (Database.TryGetEntity(id, out var entity))
            {
                Database.RemoveEntity(id);
                AddInventoryItem(id, entity);
                return true;
            }
            return false;
        }
        public bool HasInventoryItem(int id) { return _inventory.ContainsKey(id); }
        private void AddInventoryItem(int id, Entity entity) { _inventory[id] = entity; }
        private void RemoveInventoryItem(int id) { _inventory.Remove(id); }
        public IEnumerable<int> Inventory { get { return _inventory.Keys; } }
    }
}