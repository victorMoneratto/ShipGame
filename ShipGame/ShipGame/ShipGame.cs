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
            public float pushBack = 2000;
        }

        //create and initialize our player variable
        Player player = new Player();

        //######################
        // Stars
        //######################
        class Stars
        {
            public Texture2D texture;
            public static Vector2 position = new Vector2(-100000f);
            public static Rectangle sourceRect =
                new Rectangle(
                    x: (int)position.X, y: (int)position.Y,
                    width: 2 * (int)-position.X, height: 2 * (int)-position.Y);
        }

        Stars stars = new Stars();
        //######################
        // Camera
        //######################
        Vector3 cameraPosition;

        //######################
        // Lasers
        //######################
        SoundEffect laserSound;
        Texture2D laserTexture;
        Vector2 laserCenter;
        float laserScale = 3f;
        float laserSpeed = 2000;
        float lastShotTime;
        float timeBetweenShots = .2f;

        class Laser
        {
            public Vector2 position;
            public Vector2 velocity;
            public float rotation;
            public float shotTime;
        }
        List<Laser> lasers = new List<Laser>();

        //#####################
        // Enemy
        //#####################
        Texture2D enemyTexture;
        Vector2 enemyCenter;
        float enemySpeed = 900;
        float spawnDistance = 1400;

        class Enemy
        {
            public Vector2 position;
            public float rotation;
        }

        List<Enemy> enemies = new List<Enemy>();

        //######################
        // Game code
        //######################
        Random random = new Random();
        Song song;

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

            //laser
            laserTexture = Content.Load<Texture2D>("laser");
            laserCenter.X = laserTexture.Width * .5f;
            laserCenter.Y = laserTexture.Height * .5f;

            laserSound = Content.Load<SoundEffect>("shot");

            //enemy
            enemyTexture = Content.Load<Texture2D>("enemy1");
            enemyCenter.X = enemyTexture.Width * .5f;
            enemyCenter.Y = enemyTexture.Height * .5f;

            for (int i = 0; i < 3; ++i)
            {
                Enemy enemy = new Enemy();
                SpawnEnemy(enemy, player.position, spawnDistance, 0, MathHelper.TwoPi);
                enemies.Add(enemy);
            }

            song = Content.Load<Song>("song");
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();


            //time since last update
            float now = (float)gameTime.TotalGameTime.TotalSeconds;
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


            //#################
            // Laser
            //#################
            if (keys.IsKeyDown(Keys.Space) && now - lastShotTime >= timeBetweenShots)
            {
                Laser laser = new Laser();
                laser.position = player.position;
                laser.rotation = player.rotation;
                laser.velocity.X = (float)Math.Cos(laser.rotation) * laserSpeed;
                laser.velocity.Y = (float)Math.Sin(laser.rotation) * laserSpeed;

                laser.shotTime = now;
                lastShotTime = laser.shotTime;
                lasers.Add(laser);

                player.velocity.X -= (float)Math.Cos(laser.rotation) * player.pushBack;
                player.velocity.Y -= (float)Math.Sin(laser.rotation) * player.pushBack;
                laserSound.Play(.25f, (float)(.3 - .6 * new Random().NextDouble()), 0f);

            }

            for (int i = 0; i < lasers.Count; ++i)
            {
                Laser laser = lasers[i];
                if (now - laser.shotTime > 1f)
                {
                    lasers.Remove(laser);
                }
                else
                {
                    laser.position += laser.velocity * dt;
                }
            }

            //############
            // Enemy
            //############
            for (int i = 0; i < enemies.Count; ++i)
            {
                Enemy enemy = enemies[i];

                Vector2 deltaPos = player.position - enemy.position;
                deltaPos.Normalize();

                enemy.position += deltaPos * enemySpeed * dt;

                enemy.rotation = (float)Math.Atan2(deltaPos.Y, deltaPos.X);

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

            cameraPosition.X = MathHelper.Lerp(cameraPosition.X, player.position.X, 2f * dt);
            cameraPosition.Y = MathHelper.Lerp(cameraPosition.Y, player.position.Y, 4f * dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //clear screen
            GraphicsDevice.Clear(Color.Black);

            Matrix parallaxMatrix = Matrix.CreateTranslation(-.3f * cameraPosition) *
                                  Matrix.CreateTranslation(new Vector3(resolution * .5f, 0));
            //draw parallaxed stars
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicWrap, null, null, null, parallaxMatrix);
            spriteBatch.Draw(stars.texture, Stars.position, Stars.sourceRect, Color.White,
                                0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.End();


            Matrix cameraMatrix = Matrix.CreateTranslation(-cameraPosition) *
                                  Matrix.CreateTranslation(new Vector3(resolution * .5f, 0));
            //draw stars and player
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicWrap, null, null, null, cameraMatrix);
            spriteBatch.Draw(stars.texture, Stars.position, Stars.sourceRect, Color.White);

            //print lasers
            for (int i = 0; i < lasers.Count; ++i)
            {
                Laser laser = lasers[i];
                spriteBatch.Draw(laserTexture, laser.position, null, Color.White, laser.rotation, laserCenter, laserScale, SpriteEffects.None, 0);
            }

            for (int i = 0; i < enemies.Count; ++i)
            {
                Enemy enemy = enemies[i];
                spriteBatch.Draw(enemyTexture, enemy.position, null, Color.White,
                                  enemy.rotation, enemyCenter, 1f, SpriteEffects.None, 0);
            }

            spriteBatch.Draw(player.texture, player.position, null, Color.White,
                              player.rotation, player.center, 1f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        void SpawnEnemy(Enemy enemy, Vector2 origin, float distance, float angleMin, float angleMax)
        {
            float between = (float)random.NextDouble();
            float angle = MathHelper.Lerp(angleMin, angleMax, between);
            enemy.position.X = (float)Math.Cos(angle);
            enemy.position.Y = (float)Math.Sin(angle);
            enemy.position *= distance;
            enemy.position += origin;
        }
    }
}
