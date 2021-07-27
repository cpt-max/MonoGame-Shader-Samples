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

        const int ComputeGroupSize = 64; // needs to be the same as the GroupSize defined in the compute shader

        GraphicsDeviceManager graphics;
        Effect effect;
        VertexBuffer computeVertexBuffer;
        IndexBuffer computeIndexBuffer;

        SpriteBatch spriteBatch;
        SpriteFont textFont;

        Vector3 camPos;
        Vector3 mouseRayDir;
        float rotation;
        float attract;
        bool tabPressed;

        public ShaderTestGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;

            //GraphicsAdapter.UseDebugLayers = true;
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

            LoadComputeMesh(ref computeVertexBuffer, ref computeIndexBuffer);
        }

        private void LoadComputeMesh(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
        {
            // vertex and index buffers loaded through the content pipeline are not created with ShaderAccess.ReadWrite,
            // so we have to create new buffers with ShaderAccess.ReadWrite amd copy the loaded data into them.
            var model = Content.Load<Model>("Model");
            var mesh = model.Meshes[0].MeshParts[0];

            var vertices = new VertexPositionNormalTexture[mesh.NumVertices];
            var indices = new ushort[mesh.IndexBuffer.IndexCount];

            mesh.VertexBuffer.GetData(vertices);
            mesh.IndexBuffer.GetData(indices);

            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly, ShaderAccess.ReadWrite);
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly, ShaderAccess.ReadWrite);

            vertexBuffer.SetData(vertices);
            indexBuffer.SetData(indices);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                rotation += dt;
            if (keyboardState.IsKeyDown(Keys.Q))
                rotation -= dt;

            if (keyboardState.IsKeyDown(Keys.Space))
                LoadComputeMesh(ref computeVertexBuffer, ref computeIndexBuffer);
            
            if (keyboardState.IsKeyDown(Keys.Tab) && !tabPressed)
                ComputeFlipIndices();

            tabPressed = keyboardState.IsKeyDown(Keys.Tab);

            attract = mouseState.LeftButton  == ButtonState.Pressed ? -1f :
                      mouseState.RightButton == ButtonState.Pressed ?  1f : 0f;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            ComputeVertexSculpting(gameTime);
            DrawMesh();
            DrawText();

            base.Draw(gameTime);
        }

        private void ComputeVertexSculpting(GameTime gameTime)
        {
            effect.Parameters["Vertices"].SetValue(computeVertexBuffer);
            effect.Parameters["CamPos"].SetValue(camPos);
            effect.Parameters["MouseRay"].SetValue(mouseRayDir);
            effect.Parameters["Attract"].SetValue(attract);
            effect.Parameters["DeltaTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);

            foreach (var pass in effect.Techniques["SculptVertices"].Passes)
            {
                pass.ApplyCompute();

                int groupCount = (int)Math.Ceiling((double)computeVertexBuffer.VertexCount / ComputeGroupSize); 

                GraphicsDevice.DispatchCompute(groupCount, 1, 1);
            }
        }

        private void ComputeFlipIndices()
        {
            effect.Parameters["Indices"].SetValue(computeIndexBuffer);

            foreach (var pass in effect.Techniques["FlipIndices"].Passes)
            {
                pass.ApplyCompute();

                int groupCount = (int)Math.Ceiling((double)computeIndexBuffer.IndexCount / ComputeGroupSize / 6); // every thread is responsible for 2 triangle / 6 indices

                GraphicsDevice.DispatchCompute(groupCount, 1, 1);
            }
        }

        private void DrawMesh()
        {        
            camPos = new Vector3((float)Math.Sin(rotation) * 3, 0, (float)Math.Cos(rotation) * 3);
            Vector3 camLookAt = new Vector3(0, 0, 0);

            Matrix view = Matrix.CreateLookAt(camPos, camLookAt, new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            mouseRayDir = GraphicsDevice.Viewport.Unproject(new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 1), projection, view, Matrix.Identity) - camPos;
            mouseRayDir.Normalize();

            effect.Parameters["WorldViewProjection"].SetValue(view * projection);

            foreach (var pass in effect.Techniques["DrawMesh"].Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(computeVertexBuffer);
                GraphicsDevice.Indices = computeIndexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, computeIndexBuffer.IndexCount / 3);
            }
        }

        private void DrawText()
        {
            string text = "Q and W to Rotate\n";
            text +=       "Mouse to Sculpt\n";
            text +=       "Tab to Flip Triangle Winding\n";
            text +=       "Space to Reset\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.End();
        }
    }
}
