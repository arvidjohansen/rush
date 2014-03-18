using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Models
{
    // Bilklassen. Tar seg av alle beregninger og tegning av bilmodellen på skjermen.
    // Bevegelser, sving, hjulrotasjon er basert på øyemål, ikke nøyaktige beregninger.

    // Bilen kjøres ved å styre høyre/venstre, gi gass, bremse, skifte gir, som på en virkelig bil.

    // Jeg hadde litt problemer med å få koordinatsystemene til å stemme når jeg eksporterte fra 
    // 3DS Max til en .x-fil.

    // Jeg har kompensert for dette ved å velge korrekte akser og retninger iht bilmodellen.

    public class Car
    {
        #region Members Variables and Constructor

        // Mål brukt til beregning av hjulrotasjon
        private float D_A = 0.23f;
        private float Circum = 0.00065f * MathHelper.TwoPi;

        // Referanseindekser i Bones-samlingen i Model-objektet. Brukes i transformeringene
        private const int HjulBak_H = 7;
        private const int HjulBak_V = 11;
        private const int HjulForan_H = 15;
        private const int HjulForan_V = 19;

        // Farts- og aksellerasjonsvariabler brukt til beregning av tilbakelagt avstand.
        private const float Acceleration = 0.005f;
        private const float Brake = 0.010f;
        private const float MaxSpeed = 0.7f;

        private float speed;
        private float speedInKmH;
        private float distanceTravelled;

        // "Gir": 1 = forover, -1 = revers.
        private int direction = 1;
        
        // Hastighetsvektor som bestemmes av farten. Benyttes i transformasjonene.
        private Vector3 velocity;   
        
        // Vinkelen på styringsutslaget målt i radianer.
        public float SteerAngle { get; set; }
        
        // Gjeldende vinkel på hjulene med rotasjonsbevegelse.
        private float wheelRotation = 0;

        // Modell-objektet, og lagring av Bones-transformasjoner.
        private Model model;
        private Matrix[] initialTransforms;
        private Matrix[] currentTransforms;

        // Constructor
        public Car(ContentManager content, string fileName)
        {
            // Utgangsposisjon, fart og styringsutslaget.
            Speed = 0.0f;
            SteerAngle = 0.0f;

            // Les .x-objektet fra 3DS Max
            model = content.Load<Model>(fileName);

            // Juster objektet i forhold til koordinatsystemet fra 3DS Max
            Matrix rotation = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            model.Bones[0].Transform = rotation * model.Bones[0].Transform;

            // Initier tabell for utgangtransformasjoner
            initialTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(initialTransforms);

            // Initier tabell for nåværende transformasjoner
            currentTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(currentTransforms);
            //ModelLister.DumpModel(model);
            velocity = new Vector3(0, 0, 0);
        }
#endregion

        #region Reference properties
        // Property som leser eller setter farten.
        public float Speed 
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        // Read-only property som viser farten i Km/t, formatert i 000.0
        public string SpeedInKmH
        {
            get
            {
                return string.Format("{0:0.00} Km/t", speedInKmH);
            }
        }

        // Returner gjeldende gir for visning på skjermen.
        public string Gear
        {
            get
            {
                if (direction == 1)
                    return "<F>";
                else
                    return "<R>";
            }
        }

        // Property som leser eller setter posisjonen til kjøretøyet
        // Oppdaterer modellen direkte
        public Vector3 Position
        {
            get
            {
                Matrix t = model.Bones[0].Transform;
                return model.Bones[0].Transform.Translation;
            }
            set
            {
                Matrix transform = Matrix.CreateTranslation(value);
                model.Bones[0].Transform = transform;
            }
        }

        // Kameraposisjon bak bilen
        public Vector3 PositionBehind
        {
            get
            {
                return model.Bones[0].Transform.Translation + model.Bones[0].Transform.Left * 4 + model.Bones[0].Transform.Down * 2f;
            }
        }

        // Kamera-mål plassert foran bilen.
        public Vector3 PositionAhead
        {
            get
            {
                return model.Bones[0].Transform.Translation + model.Bones[0].Transform.Right * 6 + model.Bones[0].Transform.Down * 0;
            }
        }
#endregion

        #region Car Handling

        // Oppdater styringsvinkelen mot venstre, maksutslag 0.5 radianer.
        public void SteerLeft()
        {
            SteerAngle = MathHelper.Max(SteerAngle - 0.01f, -0.5f);
        }

        // Oppdater styringsvinkelen mot høyre, maksutslag 0.5 radianer.
        public void SteerRight()
        {
            SteerAngle = MathHelper.Min(SteerAngle + 0.01f, 0.5f);
        }

        // Flytt styringsvinkelen mot senter, sett til 0.0f dersom utslaget er under
        // gitt terskel.
        public void UpRight()
        {
            SteerAngle *= 0.9f;
            if (Math.Abs(SteerAngle) < 0.025)
                SteerAngle = 0;

        }

        public void Accelerate()
        {
            // Gassen trykkes.
            Speed += Acceleration * direction;
            if (Math.Abs(Speed) > MaxSpeed)
                Speed = MaxSpeed * direction;
            
        }

        public void Decelerate()
        {
            // Bremsen trykkes.
            Speed -= Brake * direction;
            if (Math.Abs(Speed) < 0.011f)
                Speed = 0;
        }

        // Sett giret i "1. gir", det eneste giret fremover.
        // Dette er kun tillat når bilen er i tilnærmet stillstand.
        public void GearForward()
        {
            if (Math.Abs(Speed) < 0.016f)
                direction = 1;
        }

        // Sett giret i revers. Bilen må stå stille (tilnærmet).
        public void GearReverse()
        {
            if (Math.Abs(Speed) < 0.016f)
                direction = -1;
        }

        #endregion

        #region Collision Detection

        // Kollisjonsdetekteringen er forholdsvis enkel, den sjekker om senter av hvert hjul
        // er innenfor reklameplakatene.

        // Det er derfor ikke benyttet objekt-objekt kollisjonstest.

        // Kun forhjulene testes når bilen går fremover, og kun bakhjulene testes når bilen
        // rygger.
        public bool Collision()
        {
            // Chassis er rotobjektet, og må derfor tas med i transformasjonsberegningen
            // til alle hjulene.
            Matrix chassis = model.Bones[0].Transform;

            // Bilen går forover, sjekk om minst ett forhjul er utenfor grensene.
            if (direction == 1)
            {
                // Lagre referanser til Bones-objektene for letter å identifisere 
                // dem i beregningene.
                Matrix suspensionFront_R = model.Bones[12].Transform;
                Matrix wheelFront_R = model.Bones[14].Transform;
                Matrix suspensionFront_L = model.Bones[16].Transform;
                Matrix wheelFront_L = model.Bones[18].Transform;

                // Finn world-matrisen til hvert forhjul ved å multiplisere med foreldre-
                // bone-objektet helt til rotnivå.
                Matrix wheelFrontRightCollision = wheelFront_R * suspensionFront_R * chassis;
                Matrix wheelFrontLeftCollision = wheelFront_L * suspensionFront_L * chassis;

                // Senter av hvert hjul i World-koordinater kan nå sjekkes mot grensene.
                return CheckBoundaries(wheelFrontRightCollision) && 
                    CheckBoundaries(wheelFrontLeftCollision);
            }
            else
            // Bilen går bakover, samme fremgangsmåte som for forhjulene benyttes.
            {
                // Referanser
                Matrix differential = model.Bones[2].Transform;
                Matrix suspensionRear_R = model.Bones[4].Transform;
                Matrix wheelRear_R = model.Bones[6].Transform;
                Matrix suspensionRear_L = model.Bones[8].Transform;
                Matrix wheelRear_L = model.Bones[10].Transform;
                // Bakakslene er koblet til differensial, så vi får et ekstra objekt som må tas
                // med på veien mot rotobjektet.
                Matrix wheelRearRightCollision = wheelRear_R * suspensionRear_R * differential * chassis;
                Matrix wheelRearLeftCollision = wheelRear_L * suspensionRear_L * differential * chassis;

                // Bakhjulenes World-transformasjoner testes.
                return CheckBoundaries(wheelRearRightCollision) && 
                    CheckBoundaries(wheelRearLeftCollision);
            }
        }

        private bool CheckBoundaries(Matrix wheel)
        {
            // Sjekker om den gitte matrisen ligger utenfor grensene, som er hardkodet for
            // reklameplakatene i dette spillet.
            bool r = wheel.Translation.X < -50 || wheel.Translation.X > 26
                || wheel.Translation.Z < -48 || wheel.Translation.Z > 28;
            return r;
        }

        #endregion

        #region Transformation, Update and Draw

        private void Transformations()
        {
            // Avstand til forakselen og styrevinkel bestemmer hvilket punkt bilen roterer rundt.
            // Dette punktet kunne optimalt brukes til å bestemme optimal styre-vinkel på begge 
            // forhjulene, samt individuell rotasjonshastighet.
            float r = D_A / (float)Math.Tan(SteerAngle);
            float rotAngle = distanceTravelled / r * MathHelper.TwoPi;
            rotAngle = rotAngle % MathHelper.TwoPi;

            // Bestem felles rotasjonshastighet. En optimal modell ville ha individuell hastighet
            // på hvert hjul i en sving.
            wheelRotation += distanceTravelled / Circum;

            // Flytt rot-objektet (chassi) direkte ved å benytte tilbakelagt avstand og beregnet kursendring på bilen.
            // Dette gir et visuelt akseptabelt resultat, selv om matematikken ikke 
            model.Bones[0].Transform = Matrix.CreateRotationY(rotAngle) * Matrix.CreateTranslation(velocity) * 
                model.Bones[0].Transform;

            // Forhjulene roteres både om Y-aksen (X-på modellen) for å vise styringsutslaget, og om Z-aksen
            // i forhold til farten på bilen.
            // Den totale transformasjonen settes opp iht ISROT, og legges til det tilhørende Bone-objektet.

            // Venstre foran
            Matrix wheelRot_fl = Matrix.CreateRotationZ(wheelRotation) * Matrix.CreateRotationX(SteerAngle) 
                * initialTransforms[HjulForan_V];
            model.Bones[HjulForan_V].Transform = wheelRot_fl;
            
            // Høyre foran
            Matrix wheelRot_fr = Matrix.CreateRotationZ(-wheelRotation) * Matrix.CreateRotationX(SteerAngle) 
                * initialTransforms[HjulForan_H];
            model.Bones[HjulForan_H].Transform = wheelRot_fr;
            
            // Bakhjulene har kun en rotasjon

            //Venstre bak
            Matrix wheelRot_rl = Matrix.CreateRotationZ(wheelRotation) * initialTransforms[HjulBak_V];
            model.Bones[HjulBak_V].Transform = wheelRot_rl;
            
            // Høyre bak
            Matrix wheelRot_rr = Matrix.CreateRotationZ(-wheelRotation) * initialTransforms[HjulBak_H];
            model.Bones[HjulBak_H].Transform = wheelRot_rr;
            model.CopyAbsoluteBoneTransformsTo(currentTransforms);
        }

        public void Update(GameTime gameTime)
        {
            // En real bråstopp når bilen kolliderer med reklameplakatene.
            if (Collision())
                Speed = 0;

            // Beregnet tilbakelagt avstand i forhold løpt tid siden forrige frame.
            distanceTravelled = Speed * gameTime.ElapsedGameTime.Milliseconds / 1000;

            // oppdater fartsvektor
            velocity.X = Speed;

            speedInKmH = Math.Abs(Speed * 216f);
        }

        public void Draw(GameTime gameTime, Matrix world, Matrix view, Matrix projection)
        {
            // Oppdater bil-transformasjonene
            Transformations();

            // Løper igjennom mesh-treet i Model-objektet, og beregner riktig world-matrise
            // for shaderen. Basiceffekt benyttes.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = currentTransforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                 
                mesh.Draw();
            }
        }
        #endregion
    }
}
