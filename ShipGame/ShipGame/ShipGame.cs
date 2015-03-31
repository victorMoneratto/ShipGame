using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ShipGame
{
    public class ShipGame : Microsoft.Xna.Framework.Game
    {
        //screen resolution
        public Vector2 resolution = new Vector2(1280, 720);
        //texture drawer
        SpriteBatch spriteBatch;

        //############################
        //  PLAYER
        //############################

        //define our player struct
        class Player
        {
            public Texture2D texture;
            public Vector2 center;
            public Vector2 position;
            public Vector2 velocity;
            public Vector2 acceleration;
            public float accelerationAmount = 5000;
            public float drag = 5;
            public float rotation;
            public float rotationAmount = MathHelper.TwoPi;
        }

        //create and initialize our player variable
        Player player = new Player();

        //######################
        // Stars
        //######################
        class Stars
        {
            public Texture2D texture;
            public Vector2 position;
        }

        Stars stars = new Stars();

        //######################
        // Game code
        //######################

        public ShipGame()
        {
            Window.Title = "Ship Game";
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;

            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            //initialize texture drawer
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //load player texture
            player.texture = Content.Load<Texture2D>("player");
            player.center.X = .5f * player.texture.Width;
            player.center.Y = .5f * player.texture.Height;

            //load stars texture
            stars.texture = Content.Load<Texture2D>("stars");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            
            //time since last update
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            
            //##################
            // PLAYER
            //##################

            
            //angle
            if (keys.IsKeyDown(Keys.A))
            {
                player.rotation -= player.rotationAmount * dt;
            }
            if (keys.IsKeyDown(Keys.D))
            {
                player.rotation += player.rotationAmount * dt;
            }

            //zero acceleration every update
            player.acceleration = Vector2.Zero;
            //acceleration
            if (keys.IsKeyDown(Keys.W))
            {
                player.acceleration += Vector2.One;
            }
            if (keys.IsKeyDown(Keys.S))
            {
                player.acceleration -= Vector2.One;
            }

            //a.x = amount * cos(rotation)
            player.acceleration.X *= player.accelerationAmount * (float)Math.Cos(player.rotation);
            //a.y = amount * sin(rotation)
            player.acceleration.Y *= player.accelerationAmount * (float)Math.Sin(player.rotation);

            //Drag
            player.acceleration -= player.drag * player.velocity;

            //dS = v*dt + (a*dt^2)/2
            player.position += player.velocity * dt + .5f * player.acceleration * dt * dt;
            //dV = a * dt
            player.velocity += dt * player.acceleration;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //clear screen
            GraphicsDevice.Clear(Color.Black);

            //begin drawing
            spriteBatch.Begin();

            //draw stars
            spriteBatch.Draw(stars.texture, stars.position, Color.White);

            //draw player
            spriteBatch.Draw(player.texture, player.position, null, Color.White,
                             player.rotation, player.center, 1f, SpriteEffects.None, 0);

            //end drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
