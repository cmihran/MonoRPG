using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoRPG.Desktop.Game.Player {

    public class HumanPlayer : Entity {

        // Core
        Color color;
        GameRPG game;
        public Vector2 pos;
        Vector2 velocity;

        public Color Color => color;
        public GameRPG Game => game;

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Keys
        private const Keys upKey = Keys.W;
        private const Keys leftKey = Keys.A;
        private const Keys downKey = Keys.S;
        private const Keys rightKey = Keys.D;
        private const Keys jumpKey = Keys.Space;

        // Jumping state
        private bool isFalling => velocity.Y != 0.0f;
        private bool requestingJump;

        // Stats
        private float movement;
        private bool isAlive;
        private Rectangle localBounds;

        // Horizontal movement constants
        private const float MOVE_ACCEL = 20000.0f;
        private const float MAX_MOVE_SPEED = 20000.0f;
        private const float GROUND_DRAG = 0.48f;
        private const float AIR_DRAG = 0.44f;

        // Vertical movement constants
        private const float JUMP_LAUNCH_VELOCITY = -2000.0f;
        private const float GRAVITY_ACCELERATION = 3400.0f;
        private const float TERMINAL_VELOCITY = 550.0f;

        // movement
        public bool CanMoveUp => pos.Y > 0;
        public bool CanMoveDown => pos.Y + localBounds.Height < Game.graphics.GraphicsDevice.Viewport.Height;
        public bool CanMoveLeft => pos.X > 0;
        public bool CanMoveRight => pos.X + localBounds.Width < Game.graphics.GraphicsDevice.Viewport.Width;

        public HumanPlayer(Color color, GameRPG game, Vector2 pos) {
            this.color = color;
            this.game = game;
            this.pos = pos;

            Reset(pos);
            LoadContent();
        }

        public void LoadContent() {
            // Load animated textures.
            idleAnimation = new Animation(game.Content.Load<Texture2D>("Player/Idle"), 0.1f, true);
            runAnimation = new Animation(game.Content.Load<Texture2D>("Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(game.Content.Load<Texture2D>("Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(game.Content.Load<Texture2D>("Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(game.Content.Load<Texture2D>("Player/Die"), 0.1f, false);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = idleAnimation.Texture.Height;
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Draw(GameTime gameTime) {
            // flip to match characters movement
            if (velocity.X > 0) flip = SpriteEffects.FlipHorizontally;
            else if (velocity.X < 0) flip = SpriteEffects.None;

            // draw character
            sprite.Draw(gameTime, game.spriteBatch, pos, flip);
        }

        public void Update(GameTime gameTime) {
            // update animations
            if (isAlive && !isFalling) {
                if (Math.Abs(velocity.X) - 0.5f > 0) {
                    sprite.PlayAnimation(runAnimation);
                } else {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // process player in the game world
            ProcessInput(Keyboard.GetState());
            ApplyPhysics(gameTime);
        }

        public void Reset(Vector2 position) {
            pos = position;
            velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
            requestingJump = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void ProcessInput(KeyboardState keyboardState) {
            if (keyboardState.IsKeyDown(leftKey)) {
                movement = -1.0f;
            } else if (keyboardState.IsKeyDown(rightKey)) {
                movement = 1.0f;
            } else {
                movement = 0.0f;
            }

            // Check if the player wants to jump.
            requestingJump = keyboardState.IsKeyDown(jumpKey);
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MOVE_ACCEL * elapsed;
            if (CanMoveDown) {
                velocity.Y = MathHelper.Clamp(velocity.Y + GRAVITY_ACCELERATION * elapsed, -TERMINAL_VELOCITY, TERMINAL_VELOCITY);
            } else {
                velocity.Y = 0;
            }

            // start jump
            if(requestingJump && !isFalling) {
                velocity.Y = JUMP_LAUNCH_VELOCITY;
                sprite.PlayAnimation(jumpAnimation);
            }

            // Apply pseudo-drag horizontally.
            velocity.X *= (isFalling ? AIR_DRAG : GROUND_DRAG);

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MAX_MOVE_SPEED, MAX_MOVE_SPEED);

            // Apply velocity.
            pos += velocity * elapsed;
            pos = new Vector2((float)Math.Round(pos.X), (float)Math.Round(pos.Y));
        }
    }
}