namespace LD39.Screens
{
    public enum ScreenChangeRequestType
    {
        Pop,
        Push
    }

    internal sealed class ScreenChangeRequest
    {
        public ScreenChangeRequest(ScreenChangeRequestType type, IScreen screen)
        {
            Type = type;
            Screen = screen;
        }

        public ScreenChangeRequestType Type { get; }
        public IScreen Screen { get; }

        public static ScreenChangeRequest Pop()
        {
            return new ScreenChangeRequest(ScreenChangeRequestType.Pop, null);
        }

        public static ScreenChangeRequest Push(IScreen screen)
        {
            return new ScreenChangeRequest(ScreenChangeRequestType.Push, screen);
        }
    }
}
