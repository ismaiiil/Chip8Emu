using System;
using System.Diagnostics;
using Chip8Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RenderingProject;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Color[] _pixels;
    private Texture2D _texture;

    

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 64 * 20;
        _graphics.PreferredBackBufferHeight = 32 * 20;
        _graphics.ApplyChanges();
        _pixels = new Color[CpuChip8.GFX_WIDTH * CpuChip8.GFX_HEIGHT];
        _texture = new Texture2D(_graphics.GraphicsDevice, CpuChip8.GFX_WIDTH, CpuChip8.GFX_HEIGHT, false,
            SurfaceFormat.Color);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);


        // TODO: use this.Content to load your game content here

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Program.QuitFlag = true;
            Exit();
        }
        
        // TODO: Add your update logic here
        for (int col = 0; col != CpuChip8.GFX_WIDTH ; col++)
        {
            for (int row = 0; row != CpuChip8.GFX_HEIGHT; row++)
            {
                int index = row * CpuChip8.GFX_WIDTH + col;
                if (CpuChip8.GFX[index] > 0)
                {
                    _pixels[index] = Color.White;
                }
                else
                {
                    _pixels[index] = Color.Black;
                }
            }
        }

        _texture.SetData(_pixels);
        

        
        
        //play square wave with Monogame Soundeffect
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        var transformMatrix = Matrix.CreateScale(new Vector3(20, 20, 1));

        // TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState: new SamplerState()
        {
            Filter = TextureFilter.Point
        }, transformMatrix: transformMatrix);
        _spriteBatch.Draw(_texture, new Rectangle(0, 0, _texture.Width, _texture.Height), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
