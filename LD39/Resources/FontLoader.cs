using SFML.Graphics;

namespace LD39.Resources
{
    internal sealed class FontLoader : ResourceLoader<FontID, Font>
    {
        protected override Font Load(string filename)
        {
            return new Font(filename);
        }
    }
}
