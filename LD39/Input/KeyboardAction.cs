using SFML.Window;
using System;
using System.Collections.Generic;

namespace LD39.Input
{
    internal sealed class KeyboardAction : IAction
    {
        private readonly HashSet<Keyboard.Key> _keys;

        public KeyboardAction(IEnumerable<Keyboard.Key> keys)
        {
            _keys = new HashSet<Keyboard.Key>(keys);
        }

        public KeyboardAction(params Keyboard.Key[] keys)
            : this((IEnumerable<Keyboard.Key>)keys)
        {
        }

        public event EventHandler Pressed;
        public event EventHandler Released;

        public bool IsHeld { get; private set; }

        public void Update()
        {
            bool wasHeld = IsHeld;

            IsHeld = false;
            foreach (Keyboard.Key key in _keys)
                if (Keyboard.IsKeyPressed(key))
                {
                    IsHeld = true;
                    break;
                }

            if (!wasHeld && IsHeld)
                Pressed?.Invoke(this, EventArgs.Empty);
            else if (wasHeld && !IsHeld)
                Released?.Invoke(this, EventArgs.Empty);
        }
    }
}
