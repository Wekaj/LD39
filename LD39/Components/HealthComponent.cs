using Artemis.Interface;
using SFML.System;

namespace LD39.Components
{
    internal sealed class HealthComponent : IComponent
    {
        public HealthComponent(int health, float height)
        {
            MaxHealth = health;
            Health = health;
            Height = height;
        }

        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public float Height { get; set; }
        public Time HitTimer { get; set; }
    }
}
