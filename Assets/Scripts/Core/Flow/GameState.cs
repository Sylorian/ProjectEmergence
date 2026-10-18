using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    /// <summary>运行时世界状态：flags / 背包 / 已生成物 / 实体状态 / 玩家房间 / ink / growth。</summary>
    public sealed class GameState
    {
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _inventory = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _spawned = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _revealedClues = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _entityStates =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        public string CurrentRoom { get; set; }
        public string PendingPassword { get; set; }
        public float Ink { get; set; }
        public float Growth { get; set; }
        public int GrowthTriggers { get; private set; }
        public int WordCount { get; private set; }

        /// <summary>初始实体状态取自 GameDatabase；startRoom 缺省取第一个实体的房间。</summary>
        public GameState(GameDatabase db = null, string startRoom = null)
        {
            if (db != null)
            {
                foreach (Entity e in db.Entities)
                {
                    if (e.State != null && e.State.Length > 0) EntityState(e.Id).UnionWith(e.State);
                }
                if (startRoom == null && db.Entities.Count > 0) startRoom = db.Entities[0].Room;
                Ink = db.Config.InkMax;
            }
            CurrentRoom = startRoom;
        }

        private HashSet<string> EntityState(string id)
        {
            if (id == null) id = string.Empty;
            HashSet<string> set;
            if (!_entityStates.TryGetValue(id, out set))
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                _entityStates[id] = set;
            }
            return set;
        }

        // —— flags ——
        public bool HasFlag(string f) { return f != null && _flags.Contains(f); }
        public void SetFlag(string f) { if (f != null) _flags.Add(f); }
        public void ClearFlag(string f) { if (f != null) _flags.Remove(f); }
        public IEnumerable<string> Flags { get { return _flags; } }

        // —— inventory ——
        public bool HasItem(string id) { return id != null && _inventory.Contains(id); }
        public void AddItem(string id) { if (id != null) _inventory.Add(id); }
        public void RemoveItem(string id) { if (id != null) _inventory.Remove(id); }
        public IEnumerable<string> Inventory { get { return _inventory; } }

        // —— spawned ——
        public bool IsSpawned(string id) { return id != null && _spawned.Contains(id); }
        public void MarkSpawned(string id) { if (id != null) _spawned.Add(id); }

        // —— clues ——
        public bool IsClueRevealed(string id) { return id != null && _revealedClues.Contains(id); }
        public void RevealClue(string id) { if (id != null) _revealedClues.Add(id); }

        // —— entity states ——
        public bool EntityHasState(string id, string s) { return s != null && EntityState(id).Contains(s); }
        public void AddEntityState(string id, string s) { if (s != null) EntityState(id).Add(s); }
        public IEnumerable<string> EntityStates(string id) { return EntityState(id); }

        public bool EntityHasAllStates(string id, string[] states)
        {
            if (states == null || states.Length == 0) return true;
            HashSet<string> set = EntityState(id);
            for (int i = 0; i < states.Length; i++) if (!set.Contains(states[i])) return false;
            return true;
        }

        // —— counters ——
        public void TriggerGrowth() { GrowthTriggers++; }
        public void CountWord() { WordCount++; }
    }
}