using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderTest
{
    struct Particle
    {
        public Vector2 pos;
        public Vector2 vel;
    };

    public class ShaderTestGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        const int MaxParticleCount = 1000000;
        const int ComputeGroupSize = 256; // has to be the same as the GroupSize define in the compute shader 

        int particleCount = 10000;
        float fps;
        bool spacePressed;
        float force;
        Vector2 forceCenter;

        GraphicsDeviceManager graphics;
        Random rand = new Random();

        Effect effect;
        Texture2D texture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        StructuredBuffer particleBuffer; // stores all the particle information, will be updated by the compute shader
        VertexBuffer quadVertexBuffer; // used for drawing the particles
        IndexBuffer quadIndexBuffer;   // used for drawing the particles

        public ShaderTestGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;

            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
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
            texture = Content.Load<Texture2D>("Texture");
            textFont = Content.Load<SpriteFont>("TextFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            particleBuffer = new StructuredBuffer(GraphicsDevice, typeof(Particle), MaxParticleCount, BufferUsage.None, ShaderAccess.ReadWrite);

            FillParticlesBufferRandomly();
            CreateQuadBufferForParticleRendering(ref quadVertexBuffer, ref quadIndexBuffer);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fps = 1 / dt;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                particleCount += (int)Math.Ceiling(dt * particleCount);
            if (keyboardState.IsKeyDown(Keys.Q))
                particleCount -= (int)Math.Ceiling(dt * particleCount);

            particleCount = Math.Max(1, Math.Min(MaxParticleCount, particleCount));

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (!spacePressed)
                    FillParticlesBufferRandomly();
                spacePressed = true;
            } 
            else spacePressed = false;

            force = mouseState.LeftButton  == ButtonState.Pressed ?  1f :
                    mouseState.RightButton == ButtonState.Pressed ? -1f : 0f;
            force *= 5;

            forceCenter = mouseState.Position.ToVector2() / new Vector2(ResolutionX, ResolutionY) * 2 - Vector2.One;
            forceCenter.Y *= -1;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            ComputeParticles(gameTime);

            DrawParticles();
            DrawText();
            DrawMousePointer();

            base.Draw(gameTime);
        }

        private void ComputeParticles(GameTime gameTime)
        {
            effect.Parameters["Particles"].SetValue(particleBuffer);
            effect.Parameters["DeltaTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            effect.Parameters["ForceCenter"].SetValue(forceCenter);
            effect.Parameters["Force"].SetValue(force);

            int groupCount = (int)Math.Ceiling((double)particleCount / ComputeGroupSize);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();
                GraphicsDevice.DispatchCompute(groupCount, 1, 1);
            }
        }

        private void DrawParticles()
        {
            effect.Parameters["ParticlesReadOnly"].SetValue(particleBuffer);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(quadVertexBuffer);
                GraphicsDevice.Indices = quadIndexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, particleCount * 2);
            }
        }

        private void DrawMousePointer()
        {
            Vector2 posPixel = (forceCenter * new Vector2(1, -1) + Vector2.One) / 2 * new Vector2(ResolutionX, ResolutionY);
            Vector2 origin = new Vector2(texture.Width, texture.Height) / 2;
            Vector2 size = Vector2.One * 0.05f;

            spriteBatch.Begin();
            spriteBatch.Draw(texture, posPixel, null, new Color(255,230,50), 0, origin, size, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        private void DrawText()
        {
            string text = "FPS\n";
            text +=       "Q and W for Particle Count\n";
            text +=       "Mouse for Force Fields\n";
            text +=       "Space for Randomize\n";

            string values = fps.ToString("0") + "\n";
            values += particleCount.ToString() + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1050, 30), Color.White);
            spriteBatch.End();
        }

        private void FillParticlesBufferRandomly()
        {
            Particle[] particles = new Particle[MaxParticleCount];

            for (int i = 0; i < MaxParticleCount; i++)
            {
                particles[i].pos = new Vector2(
                    (float)rand.NextDouble() * 2 - 1,
                    (float)rand.NextDouble() * 2 - 1);

                particles[i].vel = new Vector2(
                    (float)(rand.NextDouble() * 2 - 1) * 0.1f,
                    (float)(rand.NextDouble() * 2 - 1) * 0.1f);
            }

            particleBuffer.SetData(particles);
        }

        private void CreateQuadBufferForParticleRendering(ref VertexBuffer quadVertexBuffer, ref IndexBuffer quadIndexBuffer)
        {
            var vertices = new VertexPositionTexture[MaxParticleCount * 4];
            var indices = new int[MaxParticleCount * 6];

            for (int i = 0; i < MaxParticleCount; i++)
            {
                int vInd = i * 4;
                int iInd = i * 6;

                vertices[vInd + 0] = new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0));
                vertices[vInd + 1] = new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0));
                vertices[vInd + 2] = new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1));
                vertices[vInd + 3] = new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1));

                indices[iInd + 0] = vInd + 0;
                indices[iInd + 1] = vInd + 1;
                indices[iInd + 2] = vInd + 2;
                indices[iInd + 3] = vInd + 0;
                indices[iInd + 4] = vInd + 2;
                indices[iInd + 5] = vInd + 3;
            };

            quadVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(vertices);

            quadIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            quadIndexBuffer.SetData(indices);
        }
    }
}
