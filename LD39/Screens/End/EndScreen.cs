using LD39.Input;
using LD39.Resources;
using LD39.Screens.Menu;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;

namespace LD39.Screens.End
{
    internal sealed class EndScreen : IScreen
    {
        private readonly Context _context;
        private readonly Text _text;
        private readonly PlayerData _playerData;
        private readonly int _caches;
        private readonly Sound _blip;
        private int line = 0;
        private Time _timer;
        private ScreenChangeRequest _request = null;

        public EndScreen(Context context, PlayerData playerData)
        {
            _context = context;
            _playerData = playerData;

            _text = new Text("Targets eliminated.", context.Fonts[FontID.Normal], 8);
            _text.Position = new Vector2f(2f, 2f);
            _text.Color = LD39.Game.Shade3;

            foreach (bool cache in playerData.Caches.Values)
                if (cache)
                    _caches++;

            _blip = new Sound(context.SoundBuffers[SoundBufferID.Blip]) { Volume = 30f };
            _blip.Play();

        }

        private void Enter_Pressed(object sender, EventArgs e)
        {
            _request = ScreenChangeRequest.Replace(new MenuScreen(_context));
            _context.Actions[ActionID.Attack].Pressed -= Enter_Pressed;
        }

        public ScreenChangeRequest Update(Time deltaTime)
        {
            _timer += deltaTime;
            switch (line)
            {
                case 0:
                    if (_timer > Time.FromSeconds(2f))
                    {
                        _text.DisplayedString += "\nDisconnected from mobile assault unit.";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 1:
                    if (_timer > Time.FromSeconds(6f))
                    {
                        _text.DisplayedString += "\nData caches transferred: " + _caches + ".";
                        line++;
                        _blip.Play();
                    }
                    break;
                case 2:
                    if (_timer > Time.FromSeconds(10f))
                    {
                        _text.DisplayedString += "\nComment: ";
                        switch (_caches)
                        {
                            case 0:
                                _text.DisplayedString += "there is likely missing data \nstill to be found within the ruins.";
                                break;
                            case 1:
                                _text.DisplayedString += "good work!";
                                break;
                            case 2:
                                _text.DisplayedString += "wow, that's impressive!";
                                break;
                            case 3:
                                _text.DisplayedString += "holy [REDACTED] you found all \nof the data caches!";
                                break;
                        }
                        line++;
                        _blip.Play();
                    }
                    break;
                case 3:
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
