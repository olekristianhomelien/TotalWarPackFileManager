using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System.Collections.Generic;
using System.Drawing;
using Viewer.GraphicModels;
using Viewer.NHew;
using Viewer.Scene;


namespace WpfTest.Scenes
{
    /*
     ZeroMemory(&samplerDesc, sizeof(samplerDesc));
    samplerDesc.Filter = D3D11_FILTER::D3D11_FILTER_ANISOTROPIC;
    samplerDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.MipLODBias = 0.0f;
    samplerDesc.MaxAnisotropy = 16;
    samplerDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    samplerDesc.BorderColor[0] = 0;
    samplerDesc.BorderColor[1] = 0;
    samplerDesc.BorderColor[2] = 0;
    samplerDesc.BorderColor[3] = 0;
    samplerDesc.MinLOD = 0;
    samplerDesc.MaxLOD = D3D11_FLOAT32_MAX;
     */

    /// <summary>
    /// Displays a spinning cube and an fps counter. Background color defaults to <see cref="Color.CornflowerBlue"/> and changes to <see cref="Color.Black"/> while left mouse button down is registered.
    /// Note that this is just an example implementation of <see cref="WpfGame"/>.
    /// Based on: http://msdn.microsoft.com/en-us/library/bb203926(v=xnagamestudio.40).aspx
    /// </summary>
    public class Scene3d : WpfGame
    {
        private BasicEffect _basicEffect;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        private Matrix _projectionMatrix;

        private bool _disposed;

        ArcBallCamera _camera;
        public List<RenderItem> DrawBuffer = new List<RenderItem>();

        public delegate void LoadSceneCallback(GraphicsDevice device);
        public LoadSceneCallback LoadScene { get; set; }

        public ISceneGraphNode SceneGraphRootNode { get; set; }


        ResourceLibary _resourceLibary;
        public TextureToTextureRenderer TextureToTextureRenderer { get; private set; }
        SpriteBatch _spriteBatch;
        protected override void Initialize()
        {
            //ContentManager
            _disposed = false;
            
            new WpfGraphicsDeviceService(this);
            //Components.Add(new FpsComponent(this));
            //Components.Add(new TimingComponent(this));
            //Components.Add(new TextComponent(this, "Leftclick anywhere in the game to change background color", new Vector2(1, 0), HorizontalAlignment.Right));

            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);
            _basicEffect = ShaderConfiguration.CreateBasicEffect(GraphicsDevice);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            

            // Camera         
            _camera = new ArcBallCamera(1, new Vector3(0));
            _camera.NearPlane = 0.001f;
            _camera.Zoom = 10;

            RefreshProjection();
            CreateScene();

            base.Initialize();
        }

        public void SetResourceLibary(ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
            _resourceLibary.XnaContentManager = new ContentManager(Services)
            {
                RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\bin\Windows"
            };
            _resourceLibary.LoadEffect("Shaders\\TestShader", ShaderTypes.Mesh);
            _resourceLibary.LoadEffect("Shaders\\LineShader", ShaderTypes.Line);
            _resourceLibary.LoadEffect("Shaders\\TexturePreview", ShaderTypes.TexturePreview);

            TextureToTextureRenderer = new TextureToTextureRenderer(GraphicsDevice, _spriteBatch, _resourceLibary);
        }

        TextureCube textureCube;
        public void CreateScene()
        {
            CubemapGeneratorHelper cubemapGeneratorHelper = new CubemapGeneratorHelper();
            cubemapGeneratorHelper.Create(@"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\Textures\CubeMaps\GamleStan", GraphicsDevice);
            ////textureCube = cubemapGeneratorHelper.CreateCubemapTexture("Blur", 28);
            ////cubemapGeneratorHelper.CreateCubemapTexture("Unprocessed", 0);
            //cubemapGeneratorHelper.SuperIm();
            textureCube = cubemapGeneratorHelper.SimpleCubeMap();
          
            LoadScene?.Invoke(GraphicsDevice);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Components.Clear();
            _disposed = true;

            _basicEffect.Dispose();
            _basicEffect = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Update projection matrix values, both in the calculated matrix <see cref="_projectionMatrix"/> as well as
        /// the <see cref="_basicEffect"/> projection.
        /// </summary>
        private void RefreshProjection()
        {
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), // 45 degree angle
                (float)GraphicsDevice.Viewport.Width /
                (float)GraphicsDevice.Viewport.Height,
                .01f, 20);
            _basicEffect.Projection = _projectionMatrix;
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            _camera.Update(mouseState, keyboardState);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime time)
        {
            RefreshProjection();

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            CommonShaderParameters commonShaderParameters = new CommonShaderParameters()
            {
                Projection = _projectionMatrix,
                View = _camera.ViewMatrix,
                IBLMap = textureCube,
                CameraPosition = _camera.Position
            };

            if (SceneGraphRootNode != null)
            {
                SceneGraphRootNode.Update(time);
                SceneGraphRootNode.Render(GraphicsDevice, Matrix.Identity, commonShaderParameters);
            }

            base.Draw(time);
        }
    }

   

    public class CommonShaderParameters
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public TextureCube IBLMap { get; set; }
        public Vector3 CameraPosition { get; set; }
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

            if(_model != null)
                world = Matrix.CreateTranslation(_model.Pivot) * world;
            _shader.Parameters["World"].SetValue(world);
            _shader.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));

            if (this as MeshRenderItem != null)
            {
                _shader.Parameters["CameraPosition"].SetValue(commonShaderParameters.CameraPosition);
                _shader.Parameters["IBLTexture"].SetValue(commonShaderParameters.IBLMap);
            }
        }

        public virtual void ApplyCustomShaderParams()
        {
         
        }

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

        public Dictionary<TexureType, Texture2D> Textures { get; set; } = new Dictionary<TexureType, Texture2D>();

        public TextureMeshRenderItem(MeshModel model, Effect shader) : base(model, shader)
        { }

        public override void ApplyCustomShaderParams()
        {
            var hasDiffuse = Textures.TryGetValue(TexureType.Diffuse, out var diffuseTexture);
            _shader.Parameters["HasDiffuse"].SetValue(hasDiffuse);
            if (hasDiffuse)
                _shader.Parameters["DiffuseTexture"].SetValue(diffuseTexture);

            var hasSpecular = Textures.TryGetValue(TexureType.Specular, out var specularTexture);
            _shader.Parameters["HasSpecular"].SetValue(hasSpecular);
            if (hasDiffuse)
                _shader.Parameters["SpecularTexture"].SetValue(specularTexture);



            
            /*var hasSpecular = Textures.TryGetValue(TexureType.Specular, out var specularTexture);
            _shader.Parameters["HasSpecular"].SetValue(hasSpecular);
            if (hasDiffuse)
                _shader.Parameters["SpecularTexture"].SetValue(specularTexture);*/
        }

        public override void Dispose()
        {
            //foreach (var texture in Textures)
            //    texture.Value.Dispose();
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
