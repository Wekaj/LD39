using LD39.Input;
using LD39.Resources;
using SFML.Graphics;

namespace LD39
{
    internal sealed class Context
    {
        public Context(RenderWindow window, RenderTexture upscaleTexture, ActionManager actions,
            TextureLoader textures, FontLoader fonts, SoundBufferLoader soundBuffers)
        {
            Window = window;
            UpscaleTexture = upscaleTexture;
            Actions = actions;
            Textures = textures;
            Fonts = fonts;
            SoundBuffers = soundBuffers;
        }

        public RenderWindow Window { get; }
        public RenderTexture UpscaleTexture { get; }
        public ActionManager Actions { get; }
        public TextureLoader Textures { get; }
        public FontLoader Fonts { get; }
        public SoundBufferLoader SoundBuffers { get; }
    }
}
