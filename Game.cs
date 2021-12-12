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
        public float age;
        public float padding;
    };

    public class ShaderGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        const int MaxParticleCount = 12800000;
        const int StartParticleCount = 1000;
        const int ComputeGroupSize = 256; // has to be the same as the GroupSize defined in the compute shader 

        GraphicsDeviceManager graphics;

        Effect effect;
        Texture2D texture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
        VertexBuffer pointSpriteVertices;

        StructuredBuffer particleBuffer1;
        StructuredBuffer particleBuffer2;

        IndirectDrawBuffer indirectDrawBuffer1;
        IndirectDrawBuffer indirectDrawBuffer2;
        
        StructuredBuffer particleBufferIn => flipBuffersInOut ? particleBuffer2 : particleBuffer1;
        StructuredBuffer particleBufferOut => flipBuffersInOut ? particleBuffer1 : particleBuffer2;

        IndirectDrawBuffer indirectDrawBufferIn => flipBuffersInOut ? indirectDrawBuffer2 : indirectDrawBuffer1;
        IndirectDrawBuffer indirectDrawBufferOut => flipBuffersInOut ? indirectDrawBuffer1 : indirectDrawBuffer2;

        Random rand = new Random();

        Vector2 mousePos;
        float spawnRadius = 0.1f;
        float spawn;

        float fps;
        int particleCount = -1; 
        bool spacePressed;
        bool flipBuffersInOut;

        public ShaderGame()
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
            graphics.PreferredBackBufferWidth = ResolutionX;
            graphics.PreferredBackBufferHeight = ResolutionY;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = Content.Load<Effect>("Effect");
            texture = Content.Load<Texture2D>("Texture");
            textFont = Content.Load<SpriteFont>("TextFont");
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pointSpriteVertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), ComputeGroupSize, BufferUsage.WriteOnly);

            particleBuffer1 = new StructuredBuffer(GraphicsDevice, typeof(Particle), MaxParticleCount, BufferUsage.None, ShaderAccess.ReadWrite);
            particleBuffer2 = new StructuredBuffer(GraphicsDevice, typeof(Particle), MaxParticleCount, BufferUsage.None, ShaderAccess.ReadWrite);

            FillParticleBufferRandomly(particleBuffer1);
            FillParticleBufferRandomly(particleBuffer2);

            int indirectDrawBufferSize = DispatchComputeArguments.Count + DrawInstancedArguments.Count + 2; // +2 so we have space for extra fields: particleCount and dummyParticleID
            indirectDrawBuffer1 = new IndirectDrawBuffer(GraphicsDevice, BufferUsage.None, ShaderAccess.ReadWrite, indirectDrawBufferSize);
            indirectDrawBuffer2 = new IndirectDrawBuffer(GraphicsDevice, BufferUsage.None, ShaderAccess.ReadWrite, indirectDrawBufferSize);

            InitIndirectDrawBuffer(indirectDrawBufferIn, StartParticleCount);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fps = 1 / dt;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                spawnRadius += spawnRadius * dt;
            if (keyboardState.IsKeyDown(Keys.Q))
                spawnRadius -= spawnRadius * dt;

            spawn = mouseState.LeftButton  == ButtonState.Pressed ?  1f :
                    mouseState.RightButton == ButtonState.Pressed ? -1f : 0f;

            if (spawn != 0)
                particleCount = -1; // particle count is unknown now

            mousePos = mouseState.Position.ToVector2() / new Vector2(ResolutionX, ResolutionY) * 2 - Vector2.One;
            mousePos.Y *= -1;

            if (keyboardState.IsKeyDown(Keys.Tab)) 
            {
                var data = new uint[1];
                indirectDrawBufferIn.GetData(7*4, data, 0, 1);
                particleCount = (int)data[0];
            }

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (!spacePressed)
                {
                    FillParticleBufferRandomly(particleBufferIn);
                    InitIndirectDrawBuffer(indirectDrawBufferIn, StartParticleCount);
                }
                spacePressed = true;
            }
            else spacePressed = false;

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
            // reset the particle count (and group count) in the output buffer to zero,
            // the compute shader will then increment this counter for every live particle.  
            InitIndirectDrawBuffer(indirectDrawBufferOut, 0);

            effect.Parameters["ParticlesIn"].SetValue(particleBufferIn);
            effect.Parameters["ParticlesOut"].SetValue(particleBufferOut);
            effect.Parameters["IndirectDrawIn"].SetValue(indirectDrawBufferIn);
            effect.Parameters["IndirectDrawOut"].SetValue(indirectDrawBufferOut);
            effect.Parameters["MaxParticleCount"].SetValue(MaxParticleCount);
            effect.Parameters["RandInt"].SetValue(rand.Next());
            effect.Parameters["DeltaTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            effect.Parameters["MousePos"].SetValue(mousePos);
            effect.Parameters["Spawn"].SetValue(spawn);
            effect.Parameters["SpawnRadius"].SetValue(spawnRadius);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();
                GraphicsDevice.DispatchComputeIndirect(indirectDrawBufferIn, 0);
            }

            // flip the buffers, so this frame's output buffers become next frame's input buffers
            flipBuffersInOut = !flipBuffersInOut;
        }

        private void DrawParticles()
        {
            effect.Parameters["ParticlesDraw"].SetValue(particleBufferIn);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(pointSpriteVertices);
                GraphicsDevice.DrawInstancedPrimitivesIndirect(PrimitiveType.PointList, indirectDrawBufferIn, DispatchComputeArguments.Count * 4);
            }
        }

        private void DrawMousePointer()
        {
            Vector2 posPixel = (mousePos * new Vector2(1, -1) + Vector2.One) / 2 * new Vector2(ResolutionX, ResolutionY);
            Vector2 origin = new Vector2(texture.Width, texture.Height) / 2;
            Vector2 size = new Vector2(ResolutionX, ResolutionY) / new Vector2(texture.Width, texture.Height) * spawnRadius;
            Color col = spawn >= 0 ? Color.White : Color.Red;

            spriteBatch.Begin();
            spriteBatch.Draw(texture, posPixel, null, col, 0, origin, size, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        private void DrawText()
        {
            string text = "FPS\n";
            text +=       "Q and W for Radius\n";
            text +=       "Mouse to Spawn and Erase\n";
            text +=       "Tab to query Particle Count\n";
            text +=       "Space to Reset\n";

            string values = fps.ToString("0") + "\n";
            values += (spawnRadius*100).ToString("0.0") + "\n\n";
            values += (particleCount < 0 ? "unknown" : particleCount.ToString()) + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1050, 30), Color.White);
            spriteBatch.End();
        }

        private void FillParticleBufferRandomly(StructuredBuffer buffer)
        {
            Particle[] particles = new Particle[MaxParticleCount];

            for (int i = 0; i < MaxParticleCount; i++)
            {
                particles[i].pos = new Vector2(
                    (float)rand.NextDouble() * 2 - 1,
                    (float)rand.NextDouble() * 2 - 1);

                particles[i].vel = new Vector2(
                    (float)(rand.NextDouble() * 2 - 1) * 0.05f,
                    (float)(rand.NextDouble() * 2 - 1) * 0.05f);

                particles[i].age = 1;
            }

            buffer.SetData(particles);
        }

        private void InitIndirectDrawBuffer(IndirectDrawBuffer buffer, uint particleCount)
        {
            uint groupCount = (uint)Math.Ceiling((double)particleCount / ComputeGroupSize);

            var dispatchIndirectArgs = new DispatchComputeArguments
            {
                GroupCountX = groupCount,
                GroupCountY = 1,
                GroupCountZ = 1,
            };

            var drawIndirectArgs = new DrawInstancedArguments
            {
                VertexCountPerInstance = ComputeGroupSize,
                InstanceCount = groupCount, // each instance draws an entire group of particles. This is more efficient than 1 particle per instance
                StartVertexLocation = 0,
                StartInstanceLocation = 0,
            };

            var data = new uint[buffer.ElementCount];
            int offset = 0;

            offset += dispatchIndirectArgs.WriteToArray(data, offset);
            offset += drawIndirectArgs.WriteToArray(data, offset);

            data[offset++] = particleCount;
            data[offset++] = 0;

            buffer.SetData(data);
        }
    }
}
