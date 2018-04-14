using System.Collections.Generic;

/// Coding style follows Entitas-CSharp repo.
namespace Entitas {
    /// For context, please see README.md
    public class ObservableReactiveSystem<TEntity> : ReactiveSystem<TEntity> where TEntity : class, IEntity {

        public delegate void SystemEntityWillBeExecuted(TEntity entity, ReactiveSystem<TEntity> system);

        public static event SystemEntityWillBeExecuted OnEntityWillBeExecuted;

        protected ObservableReactiveSystem(IContext<TEntity> context) : base(context)
        {
        }

        protected override ICollector<TEntity> GetTrigger(IContext<TEntity> context)
        {
            return null;
        }

        protected override bool Filter(TEntity entity)
        {
            return false;
        }

        protected override void Execute(List<TEntity> entities)
        {
            if (OnEntityWillBeExecuted != null)
            {
                foreach (TEntity entity in entities)
                {
                    OnEntityWillBeExecuted(entity, this);
                }
            }
        }
    }
}
