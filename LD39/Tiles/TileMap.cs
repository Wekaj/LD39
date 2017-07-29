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
                    uint startIndex = (x + y * width) * 4;
                    uint tileX = (uint)map[x, y] % width;
                    uint tileY = (uint)map[x, y] / width;
                    _vertices[startIndex] = new Vertex(new Vector2f(x, y) * tileSize, 
                        new Vector2f(tileX, tileY) * tileSize);
                    _vertices[startIndex + 1] = new Vertex(new Vector2f(x + 1f, y) * tileSize, 
                        new Vector2f(tileX + 1f, tileY) * tileSize);
                    _vertices[startIndex + 2] = new Vertex(new Vector2f(x + 1f, y + 1f) * tileSize, 
                        new Vector2f(tileX + 1f, tileY + 1f) * tileSize);
                    _vertices[startIndex + 3] = new Vertex(new Vector2f(x, y + 1f) * tileSize, 
                        new Vector2f(tileX, tileY + 1f) * tileSize);
                }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            states.Texture = _tiles;

            target.Draw(_vertices, states);
        }
    }
}
