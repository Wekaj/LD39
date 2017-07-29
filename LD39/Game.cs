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

        private static readonly Time _timePerFrame = Time.FromSeconds(1f / 60f);
        private readonly RenderWindow _window;

        public Game()
        {
            _window = new RenderWindow(new VideoMode(1366, 768), "Running out of Power");
            _window.Closed += Window_Closed;
        }

        public void Run()
        {
            Clock clock = new Clock();
            Time timeSinceLastUpdate = Time.Zero;

            while (_window.IsOpen)
            {
                _window.DispatchEvents();
                timeSinceLastUpdate += clock.Restart();

                while (timeSinceLastUpdate > _timePerFrame)
                {
                    timeSinceLastUpdate -= _timePerFrame;
                    _window.DispatchEvents();
                    Update(_timePerFrame);
                }

                Draw();
            }
        }

        private void Update(Time deltaTime)
        {

        }

        private void Draw()
        {
            _window.Clear();

            _window.Display();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _window.Close();
        }
    }
}
