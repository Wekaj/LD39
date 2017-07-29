using Artemis;
using LD39.Components;
using LD39.Extensions;
using LD39.Input;
using SFML.Graphics;
using SFML.System;

namespace LD39.Systems
{
    internal sealed class CharacterMovementSystem : EntityUpdatingSystem
    {
        private const float _speed = 50f;
        private readonly ActionManager _actions;

        public CharacterMovementSystem(ActionManager actions) 
            : base(Aspect.All(typeof(CharacterComponent), typeof(VelocityComponent)))
        {
            _actions = actions;
        }

        public override void Process(Entity entity)
        {
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();

            Vector2f movement = new Vector2f();
            if (_actions[ActionID.MoveRight].IsHeld)
                movement.X++;
            if (_actions[ActionID.MoveLeft].IsHeld)
                movement.X--;
            if (_actions[ActionID.MoveDown].IsHeld)
                movement.Y++;
            if (_actions[ActionID.MoveUp].IsHeld)
                movement.Y--;

            velocityComponent.Velocity = movement.Normalize() * _speed;

            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

            if (movement.Y > 0f)
                spriteComponent.Sprite.TextureRect = new IntRect(0, 0, 16, 32);
            else if (movement.Y < 0f)
                spriteComponent.Sprite.TextureRect = new IntRect(0, 64, 16, 32);
            else if (movement.X > 0f)
                spriteComponent.Sprite.TextureRect = new IntRect(0, 32, 16, 32);
            else if (movement.X < 0f)
                spriteComponent.Sprite.TextureRect = new IntRect(0, 96, 16, 32);
        }
    }
}
