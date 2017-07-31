using Artemis;
using Artemis.Manager;
using LD39.Animation;
using LD39.Components;
using LD39.Extensions;
using LD39.Resources;
using LD39.Screens.End;
using LD39.Screens.Recharge;
using LD39.Systems;
using LD39.Tiles;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using TiledSharp;

namespace LD39.Screens.Game
{
    internal sealed class GameScreen : IScreen
    {
        private readonly Context _context;
        private readonly TileMap _background, _foreground;
        private readonly EntityWorld _entityWorld;
        private readonly Entity _character;
        private readonly Sprite _batteryBack, _batteryFill;
        private readonly FixedFrameAnimation _droneAnimation, _stationAnimation;
        private readonly PlayerData _playerData;
        private readonly Sound _cacheGet;
        private float _displayedPower = 1f;
        private int _rechargeStation = -1;
        private ScreenChangeRequest _request = null;
        private readonly Sprite _cache0, _cache1, _cache2;

        public GameScreen(Context context, PlayerData playerData)
        {
            _context = context;
            _playerData = playerData;

            TmxMap map = new TmxMap("Resources/map.tmx");

            int[,] backgroundMap = new int[map.Width, map.Height];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    backgroundMap[x, y] = map.Layers[0].Tiles[x + y * map.Width].Gid - 1;

            bool[,] collisions = new bool[backgroundMap.GetLength(0), backgroundMap.GetLength(1)];
            for (int y = 0; y < collisions.GetLength(1); y++)
                for (int x = 0; x < collisions.GetLength(0); x++)
                    collisions[x, y] = backgroundMap[x, y] == 0 || backgroundMap[x, y] == -1 || backgroundMap[x, y] == 2 || backgroundMap[x, y] == 20;

            _background = new TileMap(_context.Textures[TextureID.Tiles], 16, backgroundMap);
            _foreground = new TileMap(_context.Textures[TextureID.Tiles], 16, new int[backgroundMap.GetLength(0), backgroundMap.GetLength(1)]);

            _cacheGet = new Sound(_context.SoundBuffers[SoundBufferID.CacheGet]);

            _cache0 = new Sprite(_context.Textures[TextureID.Cache]) { Position = new Vector2f(_context.UpscaleTexture.Size.X - 54f, 2f) };
            _cache1 = new Sprite(_context.Textures[TextureID.Cache]) { Position = new Vector2f(_context.UpscaleTexture.Size.X - 36f, 2f) };
            _cache2 = new Sprite(_context.Textures[TextureID.Cache]) { Position = new Vector2f(_context.UpscaleTexture.Size.X - 18f, 2f) };

            _entityWorld = new EntityWorld();
            _entityWorld.SystemManager.SetSystem(new CharacterMovementSystem(_context.Actions, _context.Textures, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new DroneSystem(_context.Textures, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new SpikesSystem(16, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new MissileLauncherSystem(_context.Textures, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new CollisionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new TileCollisionSystem(collisions, 16f, _context.SoundBuffers), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new FrictionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new LockSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new AnimationSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new HitSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new StationSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.GetSystem<StationSystem>()[0].StationTouched += StationSystem_StationTouched;
            _entityWorld.SystemManager.SetSystem(new DrawSystem(context.UpscaleTexture, _background, _foreground), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new HealthDrawSystem(context.UpscaleTexture, context.Textures, context.SoundBuffers), GameLoopType.Draw);

            _stationAnimation = new FixedFrameAnimation(16, 16);
            _stationAnimation.AddFrame(0, 0, 1f);
            for (int i = 1; i < 6; i++)
                _stationAnimation.AddFrame(i, 0, 0.05f);
            _stationAnimation.AddFrame(6, 0, 1f);
            for (int i = 7; i < 12; i++)
                _stationAnimation.AddFrame(i, 0, 0.05f);

            _droneAnimation = new FixedFrameAnimation(16, 16).AddFrame(0, 0, 1f).AddFrame(1, 0, 0.1f).AddFrame(2, 0, 1f).AddFrame(3, 0, 0.1f);

            foreach (TmxObject obj in map.ObjectGroups[0].Objects)
            {
                if (obj.Properties.ContainsKey("Station"))
                {
                    int id = int.Parse(obj.Properties["Station"]);

                    Entity station = _entityWorld.CreateEntity();
                    station.AddComponent(new PositionComponent((float)obj.X + (float)obj.Width / 2f, (float)obj.Y + (float)obj.Height / 2f));
                    station.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Station]) { Position = new Vector2f(-8f, -8f) }, Layer.Floor));
                    station.AddComponent(new AnimationComponent());
                    station.AddComponent(new StationComponent(id));
                    station.GetComponent<AnimationComponent>().Play(_stationAnimation, Time.FromSeconds(3f), true);
                }
                else if (obj.Properties.ContainsKey("Spikes"))
                {
                    Time timer = Time.FromSeconds(float.Parse(obj.Properties["Spikes"]));

                    Entity spikes = _entityWorld.CreateEntity();
                    spikes.AddComponent(new PositionComponent());
                    spikes.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Spikes]) { TextureRect = new IntRect(0, 0, 16, 16) }, Layer.Floor));
                    spikes.AddComponent(new SpikesComponent(new Vector2i((int)(obj.X / 16f), (int)(obj.Y / 16f)), timer));
                    spikes.AddComponent(new AnimationComponent());
                }
                else if (obj.Properties.ContainsKey("Launcher"))
                {
                    Time timer = Time.FromSeconds(float.Parse(obj.Properties["Launcher"]));

                    Entity launcher = _entityWorld.CreateEntity();
                    launcher.AddComponent(new PositionComponent((float)obj.X + (float)obj.Width / 2f, (float)obj.Y + (float)obj.Height / 2f));
                    launcher.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.MissileLauncher]) { Position = new Vector2f(-8f, -16f) }, Layer.Above));
                    launcher.AddComponent(new MissileLauncherComponent(timer));
                }
                else if (obj.Properties.ContainsKey("Drone"))
                {
                    Entity drone = _entityWorld.CreateEntity();
                    drone.AddComponent(new PositionComponent((float)obj.X + (float)obj.Width / 2f, (float)obj.Y + (float)obj.Height / 2f));
                    drone.AddComponent(new VelocityComponent());
                    drone.AddComponent(new HitComponent());
                    drone.AddComponent(new FrictionComponent(10f));
                    drone.AddComponent(new AnimationComponent());
                    drone.GetComponent<AnimationComponent>().Play(_droneAnimation, Time.FromSeconds(2f), true);
                    drone.AddComponent(new TileCollisionComponent(2f));
                    drone.AddComponent(new CollisionComponent(2f));
                    drone.AddComponent(new DroneComponent());
                    drone.AddComponent(new HealthComponent(6, 28f));
                    drone.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Drone])
                    {
                        TextureRect = new IntRect(0, 0, 16, 16),
                        Position = new Vector2f(-8f, -16f)
                    }, Layer.Player));
                }
                else if (obj.Properties.ContainsKey("Cache"))
                {
                    int id = int.Parse(obj.Properties["Cache"]);

                    if (_playerData.Caches[id])
                        continue;

                    Entity cache = _entityWorld.CreateEntity();
                    cache.AddComponent(new PositionComponent((float)obj.X + (float)obj.Width / 2f, (float)obj.Y + (float)obj.Height / 2f));
                    cache.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Cache]) { Position = new Vector2f(-8f, -8f) }, Layer.Floor));
                    cache.AddComponent(new CollisionComponent(4f, false));
                    cache.GetComponent<CollisionComponent>().Collided += (sender, e) => Cache_Collided(id, cache);
                }
                else if (obj.Properties.ContainsKey("Finish"))
                {
                    Entity finish = _entityWorld.CreateEntity();
                    finish.AddComponent(new PositionComponent((float)obj.X + (float)obj.Width / 2f, (float)obj.Y + (float)obj.Height / 2f));
                    finish.AddComponent(new CollisionComponent(10f, false));
                    finish.GetComponent<CollisionComponent>().Collided += (sender, e) => Finish_Collided();
                }
            }

            _character = _entityWorld.CreateEntity();
            _character.AddComponent(new CharacterComponent());
            _character.AddComponent(new HitComponent());
            foreach (Entity stationEntity in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(PositionComponent), typeof(StationComponent))))
            {
                PositionComponent positionComponent = stationEntity.GetComponent<PositionComponent>();
                StationComponent stationComponent = stationEntity.GetComponent<StationComponent>();

                if (stationComponent.ID == _playerData.LastStation)
                {
                    _character.AddComponent(new PositionComponent(positionComponent.Position));
                    break;
                }
            }
            _character.AddComponent(new VelocityComponent());
            _character.AddComponent(new AnimationComponent());
            _character.AddComponent(new TileCollisionComponent(2f));
            _character.AddComponent(new CollisionComponent(4f));
            _character.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Character])
            {
                TextureRect = new IntRect(0, 0, 16, 32),
                Position = new Vector2f(-8f, -25f)
            }, Layer.Player));

            //Entity character2 = _entityWorld.CreateEntity();
            //character2.AddComponent(new PositionComponent(128f, 128f));
            //character2.AddComponent(new VelocityComponent());
            //character2.AddComponent(new AnimationComponent());
            //character2.AddComponent(new TileCollisionComponent(2f));
            //character2.AddComponent(new CollisionComponent(4f));
            //character2.AddComponent(new FrictionComponent(500f));
            //character2.AddComponent(new HealthComponent(10, 28f));
            //character2.AddComponent(new SpriteComponent(new Sprite(_context.Textures[TextureID.Character])
            //{
            //    TextureRect = new IntRect(0, 0, 16, 32),
            //    Position = new Vector2f(-8f, -25f)
            //}, Layer.Player));

            _batteryBack = new Sprite(_context.Textures[TextureID.BatteryBack]);
            _batteryBack.Position = new Vector2f(_context.UpscaleTexture.Size.X / 2f - _batteryBack.Texture.Size.X / 2f,
                _context.UpscaleTexture.Size.Y - 4f - _batteryBack.Texture.Size.Y);

            _batteryFill = new Sprite(_context.Textures[TextureID.BatteryFill]);
            _batteryFill.Position = _batteryBack.Position + new Vector2f(3f, 3f);

            _entityWorld.Update(0);
        }

        private void Cache_Collided(int id, Entity cache)
        {
            cache.Delete();
            _playerData.Caches[id] = true;
            _cacheGet.Play();
        }

        private void Finish_Collided()
        {
            new Sound(_context.SoundBuffers[SoundBufferID.Explosion]) { Volume = 30f }.Play();
            _entityWorld.SystemManager.GetSystem<CharacterMovementSystem>()[0].Detach();
            _request = ScreenChangeRequest.Replace(new EndScreen(_context, _playerData));
        }

        private void StationSystem_StationTouched(object sender, StationEventArgs e)
        {
            _rechargeStation = e.ID;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _entityWorld.Update(deltaTime.AsMicroseconds() * 10);

            if (_rechargeStation >= 0)
            {
                _playerData.LastStation = _rechargeStation;
                _entityWorld.SystemManager.GetSystem<CharacterMovementSystem>()[0].Detach();
                return ScreenChangeRequest.Replace(new RechargeScreen(_context, _playerData));
            }

            CharacterComponent characterComponent = _character.GetComponent<CharacterComponent>();

            if (characterComponent.Power <= 0f)
                return ScreenChangeRequest.Replace(new RechargeScreen(_context, _playerData));

            characterComponent.Power = Math.Min(1f, characterComponent.Power);

            float power = Math.Max(characterComponent.Power, 0f);
            _displayedPower += (power - _displayedPower) * 10f * deltaTime.AsSeconds();

            _batteryFill.TextureRect = new IntRect(0, 
                0, 
                (int)(_batteryFill.Texture.Size.X * _displayedPower), 
                (int)_batteryFill.Texture.Size.Y);

            PositionComponent positionComponent = _character.GetComponent<PositionComponent>();
            Listener.Position = new Vector3f(positionComponent.Position.X, positionComponent.Position.Y, 0f);
            Listener.UpVector = new Vector3f(0f, 0f, 1f);
            Listener.Direction = new Vector3f(0f, 0f, -1f);

            return _request;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            View view = _context.UpscaleTexture.GetView();
            view.Center = _character.GetComponent<PositionComponent>().Position.Floor();
            _context.UpscaleTexture.SetView(view);

            _entityWorld.SystemManager.GetSystem<DrawSystem>()[0].RenderStates = states;
            _entityWorld.SystemManager.GetSystem<HealthDrawSystem>()[0].RenderStates = states;

            _entityWorld.Draw();

            _context.UpscaleTexture.SetView(_context.UpscaleTexture.DefaultView);

            target.Draw(_batteryBack, states);
            target.Draw(_batteryFill, states);

            if (_playerData.Caches[0])
                target.Draw(_cache0, states);
            if (_playerData.Caches[1])
                target.Draw(_cache1, states);
            if (_playerData.Caches[2])
                target.Draw(_cache2, states);
        }
    }
}
