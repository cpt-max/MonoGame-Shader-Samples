using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderSample
{
    struct Particle
    {
        public Vector2 pos;
        public Vector2 vel;
    };

    public class ShaderGame : Game
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
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        StructuredBuffer particleBuffer; // stores all the particle information, will be updated by the compute shader
        VertexBuffer vertexBuffer; // used for drawing the particles

        public ShaderGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

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
            textFont = Content.Load<SpriteFont>("TextFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            particleBuffer = new StructuredBuffer(GraphicsDevice, typeof(Particle), MaxParticleCount, BufferUsage.None, ShaderAccess.ReadWrite);
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), MaxParticleCount, BufferUsage.WriteOnly); // no need to initialize, all the data for drawing the particles is coming from the structured buffer

            FillParticlesBufferRandomly();
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

                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, particleCount);
            }
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
    }
}
