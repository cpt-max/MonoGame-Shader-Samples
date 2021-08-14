using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderTest
{
    struct Monkey
    {
        public Vector3 pos;
        public float pad1; // pad to Vector4
        public Vector3 vel;
        public float pad2; // pad to Vector4
    };

    public class ShaderTestGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        const int WorldSize = 100;
        const int ComputeGroupSize = 64; // has to be the same as the GroupSize defined in the compute shader 
        const int MonkeyCount = ComputeGroupSize * 100;

        GraphicsDeviceManager graphics;

        Effect effect;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
        Model monkeyModel;

        StructuredBuffer allMonkeyBuffer;
        StructuredBuffer visibleMonkeyBuffer;
        IndirectDrawBuffer indirectDrawBuffer;
    
        Random rand = new Random();
        float cullRadius = 30;

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
            textFont = Content.Load<SpriteFont>("TextFont");
            monkeyModel = Content.Load<Model>("Model");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            allMonkeyBuffer = new StructuredBuffer(GraphicsDevice, typeof(Monkey), MonkeyCount, BufferUsage.None, ShaderAccess.ReadWrite);
            visibleMonkeyBuffer = new StructuredBuffer(GraphicsDevice, typeof(Monkey), MonkeyCount, BufferUsage.None, ShaderAccess.ReadWrite);
            indirectDrawBuffer = new IndirectDrawBuffer(GraphicsDevice, BufferUsage.None, ShaderAccess.ReadWrite);

            FillMonkeyBufferRandomly(allMonkeyBuffer);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                cullRadius += cullRadius * dt;
            if (keyboardState.IsKeyDown(Keys.Q))
                cullRadius -= cullRadius * dt;

            cullRadius = Math.Min(cullRadius, WorldSize);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            ComputeMonkeys(gameTime);
            DrawMonkeys();
            DrawText();

            base.Draw(gameTime);   
        }

        private void ComputeMonkeys(GameTime gameTime)
        {
            indirectDrawBuffer.SetData(new DrawIndexedInstancedArguments
            {
                IndexCountPerInstance = (uint)monkeyModel.Meshes[0].MeshParts[0].IndexBuffer.IndexCount,
                InstanceCount = 0,
                StartIndexLocation = 0,
                BaseVertexLocation = 0,
                StartInstanceLocation = 0,
            });

            effect.Parameters["AllMonkeys"].SetValue(allMonkeyBuffer);
#if OPENGL
            effect.Parameters["VisibleMonkeys_1"].SetValue(visibleMonkeyBuffer);
#else
            effect.Parameters["VisibleMonkeys"].SetValue(visibleMonkeyBuffer);
#endif
            effect.Parameters["IndirectDraw"].SetValue(indirectDrawBuffer);
            effect.Parameters["WorldSize"].SetValue((float)WorldSize);
            effect.Parameters["DeltaTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            effect.Parameters["CullRadius"].SetValue(cullRadius);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();
                GraphicsDevice.DispatchCompute(MonkeyCount / ComputeGroupSize, 1, 1);
            }
        }

        private void DrawMonkeys()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix view = Matrix.CreateLookAt(new Vector3(40, 10, 80), Vector3.Zero, new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.Parameters["ViewProjection"].SetValue(view * projection);
            effect.Parameters["VisibleMonkeysReadonly"].SetValue(visibleMonkeyBuffer);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(monkeyModel.Meshes[0].MeshParts[0].VertexBuffer);
                GraphicsDevice.Indices = monkeyModel.Meshes[0].MeshParts[0].IndexBuffer;
                GraphicsDevice.DrawIndexedInstancedPrimitivesIndirect(PrimitiveType.TriangleList, indirectDrawBuffer);
            }
        }

        private void DrawText()
        {
            string text = "Q and W for Cull Radius\n";
            string values = cullRadius.ToString("0") + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1050, 30), Color.White);
            spriteBatch.End();
        }

        private void FillMonkeyBufferRandomly(StructuredBuffer buffer)
        {
            Monkey[] monkey = new Monkey[MonkeyCount];

            for (int i = 0; i < MonkeyCount; i++)
            {
                monkey[i].pos = WorldSize * new Vector3(
                    (float)rand.NextDouble() * 2 - 1,
                    (float)rand.NextDouble() * 2 - 1,
                    (float)rand.NextDouble() * 2 - 1);

                monkey[i].vel = new Vector3(
                    (float)(rand.NextDouble() * 2 - 1),
                    (float)(rand.NextDouble() * 2 - 1),
                    (float)(rand.NextDouble() * 2 - 1));
            }

            buffer.SetData(monkey);
        }
    }
}
