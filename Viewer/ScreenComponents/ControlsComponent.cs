using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using System;

namespace Viewer.ScreenComponents
{
    public class ControlsComponent : WpfDrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        public ControlsComponent(WpfGame game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>("Fonts\\DefaultFont");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            DrawText(0, $"Toggle Help", "F1");
            DrawText(1, $"Toggle Shader", "F3");
            DrawText(2, $"Camera Zoom", "Q || E");
            DrawText(3, $"Camera Up/Down", "W || S");
            DrawText(4, $"Rotate camera", "Left Alt + mouse");
            DrawText(5, $"Reset  camera", "F4");
            _spriteBatch.End();
        }

        void DrawText(int index, string header, string value)
        {
            float headerLength = 140;
            float offset = 20;
            float spacing = 18;
            _spriteBatch.DrawString(_font, header, new Vector2(5, offset + (spacing * index)), Color.White);
            _spriteBatch.DrawString(_font, ":", new Vector2(5 + headerLength, offset + (spacing * index)), Color.White);
            _spriteBatch.DrawString(_font, value, new Vector2(5 + headerLength + 5, offset + (spacing * index)), Color.White);
        }
    }
}
