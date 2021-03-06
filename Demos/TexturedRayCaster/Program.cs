using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.IO;
using System.Collections.Generic;
using RayCasting;
using System.Threading.Tasks;

namespace TexturedRayCastingDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Textured Ray caster!");

            // Textured Raycaster
            // Rendering things
            int screenWidth = 640;
            int screenHeight = 360;

            int VertexArrayObject = 0;
            int VertexBufferObject = 0;
            int ElementBufferObject = 0;

            int VertexLocation = 0;
            int TexCoordLocation = 0;

            int TexHandle = 0;

            // Game things
            double posX = 22, posY = 12;

            bool W_down = false, A_down = false, S_down = false, D_down = false;

            int[,] TexturedWorldMap =
            {
                  {8,8,8,8,8,8,8,8,8,8,8,4,4,6,4,4,6,4,6,4,4,4,6,4},
                  {8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,0,0,0,0,0,0,4},
                  {8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,6},
                  {8,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6},
                  {8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,4},
                  {8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,6,6,6,0,6,4,6},
                  {8,8,8,8,0,8,8,8,8,8,8,4,4,4,4,4,4,6,0,0,0,0,0,6},
                  {7,7,7,7,0,7,7,7,7,0,8,0,8,0,8,0,8,4,0,4,0,6,0,6},
                  {7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,0,0,0,0,0,6},
                  {7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,0,0,0,0,4},
                  {7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,6,0,6,0,6},
                  {7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,4,6,0,6,6,6},
                  {7,7,7,7,0,7,7,7,7,8,8,4,0,6,8,4,8,3,3,3,0,3,3,3},
                  {2,2,2,2,0,2,2,2,2,4,6,4,0,0,6,0,6,3,0,0,0,0,0,3},
                  {2,2,0,0,0,0,0,2,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3},
                  {2,0,0,0,0,0,0,0,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3},
                  {1,0,0,0,0,0,0,0,1,4,4,4,4,4,6,0,6,3,3,0,0,0,3,3},
                  {2,0,0,0,0,0,0,0,2,2,2,1,2,2,2,6,6,0,0,5,0,5,0,5},
                  {2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5},
                  {2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5},
                  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5},
                  {2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5},
                  {2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5},
                  {2,2,2,2,1,2,2,2,2,2,2,1,2,2,2,5,5,5,5,5,5,5,5,5}
            };

            // Saves map in case if there is no map.json
            Map saveMap = new Map();
            saveMap.map = TexturedWorldMap;
            saveMap.SaveMap("./maps/map1.json");

            Map map = new Map();
            map.LoadMap("./maps/map1.json");

            // Creating camera
            Camera camera = new Camera(screenWidth, screenHeight, posX, posY, -1, 0, 0f, 0.66f);       // Creating two cameras for demo of two cameras
            Camera camera2 = new Camera(screenWidth, screenHeight, posX - 1, posY);

            // Generating textures
            string[] TexPaths =
            {
                "./textures/eagle.png",
                "./textures/redbrick.png",
                "./textures/purplestone.png",
                "./textures/greystone.png",
                "./textures/bluestone.png",
                "./textures/mossy.png",
                "./textures/wood.png",
                "./textures/colorstone.png"
            };

            List<Texture> textures = new List<Texture>();
            foreach(string path in TexPaths)
            {
                textures.Add(new Texture(path));
            }

            // Generating sprites
            Texture[] spriteTextures = new Texture[]
            {
                new Texture("./textures/barrel.png"),
                new Texture("./textures/pillar.png"),
                new Texture("./textures/greenlight_smaller.png"),
                new Texture("./textures/transparent_barrel.png")
            };
            List<Sprite> sprites = new List<Sprite>();

            sprites.Add(new Sprite() { posX = 20.5, posY = 11.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] }); // Green light in front of playerstart
            // Green lights in every room
            sprites.Add(new Sprite() { posX = 18.5, posY = 4.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 10.0, posY = 4.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 10.0, posY = 12.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 3.5, posY = 6.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 3.5, posY = 20.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 3.5, posY = 14.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });
            sprites.Add(new Sprite() { posX = 14.5, posY = 20.5, posZ = 128 + 28, scaleX = 0.3125, scaleY = 0.171875, texture = spriteTextures[2] });

            // Row of pillars in front of wall
            sprites.Add(new Sprite() { posX = 18.5, posY = 10.5, texture = spriteTextures[1] });
            sprites.Add(new Sprite() { posX = 18.5, posY = 11.5, texture = spriteTextures[1] });
            sprites.Add(new Sprite() { posX = 18.5, posY = 12.5, texture = spriteTextures[1] });

            // Some barrels around the map
            sprites.Add(new Sprite() { posX = 21.5, posY = 1.5, texture = spriteTextures[0] });
            sprites.Add(new Sprite() { posX = 15.5, posY = 1.5, posZ = 128, scaleX = 0.5, scaleY = 0.5, texture = spriteTextures[3] }); // Testing transparency and positioning + scailing
            sprites.Add(new Sprite() { posX = 16.0, posY = 1.8, texture = spriteTextures[3] }); // Testing transparency
            sprites.Add(new Sprite() { posX = 16.2, posY = 1.2, posZ = -64, scaleX = 0.7, scaleY = 0.7, texture = spriteTextures[3] }); // Testing transparency and positioning + scailing
            sprites.Add(new Sprite() { posX = 3.5, posY = 2.5, texture = spriteTextures[0] });
            sprites.Add(new Sprite() { posX = 9.5, posY = 15.5, texture = spriteTextures[0] });
            sprites.Add(new Sprite() { posX = 10.0, posY = 15.1, texture = spriteTextures[0] });
            sprites.Add(new Sprite() { posX = 10.5, posY = 15.8, texture = spriteTextures[0] });

            // Loading sounds
            RayCasting.Sound.Sound walk = new RayCasting.Sound.Sound("./sounds/walk.wav");

            float[] vertices =
            {
                //Position          Texture coordinates
                 1.0f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
                 1.0f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f  // top left
            };

            uint[] indicies = {
                0, 1, 3,    // First triangle
                1, 2, 3     // Second triangle
            };

            // Setting up OpenGL for textured rayCaster
            GameWindowSettings gws2 = GameWindowSettings.Default;
            NativeWindowSettings nws2 = NativeWindowSettings.Default;
            gws2.IsMultiThreaded = false;
            gws2.RenderFrequency = 60;
            gws2.UpdateFrequency = 60;

            nws2.APIVersion = Version.Parse("4.1.0");
            nws2.Size = new Vector2i(screenWidth, screenHeight);
            nws2.Title = "C# Textured Ray Caster";

            // Creating two windows for two cameras
            GameWindow window = new GameWindow(gws2, nws2);

            // Setting up textured RayCasting
            TexturedRayCaster RCaster = new TexturedRayCaster();

            ShaderProgram shaderProgram = new ShaderProgram(); 
            window.Load += () =>
            {
                shaderProgram = LoadShaderProgram("./TexturedShaders/vertex_shader.glsl", "./TexturedShaders/fragment_shader.glsl");

                //VertexLocation = GL.GetUniformLocation(shaderProgram2.id, "aPosition");
                //TexCoordLocation = GL.GetUniformLocation(shaderProgram2.id, "aTexCoord");
                VertexLocation = 0;
                TexCoordLocation = 1;

                TexHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, TexHandle);

                VertexArrayObject = GL.GenVertexArray();
                GL.BindVertexArray(VertexArrayObject);

                VertexBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                ElementBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indicies.Length * sizeof(uint), indicies, BufferUsageHint.StaticDraw);

                RCaster.MultiThreaded(8);

                RCaster.LoadTextures(textures);     // Loading textures to raycaster that I want to use
                RCaster.LoadSprites(sprites);       // Loading sprites that I want to use
                RCaster.CreateMap(map);

                RCaster.CreateCamera(camera);
                RCaster.CreateCamera(camera2);    // Second camera can run simoultaneously with the first one

                RCaster.UseFloorCeilingTextures(3, 6);
                //RCaster.UseFloorCeilingColors(new byte[] {0,0,0 }, new byte[] { 255,255,255});    // Using colors as floor and ceiling instead of textures
            };

            byte[,,] data;
            window.RenderFrame += (FrameEventArgs args) =>
            {
                GL.UseProgram(shaderProgram.id);

                GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                // Finding key presses
                W_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W);
                A_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A);
                S_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S);
                D_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D);

                // Playing walk sound
                if ((W_down || S_down))
                {
                    if (!walk.Playing)
                    {
                        walk.Play();
                    }
                }
                else walk.Stop();

                // Ray Casting is implemented in the TexturedRayCaster.UpdateRayCast method
                //RCaster.Move(W_down, A_down, S_down, D_down);
                RCaster.Move(W_down, A_down, S_down, D_down, camera.Id);
                //RCaster.Move(W_down, A_down, S_down, D_down, camera2.Id);
                RCaster.UpdateRayCast();

                //data = RCaster.GetRawBuffer();        // Deprecated by adding Camera class that holds it's own buffer
                data = camera.buffer;
                //data = camera2.buffer;

                // Creating Texture from the data
                DrawPixelArray(data, window.Size.X, window.Size.Y);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TexHandle);

                GL.VertexAttribPointer(VertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(VertexLocation);

                GL.VertexAttribPointer(TexCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(TexCoordLocation);

                GL.BindVertexArray(VertexArrayObject);
                GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, (IntPtr)0);

                window.SwapBuffers();
            };

            window.Resize += (ResizeEventArgs args) =>
            {
                GL.Viewport(0, 0, window.Size.X, window.Size.Y);
            };

            window.Run();
        }

        private static void DrawPixelArray(byte[,,] pixels, int width, int height)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, pixels);

            // Texture wrapping
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // Texture filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Generating Mipmaps (smaller images)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private static Shader LoadShader(string shaderLocation, ShaderType type)
        {
            int shaderId = GL.CreateShader(type);
            GL.ShaderSource(shaderId, File.ReadAllText(shaderLocation));
            GL.CompileShader(shaderId);
            string infoLog = GL.GetShaderInfoLog(shaderId);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception(infoLog);
            }

            return new Shader() { id = shaderId };
        }

        private static ShaderProgram LoadShaderProgram(string vertexShaderLocation, string fragmentShaderLocation)
        {
            int shaderProgramId = GL.CreateProgram();

            Shader vertexShader = LoadShader(vertexShaderLocation, ShaderType.VertexShader);
            Shader fragmentShader = LoadShader(fragmentShaderLocation, ShaderType.FragmentShader);

            GL.AttachShader(shaderProgramId, vertexShader.id);
            GL.AttachShader(shaderProgramId, fragmentShader.id);
            GL.LinkProgram(shaderProgramId);
            GL.DetachShader(shaderProgramId, vertexShader.id);
            GL.DetachShader(shaderProgramId, fragmentShader.id);
            GL.DeleteShader(vertexShader.id);
            GL.DeleteShader(fragmentShader.id);

            string infoLog = GL.GetProgramInfoLog(shaderProgramId);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception(infoLog);
            }

            return new ShaderProgram() { id = shaderProgramId };
        }

        public struct Shader
        {
            public int id;
        }

        public struct ShaderProgram
        {
            public int id;
        }
    }
}
