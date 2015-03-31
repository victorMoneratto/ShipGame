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
            //amount the player moves on one axis
            //in pixels/second
            float playerStep = 1000;
            if(keys.IsKeyDown(Keys.A))
            {
                player.position.X -= playerStep * dt;
            }
            if(keys.IsKeyDown(Keys.D))
            {
                player.position.X += playerStep * dt;
            }

            if(keys.IsKeyDown(Keys.W))
            {
                player.position.Y -= playerStep * dt;
            }
            if(keys.IsKeyDown(Keys.S))
            {
                player.position.Y += playerStep * dt;
            }
            
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
