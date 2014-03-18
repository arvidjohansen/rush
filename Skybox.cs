using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Game
{
    // Min implementasjon av skyboks for dette bilspillet.
    // Skyboksen består av 5 sider, høyre, venstre, foran, bak og opp.
    // jeg tok ikke med undersiden, da bilens bevegelser er begrenset slik
    // at den aldri vil synes.

    // Jeg har brukt en sky-bakgrunn med litt rufsete vær, og har lastet den ned fra
    // http://www.custommapmakers.org/skyboxes.php

    // Posisjonen låses til kameraet, og for å gi effekten av at det faktisk blåser litt,
    // prøvde jeg å legge inn en liten rotasjon rundt y-aksen. Dette skaper illusjonen av 
    // at skyene flytter på seg. Rotasjonen må imidlertid slås av dersom man benytter bakgrunn
    // med fjell eller bygninger.

    // I blant vises smale skjøter i hvert hjørne av skyboksen.
    class Skybox
    {
        // Avstand fra kameraet til midten av hver vegg i skyboksen
        private const float BOUNDARY = 1000;
        // Justering av midtpunktet opp/ned når skyboksen tegnes.
        private const float y = -1;

        // Contentmanager og filnavn for å lese teksturen.
        private ContentManager content;
        private string fileName;

        // Transformasjonsmatriser, og rotasjonsvinkel rundt Y-aksen.
        private Vector3 position = new Vector3(0, y, 0);
        private Matrix translation = Matrix.Identity;
        private Matrix rotation = Matrix.CreateRotationY(0);
        private float rotY = 0;

        // Geometri og indekstabeller
        private VertexPositionTexture[] vertices;
        private List<int[]> indices = new List<int[]>();
        
        // Benytter Basiceffect
        private BasicEffect effect;
        private GraphicsDevice device;

        private Matrix world;

        // Liste med teksturer
        private List<Texture2D> textures = new List<Texture2D>();

        public Skybox(GraphicsDevice device, ContentManager content, string FileName)
        {
            this.fileName = FileName;
            this.device = device;
            this.content = content;

            AddTextures();

            Position = new Vector3(0, 0, 0);

            effect = new BasicEffect(device);
            effect.TextureEnabled = true;
            //effect.VertexColorEnabled = true;
            initVertices();
        }

        private void AddTextures()
        {
            // forventer prefiks på teksturnavnet, som bestemmer hvilken retning
            // teksturen representerer.
            AddTexture("lf");
            AddTexture("ft");
            AddTexture("rt");
            AddTexture("bk");
            AddTexture("up");
            //AddTexture("dn");
        }

        // Setter sammen riktig filnavn, og leser tekstur fra ContentManager.
        private void AddTexture(string Postfix)
        {
            textures.Add(content.Load<Texture2D>(String.Format("{0}_{1}", fileName, Postfix)));
        }

        // Geometrien for kuben settes opp
        private void initVertices()
        {
            Vector3[] coords = new Vector3[]
            {
                new Vector3(-BOUNDARY, -BOUNDARY, BOUNDARY), //0
                new Vector3(-BOUNDARY, BOUNDARY, BOUNDARY), //1
                new Vector3(-BOUNDARY, BOUNDARY, -BOUNDARY), //2
                new Vector3(-BOUNDARY, -BOUNDARY, -BOUNDARY), //3
                new Vector3(BOUNDARY, -BOUNDARY, -BOUNDARY), //4
                new Vector3(BOUNDARY, BOUNDARY, -BOUNDARY), //5
                new Vector3(BOUNDARY, BOUNDARY, BOUNDARY), //6
                new Vector3(BOUNDARY, -BOUNDARY, BOUNDARY)  //7
            };

            // Sett opp vertekstabell med referanse til koordinatene over og med 
            // korrekte teksturkoordinater
            // Jeg endte opp med å måtte speile teksturkoordinatene for denne teksturen,
            // noe som ikke var nødvendig for andre skyboxer som ble testet.

            vertices = new VertexPositionTexture[24];

            // Venstre
            vertices[0] = new VertexPositionTexture(coords[0], new Vector2(1, 1));
            vertices[1] = new VertexPositionTexture(coords[1], new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(coords[2], new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(coords[3], new Vector2(0, 1));

            // Foran
            vertices[4] = new VertexPositionTexture(coords[3], new Vector2(1, 1));
            vertices[5] = new VertexPositionTexture(coords[2], new Vector2(1, 0));
            vertices[6] = new VertexPositionTexture(coords[5], new Vector2(0, 0));
            vertices[7] = new VertexPositionTexture(coords[4], new Vector2(0, 1));

            // Høyre
            vertices[8] = new VertexPositionTexture(coords[4], new Vector2(1, 1));
            vertices[9] = new VertexPositionTexture(coords[5], new Vector2(1, 0));
            vertices[10] = new VertexPositionTexture(coords[6], new Vector2(0, 0));
            vertices[11] = new VertexPositionTexture(coords[7], new Vector2(0, 1));

            // Bak
            vertices[12] = new VertexPositionTexture(coords[7], new Vector2(1, 1));
            vertices[13] = new VertexPositionTexture(coords[6], new Vector2(1, 0));
            vertices[14] = new VertexPositionTexture(coords[1], new Vector2(0, 0));
            vertices[15] = new VertexPositionTexture(coords[0], new Vector2(0, 1));

            // Topp
            vertices[16] = new VertexPositionTexture(coords[2], new Vector2(1, 0));
            vertices[17] = new VertexPositionTexture(coords[1], new Vector2(0, 0));
            vertices[18] = new VertexPositionTexture(coords[6], new Vector2(0, 1));
            vertices[19] = new VertexPositionTexture(coords[5], new Vector2(1, 1));

            // Jeg lot bunnen være med i tilfelle jeg skulle bruke den likevel.
            vertices[20] = new VertexPositionTexture(coords[1], new Vector2(0, 1));
            vertices[21] = new VertexPositionTexture(coords[2], new Vector2(0, 1));
            vertices[22] = new VertexPositionTexture(coords[6], new Vector2(0, 1));
            vertices[23] = new VertexPositionTexture(coords[5], new Vector2(0, 1));

            // En triangle strip per side. Rekkefølgen er den samme som tekturene blir lest fra fil.
            indices.Add(new int[] { 0, 1, 3, 2 });
            indices.Add(new int[] { 4, 5, 7, 6 });
            indices.Add(new int[] { 8, 9, 11, 10 });
            indices.Add(new int[] { 12, 13, 15, 14 });

            indices.Add(new int[] { 16, 17, 19, 18 });
            
            // Bunnen kommentert ut
            //indices.Add(new int[] { 20, 21, 22, 23 });
        }

        // Property som leser og skriver posisjonen.
        // Worldmatrisa oppdateres når posisjonen flyttes.
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                position.Y = y;
                Matrix.CreateTranslation(ref position, out translation);
                world = Matrix.Identity * rotation * translation;
            }
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // En ørliten rotasjon per frame gir en fin effekt av at det blåser. Det passer teksturen
            // på skyboksen veldig godt.
            // World-matrisen oppdateres med denne rotasjonen hver gang Posisjons-property settes. 
            // Dette gjøres fra CarGame-klassen, når skybox får samme koordinater som kameraet.
            rotY -= 0.0001f;
            rotY = rotY % MathHelper.TwoPi;
            rotation = Matrix.CreateRotationY(rotY);

            // WVP til BasicEffect-objektet.
            effect.World = world;
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            
            int textureID = 0;
            foreach (int[] list in indices)
            {
                // Tegn hver side med riktig tekstur
                effect.Texture = textures[textureID++];
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, list, 0, 2, VertexPositionTexture.VertexDeclaration);
                }
                
            }
        }
    }
}
