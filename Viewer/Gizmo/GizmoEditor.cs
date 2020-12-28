using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Viewer.Scene;
using WpfTest.Scenes;
using Keyboard = Viewer.Input.Keyboard;

namespace Viewer.Gizmo
{
    public delegate void GizmoUpdated();

    public class GizmoEditor
    {
        public event TransformationEventHandler RotateEvent;
        public event TransformationEventHandler TranslateEvent;
        public event GizmoUpdated GizmoUpdatedEvent;

        GizmoComponent _gizmo;
        SpriteBatch _spriteBatch;

        Keyboard _keyboard;
        ArcBallCamera _camera;
        public Matrix AxisMatrix { get { return _gizmo.AxisMatrix; } }
        public bool UpdateGizmo { get; set; } = false;

        public void Create(ResourceLibary resourceLibary, GraphicsDevice graphicsDevice, Keyboard keyboard, ArcBallCamera camera)
        {
            _keyboard = keyboard;
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphicsDevice);

            var font = resourceLibary.XnaContentManager.Load<SpriteFont>("Fonts\\DefaultFont");

            _gizmo = new GizmoComponent(graphicsDevice, _spriteBatch, font, _keyboard);
            _gizmo.TranslateEvent += GizmoTranslateEvent;
            _gizmo.RotateEvent += GizmoRotateEvent;
            _gizmo.ActivePivot = PivotType.ObjectCenter;
            _gizmo.SelectionBoxesIsVisible = false;
        }

        public void Update(GameTime time, MouseState mouseState)
        {
            if (!UpdateGizmo)
                return;
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

        public void UpdatePositionOfItems(bool force)
        {
            foreach (var item in _gizmo.Selection)
                item.Update(force);
        }

        public void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            if(UpdateGizmo)
                _gizmo.Draw(false);
        }

        private void GizmoTranslateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            transformable.Position += (Vector3)e.Value;
            TranslateEvent?.Invoke(transformable, e);
            GizmoUpdatedEvent?.Invoke();
        }

        private void GizmoRotateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            transformable.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromQuaternion(transformable.Orientation) * (Matrix)e.Value);
            RotateEvent?.Invoke(transformable, e);
            GizmoUpdatedEvent?.Invoke();
        }

        public void SelectItem(ITransformable item)
        {
            foreach (var itemToRemove in _gizmo.Selection)
                itemToRemove.Dispose();
            _gizmo.Selection.Clear();
            if(item != null)
                _gizmo.Selection.Add(item);
            _gizmo.ResetDeltas();
        }
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


        public Matrix CurrentOriantati = Matrix.Identity;
        public Vector3 Forward
        {
            get
            {
             //   return Vector3.Transform(Vector3.Forward, CurrentOriantati);
               return Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Orientation));
            }
        }
        public Vector3 Up
        {
            get
            {
               // return Vector3.Transform(Vector3.Up, CurrentOriantati);
                return Vector3.Transform(Vector3.Up, Matrix.CreateFromQuaternion(Orientation));
            }
        }

        public virtual void Update(bool force)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
