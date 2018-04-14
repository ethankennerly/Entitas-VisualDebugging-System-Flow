using System.Collections.Generic;

namespace Entitas {

    public delegate void SystemEntityWillBeExecuted(IEntity entity, ISystem system);
    public static class ObservableSystem {

        public static event SystemEntityWillBeExecuted OnEntityWillBeExecuted;

        public static void Execute(IEntity entity, ISystem system)
        {
            if (OnEntityWillBeExecuted != null) {
                OnEntityWillBeExecuted(entity, system);
            }
        }
    }

    /// For context, please see README.md
    public class ObservableReactiveSystem<TEntity> : ReactiveSystem<TEntity> where TEntity : class, IEntity {

        protected ObservableReactiveSystem(IContext<TEntity> context) : base(context) {
        }

        protected override ICollector<TEntity> GetTrigger(IContext<TEntity> context) {
            return null;
        }

        protected override bool Filter(TEntity entity) {
            return false;
        }

        protected override void Execute(List<TEntity> entities) {
            foreach (TEntity entity in entities)
            {
                ObservableSystem.Execute(entity, this);
            }
        }
    }
}
