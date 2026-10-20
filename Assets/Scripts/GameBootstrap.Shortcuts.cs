using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.Presenter;
using UnityEngine;

namespace Hyperplasia.Runtime
{
    public partial class GameBootstrap
    {
        public static TyperPresenter TyperPresenter => Instance != null ? Instance._typerPresenter : null;
        public static WorldPresenter WorldPresenter => Instance != null ? Instance._worldPresenter : null;
        public static GameSession Session => Instance != null ? Instance._session : null;
        public static GameDatabase Database => Session?.Database;
        public static TyperEngine TyperEngine => Session?.TyperEngine;
        public static ResolveEngine ResolveEngine => TyperEngine?.Resolver;
        public static InputController Controller => Instance != null ? Instance._controller : null;

        public static bool TryGetEntity(int id, out Entity e) => Database.TryGetEntity(id, out e);
        public static bool TryGetVerb(int id, out Verb v) => Database.TryGetVerb(id, out v);
        public static bool TryGetRule(int verbId, out List<Rule> rules) => Database.TryGetRule(verbId, out rules);

        public static void Register(int id, GameObject gameObject) => WorldPresenter?.Register(id, gameObject);
        public static void Unregister(int id, GameObject gameObject) => WorldPresenter?.Unregister(id, gameObject);

        public static T GetComponentById<T>(int id) where T : Component => WorldPresenter?.GetComponentById<T>(id);
    }
}