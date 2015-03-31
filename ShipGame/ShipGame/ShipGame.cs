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
            public Vector2 position;
            public Vector2 velocity;
            public Vector2 acceleration;
            public float accelerationAmount = 5000;
            public float drag = 5;
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

            //zero acceleration every update
            player.acceleration = Vector2.Zero;
            //x coordinate acceleration
            if (keys.IsKeyDown(Keys.A))
            {
                player.acceleration.X -= player.accelerationAmount;
            }
            if (keys.IsKeyDown(Keys.D))
            {
                player.acceleration.X += player.accelerationAmount;
            }

            //y coordinate acceleration
            if (keys.IsKeyDown(Keys.W))
            {
                player.acceleration.Y -= player.accelerationAmount;
            }
            if (keys.IsKeyDown(Keys.S))
            {
                player.acceleration.Y += player.accelerationAmount;
            }

            //Drag
            player.acceleration -= player.drag* player.velocity;

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
            spriteBatch.Draw(player.texture, player.position, Color.White);

            //end drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
