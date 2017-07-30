using System;

namespace LD39.Systems
{
    internal class StationEventArgs : EventArgs
    {
        public StationEventArgs(int id)
        {
            ID = id;
        }

        public int ID { get; }
    }
}