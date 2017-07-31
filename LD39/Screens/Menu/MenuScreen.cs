using LD39.Input;
using LD39.Resources;
using LD39.Screens.Game;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;

namespace LD39.Screens.Menu
{
    internal sealed class MenuScreen : IScreen
    {
        private readonly Context _context;
        private readonly Text _text;
        private readonly Sound _blip;
        private int line = 0;
        private Time _timer;
        private ScreenChangeRequest _request = null;

        public MenuScreen(Context context)
        {
            _context = context;

            _text = new Text("Connecting to mobile assault unit...", context.Fonts[FontID.Normal], 8);
            _text.Position = new Vector2f(2f, 2f);
            _text.Color = LD39.Game.Shade3;

            _blip = new Sound(context.SoundBuffers[SoundBufferID.Blip]) { Volume = 30f };
            _blip.Play();

        }

        private void Enter_Pressed(object sender, EventArgs e)
        {
            _request = ScreenChangeRequest.Replace(new GameScreen(_context, new PlayerData()));
            _context.Actions[ActionID.Attack].Pressed -= Enter_Pressed;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _timer += deltaTime;
            switch (line)
            {
                case 0:
                    if (_timer > Time.FromSeconds(3f))
                    {
                        _text.DisplayedString += "\nConnection established.";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 1:
                    if (_timer > Time.FromSeconds(5f))
                    {
                        _text.DisplayedString += "\nERROR: structural failure encountered \nwhile rotating conductive matrices.";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 2:
                    if (_timer > Time.FromSeconds(8f))
                    {
                        _text.DisplayedString += "\nWARNING: resorting to back-up battery.";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 3:
                    if (_timer > Time.FromSeconds(11f))
                    {
                        _text.DisplayedString += "\nInitializing visual interface...";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 4:
                    if (_timer > Time.FromSeconds(14f))
                    {
                        _text.DisplayedString += "\nPress [Z] to continue...";
                        line++;
                        _blip.Play();
                        _context.Actions[ActionID.Attack].Pressed += Enter_Pressed;
                    }
                    break;
            }

            return _request;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_text, states);
        }
    }
}
