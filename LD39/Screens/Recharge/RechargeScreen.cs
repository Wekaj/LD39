using LD39.Resources;
using LD39.Screens.Game;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace LD39.Screens.Recharge
{
    internal sealed class RechargeScreen : IScreen
    {
        private readonly Context _context;
        private readonly Time _time = Time.FromSeconds(0.5f);
        private readonly PlayerData _playerData;
        private Time _timer;

        public RechargeScreen(Context context, PlayerData playerData)
        {
            _context = context;
            _playerData = playerData;

            _timer = _time;

            new Sound(context.SoundBuffers[SoundBufferID.MegaSlash]) { Volume = 21f }.Play();
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            if (_timer > deltaTime)
                _timer -= deltaTime;
            else
                return ScreenChangeRequest.Replace(new GameScreen(_context, _playerData));
            return null;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            Color color = LD39.Game.Shade3;
            if (_timer < _time / 4f)
                color = LD39.Game.Shade0;
            else if (_timer < _time / 2f)
                color = LD39.Game.Shade1;
            else if(_timer < _time * 3f / 4f)
                color = LD39.Game.Shade2;
            _context.UpscaleTexture.Clear(color);
        }
    }
}
