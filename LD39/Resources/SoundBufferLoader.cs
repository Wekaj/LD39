using SFML.Audio;

namespace LD39.Resources
{
    internal sealed class SoundBufferLoader : ResourceLoader<SoundBufferID, SoundBuffer>
    {
        protected override SoundBuffer Load(string filename)
        {
            return new SoundBuffer(filename);
        }
    }
}
