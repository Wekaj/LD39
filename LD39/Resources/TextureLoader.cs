using SFML.Graphics;

namespace LD39.Resources
{
    internal sealed class TextureLoader : ResourceLoader<TextureID, Texture>
    {
        protected override Texture Load(string filename)
        {
            return new Texture(filename);
        }
    }
}
