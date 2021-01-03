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
        Input.Keyboard _keyboard;

        bool _displayExtraInfo = true;

        public ControlsComponent(WpfGame game, Input.Keyboard keyboard) : base(game)
        {
            _keyboard = keyboard;
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

            if (_displayExtraInfo)
            {
                // Camera controls
                DrawText(1, $"Reset Camera", "F4");
                DrawText(2, $"Camera Zoom", "Alt + mouse wheel");
                DrawText(3, $"Camera Pan", "Alt + right mouse button");
                DrawText(4, $"Camera Rotate", "Alt + left mouse button");
                DrawText(5, $"Rotate lightmap", "PageUp/PageDown");

                DrawText(7, $"Rotation Gizmo", "R");
                DrawText(8, $"Translation Gizmo", "T");
                //DrawText(9, $"Toggle bone space", "Home");
            }
            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if (_keyboard.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.F1))
                _displayExtraInfo = !_displayExtraInfo;
            base.Update(gameTime);
        }

        void DrawText(int lineIndex, string header, string value)
        {
            float headerLength = 140;
            float offset = 20;
            float spacing = 18;
            _spriteBatch.DrawString(_font, header, new Vector2(5, offset + (spacing * lineIndex)), Color.White);
            _spriteBatch.DrawString(_font, ":", new Vector2(5 + headerLength, offset + (spacing * lineIndex)), Color.White);
            _spriteBatch.DrawString(_font, value, new Vector2(5 + headerLength + 5, offset + (spacing * lineIndex)), Color.White);
        }
    }
}
