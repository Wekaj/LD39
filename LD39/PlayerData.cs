using System.Collections.Generic;

namespace LD39
{
    internal sealed class PlayerData
    {
        public int LastStation { get; set; } = 0;
        public Dictionary<int, bool> Caches { get; } = new Dictionary<int, bool>()
        {
            { 0, false },
            { 1, false },
            { 2, false },
        };
    }
}
