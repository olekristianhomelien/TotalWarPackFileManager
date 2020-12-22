using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;
using Viewer.Input;
using Keyboard = Viewer.Input.Keyboard;

namespace Viewer.Gizmo
{
    public class GizmoEditor
    {
        GizmoComponent _gizmo;
        SpriteBatch _spriteBatch;

        LineBox _testItem;
        LineRenderItem _testRenderItem;
        GizmoItemWrapper _wrapperItem;

        Keyboard _keyboard;
        ArcBallCamera _camera;

        public void Create(ResourceLibary resourceLibary, GraphicsDevice graphicsDevice, Keyboard keyboard, ArcBallCamera camera)
        {
            _testItem = new LineBox();
            _wrapperItem = new GizmoItemWrapper();
            _testRenderItem = new LineRenderItem(_testItem, resourceLibary.GetEffect(ShaderTypes.Line));
            
            _wrapperItem.OnTranformChanged += _wrapperItem_OnTranformChanged;

            _keyboard = keyboard;
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphicsDevice);

            var font = resourceLibary.XnaContentManager.Load<SpriteFont>("Fonts\\DefaultFont");

            _gizmo = new GizmoComponent(graphicsDevice, _spriteBatch, font, _keyboard);
            _gizmo.TranslateEvent += GizmoTranslateEvent;
            _gizmo.RotateEvent += GizmoRotateEvent;
            //_gizmo.ScaleEvent += GizmoScaleEvent;
            _gizmo.Selection.Add(_wrapperItem);
            _gizmo.ActivePivot = PivotType.ObjectCenter;
            _gizmo.SelectionBoxesIsVisible = false;
        }

        private void _wrapperItem_OnTranformChanged(GizmoItemWrapper item)
        {
            _testItem.ModelMatrix = Matrix.CreateScale(1) * Matrix.CreateFromQuaternion(item.Orientation) * Matrix.CreateTranslation(item.Position);
        }

        public void Update(GameTime time, MouseState mouseState)
        {
            _gizmo.UpdateCameraProperties(_camera.ViewMatrix, _camera.ProjectionMatrix, _camera.Position);

            // Toggle transform mode:
            if (_keyboard.IsKeyReleased(Keys.R))
                _gizmo.ActiveMode = GizmoMode.Rotate;

            if (_keyboard.IsKeyReleased(Keys.T))
                _gizmo.ActiveMode = GizmoMode.Translate;

            // Toggle space mode:
            if (_keyboard.IsKeyReleased(Keys.Home))
                _gizmo.ToggleActiveSpace();

            _gizmo.Update(mouseState, time);
        }


        public void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            _testRenderItem.Draw(device, world, commonShaderParameters);
            _gizmo.Draw();
        }

        private void GizmoTranslateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            transformable.Position += (Vector3)e.Value;
        }

        private void GizmoRotateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            transformable.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromQuaternion(transformable.Orientation) * (Matrix)e.Value);
        }

        //private void GizmoScaleEvent(ITransformable transformable, TransformationEventArgs e)
        //{
        //    Vector3 delta = (Vector3)e.Value;
        //    if (_gizmo.ActiveMode == GizmoMode.UniformScale)
        //        transformable.Scale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
        //    else
        //        transformable.Scale += delta;
        //    transformable.Scale = Vector3.Clamp(transformable.Scale, Vector3.Zero, transformable.Scale);
        //
        //    _testItem.UpdateWorld();
        //}
    }

    public delegate void TransformChanged(GizmoItemWrapper item);  
    public class GizmoItemWrapper : ITransformable
    {
        public event TransformChanged OnTranformChanged;

        Vector3 _position = Vector3.Zero;
        public Vector3 Position { get { return _position; } set { _position = value; OnTranformChanged?.Invoke(this); } }

        Vector3 _scale = Vector3.One;
        public Vector3 Scale { get { return _scale; } set { _scale = value; OnTranformChanged?.Invoke(this); } }

        Quaternion _orientation = Quaternion.Identity;
        public Quaternion Orientation { get { return _orientation; } set { _orientation = value; OnTranformChanged?.Invoke(this); } }
        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Orientation));
            }
        }
        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.Up, Matrix.CreateFromQuaternion(Orientation));
            }
        }
    }
}
