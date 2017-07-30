using Artemis;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Input;
using LD39.Resources;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;

namespace LD39.Systems
{
    internal enum Direction
    {
        None,
        Right,
        Down,
        Left,
        Up
    }

    internal sealed class CharacterMovementSystem : EntityUpdatingSystem
    {
        // TODO: add shield. add power (draw battery). add enemies. add collisions.

        private const float _speed = 40f, _acceleration = 200f, _dash = 100f, _slashPower = 100f;
        private readonly ActionManager _actions;
        private readonly FixedFrameAnimation _standingDown, _standingUp, _standingRight, _standingLeft,
            _walkingDown, _walkingUp, _walkingRight, _walkingLeft,
            _turningDownRight, _turningRightDown, _turningDownLeft, _turningLeftDown,
            _turningUpRight, _turningRightUp, _turningUpLeft, _turningLeftUp;
        private readonly FixedFrameAnimation _slashAnimation;
        private readonly TextureLoader _textures;
        private readonly Sound _slashSound, _megaSlashSound, _dashSound, _turnSound;
        private bool _canDash = true;
        private Direction _dashDirection = Direction.None;
        private bool _slash = false;
        private Time _slashCooldown = Time.Zero;

        public CharacterMovementSystem(ActionManager actions, TextureLoader textures, SoundBufferLoader soundBuffers) 
            : base(Aspect.All(typeof(CharacterComponent), typeof(AnimationComponent), typeof(VelocityComponent)))
        {
            _actions = actions;
            _textures = textures;

            _slashSound = new Sound(soundBuffers[SoundBufferID.Slash]);
            _megaSlashSound = new Sound(soundBuffers[SoundBufferID.MegaSlash]) { Volume = 21f };
            _dashSound = new Sound(soundBuffers[SoundBufferID.Dash]) { Volume = 7f };
            _turnSound = new Sound(soundBuffers[SoundBufferID.Turn]) { Volume = 21f };

            _actions[ActionID.MoveDown].Pressed += MoveDown_Pressed;
            _actions[ActionID.MoveUp].Pressed += MoveUp_Pressed;
            _actions[ActionID.MoveRight].Pressed += MoveRight_Pressed;
            _actions[ActionID.MoveLeft].Pressed += MoveLeft_Pressed;
            _actions[ActionID.Attack].Pressed += Attack_Pressed;

            _standingDown = new FixedFrameAnimation(16, 32).AddFrame(0, 0, 1f).AddFrame(1, 0, 0.1f).AddFrame(2, 0, 0.5f).AddFrame(3, 0, 0.1f);
            _standingUp = new FixedFrameAnimation(16, 32).AddFrame(0, 2, 1f).AddFrame(1, 2, 0.1f).AddFrame(2, 2, 0.5f).AddFrame(3, 2, 0.1f);
            _standingRight = new FixedFrameAnimation(16, 32).AddFrame(0, 1, 1f).AddFrame(1, 1, 0.1f).AddFrame(2, 1, 0.5f).AddFrame(3, 1, 0.1f);
            _standingLeft = new FixedFrameAnimation(16, 32).AddFrame(0, 3, 1f).AddFrame(1, 3, 0.1f).AddFrame(2, 3, 0.5f).AddFrame(3, 3, 0.1f);

            _walkingDown = new FixedFrameAnimation(16, 32).AddFrame(0, 4, 1f).AddFrame(1, 4, 1f).AddFrame(2, 4, 1f).AddFrame(3, 4, 1f).AddFrame(4, 4, 1f).AddFrame(5, 4, 1f);
            _walkingUp = new FixedFrameAnimation(16, 32).AddFrame(0, 6, 1f).AddFrame(1, 6, 1f).AddFrame(2, 6, 1f).AddFrame(3, 6, 1f).AddFrame(4, 6, 1f).AddFrame(5, 6, 1f);
            _walkingRight = new FixedFrameAnimation(16, 32).AddFrame(0, 5, 1f).AddFrame(1, 5, 1f).AddFrame(2, 5, 1f).AddFrame(3, 5, 1f).AddFrame(4, 5, 1f).AddFrame(5, 5, 1f);
            _walkingLeft = new FixedFrameAnimation(16, 32).AddFrame(0, 7, 1f).AddFrame(1, 7, 1f).AddFrame(2, 7, 1f).AddFrame(3, 7, 1f).AddFrame(4, 7, 1f).AddFrame(5, 7, 1f);

            _turningDownRight = new FixedFrameAnimation(16, 32).AddFrame(4, 0, 1f).AddFrame(5, 0, 1f);
            _turningRightDown = new FixedFrameAnimation(16, 32).AddFrame(5, 0, 1f).AddFrame(1, 0, 1f);
            _turningDownLeft = new FixedFrameAnimation(16, 32).AddFrame(4, 1, 1f).AddFrame(5, 1, 1f);
            _turningLeftDown = new FixedFrameAnimation(16, 32).AddFrame(5, 1, 1f).AddFrame(4, 1, 1f);

            _turningUpRight = new FixedFrameAnimation(16, 32).AddFrame(4, 2, 1f).AddFrame(5, 2, 1f);
            _turningRightUp = new FixedFrameAnimation(16, 32).AddFrame(5, 2, 1f).AddFrame(1, 2, 1f);
            _turningUpLeft = new FixedFrameAnimation(16, 32).AddFrame(4, 3, 1f).AddFrame(5, 3, 1f);
            _turningLeftUp = new FixedFrameAnimation(16, 32).AddFrame(5, 3, 1f).AddFrame(4, 3, 1f);

            _slashAnimation = new FixedFrameAnimation(32, 32);
            for (int i = 0; i < 15; i++)
                _slashAnimation.AddFrame(i, 0, 1f);
        }

