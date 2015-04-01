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
        class Player
        {
            //ship texture to draw
            public Texture2D texture;

            //center of texture, mainly for rotation
            public Vector2 center;

            //current position
            public Vector2 position;

            //current velocity
            public Vector2 velocity;

            //current acceleration (reset every frame)
            public Vector2 acceleration;

            //how much we accelerate 
            public float accelerationAmount = 5000;
            
            //drag (air friction) coefficient
            public float drag = 5;

            //current ship rotation
            public float rotation;
            
            //how much we rotate in one second
            public float rotationAmount = MathHelper.TwoPi;
            
            //magnitude of force applied when shooting
            public float pushBack = 2000;

            //radius of collision sphere
            public float collisionRadius = 30;
            
            //score
            public int kills = 0;
            
            //hits we can take
            public const int initialHp = 5;

            //hits left
            public int hp = initialHp;
        }

        //player instance
        Player player = new Player();

        //######################
        // Stars
        //######################
        class Stars
        {
            //star field texture
            public Texture2D texture;

            //position and rectangle used for a nasty rendering hack (repeating texture with wrap)
            public static Vector2 position = new Vector2(-100000f);
            public static Rectangle sourceRect =
                new Rectangle(
                    x: (int)position.X, y: (int)position.Y,
                    width: 2 * (int)-position.X, height: 2 * (int)-position.Y);
        }

        //stars instance
        Stars stars = new Stars();

        //######################
        // Camera
        //######################
        //NOTE: camera variables are not in struct, just to show languange features for newbies
        
        //position of the camrea
        Vector3 cameraPosition;

        //how much zoomed the camera is
        float cameraZoom = 1f;

        //speed of camera zoom reset
        float cameraComeBack = 2f;

        //######################
        // Lasers
        //######################
        //NOTE: there's laser stuff inside and out of the struct, just to show how this is not that good idea
        //this stuff should be inside the struct and static or const

        //sound when we shoot
        SoundEffect laserSound;

        //texture of a laser beam
        Texture2D laserTexture;

        //center of the laser texture
        Vector2 laserCenter;

        //scale of the laser texture
        float laserScale = 3f;

        //speed of a laser beam
        float laserSpeed = 2000;

        //last time ship shot NOTE:(should be on the player struct)
        float lastShotTime;

        //wait time between 2 shots (should be on the player struct as well)
        float timeBetweenShots = .2f;

        //radius of the laser beam circle collision
        float laserCollisionRadius = 15;

        class Laser
        {
            //position of a laser beam
            public Vector2 position;

            //velocity of a laser beam
            public Vector2 velocity;

            //rotation of a laser beam
            public float rotation;

            //time the beam was shot
            public float shotTime;
        }

        //collection of lasers
        List<Laser> lasers = new List<Laser>();

        //#####################
        // Enemy
        //#####################
        //NOTE: Due to similarity to laser code, this one was done the same way for familiarity

        //texture of the enemies (could different for each enemy if placed inside struct)
        Texture2D enemyTexture;

        //center of the enemy texture
        Vector2 enemyCenter;

        //speed of an enemy
        float enemySpeed = 1000;

        //spawn distance from player 
        float spawnDistance = 1400;

        //collision radius of an enemy
        float enemyCollisionRadius = 35;

        class Enemy
        {
            //position of an enemy
            public Vector2 position;

            //rotation of an enemy ship
            public float rotation;

            //velocity multiplier (a hack so ships take somewhat different paths to the player)
            public Vector2 directionMult;
        }

        //collection of enemies
        List<Enemy> enemies = new List<Enemy>();
        
        
        //######################
        // Other stuff
        //######################
        
        //random number generator
        Random random = new Random();

        //background song
        Song song;

        //song when 2 things hit
        SoundEffect crashSound;

        //font for UI
        SpriteFont font;

        //highscore, you know
        int highscore = 0;

        //#######################
        // Pause
        //######################
        //are we running the game (not paused)
        bool isRunning = true;
        //last time we toggled pause
        float lastPauseTime;

        //######################
        // Game code
        //######################
        public ShipGame()
        {
            //initialize game window
            Window.Title = "Ship Game";
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;

            Content.RootDirectory = "Content";
        }

        //NOTE: code generally put on Initialize is placed here, for simplicity
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

            //spawn enemies
            for (int i = 0; i < 3; ++i)
            {
                Enemy enemy = new Enemy();
                SpawnEnemy(enemy, player.position, spawnDistance, 0, MathHelper.TwoPi);
                enemies.Add(enemy);
            }

            //song
            song = Content.Load<Song>("song");
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);

            //crash
            crashSound = Content.Load<SoundEffect>("crash");
            
            //font
            font = Content.Load<SpriteFont>("font");
        }

        protected override void Update(GameTime gameTime)
        {
            //get state of keys
            KeyboardState keys = Keyboard.GetState();


            //time since last update
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            //current time
            float now = (float)gameTime.TotalGameTime.TotalSeconds;

            //##############
            // PAUSE and Exit
            //##############
            
            // Esc to leave
            if (keys.IsKeyDown(Keys.Escape)) Exit();

            //Enter to Pause
            if(keys.IsKeyDown(Keys.Enter))
            {
                //if some times passed since we last toggled pause 
                if (now - lastPauseTime > .1f)
                {
                    //save variables for pausing
                    lastPauseTime = now;
                    isRunning = !isRunning;

                    //pause or resume music
                    if (isRunning)
                    {
                        MediaPlayer.Resume();
                    }
                    else
                    {
                        MediaPlayer.Pause();
                    }
                }
            }

            //do nothing more if we're paused
            if (!isRunning) return;

            //##################
            // PLAYER
            //##################
            
            //angle
            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
            {
                player.rotation -= player.rotationAmount * dt;
            }
            if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
            {
                player.rotation += player.rotationAmount * dt;
            }

            //zero acceleration every update
            player.acceleration = Vector2.Zero;
            //acceleration
            if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
            {
                player.acceleration += Vector2.One;
            }
            if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
            {
                player.acceleration -= Vector2.One;
            }


            //#################
            // Laser
            //#################

            //create laser beams
            if (keys.IsKeyDown(Keys.Space))
            {
                //if enough time has passed since last shot
                if (now - lastShotTime >= timeBetweenShots)
                {
                    //create a new shot (later on optimization we would totally reuse laser shots,
                    //                   but this code if for total beginners, right?)
                    Laser laser = new Laser();

                    //laser beam starts at player position and rotation
                    laser.position = player.position;
                    laser.rotation = player.rotation;

                    //laser velocity is based on laser rotation
                    laser.velocity.X = (float)Math.Cos(laser.rotation) * laserSpeed;
                    laser.velocity.Y = (float)Math.Sin(laser.rotation) * laserSpeed;

                    //save those time variables
                    laser.shotTime = now;
                    lastShotTime = laser.shotTime;

                    //add to list
                    lasers.Add(laser);

                    //add instant camera zoom
                    cameraZoom += .05f;

                    //add push back force based on laser rotation
                    //NOTE: we already calculated most of this, we should not do it again
                    player.velocity.X -= (float)Math.Cos(laser.rotation) * player.pushBack;
                    player.velocity.Y -= (float)Math.Sin(laser.rotation) * player.pushBack;

                    //play laser sound, whith .25 volume, pitch between -.3 and +.3 at the "center"
                    laserSound.Play(.25f, (float)(.3 - .6 * new Random().NextDouble()), 0f);
                }
            }

            //move/destroy laser beams
            for (int i = 0; i < lasers.Count; ++i)
            {
                Laser laser = lasers[i];

                //if beam lived long enough
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
                
                //if enemy collides with player
                if (intersectCircles(player.position, player.collisionRadius,
                                    enemy.position, enemyCollisionRadius))
                {
                    //player looses health
                    player.hp--;

                    //if player has died
                    //NOTE: this is a really dumb place to put this code, but it's direct from it's cause
                    if(player.hp == 0)
                    {
                        //update highscore
                        if (player.kills > highscore)
                        {
                            highscore = player.kills;
                        }

                        //reset player health, score, and physics
                        player.hp = Player.initialHp;
                        player.kills = 0;
                        player.position = Vector2.Zero;
                        player.velocity = Vector2.Zero;
                        cameraPosition = Vector3.Zero;
                        cameraZoom = 1f;

                        //start music again
                        MediaPlayer.MoveNext();

                        //respawn enemies
                        for(int j = 0; j < enemies.Count; ++j)
                        {
                            SpawnEnemy(enemies[j], Vector2.Zero, 2000, 0, MathHelper.TwoPi);
                        }
                    }

                    //play hit sound
                    crashSound.Play(.5f, 0.0f, 0f);

                    //add some camera zoom
                    cameraZoom += .1f;

                    //add some weird physics to the crash
                    player.velocity = -player.velocity;

                    //respawn this enemy
                    //NOTE: if player has just died, this call will respawn some already respawned enemy
                    SpawnEnemy(enemy, player.position, spawnDistance, player.rotation - MathHelper.PiOver2, player.rotation + MathHelper.PiOver2);
                }

                for (int j = 0; j < lasers.Count; ++j)
                {
                    Laser laser = lasers[j];

                    //if laser collided with enemy
                    if (intersectCircles(laser.position, laserCollisionRadius,
                                        enemy.position, enemyCollisionRadius))
                    {
                        //player scored!
                        player.kills++;

                        //play hit sound
                        crashSound.Play(.5f, 1.0f, 0f);
                        
                        //respawn enemy
                        SpawnEnemy(enemy, player.position, spawnDistance, player.rotation - MathHelper.PiOver2, player.rotation + MathHelper.PiOver2);
                    }
                }

                //normalized direction to player
                Vector2 deltaPos = player.position - enemy.position;
                deltaPos.Normalize();

                //fly to player's direction
                enemy.position += deltaPos * enemy.directionMult * enemySpeed * dt;

                //rotate to player
                enemy.rotation = (float)Math.Atan2(deltaPos.Y, deltaPos.X);
            }

            //#########################
            // MOVE PLAYER (down here so this up can affect)
            //########################

            //a.x = amount * cos(rotation) 
            player.acceleration.X *= player.accelerationAmount * (float)Math.Cos(player.rotation);
            //a.y = amount * sin(rotation)
            player.acceleration.Y *= player.accelerationAmount * (float)Math.Sin(player.rotation);

            //NOTE: I know, I know, it's in space... but hey, this is a tutorial hehe
            //Drag (air friction), a restoring force
            player.acceleration -= player.drag * player.velocity;

            //dS = v*dt + (a*dt^2)/2
            player.position += player.velocity * dt + .5f * player.acceleration * dt * dt;
            //dV = a * dt
            player.velocity += dt * player.acceleration;

            //#####################
            // Camera
            //#####################

            //move camera position to player position smoothly (lag)
            cameraPosition.X = MathHelper.Lerp(cameraPosition.X, player.position.X, 2f * dt);
            cameraPosition.Y = MathHelper.Lerp(cameraPosition.Y, player.position.Y, 4f * dt);

            //smoothly restore camera zoom back to normal
            cameraZoom = MathHelper.Lerp(cameraZoom, 1f, cameraComeBack * dt);
        }

        protected override void Draw(GameTime gameTime)
        {
            //clear screen
            GraphicsDevice.Clear(Color.Black);

            //camera matrix for parallaxed (far way) objects
            Matrix parallaxMatrix = Matrix.CreateTranslation(-.3f * cameraPosition) *
                                    Matrix.CreateScale(cameraZoom) *
                                    Matrix.CreateTranslation(new Vector3(resolution * .5f, 0));     // <- center objects on screen

            //start drawing with parallax matrix
            //NOTE: texture AdressMode Wrap to repeat texture
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicWrap, null, null, null, parallaxMatrix);
            
            //draw parallaxed stars
            spriteBatch.Draw(stars.texture, Stars.position, Stars.sourceRect, Color.White,
                                0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);

            //end of drawing stars
            spriteBatch.End();

            //camera matrix for close objects
            Matrix cameraMatrix = Matrix.CreateTranslation(-cameraPosition) *
                                  Matrix.CreateScale(cameraZoom) *
                                  Matrix.CreateTranslation(new Vector3(resolution * .5f, 0));   // <- center objects on screen

            //draw stars and player
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicWrap, null, null, null, cameraMatrix);
            
            //draw "closer" stars
            spriteBatch.Draw(stars.texture, Stars.position, Stars.sourceRect, Color.White);

            //draw laser beams
            for (int i = 0; i < lasers.Count; ++i)
            {
                Laser laser = lasers[i];
                spriteBatch.Draw(laserTexture, laser.position, null, Color.White, laser.rotation, laserCenter, laserScale, SpriteEffects.None, 0);
            }


            //draw enemies
            for (int i = 0; i < enemies.Count; ++i)
            {
                Enemy enemy = enemies[i];
                spriteBatch.Draw(enemyTexture, enemy.position, null, Color.White,
                                  enemy.rotation, enemyCenter, 1f, SpriteEffects.None, 0);
            }

            //draw player
            spriteBatch.Draw(player.texture, player.position, null, Color.White,
                              player.rotation, player.center, 1f, SpriteEffects.None, 0);
            
            //end of drawing close stuff
            spriteBatch.End();

            //begin drawing UI
            spriteBatch.Begin();

            //Draw highscore at the top
            spriteBatch.DrawString(font, "HIGHSCORE: " + highscore, Vector2.Zero, Color.White);

            //Draw score two lines below
            spriteBatch.DrawString(font, "SCORE: " + player.kills, 2 * Vector2.UnitY * font.LineSpacing, Color.White);

            //Draw hp one more line below
            spriteBatch.DrawString(font, "HP: " + player.hp, 3 * Vector2.UnitY * font.LineSpacing, Color.White);

            //end of drawing UI
            spriteBatch.End();

        }

        //function to spawn an enemy
        void SpawnEnemy(Enemy enemy, Vector2 origin, float distance, float angleMin, float angleMax)
        {
            //some value between 0 and 1
            float between = (float)random.NextDouble();

            //an angle between min and max
            float angle = MathHelper.Lerp(angleMin, angleMax, between);

            //first calculate direction
            enemy.position.X = (float)Math.Cos(angle);
            enemy.position.Y = (float)Math.Sin(angle);

            //direction * distance
            enemy.position *= distance;

            //add to where we want distance from
            enemy.position += origin;

            //multipliers from .5 to 1 so enemies weight their path
            enemy.directionMult = new Vector2(.5f + .5f * (float)random.NextDouble(), .5f + .5f * (float)random.NextDouble());
        }

        //code to see if two circles intersect
        bool intersectCircles(Vector2 center1, float radius1, Vector2 center2, float radius2)
        {
            //sum of radius
            //NOTE: radii is a plural of radius, if you don't know
            float radii = radius1 + radius2;
            
            //distance from both centers
            float distance = Vector2.Distance(center1, center2);

            //if we are closer than the sum of radius
            return distance <= radii;
        }
    }
}
