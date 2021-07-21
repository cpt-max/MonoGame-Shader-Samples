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

        const int ComputeGroupSizeXY = 8; // needs to be the same as the GroupSizeXY defined in the compute shader

        GraphicsDeviceManager graphics;
        Effect effect;
        RenderTarget2D renderTarget;
        Texture2D circleTexture;
        Texture2D computeTexture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
    
        bool initRenderTarget = true;
        int pixelOffsetX;

        public ShaderTestGame()
        {
            Content.RootDirectory = "Content";

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
            circleTexture = Content.Load<Texture2D>("Texture");
            textFont = Content.Load<SpriteFont>("TextFont");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, ResolutionX, ResolutionY);
            computeTexture = new Texture2D(GraphicsDevice, ResolutionX, ResolutionY, false, SurfaceFormat.Color, ShaderAccess.ReadWrite);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space))
                initRenderTarget = true;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (initRenderTarget)
                InitRenderTarget(); // draw a bunch of circles

            RunCompute();

            DrawTextureToTarget(computeTexture, renderTarget); // update the render target, it will be the input for next frame's compute iteration
            DrawTextureToTarget(computeTexture, null); // draw also to backbuffer, so we can see something

            DrawText();

            base.Draw(gameTime);
        }

        private void RunCompute()
        {
            effect.Parameters["Input"].SetValue(renderTarget);
            effect.Parameters["Output"].SetValue(computeTexture);
            effect.Parameters["OffsetX"].SetValue(pixelOffsetX);
            effect.Parameters["Width"].SetValue(ResolutionX);          

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();

                int groupCountX = ResolutionX / ComputeGroupSizeXY / 2; // the division by 2 is because a single thread operates on a pair of pixels, not on individual pixels
                int groupCountY = ResolutionY / ComputeGroupSizeXY;

                GraphicsDevice.DispatchCompute(groupCountX, groupCountY, 1);
            }

            // alternate the x-offset btw 0 and 1, to make sure we don't pair up the same pixels every frame,
            // which would make it impossible to sort the whole image
            pixelOffsetX = pixelOffsetX == 0 ? 1 : 0;
        }

        private void InitRenderTarget()
        {
            Random rand = new Random();

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            for (int i = 0; i < 100; i++)
            {
                var pos = new Vector2(
                    (float)rand.NextDouble() * ResolutionX - 128,
                    (float)rand.NextDouble() * ResolutionY - 128);

                var col = new Color(
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble());

                spriteBatch.Draw(circleTexture, pos, col);
            }
                
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            initRenderTarget = false;
        }

        private void DrawTextureToTarget(Texture2D tex, RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);

            spriteBatch.Begin();
            spriteBatch.Draw(tex, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        private void DrawText()
        {
            string text = "Press Space to Reset\n"; 

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.End();
        }
    }
}
