using System.Collections.Generic;

namespace LD39.Input
{
    internal sealed class ActionManager
    {
        private readonly Dictionary<ActionID, IAction> _actions = new Dictionary<ActionID, IAction>();

        public IAction this[ActionID id] => _actions[id];

        public void Add(ActionID id, IAction action)
        {
            _actions.Add(id, action);
        }

        public void Update()
        {
            foreach (IAction action in _actions.Values)
                action.Update();
        }
    }
}
