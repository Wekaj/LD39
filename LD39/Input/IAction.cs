using SFML.Window;
using System;

namespace LD39.Input
{
    internal interface IAction
    {
        event EventHandler Pressed;
        event EventHandler Released;

        bool IsHeld { get; }

        void Update();
    }
}
