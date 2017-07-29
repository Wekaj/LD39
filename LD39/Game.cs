﻿using LD39.Input;
using LD39.Resources;
using LD39.Screens;
using LD39.Screens.Game;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace LD39
{
    internal sealed class Game : IDisposable
    {
        #region IDisposable Support
        private bool _disposed = false;

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private const uint _scale = 2;
        private static readonly Time _timePerFrame = Time.FromSeconds(1f / 60f);
        private readonly RenderWindow _window;
        private readonly RenderTexture _upscaleTexture;
        private readonly Sprite _upscaleSprite;
        private readonly ActionManager _actions;
        private readonly TextureLoader _textures;
        private readonly FontLoader _fonts;
        private readonly SoundBufferLoader _soundBuffers;
        private readonly Context _context;
        private readonly ScreenStack _screens;

        public Game()
        {
            _window = new RenderWindow(new VideoMode(1366, 768), "Running out of Power");
            _window.Closed += Window_Closed;

            _upscaleTexture = new RenderTexture(_window.Size.X / _scale, _window.Size.Y / _scale);
            _upscaleSprite = new Sprite(_upscaleTexture.Texture) { Scale = new Vector2f(_scale, _scale) };

            _actions = new ActionManager();

            _textures = new TextureLoader();
            _fonts = new FontLoader();
            _soundBuffers = new SoundBufferLoader();

            _context = new Context(_window, _upscaleTexture, _actions, _textures, _fonts, _soundBuffers);

            _screens = new ScreenStack(new GameScreen(_context));
        }

        public void Run()
        {
            Clock clock = new Clock();
            Time timeSinceLastUpdate = Time.Zero;

            while (_window.IsOpen)
            {
                ProcessInput();
                timeSinceLastUpdate += clock.Restart();

                while (timeSinceLastUpdate > _timePerFrame)
                {
                    timeSinceLastUpdate -= _timePerFrame;
                    ProcessInput();
                    Update(_timePerFrame);
                }

                Draw();
            }
        }

        private void ProcessInput()
        {
            _window.DispatchEvents();
            _actions.Update();
        }

        private void Update(Time deltaTime)
        {
            _screens.Update(deltaTime);
        }

        private void Draw()
        {
            _upscaleTexture.Clear();
            _upscaleTexture.Draw(_screens);
            _upscaleTexture.Display();

            _window.Clear();
            _window.Draw(_upscaleSprite);
            _window.Display();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _window.Close();
        }
    }
}