        private void Attack_Pressed(object sender, EventArgs e)
        {
            if (_slashCooldown <= Time.Zero)
            {
                _slash = true;
                _slashCooldown = Time.FromSeconds(0.6f);
            }
        }

        private void MoveLeft_Pressed(object sender, EventArgs e)
        {
            _dashDirection = Direction.Left;
        }

        private void MoveRight_Pressed(object sender, EventArgs e)
        {
            _dashDirection = Direction.Right;
        }

        private void MoveUp_Pressed(object sender, EventArgs e)
        {
            _dashDirection = Direction.Up;
        }

        private void MoveDown_Pressed(object sender, EventArgs e)
        {
            _dashDirection = Direction.Down;
        }

        public override void Process(Entity entity)
        {
            // TODO: move this stuff to the screen class, or break it up into multiple systems.

            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            AnimationComponent animationComponent = entity.GetComponent<AnimationComponent>();

            if (_slashCooldown > DeltaTime)
                _slashCooldown -= DeltaTime;
            else
                _slashCooldown = Time.Zero;

            if (animationComponent.Animation == null)
                animationComponent.Play(_standingDown, Time.FromSeconds(2f), true, true);

            Direction currentDirection = Direction.None;
            if (animationComponent.Animation == _standingDown || animationComponent.Animation == _walkingDown
                || animationComponent.Animation == _turningLeftDown || animationComponent.Animation == _turningRightDown)
                currentDirection = Direction.Down;
            else if (animationComponent.Animation == _standingLeft || animationComponent.Animation == _walkingLeft
                || animationComponent.Animation == _turningDownLeft || animationComponent.Animation == _turningUpLeft)
                currentDirection = Direction.Left;
            else if (animationComponent.Animation == _standingUp || animationComponent.Animation == _walkingUp
                || animationComponent.Animation == _turningLeftUp || animationComponent.Animation == _turningRightUp)
                currentDirection = Direction.Up;
            else if (animationComponent.Animation == _standingRight || animationComponent.Animation == _walkingRight
                || animationComponent.Animation == _turningDownRight || animationComponent.Animation == _turningUpRight)
                currentDirection = Direction.Right;

            CharacterComponent characterComponent = entity.GetComponent<CharacterComponent>();

            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            float velocity = velocityComponent.Velocity.GetLength();

            if (_slash)
            {
                Entity slash = EntityWorld.CreateEntity();
                slash.AddComponent(new PositionComponent(positionComponent.Position));

                slash.AddComponent(new AnimationComponent());
                slash.GetComponent<AnimationComponent>().Play(_slashAnimation, Time.FromSeconds(0.5f), false);
                slash.GetComponent<AnimationComponent>().DestroyAtEnd = true;

                slash.AddComponent(new LockComponent(entity, new Vector2f()));

                slash.AddComponent(new CollisionComponent(12f, false));
                slash.GetComponent<CollisionComponent>().Ignore = entity;
                slash.GetComponent<CollisionComponent>().Collided += (sender, e) => Slash_Collided(entity, e.Entity, currentDirection, velocity > _speed);

                switch (currentDirection)
                {
                    case Direction.Down:
                        slash.AddComponent(new SpriteComponent(new Sprite(_textures[TextureID.SlashDown]) { Position = new Vector2f(-16f, -16f) }, Layer.Player));
                        slash.GetComponent<LockComponent>().Offset = new Vector2f(0f, 9f);
                        break;
                    case Direction.Left:
                        slash.AddComponent(new SpriteComponent(new Sprite(_textures[TextureID.SlashLeft]) { Position = new Vector2f(-16f, -16f) }, Layer.Player));
                        slash.GetComponent<LockComponent>().Offset = new Vector2f(-18f, -6f);
                        break;
                    case Direction.Right:
                        slash.AddComponent(new SpriteComponent(new Sprite(_textures[TextureID.SlashRight]) { Position = new Vector2f(-16f, -16f) }, Layer.Player));
                        slash.GetComponent<LockComponent>().Offset = new Vector2f(18f, -6f);
                        break;
                    case Direction.Up:
                        slash.AddComponent(new SpriteComponent(new Sprite(_textures[TextureID.SlashUp]) { Position = new Vector2f(-16f, -16f) }, Layer.Player));
                        slash.GetComponent<LockComponent>().Offset = new Vector2f(0f, -25f);
                        break;
                }

                characterComponent.Power -= 1f / 14f;
                if (velocity > _speed)
                    _megaSlashSound.Play();
                else
                    _slashSound.Play();
                _slash = false;
            }

            if (animationComponent.Playing)
                if (animationComponent.Animation == _turningDownRight || animationComponent.Animation == _turningRightDown
                    || animationComponent.Animation == _turningDownLeft || animationComponent.Animation == _turningLeftDown
                    || animationComponent.Animation == _turningUpRight || animationComponent.Animation == _turningRightUp
                    || animationComponent.Animation == _turningUpLeft || animationComponent.Animation == _turningLeftUp)
                {
                    if (_dashDirection != Direction.None && _canDash)
                    {
                        if (_dashDirection == currentDirection)
                        {
                            velocityComponent.Velocity = currentDirection.ToVector() * _dash;
                            _dashDirection = Direction.None;
                            _canDash = false;
                            characterComponent.Power -= 1f / 21f;
                            _dashSound.Play();
                            return;
                        }
                    }

                    if (velocity > _acceleration * DeltaTime.AsSeconds())
                        velocity -= _acceleration * DeltaTime.AsSeconds();
                    else
                        velocity = 0f;
                    velocityComponent.Velocity = velocityComponent.Velocity.Normalize() * velocity;
                    return;
                }

            _dashDirection = Direction.None;
            _canDash = true;

            Vector2f movement = new Vector2f();
            if (_actions[ActionID.MoveUp].IsHeld)
                movement.Y--;
            if (_actions[ActionID.MoveDown].IsHeld)
                movement.Y++;
            if (_actions[ActionID.MoveRight].IsHeld)
                movement.X++;
            if (_actions[ActionID.MoveLeft].IsHeld)
                movement.X--;

            if (movement.X != 0f && movement.Y != 0f)
                movement.X = 0f;

            Direction targetDirection = Direction.None;
            if (movement.X > 0f)
                targetDirection = Direction.Right;
            else if (movement.Y > 0f)
                targetDirection = Direction.Down;
            else if (movement.X < 0f)
                targetDirection = Direction.Left;
            else if (movement.Y < 0f)
                targetDirection = Direction.Up;
            
            if (targetDirection != Direction.None && targetDirection != currentDirection)
            {
                IAnimation turningAnimation = null;
                if (currentDirection == Direction.Down && targetDirection == Direction.Left)
                    turningAnimation = _turningDownLeft;
                else if (currentDirection == Direction.Left
                    && (targetDirection == Direction.Down || targetDirection == Direction.Right))
                    turningAnimation = _turningLeftDown;
                else if (currentDirection == Direction.Down
                    && (targetDirection == Direction.Right || targetDirection == Direction.Up))
                    turningAnimation = _turningDownRight;
                else if (currentDirection == Direction.Right
                    && (targetDirection == Direction.Down || targetDirection == Direction.Left))
                    turningAnimation = _turningRightDown;
                else if (currentDirection == Direction.Up
                    && (targetDirection == Direction.Left || targetDirection == Direction.Down))
                    turningAnimation = _turningUpLeft;
                else if (currentDirection == Direction.Left && targetDirection == Direction.Up)
                    turningAnimation = _turningLeftUp;
                else if (currentDirection == Direction.Up && targetDirection == Direction.Right)
                    turningAnimation = _turningUpRight;
                else if (currentDirection == Direction.Right && targetDirection == Direction.Up)
                    turningAnimation = _turningRightUp;
                _turnSound.Play();
                animationComponent.Play(turningAnimation, Time.FromSeconds(0.25f));
                return;
            }

            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();

            if (movement.X != 0f || movement.Y != 0f)
            {
                if (velocity < _speed - _acceleration * DeltaTime.AsSeconds())
                    velocity += _acceleration * DeltaTime.AsSeconds();
                else if (velocity > _speed + _acceleration * DeltaTime.AsSeconds())
                    velocity -= _acceleration * DeltaTime.AsSeconds();
                else
                    velocity = _speed;
                velocityComponent.Velocity = movement.Normalize() * velocity;

                bool keepProgress = animationComponent.Animation == _walkingDown
                    || animationComponent.Animation == _walkingUp
                    || animationComponent.Animation == _walkingRight
                    || animationComponent.Animation == _walkingLeft;
                if (movement.Y > 0f)
                    animationComponent.Play(_walkingDown, Time.FromSeconds(0.8f), true, keepProgress);
                else if (movement.Y < 0f)
                    animationComponent.Play(_walkingUp, Time.FromSeconds(0.8f), true, keepProgress);
                else if (movement.X > 0f)
                    animationComponent.Play(_walkingRight, Time.FromSeconds(0.8f), true, keepProgress);
                else if (movement.X < 0f)
                    animationComponent.Play(_walkingLeft, Time.FromSeconds(0.8f), true, keepProgress);
            }
            else
            {
                if (velocity > _acceleration * DeltaTime.AsSeconds())
                    velocity -= _acceleration * DeltaTime.AsSeconds();
                else
                    velocity = 0f;
                velocityComponent.Velocity = velocityComponent.Velocity.Normalize() * velocity;

                if (currentDirection == Direction.Down)
                    animationComponent.Play(_standingDown, Time.FromSeconds(2f), true);
                else if (currentDirection == Direction.Up)
                    animationComponent.Play(_standingUp, Time.FromSeconds(2f), true);
                else if (currentDirection == Direction.Right)
                    animationComponent.Play(_standingRight, Time.FromSeconds(2f), true);
                else if (currentDirection == Direction.Left)
                    animationComponent.Play(_standingLeft, Time.FromSeconds(2f), true);
            }

            characterComponent.LastVelocity = velocityComponent.Velocity;
        }

        private void Slash_Collided(Entity player, Entity entity, Direction slashDirection, bool mega)
        {
            if (!entity.HasComponent<VelocityComponent>() || !entity.HasComponent<HealthComponent>())
                return;

            HealthComponent healthComponent = entity.GetComponent<HealthComponent>();

            if (healthComponent.HitTimer > Time.Zero)
                return;

            float power = _slashPower;
            int damage = 2;
            if (mega)
            {
                power *= 2f;
                damage *= 2;
            }

            player.GetComponent<CharacterComponent>().Power += mega ? 1f / 21f : 1f / 28f;

            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            velocityComponent.Velocity += slashDirection.ToVector() * power;

            healthComponent.Health -= damage;
            healthComponent.HitTimer += Time.FromSeconds(0.6f);
        }
    }
}
