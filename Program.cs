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

            int mapWidth = 24;
            int mapHeight = 24;
            int screenWidth = 1280;
            int screenHeight = 720;
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

            double posX = 22, posY = 12;

            bool W_down = false, A_down = false, S_down = false, D_down = false;

            float[] vertexData = new float[screenWidth * 12];

            /*// Setting up untextured RayCasting
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
                
                SimpleRCaster.CreateMap(worldMap, posX, posY);
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
            int VertexArrayObject = 0;
            int VertexBufferObject = 0;
            int ElementBufferObject = 0;

            int VertexLocation = 0;
            int TexCoordLocation = 0;

            int TexHandle = 0;

            int[,] TexturedWorldMap =
{
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,8,0,3,0,0,0,1},
                { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                { 1,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,0,1},
                { 1,5,5,5,5,5,5,5,5,0,0,0,0,0,4,0,0,0,0,0,0,4,0,1},
                { 1,5,0,4,0,0,0,0,5,0,0,0,0,0,4,0,4,4,4,4,0,4,0,1},
                { 1,5,0,0,0,0,6,0,5,0,0,0,0,0,4,0,4,0,0,4,0,4,0,1},
                { 1,5,0,5,0,0,0,0,5,0,0,0,0,0,4,0,4,4,0,4,0,4,0,1},
                { 1,5,0,5,5,5,5,5,5,0,0,0,0,0,4,0,0,0,0,4,0,4,0,1},
                { 1,5,0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,0,4,0,1},
                { 1,5,5,5,5,5,5,5,5,0,0,0,0,0,0,0,0,0,0,0,0,4,0,1},
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

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
            TexturedRayCaster RCaster = new TexturedRayCaster(screenWidth, screenHeight, textures[0].Width, textures[0].Height, renderingScale);

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

                RCaster.LoadTextures(textures);
                RCaster.CreateMap(TexturedWorldMap, posX, posY);
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
