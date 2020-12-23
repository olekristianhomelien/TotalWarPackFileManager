using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Viewer.Gizmo;
using Viewer.GraphicModels;
using Viewer.Input;
using Viewer.Scene;
using Viewer.ScreenComponents;

namespace WpfTest.Scenes
{


    public class Scene3d : WpfGame
    {
        WpfMouse _mouse;
        Keyboard _keyboard;
        ArcBallCamera _camera;
        LineRenderItem _grid;
        SpriteBatch _spriteBatch;

        ResourceLibary _resourceLibary;
        float envRotate = 0;
        bool _disposed;

        

        public delegate void LoadSceneCallback(GraphicsDevice device);
        public LoadSceneCallback On3dWorldReady { get; set; }
        public ISceneGraphNode SceneGraphRootNode { get; set; }

        
        public TextureToTextureRenderer TextureToTextureRenderer { get; private set; }
        
        public GizmoEditor Gizmo { get; private set; }


        protected override void Initialize()
        {
            _disposed = false;
            
            new WpfGraphicsDeviceService(this);
            Components.Add(new FpsComponent(this));
            Components.Add(new ControlsComponent(this));

            _keyboard = new Keyboard(new WpfKeyboard(this));
            _mouse = new WpfMouse(this);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }


        public void SetResourceLibary(ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
        }

        protected override void LoadContent()
        {
            _resourceLibary.XnaContentManager = Content;

            _resourceLibary.LoadEffect("Shaders\\TestShader", ShaderTypes.Mesh);
            _resourceLibary.LoadEffect("Shaders\\LineShader", ShaderTypes.Line);
            _resourceLibary.LoadEffect("Shaders\\TexturePreview", ShaderTypes.TexturePreview);

            _resourceLibary.LoadEffect("Shaders\\Phazer\\main", ShaderTypes.Phazer);
            //_skyBox = new Skybox("textures\\phazer\\rad_rustig_rgba32f_raw", _resourceLibary.XnaContentManager);

            TextureToTextureRenderer = new TextureToTextureRenderer(GraphicsDevice, _spriteBatch, _resourceLibary);
            _camera = new ArcBallCamera(1, new Vector3(0), 10, GraphicsDevice);

            _grid = new LineRenderItem(new LineGrid(), _resourceLibary.GetEffect(ShaderTypes.Line));
            Gizmo = new GizmoEditor();
            Gizmo.Create(_resourceLibary, GraphicsDevice, _keyboard, _camera);

            On3dWorldReady?.Invoke(GraphicsDevice);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Components.Clear();
            _disposed = true;

            base.Dispose(disposing);
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = _mouse.GetState();
            _keyboard.Update();

            if (_keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.PageUp))
                envRotate += 0.06f;

            if (_keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.PageDown))
                envRotate -= 0.06f;

            _camera.Update(mouseState, _keyboard);
            Gizmo.Update(gameTime, mouseState);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            CommonShaderParameters commonShaderParameters = new CommonShaderParameters()
            {
                Projection = _camera.ProjectionMatrix,
                View = _camera.ViewMatrix,
                CameraPosition = _camera.Position,
                CameraLookAt = _camera.LookAt,
                EnvRotate = envRotate
            };

            if (SceneGraphRootNode != null)
            {
                SceneGraphRootNode.Update(time);
                SceneGraphRootNode.Render(GraphicsDevice, Matrix.Identity, commonShaderParameters);
            }

