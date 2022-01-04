using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.IO;
using System.Collections.Generic;
using RayCasting;

namespace UntexturedRayCastingDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Simple Ray caster!");

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

            // Saves map, in case of non existing map.json
            Map saveMap = new Map();
            saveMap.map = worldMap;
            saveMap.SaveMap("./maps/map1.json");

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

                SimpleRCaster.UseDefaultMovement();

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

            window.Run();
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
