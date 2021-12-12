using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderSample
{
    struct Circle
    {
        public Vector2 pos;
    };

    struct Collision
    {
        public int collisionCount;
    };

    public class ShaderGame : Game
    {
        const int ResolutionX = 1280;
        const int ResolutionY = 720;

        const int ComputeGroupSize = 64; // needs to be the same as the GroupSize define in the compute shader
        const int MaxCircleCount = 10000;

        int circleCount { get { return (int)circleCountFloat; } }
        float circleCountFloat = 50;
        float circleSize = 80;

        Circle[] circles = new Circle[MaxCircleCount];
        Collision[] collisions = new Collision[MaxCircleCount];

        StructuredBuffer circlesBuffer;
        StructuredBuffer collisionsBuffer;

        GraphicsDeviceManager graphics;
        Effect effect;
        Texture2D texture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;

        Random rand = new Random();

        public ShaderGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;

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

            circlesBuffer = new StructuredBuffer(GraphicsDevice, typeof(Circle), MaxCircleCount, BufferUsage.None, ShaderAccess.Read);
            collisionsBuffer = new StructuredBuffer(GraphicsDevice, typeof(Collision), MaxCircleCount, BufferUsage.None, ShaderAccess.ReadWrite);

            FillBufferWithRandomCircles();
        }

        private void FillBufferWithRandomCircles()
        {
            for (int i = 0; i < MaxCircleCount; i++)
            {
                circles[i].pos = new Vector2(
                    (float)rand.NextDouble() * ResolutionX,
                    (float)rand.NextDouble() * ResolutionY);
            }

            circlesBuffer.SetData(circles);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                circleSize += dt * circleSize;
            if (keyboardState.IsKeyDown(Keys.Q))
                circleSize -= dt * circleSize;

            if (keyboardState.IsKeyDown(Keys.S))
                circleCountFloat += dt * circleCountFloat;
            if (keyboardState.IsKeyDown(Keys.A))
                circleCountFloat -= dt * circleCountFloat;

            circleSize = Math.Max(5, Math.Min(300, circleSize));
            circleCountFloat = Math.Max(1, Math.Min(MaxCircleCount, circleCountFloat));      

            if (keyboardState.IsKeyDown(Keys.Space))
                FillBufferWithRandomCircles();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            RunCompute();
            DrawCircles();
            DrawText();

            base.Draw(gameTime);
        }

        private void RunCompute()
        {
            effect.Parameters["ObjectCount"].SetValue(circleCount);
            effect.Parameters["ObjectSize"].SetValue(circleSize);
            effect.Parameters["Inputs"].SetValue(circlesBuffer);
            effect.Parameters["Outputs"].SetValue(collisionsBuffer);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();

                int dispatchCount = (int)Math.Ceiling((double)circleCount / ComputeGroupSize);
                GraphicsDevice.DispatchCompute(dispatchCount, 1, 1);
            }

            collisionsBuffer.GetData(collisions, 0, circleCount);
        }

        private void DrawCircles()
        {
            spriteBatch.Begin();

            for (int i = 0; i < circleCount; i++)
            {
                Point size = new Point((int)circleSize);
                Point radius = new Point((int)(circleSize/2));
                Point pos = circles[i].pos.ToPoint() - radius;    
                Color col;
                switch(collisions[i].collisionCount)
                {
                    case 0: col = new Color(255, 255, 255); break;
                    case 1: col = new Color(86,255,0); break;
                    case 2: col = new Color(255, 80, 235); break;
                    case 3: col = new Color(255, 231, 0); break;
                    default: col = new Color(0, 175, 255); break;
                }

                spriteBatch.Draw(texture, new Rectangle(pos, size), null, col);
            }
                
            spriteBatch.End(); 
        }

        private void DrawText()
        {
            string text = "Q and W for Circle Size: \n"; 
            text +=       "A and S for Circle Count: \n";
            text +=       "Collision Checks: \n";
            text +=       "Space for Randomize\n";

            string values = circleSize.ToString("0") + "\n";
            values += circleCount.ToString() + "\n";
            values += (circleCount * circleCount).ToString() + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1050, 30), Color.White);
            spriteBatch.End();
        }
    }
}
