using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderSample
{
    public class ShaderGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        const int TextureSize = 128;
        const int ComputeGroupSizeXYZ = 4; // needs to be the same as the GroupSizeXYX defined in the compute shader    

        GraphicsDeviceManager graphics;
        Effect effect;
        Texture3D computeTexture;
        VertexBuffer cubeSlices;
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        Random rand = new Random();

        float rotation;
        float rotationOld;

        public ShaderGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;

            // GraphicsAdapter.UseDebugLayers = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = ResolutionX;
            graphics.PreferredBackBufferHeight = ResolutionY;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = Content.Load<Effect>("Effect");
            textFont = Content.Load<SpriteFont>("TextFont");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            computeTexture = new Texture3D(GraphicsDevice, TextureSize, TextureSize, TextureSize, false, SurfaceFormat.Color, ShaderAccess.ReadWrite);

            InitTexture3DRandomly(computeTexture);
            cubeSlices = CreateCubeSlices();
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();

            rotationOld = rotation;
            if (keyboardState.IsKeyDown(Keys.W))
                rotation += dt;
            if (keyboardState.IsKeyDown(Keys.Q))
                rotation -= dt;

            if (keyboardState.IsKeyDown(Keys.Space))
                InitTexture3DRandomly(computeTexture);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            RunCompute();
            Draw3DTextureCube();
            DrawText();

            base.Draw(gameTime);
        }

        private void RunCompute()
        {
            // don't update while rotating
            if (rotation != rotationOld)
                return;

            // update the 3D texture in the compute shader
            effect.Parameters["Texture"].SetValue(computeTexture);
            effect.Parameters["TextureSize"].SetValue(TextureSize);

            int groupCount = TextureSize / ComputeGroupSizeXYZ;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();
                GraphicsDevice.DispatchCompute(groupCount, groupCount, groupCount);
            }
        }

        private void Draw3DTextureCube()
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.Additive;

            Vector3 camPos = new Vector3((float)Math.Sin(rotation) * 2, 1, (float)Math.Cos(rotation) * 2);

            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.Parameters["TextureReadOnly"].SetValue(computeTexture);
            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);          

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(cubeSlices);
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, TextureSize * 2);
            }
        }

        private void DrawText()
        {
            string text = "Q and W to Rotate\n";
            text +=       "Space for Reset\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.End();
        }

        private void InitTexture3DRandomly(Texture3D tex)
        {
            var pixels = new Color[tex.Width * tex.Height * tex.Depth];

            for (int i = 0; i < 1000; i++)
            {
                int x = rand.Next(tex.Width);
                int y = rand.Next(tex.Height);
                int z = rand.Next(tex.Depth);

                int r = rand.Next(256);
                int g = rand.Next(256);
                int b = rand.Next(256);

                int ind = x + y * tex.Height + z * tex.Height * tex.Depth;
                pixels[ind] = new Color(r, g, b);
            }

            tex.SetData(pixels);
        }

        private VertexBuffer CreateCubeSlices()
        {
            // create one quad for every z-slice of the 3D texture => 2 triangles per slice => 6 vertices
            var vertices = new VertexPositionTexture[TextureSize * 6];

            for (int i = 0; i < TextureSize; i++)
            {
                float z = (float)i / (TextureSize - 1);
                Vector3 center = new Vector3(0.5f);

                vertices[i * 6 + 0] = new VertexPositionTexture(new Vector3(0, 0, z) - center, new Vector2(0, 1));
                vertices[i * 6 + 1] = new VertexPositionTexture(new Vector3(0, 1, z) - center, new Vector2(0, 0));
                vertices[i * 6 + 2] = new VertexPositionTexture(new Vector3(1, 1, z) - center, new Vector2(1, 0));

                vertices[i * 6 + 3] = new VertexPositionTexture(new Vector3(0, 0, z) - center, new Vector2(0, 1));
                vertices[i * 6 + 4] = new VertexPositionTexture(new Vector3(1, 1, z) - center, new Vector2(1, 0));
                vertices[i * 6 + 5] = new VertexPositionTexture(new Vector3(1, 0, z) - center, new Vector2(1, 1));
            };

            var vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vb.SetData(vertices);

            return vb;
        }
    }
}
