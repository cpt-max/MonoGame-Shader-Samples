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
        Texture2D texture;
        TextureCube cubeTexture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
        RasterizerState rasterStateWire;

        float tesselation = 20;
        float radius = 0.2f;
        float rotation = (float)Math.PI * 5 / 4;
        float roundness = 1;
        bool enableWireFrame = true;
        bool enableTexture = false;

        bool spaceKeyWasUpLastFrame;
        bool tabKeyWasUpLastFrame;

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
            texture = Content.Load<Texture2D>("Texture");
            cubeTexture = Content.Load<TextureCube>("Cubemap");
            textFont = Content.Load<SpriteFont>("TextFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            CreateCubeMesh(ref vertexBuffer, ref indexBuffer);
            
            rasterStateWire = new RasterizerState();
            rasterStateWire.CullMode = CullMode.None;
            rasterStateWire.FillMode = FillMode.WireFrame;
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space) && spaceKeyWasUpLastFrame)
                enableWireFrame = !enableWireFrame;
            spaceKeyWasUpLastFrame = keyboardState.IsKeyUp(Keys.Space);

            if (keyboardState.IsKeyDown(Keys.Tab) && tabKeyWasUpLastFrame)
                enableTexture = !enableTexture;
            tabKeyWasUpLastFrame = keyboardState.IsKeyUp(Keys.Tab);
            

            if (keyboardState.IsKeyDown(Keys.W))
                tesselation += dt * 10;
            if (keyboardState.IsKeyDown(Keys.Q))
                tesselation -= dt * 10;

            if (keyboardState.IsKeyDown(Keys.S))
                radius += dt * 0.3f;
            if (keyboardState.IsKeyDown(Keys.A))
                radius -= dt * 0.3f;

            if (keyboardState.IsKeyDown(Keys.X))
                roundness += dt * 2;
            if (keyboardState.IsKeyDown(Keys.Z))
                roundness -= dt * 2;

            if (keyboardState.IsKeyDown(Keys.Right))
                rotation += dt;
            if (keyboardState.IsKeyDown(Keys.Left))
                rotation -= dt;

            tesselation = Math.Max(0, Math.Min(30, tesselation));
            radius = Math.Max(0, Math.Min(0.5f, radius));
            roundness = Math.Max(0, Math.Min(1, roundness));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = enableWireFrame  ? rasterStateWire : RasterizerState.CullClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Vector3 camPos = new Vector3(0, -0.9f, 0.6f);
            Vector3 camLookAt = new Vector3(0, 0, 0.05f);

            Matrix worldRot = Matrix.CreateRotationZ(rotation);
            Matrix world = worldRot;
            Matrix view = Matrix.CreateLookAt(camPos, camLookAt, new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldRot"].SetValue(worldRot);
            effect.Parameters["ViewProjection"].SetValue(view * projection);
            effect.Parameters["CamPos"].SetValue(camPos);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            effect.Parameters["Tesselation"].SetValue(1f + (int)(tesselation) * 2f);
            effect.Parameters["Radius"].SetValue(Math.Max(radius, 0.000001f));
            effect.Parameters["EnableTexture"].SetValue(enableTexture);
            effect.Parameters["Test"].SetValue(roundness);

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
            text +=       "A and S for Radius: \n";
            text +=       "Left and Right for Rotation: \n";
            text +=       "Space for Wireframe: \n";
            text +=       "Tab for Texture: \n";

            string values = tesselation.ToString() + "\n";
            values += radius.ToString() + "\n";
            values += rotation.ToString() + "\n";
            values += (enableWireFrame ? "On" : "Off") + "\n";
            values += (enableTexture ? "On" : "Off") + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1000, 30), Color.White);
            spriteBatch.End();
        }

        private void CreateCubeMesh(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
        {
            float s = 0.5f;
            float n = (float)Math.Sqrt(1.0/3);

            var vertices = new VertexPositionNormalTexture[]
            {   
                // top vertices
                new VertexPositionNormalTexture(new Vector3(-s,  s, s),  new Vector3(-n,  n, n),  new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3( s,  s, s),  new Vector3( n,  n, n),  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3( s, -s, s),  new Vector3( n, -n, n),  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-s, -s, s),  new Vector3(-n, -n, n),  new Vector2(0, 0)),
                // bottom vertices
                new VertexPositionNormalTexture(new Vector3(-s,  s, -s), new Vector3(-n,  n, -n), new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3( s,  s, -s), new Vector3( n,  n, -n), new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3( s, -s, -s), new Vector3( n, -n, -n), new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-s, -s, -s), new Vector3(-n, -n, -n), new Vector2(0, 1)),
            };

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            // create one patch for every cube face
            // every patch is defined by 4 indices in clockwise order
            var indices = new int[24] 
            {
                0, 1, 2, 3, // top
                3, 2, 6, 7, // front
                0, 3, 7, 4, // left
                1, 0, 4, 5, // rear
                2, 1, 5, 6, // right
                7, 6, 5, 4, // bottom
            };

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            /*
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
            indexBuffer.SetData(indices);*/
        }
    }
}
