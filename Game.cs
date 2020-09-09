using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderTest
{
    public class ShaderTestGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        GraphicsDeviceManager graphics;
        Effect effect;
        Texture texture;
        VertexBuffer triangle;
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        int technique = 3;
        bool spaceKeyWasUpLastFrame;

        float textureDisplacement = 0.5f;
        float tesselation = 8;
        float geometryGeneration = 5;
        float bend;

        public ShaderTestGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;  
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics.PreferredBackBufferWidth = ResolutionX;
            graphics.PreferredBackBufferHeight = ResolutionY;
            graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            effect = Content.Load<Effect>("Effect");
            texture = Content.Load<Texture>("Texture");
            textFont = Content.Load<SpriteFont>("TextFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            triangle = CreateTriangle();   
        }

        protected override void Update(GameTime gameTime)
        {  
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bend = 4f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.5f);

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space) && spaceKeyWasUpLastFrame)
                technique = ++technique % 4;
            spaceKeyWasUpLastFrame = keyboardState.IsKeyUp(Keys.Space);

            if (keyboardState.IsKeyDown(Keys.W))
                tesselation += dt * 5;
            if (keyboardState.IsKeyDown(Keys.Q))
                tesselation -= dt * 5;

            if (keyboardState.IsKeyDown(Keys.S))
                geometryGeneration += dt * 5;
            if (keyboardState.IsKeyDown(Keys.A))
                geometryGeneration -= dt * 5;

            if (keyboardState.IsKeyDown(Keys.X))
                textureDisplacement += dt * 1;
            if (keyboardState.IsKeyDown(Keys.Z))
                textureDisplacement -= dt * 1;

            tesselation = Math.Max(1, Math.Min(20, tesselation));
            geometryGeneration = Math.Max(1, Math.Min(10, geometryGeneration));
            textureDisplacement = Math.Max(0, Math.Min(1, textureDisplacement));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix world = Matrix.CreateScale(1);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, -1, 5), new Vector3(0, 1, 0), new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.CurrentTechnique = effect.Techniques[technique];
            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["Bend"].SetValue(bend);
            effect.Parameters["Tesselation"].SetValue(tesselation);
            effect.Parameters["GeometryGeneration"].SetValue(geometryGeneration);
            effect.Parameters["TextureDisplacement"].SetValue(textureDisplacement);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                PrimitiveType primitiveType = hullShaderActive ?
                    PrimitiveType.PatchListWith3ControlPoints :
                    PrimitiveType.TriangleList;

                GraphicsDevice.SetVertexBuffer(triangle);
                GraphicsDevice.DrawPrimitives(primitiveType, 0, triangle.VertexCount / 3);
            }

            DrawText();

            base.Draw(gameTime);
        }

        private void DrawText()
        {
            string text = "Space to change Shader Stages: \n";
            text += "Q and W to change Tesselation: \n";
            text += "A and S to change GeometryGeneration: \n";
            text += "Z and X to change TextureDisplacement: \n";

            string values = effect.CurrentTechnique.Name + "\n";
            values += (hullShaderActive ? tesselation.ToString() : "none") + "\n";
            values += (geometryShaderActive ? geometryGeneration.ToString() : "none") + "\n";
            values += (geometryShaderActive || hullShaderActive ? textureDisplacement.ToString() : "none") + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(800, 50), Color.White);
            spriteBatch.End();
        }

        private VertexBuffer CreateTriangle()
        {
            var vertices = new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3( 0, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3( 1, 0, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(1, 1)),
            };

            var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            return vertexBuffer;
        }

        private bool hullShaderActive => effect.CurrentTechnique.Name.Contains("Hull");
        private bool geometryShaderActive => effect.CurrentTechnique.Name.Contains("Geometry");
    }
}