            _grid.Draw(GraphicsDevice, Matrix.Identity, commonShaderParameters);
            Gizmo.Draw(GraphicsDevice, Matrix.Identity, commonShaderParameters);
            base.Draw(time);
        }
    }

   

    public class CommonShaderParameters
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraLookAt { get; set; }
        public float EnvRotate;
    }

    public class RenderItem 
    {
        protected IRenderableContent _model;
        protected Effect _shader;
        public bool Visible { get; set; } = true;

        public RenderItem(IRenderableContent model, Effect shader)
        {
            _model = model;
            _shader = shader;
        }

        public virtual void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            if (!Visible)
                return;

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                ApplyCommonShaderParameters(commonShaderParameters, world);
                ApplyCustomShaderParams();
                pass.Apply();
                _model.Render(device);
            }
        }

        protected void ApplyCommonShaderParameters(CommonShaderParameters commonShaderParameters, Matrix world)
        {
            _shader.Parameters["View"].SetValue(commonShaderParameters.View);
            _shader.Parameters["Projection"].SetValue(commonShaderParameters.Projection);
    
            if (_model != null)
                world = Matrix.CreateTranslation(_model.Pivot) * _model.ModelMatrix *  world;
            _shader.Parameters["World"].SetValue(world);

            if (this as MeshRenderItem != null)
            {
                _shader.Parameters["cameraPosition"].SetValue(commonShaderParameters.CameraPosition);
                _shader.Parameters["cameraLookAt"].SetValue(commonShaderParameters.CameraLookAt);
                _shader.Parameters["ViewInverse"].SetValue(Matrix.Invert(commonShaderParameters.View));
                _shader.Parameters["EnvMapTransform"].SetValue((Matrix.CreateRotationY(commonShaderParameters.EnvRotate)));
            }
        }

        public virtual void ApplyCustomShaderParams()
        {}

        public virtual void Update(GameTime time)
        { }

        public virtual void Dispose()
        {
            if(_model != null)
                _model.Dispose();
        }
    }

    public class LineRenderItem : RenderItem
    {
        public Vector3 Colour { get; set; } = new Vector3(0, 0, 0);

        public LineRenderItem(LineModel model, Effect shader) :base(model, shader){ }

        public override void ApplyCustomShaderParams()
        {
            _shader.Parameters["Color"].SetValue(Colour);
        }
    }


    public class MeshRenderItem : RenderItem
    {
        public MeshRenderItem(MeshModel model, Effect shader) : base(model, shader) 
        {
        }
    }

    public class TextureMeshRenderItem : MeshRenderItem
    {
        public int AlphaMode { get; set; } = 0;

        public Dictionary<TexureType, Texture2D> Textures { get; set; } = new Dictionary<TexureType, Texture2D>();

        private TextureCube m_pbrDiffuse;
        private TextureCube m_pbrSpecular;
        private Texture2D m_BRDF_LUT;


        public TextureMeshRenderItem(MeshModel model, Effect shader, ResourceLibary resourceLibary) : base(model, shader)
        {
            m_pbrDiffuse = resourceLibary.XnaContentManager.Load<TextureCube>("textures\\phazer\\rustig_koppie_DiffuseHDR");
            m_pbrSpecular = m_pbrDiffuse;// resourceLibary.XnaContentManager.Load<TextureCube>("textures\\phazer\\rustig_koppie_SpecularHDR");
            m_BRDF_LUT = resourceLibary.XnaContentManager.Load<Texture2D>("textures\\phazer\\Brdf_rgba32f_raw");
        }

        public override void ApplyCustomShaderParams()
        {
            var hasDiffuse = Textures.TryGetValue(TexureType.Diffuse, out var diffuseTexture);
            var hasSpec = Textures.TryGetValue(TexureType.Specular, out var specTexture);
            var hasNormal = Textures.TryGetValue(TexureType.Normal, out var normalTexture);
            var hasGloss = Textures.TryGetValue(TexureType.Gloss, out var glossTexture);

            _shader.Parameters["DiffuseTexture"].SetValue(diffuseTexture);
            _shader.Parameters["SpecularTexture"].SetValue(specTexture);
            _shader.Parameters["NormalTexture"].SetValue(normalTexture);
            _shader.Parameters["GlossTexture"].SetValue(glossTexture);

            _shader.Parameters["tex_cube_specular"].SetValue(m_pbrDiffuse);
            _shader.Parameters["specularBRDF_LUT"].SetValue(m_BRDF_LUT);
            _shader.Parameters["tex_cube_diffuse"].SetValue(m_pbrSpecular);

            _shader.Parameters["UseAlpha"].SetValue(AlphaMode == 1);
            _shader.Parameters["doAnimation"].SetValue(true);
            
            Matrix[] data = new Matrix[256];
            for (int i = 0; i < 256; i++)
                data[i] = Matrix.Identity;
           
            var animatedModel = _model as Rmv2RenderModel;
            if (animatedModel != null)
            {
                _shader.Parameters["WeightCount"].SetValue(animatedModel.WeightCount);

                var player = animatedModel._animationPlayer;
                if (player != null)
                {
                    var frame = player.GetCurrentFrame();
                    if (frame != null)
                    {
                        for (int i = 0; i < frame.BoneTransforms.Count(); i++)
                            data[i] = frame.BoneTransforms[i].WorldTransform;
                    }
                }

               // animatedModel.UpdateVertexBuffer();
            }

           
            _shader.Parameters["tranforms"].SetValue(data);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
    public interface ISceneGraphNode
    {
        Matrix WorldTransform { get; set; }
        void Render(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters);
        void Update(GameTime time);
    }
}
