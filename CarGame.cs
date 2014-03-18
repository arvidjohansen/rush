using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Models;

namespace Game
{
    public class CarGame : Microsoft.Xna.Framework.Game
    {
        // Disse begynner å bli kjente.
        private GraphicsDeviceManager graphics;
        public GraphicsDevice device;
        private SpriteBatch spriteBatch;

        // For å tegne tekst
        private SpriteFont spritefont;
        
        // Egendefinerte objekter for spillet
        private Camera camera;
        private Car car;
        private Obstacle obstacle;
        private Track track;
        private Terrain terrain;
        private Skybox skybox;

        // Kameraoperasjoner
        private int cameraMode = 0;
        private Vector3 cameraPosition = new Vector3(10, 5f, 10);

        // Constructor
        public CarGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // Initier GraphicsDeviceManager og -Device
            InitGraphics();

            // Svært viktig funksjonskall
            Window.Title = "Bilspill - Karaktergivende Oppgave - Ole Petter W. Andersen";

            // Sett opp kamera
            camera = new Camera(cameraPosition, Vector3.Zero, new Vector3(0, 1, 0),
                (float)graphics.GraphicsDevice.Viewport.Width / 
                (float)graphics.GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        protected void InitGraphics()
        {
            // Sett opp device
            device = graphics.GraphicsDevice;

            // Oppløsning, vindusmodus
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;

            graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // La inn en spritefont i content-prosjektet for å
            // tegne tekst. Denne vises her.
            spritefont = Content.Load<SpriteFont>("Arial");
            
            // Opprett egendefinerte spillobjekter
            car = new Car(Content, "buggy");
            track = new Track(device, Content);
            obstacle = new Obstacle(device, Content);
            terrain = new Terrain(device, Content);
            skybox = new Skybox(device, Content, "miramar");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kbdState = Keyboard.GetState();

            // Keyboardpolling
            if (kbdState.IsKeyDown(Keys.Escape)) this.Exit();
            if (kbdState.IsKeyDown(Keys.Left)) car.SteerLeft();
            if (kbdState.IsKeyDown(Keys.Right)) car.SteerRight();
            if (kbdState.IsKeyDown(Keys.Up)) car.UpRight();
            if (kbdState.IsKeyDown(Keys.W)) car.Accelerate();
            if (kbdState.IsKeyDown(Keys.S)) car.Decelerate();
            if (kbdState.IsKeyDown(Keys.F)) car.GearForward();
            if (kbdState.IsKeyDown(Keys.R)) car.GearReverse();
            if (kbdState.IsKeyDown(Keys.F5)) cameraMode = 0;
            if (kbdState.IsKeyDown(Keys.F6)) cameraMode = 1;

            // Bestem kameraposisjon ut fra kameramodus
            if (cameraMode == 0)
                // Plassert bak bilen
                camera.Position = car.PositionBehind;
            else
                // Plassert over banen
                camera.Position = cameraPosition;

            // Kameraet ser litt foran bilen. Dette gir en illusjon av
            // at kameraoperatøren ikke klarer å følge bilen når den 
            // passerer på nært hold.
            camera.Target = car.PositionAhead;

            // Det å oppdatere bilens posisjon etter kameraet gjør at
            // kameraposisjon og vinkel blir avhengig av bilens fart
            // og svinghastighet, slik at kameraet ikke alltid er på
            // nøyaktig samme plass bak bilen.
            car.Update(gameTime);

            // Oppdater skyboksen iht kameraet.
            skybox.Position = camera.Position;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Tøm skjermen
            device.Clear(Color.White);

            // Tegn objektene i spillet
            skybox.Draw(gameTime, camera);
            terrain.Draw(gameTime, camera);
            track.Draw(gameTime, camera);
            obstacle.Draw(gameTime, camera);
            car.Draw(gameTime, Matrix.Identity, camera.View, camera.Projection);

            // Oppdater Tekst
            spriteBatch.Begin();
            spriteBatch.DrawString(spritefont, car.SpeedInKmH, new Vector2(10, 10), Color.Yellow, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(spritefont, car.Gear, new Vector2(10, 60), Color.Yellow, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();

            // Rydd opp etter spriteBatch for at 3D-grafikk skal tegnes som forventet neste frame
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);
        }
    }
}
