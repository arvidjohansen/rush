using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game;
using Microsoft.Xna.Framework.Content;

namespace Models
{
    // Den gruslagte, teksturerte delen av banen.
    class Track
    {
        // Verteks- og indekstabell
        private VertexPositionTexture[] vertices;
        private int[] indices;

        // BasicEffect, GraphicsDevice og World-matrisa.
        // Alltid plassert i sentrum av World.
        private BasicEffect effect;
        private GraphicsDevice device;
        private Matrix world = Matrix.Identity;

        // Tekstur
        private Texture2D texture;

        public Track(GraphicsDevice device, ContentManager content)
        {
            // Initier device og effect
            this.device = device;
            effect = new BasicEffect(device);
            
            // Last tekstur og klargjør BasicEffect for å vise tekstur.
            texture = content.Load<Texture2D>("ground_stone");
            effect.Texture = texture;
            effect.TextureEnabled = true;

            // Sett opp kvadratet med tekstur
            initVertices();
        }

        // Setter opp geometri og teksturkoordinater.
        private void initVertices()
        {
            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(-100, 0, -100), new Vector2(0.0f, 50.0f));
            vertices[1] = new VertexPositionTexture(new Vector3(-100, 0, 100), new Vector2(0.0f, 0.0f));
            vertices[2] = new VertexPositionTexture(new Vector3(100, 0, 100), new Vector2(50.0f, 0.0f));
            vertices[3] = new VertexPositionTexture(new Vector3(100, 0, -100), new Vector2(50.0f, 50.0f));

            // Trianglestrip for to trekanter
            indices = new int[] { 0, 3, 1, 2 };
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // WVP til shaderen.
            effect.World = world;
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, indices, 0, 2, VertexPositionTexture.VertexDeclaration);
            }
        }
    }
}
