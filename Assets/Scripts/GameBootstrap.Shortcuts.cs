using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.Presenter;

namespace Hyperplasia.Runtime
{
    public partial class GameBootstrap
    {
        public static TyperPresenter TyperPresenter => Instance._typerPresenter;
        public static WorldPresenter WorldPresenter => Instance._worldPresenter;
        public static GameSession Session => Instance._session;
        public static GameDatabase Database => Session?.Database;
        public static TyperEngine TyperEngine => Session?.TyperEngine;
        public static ResolveEngine ResolveEngine => TyperEngine.Resolver;
        public static InputController Controller => Instance._controller;

        public static bool TryGetEntity(int id, out Entity e) => Database.TryGetEntity(id, out e);
        public static bool TryGetVerb(int id, out Verb v) => Database.TryGetVerb(id, out v);
        public static bool TryGetRule(int verbId, out List<Rule> rules) => Database.TryGetRule(verbId, out rules);
    }
}