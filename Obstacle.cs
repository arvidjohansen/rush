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
    // Reklame-skilt som markerer banen bilen kan kjøre på.
    // Inneholder en nøstet struct for å holde styr på transformasjonen til hver enkelt plakat
    // når den skal tegnes.
    public class Obstacle
    {
        // Lagrer posisjon, rotasjon rundt y-aksen, samt tekstur for hver plakat.
        // Det er kun implementert bruk av samme tekstur i denne versjonen.
        private struct Instance
        {
            public Vector3 Position { get; set; }
            public float rotY { get; set; }
            public int texture;
        }

        // Vertekstabell, indekstabell, effekt, device, world-matrise.
        private VertexPositionTexture[] vertices;
        private int[] indices;
        private BasicEffect effect;
        private GraphicsDevice device;
        private Matrix world = Matrix.Identity;
        
        // Tekstur
        private Texture2D texture;

        // Tabell med alle individuelle posisjoner og rotasjoner.
        private List<Instance> instances;

        // Constructor
        public Obstacle(GraphicsDevice device, ContentManager content)
        {
            // Sett device og effect (BasicEffect);
            this.device = device;
            effect = new BasicEffect(device);

            // Last tekstur, klargjør BasicEffect for å vise tekstur
            texture = content.Load<Texture2D>("reklame");
            effect.Texture = texture;
            effect.TextureEnabled = true;
 
            // Sett opp vertekstabell og indekstabell
            initVertices();
            instances = new List<Instance>();

            // Sett opp banen
            initInstances();
        }

        private void initVertices()
        {
            // Sett opp koordinater og teksturkoordinater
            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(-2, 0, 0), new Vector2(0.0f, 0.25f));
            vertices[1] = new VertexPositionTexture(new Vector3(-2, 1, 0), new Vector2(0.0f, 0.0f));
            vertices[2] = new VertexPositionTexture(new Vector3(2, 1, 0), new Vector2(1.0f, 0.0f));
            vertices[3] = new VertexPositionTexture(new Vector3(2, 0, 0), new Vector2(1.0f, 0.25f));

            // Trianglestrip
            indices = new int[] { 3, 0, 2, 1 };
        }

        // Oppretter alle plakatene, legger dem i listen "instances"
        private void initInstances()
        {
            // Sett opp ytre kvadrat med 20 x 20 plakater
            for (int i = 0; i < 20; i++)
            {
                Instance instance = new Instance();
                instance.Position = new Vector3(-50 + i * 4, 0, -50);
                instance.rotY = 0;
                instance.texture = 0;
                instances.Add(instance);

                Instance instance2 = instance;
                instance2.rotY = MathHelper.Pi;
                instance2.Position = new Vector3(-50 + i * 4, 0, 30);
                instances.Add(instance2);

                Instance instance3 = instance;
                instance3.rotY = -MathHelper.PiOver2;
                instance3.Position = new Vector3(28, 0, -48 + i * 4);
                instances.Add(instance3);

                Instance instance4 = instance;
                instance4.rotY = MathHelper.PiOver2;
                instance4.Position = new Vector3(-52, 0, -48 + i * 4);
                instances.Add(instance4);
            }

            // Indre kvadrat, 14 x 14 plakater
            for (int i = 0; i < 14; i++)
            {
                Instance instance = new Instance();
                instance.Position = new Vector3(-38 + i * 4, 0, -38);
                instance.rotY = 0;
                instance.texture = 0;
                instances.Add(instance);

                Instance instance2 = instance;
                instance2.rotY = MathHelper.Pi;
                instance2.Position = new Vector3(-38 + i * 4, 0, 18);
                instances.Add(instance2);

                Instance instance3 = instance;
                instance3.rotY = -MathHelper.PiOver2;
                instance3.Position = new Vector3(16, 0, -36 + i * 4);
                instances.Add(instance3);

                Instance instance4 = instance;
                instance4.rotY = MathHelper.PiOver2;
                instance4.Position = new Vector3(-40, 0, -36 + i * 4);
                instances.Add(instance4);
            }

            
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // Oppdater shaderen (world settes for hver plakat)    
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            // Løp gjennom alle plakater
            foreach (Instance instance in instances)
            {
                // ISROT for plakaten settes i World
                Matrix rotation = Matrix.CreateRotationY(instance.rotY);
                Matrix translation = Matrix.CreateTranslation(instance.Position);
                effect.World = rotation * translation;

                // Tegn forsiden
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, indices, 0, 2, VertexPositionTexture.VertexDeclaration);
                }
                
                // Tegner også plakaten på baksiden ved å beregne world-matrisa på nytt.
                rotation = Matrix.CreateRotationY(instance.rotY - MathHelper.Pi);
                effect.World = rotation * translation;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, indices, 0, 2, VertexPositionTexture.VertexDeclaration);
                }


            }
        }
    }
}
