using SFML.Graphics;
using SFML.System;

namespace LD39.Tiles
{
    internal sealed class TileMap : Transformable, Drawable
    {
        private readonly Texture _tiles;
        private readonly VertexArray _vertices;

        public TileMap(Texture tiles, int tileSize, int[,] map)
        {
            _tiles = tiles;

            uint width = (uint)map.GetLength(0), height = (uint)map.GetLength(1);

            _vertices = new VertexArray(PrimitiveType.Quads, width * height * 4);
            for (uint y = 0; y < height; y++)
                for (uint x = 0; x < width; x++)
                {
                    uint startIndex = x + y * width;
                    _vertices[startIndex] = new Vertex(new Vector2f(x, y) * tileSize);
                    _vertices[startIndex + 1] = new Vertex(new Vector2f(x + 1f, y) * tileSize);
                    _vertices[startIndex + 2] = new Vertex(new Vector2f(x + 1f, y + 1f) * tileSize);
                    _vertices[startIndex + 3] = new Vertex(new Vector2f(x, y + 1f) * tileSize);
                }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            states.Texture = _tiles;

            target.Draw(_vertices);
        }
    }
}
