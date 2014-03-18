using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Game
{
    // Generering av terreng basert på Riemers tutorial.
    // http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series1/Terrain_from_file.php
    
    // Terrengets høyde bestemmes fra en bitmap-fil som har et flatt sentrum omsluttet av høyder
    // i alle retninger.

    // Fargene varierer med høyden, og det er valgt farger som får terrenget til å fremstå
    // som fjelltopper rundt banen. Fargen på istoppene er valgt for å matche med skybox.

    // Fjelltoppene ser ut til å være lenger unna enn hva som er tilfellet, i alle fall
    // var dette intensjonen.

    class Terrain
    {
        // Egendefinert effekt for terrenget.
        private Effect effect;

        // Variabler for å lagre egenskaper for terrenget i henholdt til Riemers.
        private Texture2D heightMap;
        private int terrainWidth;
        private int terrainHeight;
        private float[,] heightData;

        // Egendefinert Vertex-objekt i vertekstabellen
        private VertexPositionColorNormal[] vertices;
        private int[] indices;

        private GraphicsDevice device;
        private ContentManager content;

        // Transformasjoner
        private Matrix scale = Matrix.CreateScale(1.5f, 0.6f, 1.5f);
        private Matrix position = Matrix.CreateTranslation(-175, -5, 175);
        private Matrix world;

        public Terrain(GraphicsDevice device, ContentManager content)
        {
            // Lagre referanser til device og ContentManager.
            this.device = device;
            this.content = content;

            // Last egendefinert effekt.
            effect = content.Load<Effect>("terraineffect");
            
            // Tekstur for å bestemme høyde
            heightMap = content.Load<Texture2D>("bilterreng");

            // Kall til rutiner som kalkulerer terrenget.
            LoadHeightData(heightMap);
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
        }

        // Denne funksjonen er hentet fra Riemers Tutorial.
        private void SetUpVertices()
        {
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainWidth; x++)
            {
                // Finn maks- og minverdier for høyden for å fordele farger.
                for (int y = 0; y < terrainHeight; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }

            // Sett opp vertextabell og sett fargen i forhold til høyden
            // Jeg har valgt farger som passer med skybox.
            vertices = new VertexPositionColorNormal[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    
                    if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                        vertices[x + y * terrainWidth].Color = Color.Green;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        vertices[x + y * terrainWidth].Color = Color.DarkGreen;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        vertices[x + y * terrainWidth].Color = Color.SlateGray;
                    else
                        vertices[x + y * terrainWidth].Color = new Color(new Vector3(159,180,188));
                }
            }
        }

        // Funksjon hentet fra Riemers Tutorial.
        // Beregner normaler for bruk av shader.
        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        // Hentet fra Riemers Tutorial.
        // Sett opp indekstabell for triangle list.
        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        // Hentet fra Riemers Tutorial
        // Konverterer pikselfarge til høyde-kart.
        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // Sett opp world-matrisa
            world = scale * position;

            // Parametre til shaderen.
            effect.Parameters["xAmbient"].SetValue(0.5f);
            effect.Parameters["xView"].SetValue(camera.View);
            effect.Parameters["xProjection"].SetValue(camera.Projection);
            effect.Parameters["xWorld"].SetValue(world);
            effect.Parameters["xLightDirection"].SetValue(Vector3.Normalize(new Vector3(-0.5f, -1, 0)));
          
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionColorNormal.VertexDeclaration);

            }
        }
    }
}
