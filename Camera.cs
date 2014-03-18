using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    // Innkapsler kamerafunksjonaliteten i et objekt
    public class Camera
    {
        // Kameraparametre
        private float aspectRatio;
        private float nearPlaneDistance = 0.1f;
        private float farPlaneDistance = 2000;

        // Parametre for beregning av view og projection
        private Vector3 position;
        private Vector3 target;
        private Vector3 up;

        // Lagrer view og projection
        private Matrix view;
        private Matrix projection;

        // Constructor initierer kameraet i en gitt posisjon, med de nødvendigste
        // parametre for å beregne view og projection
        public Camera(Vector3 position, Vector3 target, Vector3 up, float aspectRatio)
        {
            // kopier parametre til variabler
            this.position = position;
            this.target = target;
            this.up = up;
            this.aspectRatio = aspectRatio;

            // Beregn view og projection
            UpdateView();
            UpdateProjection();
        }

        // Beregner View
        private void UpdateView()
        {
            Matrix.CreateLookAt(ref position, ref target, ref up, out view);
        }

        // Beregner Projection
        private void UpdateProjection()
        {
            Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio,
                nearPlaneDistance, farPlaneDistance, out projection);
        }

        // View og Projection som read-only properties
        public Matrix View { get { return view; } }
        public Matrix Projection { get { return projection; } }

        // Property for posisjon
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                // View må beregnes på nytt når kameraet flytter på seg.
                UpdateView();
            }
        }

        // Property for se-på mål.
        public Vector3 Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
                // View må beregnes på nytt når synsvinkelen endrer ArraySegment.
                UpdateView();
            }
        }
    }
}
