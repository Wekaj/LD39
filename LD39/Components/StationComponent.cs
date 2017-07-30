using Artemis.Interface;

namespace LD39.Components
{
    internal sealed class StationComponent : IComponent
    {
        public StationComponent(int id)
        {
            ID = id;
        }

        public int ID { get; set; }
        public bool Colliding { get; set; } = true;
    }
}
