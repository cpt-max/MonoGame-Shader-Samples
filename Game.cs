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

        GraphicsDeviceManager graphics;
        Effect effect;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        Texture2D texture;
        TextureCube cubeTexture;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
        RasterizerState rasterStateWire;

        float tesselation = 15;
        float radius = 0.15f;
        float rotation = (float)Math.PI * 5 / 4;
        float testValue = 1;
        Vector2 shearAngle;
        bool enableWireFrame = true;
        bool enableTexture = false;

        bool spaceKeyWasUpLastFrame;
        bool tabKeyWasUpLastFrame;

        public ShaderGame()
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
            
            if (keyboardState.IsKeyDown(Keys.W))
                tesselation += dt * 10;
            if (keyboardState.IsKeyDown(Keys.Q))
                tesselation -= dt * 10;

            if (keyboardState.IsKeyDown(Keys.S))
                radius += dt * 0.3f;
            if (keyboardState.IsKeyDown(Keys.A))
                radius -= dt * 0.3f;

            if (keyboardState.IsKeyDown(Keys.D2))
                testValue += dt * 2;
            if (keyboardState.IsKeyDown(Keys.D1))
                testValue -= dt * 2;

            if (keyboardState.IsKeyDown(Keys.X))
                rotation += dt;
            if (keyboardState.IsKeyDown(Keys.Z))
                rotation -= dt;

            Vector2 shearAngleOld = shearAngle;

            if (keyboardState.IsKeyDown(Keys.Left))
                shearAngle.X += dt;
            if (keyboardState.IsKeyDown(Keys.Right))
                shearAngle.X -= dt;
            if (keyboardState.IsKeyDown(Keys.Up))
                shearAngle.Y += dt;
            if (keyboardState.IsKeyDown(Keys.Down))
                shearAngle.Y -= dt;

            if (keyboardState.IsKeyDown(Keys.Space) && spaceKeyWasUpLastFrame)
                enableWireFrame = !enableWireFrame;

            if (keyboardState.IsKeyDown(Keys.Tab) && tabKeyWasUpLastFrame)
                enableTexture = !enableTexture;

            spaceKeyWasUpLastFrame = keyboardState.IsKeyUp(Keys.Space);
            tabKeyWasUpLastFrame = keyboardState.IsKeyUp(Keys.Tab);

            tesselation = Math.Max(0, Math.Min(30, tesselation));
            radius = Math.Max(0, Math.Min(0.5f, radius));
            testValue = Math.Max(0, Math.Min(1, testValue));
            shearAngle.X = Math.Max(-1+radius*2, Math.Min(1-radius*2, shearAngle.X));
            shearAngle.Y = Math.Max(-1+radius*2, Math.Min(1-radius*2, shearAngle.Y));

            if (shearAngle != shearAngleOld)
                CreateCubeMesh(ref vertexBuffer, ref indexBuffer);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = enableWireFrame ? rasterStateWire : RasterizerState.CullClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Vector3 camPos = new Vector3(0, -1.4f, 0.9f) * (1 + shearAngle.Length()*0.3f);
            Vector3 camLookAt = new Vector3(0, 0, 0.05f);

            Matrix worldRot = Matrix.CreateRotationZ(rotation);
            Matrix world = worldRot;
            Matrix view = Matrix.CreateLookAt(camPos, camLookAt, new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)ResolutionX / (float)ResolutionY, 0.1f, 1000f);

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldRot"].SetValue(worldRot);
            effect.Parameters["ViewProjection"].SetValue(view * projection);
            effect.Parameters["CamPos"].SetValue(camPos);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            effect.Parameters["Tesselation"].SetValue(1f + (int)(tesselation) * 2f);
            effect.Parameters["Radius"].SetValue(Math.Max(radius, 0.000001f));
            effect.Parameters["EnableTexture"].SetValue(enableTexture);
            effect.Parameters["Test"].SetValue(testValue); 

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
            text +=       "Z and X for Rotation: \n";
            text +=       "Arrow for Shearing: \n";
            text +=       "Space for Wireframe: \n";
            text +=       "Tab for Texture: \n";

            string values = tesselation.ToString("0.0") + "\n";
            values += radius.ToString("0.00") + "\n";
            values += rotation.ToString("0.00") + "\n";
            values += shearAngle.X.ToString("0.00") + " / " + shearAngle.Y.ToString("0.00") + "\n";
            values += (enableWireFrame ? "On" : "Off") + "\n";
            values += (enableTexture ? "On" : "Off") + "\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(textFont, text, new Vector2(30, 30), Color.White);
            spriteBatch.DrawString(textFont, values, new Vector2(1050, 30), Color.White);
            spriteBatch.End();
        }

        private void CreateCubeMesh(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
        {
            float s = 0.5f;
            float sy = shearAngle.X;
            float sz = shearAngle.Y;

            // setup corner points
            var pos = new Vector3[8]
            {
                new Vector3(-s,  s-sy,  s-sz), // top-rear-left
                new Vector3( s,  s+sy,  s+sz), // top-rear-right
                new Vector3( s, -s+sy,  s+sz), // top-front-right
                new Vector3(-s, -s-sy,  s-sz), // top-front-left
                new Vector3(-s,  s-sy, -s-sz), // bottom-rear-left
                new Vector3( s,  s+sy, -s+sz), // bottom-rear-right
                new Vector3( s, -s+sy, -s+sz), // bottom-front-right
                new Vector3(-s, -s-sy, -s-sz), // bottom-front-left
            };

            // setup normals
            // those normals are used for edge rounding claculations, so changing the calculation method would mess up the rounding
            var norm = new Vector3[8]
            {
                CalcNorm(pos, 0, 1, 3, 4), // top-rear-left
                CalcNorm(pos, 1, 0, 2, 5), // top-rear-right
                CalcNorm(pos, 2, 1, 3, 6), // top-front-right
                CalcNorm(pos, 3, 0, 2, 7), // top-front-left
                CalcNorm(pos, 4, 0, 5, 7), // bottom-rear-left
                CalcNorm(pos, 5, 1, 4, 6), // bottom-rear-right
                CalcNorm(pos, 6, 2, 5, 7), // bottom-front-right
                CalcNorm(pos, 7, 3, 4, 6), // bottom-front-left
            };

            // setup texture coordinates
            // usually when texturing a cube the vertices are split, so each face has 4 independent vertices, instead of sharing vertices with other faces.
            // to keep things simple (the focus here is not on texturing) we only use 8 vertices, which means texturing is a bit limited compared to a 24 vertex version.
            var tex = new Vector2[8]
            {
                new Vector2(0, 0), // top-rear-left
                new Vector2(1, 0), // top-rear-right
                new Vector2(1, 0), // top-front-right
                new Vector2(0, 0), // top-front-left
                new Vector2(0, 1), // bottom-rear-left
                new Vector2(1, 1), // bottom-rear-right
                new Vector2(1, 1), // bottom-front-right
                new Vector2(0, 1), // bottom-front-left
            };

            // create one quad patch for every cube face
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

            // create and fill the vertex and index buffer
            var vertices = new VertexPositionNormalTexture[8];
            for (int i = 0; i < 8; i++)
                vertices[i] = new VertexPositionNormalTexture(pos[i], norm[i], tex[i]);

            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);
            indexBuffer.SetData(indices);
        }

        Vector3 CalcNorm(Vector3[] pos, int ind, int indNext1, int indNext2, int indNext3)
        {
            // adjacent edges
            Vector3 v1 = Vector3.Normalize(pos[indNext1] - pos[ind]);
            Vector3 v2 = Vector3.Normalize(pos[indNext2] - pos[ind]);
            Vector3 v3 = Vector3.Normalize(pos[indNext3] - pos[ind]);

            // adjacent face normals
            Vector3 n1 = Vector3.Normalize(Vector3.Cross(v1, v2));
            Vector3 n2 = Vector3.Normalize(Vector3.Cross(v2, v3));
            Vector3 n3 = Vector3.Normalize(Vector3.Cross(v3, v1));

            // calculate a normal vector that "evenly splits the face normals", which means the following condition has to be met:
            // dot(norm, n1) == dot(norm, n2) == dot(norm, n3)
            // solving the system of equations obtained by this condition yields the following solution: 
            float div = (n2.X * (n3.Y - n1.Y) + n2.Y * (n1.X - n3.X) - n1.X * n3.Y + n1.Y * n3.X);
            float x   = (n2.Y * (n3.Z - n1.Z) + n2.Z * (n1.Y - n3.Y) - n1.Y * n3.Z + n1.Z * n3.Y) / div;
            float y   = (n2.X * (n3.Z - n1.Z) + n2.Z * (n1.X - n3.X) - n1.X * n3.Z + n1.Z * n3.X) / div;

            Vector3 norm = Vector3.Normalize(new Vector3(x, y, 1));

            // there are multiple solutions because the x, y and z compnents of the normal vector can be flipped.
            // the solution we are interested in is the one that is closest to the following guide normal:
            Vector3 normGuide = v1 + v2 + v3;

            norm.X *= norm.X * normGuide.X < 0 ? 1 : -1;
            norm.Y *= norm.Y * normGuide.Y < 0 ? 1 : -1;
            norm.Z *= norm.Z * normGuide.Z < 0 ? 1 : -1;

            return norm;
        }
    }
}
