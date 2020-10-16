using System;
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
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        float tesselation = 30;
        float radius = 0.1f;
        float rotation = 0;

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
            textFont = Content.Load<SpriteFont>("TextFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            CreateMesh(ref vertexBuffer, ref indexBuffer);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                tesselation += dt * 5;
            if (keyboardState.IsKeyDown(Keys.Q))
                tesselation -= dt * 5;

            if (keyboardState.IsKeyDown(Keys.S))
                radius += dt * 0.1f;
            if (keyboardState.IsKeyDown(Keys.A))
                radius -= dt * 0.1f;

            if (keyboardState.IsKeyDown(Keys.X))
                rotation += dt;
            if (keyboardState.IsKeyDown(Keys.Z))
                rotation -= dt;

            tesselation = Math.Max(0, Math.Min(30, tesselation));
            radius = Math.Max(0, Math.Min(0.25f, radius));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix world = Matrix.CreateRotationZ(rotation);// CreateScale(10);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, -1, 0.5f), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            effect.Parameters["Tesselation"].SetValue(1f + (int)(tesselation) * 2f);
            effect.Parameters["Radius"].SetValue(radius);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.PatchListWith4ControlPoints, 0, 0, indexBuffer.IndexCount / 4);
            }

            DrawText();

            base.Draw(gameTime);
        }

        private void DrawText()
        {
            string text = "Q and W for Tesselation: \n";
            text       += "A and S for Radius: \n";
            text       += "Z and X for Rotation: \n";

            string values = tesselation.ToString() + "\n";
            values += radius.ToString() + "\n";
            values += rotation.ToString() + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(800, 50), Color.White);
            spriteBatch.End();
        }

        private void CreateMesh(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
        {
            float px = 0.4330127f;
            float py = 0.25f;
            float ph = -1.5f;
            float nx = 0.6862392f;
            float ny = 0.3961913f;
            float nz = 0.6099734f;
            float ny2 = 0.7923826f;

            var vertices = new VertexPositionNormalTexture[] {
                // top vertices
                new VertexPositionNormalTexture(new Vector3(0, 0.5f, 0),  new Vector3(0, ny2, nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(px, py, 0),   new Vector3(nx, ny, nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(px, -py, 0),  new Vector3(nx, -ny, nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0, -0.5f, 0), new Vector3(0, -ny2, nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-px, -py, 0), new Vector3(-nx, -ny, nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-px, py, 0),  new Vector3(-nx, ny, nz), new Vector2(0, 0)),
                // bottom vertices
                new VertexPositionNormalTexture(new Vector3(0, 0.5f, ph),  new Vector3(0, 1, -nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(px, py, ph),   new Vector3(nx, ny, -nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(px, -py, ph),  new Vector3(nx, -ny, -nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0, -0.5f, ph), new Vector3(0, -1, -nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-px, -py, ph), new Vector3(-nx, -ny, -nz), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-px, py, ph),  new Vector3(-nx, ny, -nz), new Vector2(0, 0)),
            };

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            var indices = new int[32];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 3;

            indices[4] = 0;
            indices[5] = 3;
            indices[6] = 4;
            indices[7] = 5;
            
            for (int i = 0; i < 5; i++)
            {
                indices[8 + i * 4] = 0 + i;
                indices[9 + i * 4] = 6 + i;
                indices[10+ i * 4] = 7 + i;
                indices[11+ i * 4] = 1 + i;
            }

            indices[28] = 5;
            indices[29] = 11;
            indices[30] = 6;
            indices[31] = 0;
            
            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }
    }
}
