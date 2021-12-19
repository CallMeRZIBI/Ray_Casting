using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.IO;
using System.Collections.Generic;
using RayCasting.RayCasting;

namespace RayCasting
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Ray caster!");

            /*int mapWidth = 24;
            int mapHeight = 24;
            int screenWidth = 640;
            int screenHeight = 360;
            float renderingScale = 1.0f;

            int[,] worldMap =
            {
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,2,0,3,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                { 1,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,0,1},
                { 1,4,4,4,4,4,4,4,4,0,0,0,0,0,4,0,0,0,0,0,0,4,0,1},
                { 1,4,0,4,0,0,0,0,4,0,0,0,0,0,4,0,4,4,4,4,0,4,0,1},
                { 1,4,0,0,0,0,5,0,4,0,0,0,0,0,4,0,4,0,0,4,0,4,0,1},
                { 1,4,0,4,0,0,0,0,4,0,0,0,0,0,4,0,4,4,0,4,0,4,0,1},
                { 1,4,0,4,4,4,4,4,4,0,0,0,0,0,4,0,0,0,0,4,0,4,0,1},
                { 1,4,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,0,4,0,1},
                { 1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,4,0,1},
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            Map map = new Map();
            map.LoadMap("./maps/map1.json");

            double posX = 22, posY = 12;

            bool W_down = false, A_down = false, S_down = false, D_down = false;

            float[] vertexData = new float[screenWidth * 12];

            // Setting up untextured RayCasting
            UntexturedRayCaster SimpleRCaster = new UntexturedRayCaster(screenWidth, screenHeight, renderingScale);

            // Setting up OpenGl for simple RayCasting
            GameWindowSettings gws = GameWindowSettings.Default;
            NativeWindowSettings nws = NativeWindowSettings.Default;
            gws.IsMultiThreaded = false;
            gws.RenderFrequency = 60;
            gws.UpdateFrequency = 60;

            nws.APIVersion = Version.Parse("4.1.0");
            nws.Size = new Vector2i(screenWidth, screenHeight);
            nws.Title = "C# Simple Ray Caster";

            GameWindow window = new GameWindow(gws, nws);

            //int uniformProj = -1;
            ShaderProgram shaderProgram = new ShaderProgram() { id = 0 };
            window.Load += () =>
            {
                shaderProgram = LoadShaderProgram("./vertex_shader.glsl", "./fragment_shader.glsl");
                //uniformProj = GL.GetUniformLocation(shaderProgram.id, "proj");
                
                SimpleRCaster.CreateMap(map, posX, posY);
            };

            window.RenderFrame += (FrameEventArgs args) =>
            {
                GL.UseProgram(shaderProgram.id);

                GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                //Matrix4 projMatrix = Matrix4.CreateOrthographic(1000.0f, 1000.0f, 0.1f, 1000.0f);
                //GL.UniformMatrix4(uniformProj, false, ref projMatrix);

                // Finding key presses
                W_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W);
                A_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A);
                S_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S);
                D_down = window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D);

                // Ray Casting is implemented in the UntexturedRayCaster.UpdateRayCast method
                SimpleRCaster.UpdateRayCast(W_down, A_down, S_down, D_down);

                // Get the rays in OenGL Vertex form
                vertexData = SimpleRCaster.GetGLVertices();

                int vao = GL.GenVertexArray();
                int vertices = GL.GenBuffer();
                int colors = GL.GenBuffer();
                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertices);
                GL.BufferStorage(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferStorageFlags.None);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 24, 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 24, 12);
                GL.LineWidth(SimpleRCaster.LineThickness);

                GL.DrawArrays(PrimitiveType.Lines, 0, window.Size.X * 2);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                GL.DeleteVertexArray(vao);
                GL.DeleteBuffer(vertices);

                window.SwapBuffers();
            };

            window.Run();*/

            // Textured Raycaster
            // Rendering things
            int screenWidth = 1280;
            int screenHeight = 720;
            float renderingScale = 1.0f;

            int VertexArrayObject = 0;
            int VertexBufferObject = 0;
            int ElementBufferObject = 0;

            int VertexLocation = 0;
            int TexCoordLocation = 0;

            int TexHandle = 0;

            // Game things
            int mapWidth = 24;
            int mapHeight = 24;
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

            Map map = new Map();
            map.LoadMap("./maps/map1.json");

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

            GameWindow window2 = new GameWindow(gws2, nws2);

            // Setting up textured RayCasting
            TexturedRayCaster RCaster = new TexturedRayCaster(screenWidth, screenHeight, renderingScale);

            ShaderProgram shaderProgram2 = new ShaderProgram(); 
            window2.Load += () =>
            {
                shaderProgram2 = LoadShaderProgram("./TexturedShaders/vertex_shader.glsl", "./TexturedShaders/fragment_shader.glsl");

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

                RCaster.MultiThreaded(4);

                RCaster.LoadTextures(textures);     // Loading textures to raycaster that I want to use
                RCaster.LoadSprites(sprites);       // Loading sprites that I want to use to raycaster
                RCaster.CreateMap(map, posX, posY);
                RCaster.UseFloorCeilingTextures(3, 6);
                //RCaster.UseFloorCeilingColors(new byte[] {0,0,0 }, new byte[] { 255,255,255});
            };

            byte[,,] data;
            window2.RenderFrame += (FrameEventArgs args) =>
            {
                GL.UseProgram(shaderProgram2.id);

                GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                // Finding key presses
                W_down = window2.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W);
                A_down = window2.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A);
                S_down = window2.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S);
                D_down = window2.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D);

                // Ray Casting is implemented in the TexturedRayCaster.UpdateRayCast method
                RCaster.UpdateRayCast(W_down, A_down, S_down, D_down);

                data = RCaster.GetRawBuffer();

                // Creating Texture from the data
                DrawPixelArray(data, window2.Size.X, window2.Size.Y);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TexHandle);

                GL.VertexAttribPointer(VertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(VertexLocation);

                GL.VertexAttribPointer(TexCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(TexCoordLocation);

                GL.BindVertexArray(VertexArrayObject);
                GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, (IntPtr)0);

                window2.SwapBuffers();
            };

            window2.Resize += (ResizeEventArgs args) =>
            {
                GL.Viewport(0, 0, window2.Size.X, window2.Size.Y);
            };

            window2.Run();
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
