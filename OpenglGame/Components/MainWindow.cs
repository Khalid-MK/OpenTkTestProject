using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenglGame.Components
{
    public class MainWindow : GameWindow
    {
        public MainWindow() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        private int _program;
        private int _vertexArray;
        private int _buffer;
        private double _time;

        protected override void OnLoad()
        {
            CursorVisible = true;

            const string vertexShaderPath = @"Components\Shaders\vertexShader.vert";
            const string fragmentsShaderPath = @"Components\Shaders\fragmentShader.frag";

            _program = CreateProgram(vertexShaderPath, fragmentsShaderPath);

            #region Vertex Array
            Vertex[] vertices =
            {
                new Vertex(new Vector4(-0.25f, 0.25f, 0.5f, 1-0f), Color4.HotPink),
                new Vertex(new Vector4( 0.0f, -0.25f, 0.5f, 1-0f), Color4.HotPink),
                new Vertex(new Vector4( 0.25f, 0.25f, 0.5f, 1-0f), Color4.HotPink),
            };

            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();

            GL.BindVertexArray(_vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            GL.NamedBufferStorage(
                buffer: _buffer,
                size: Vertex.Size * vertices.Length,
                data: vertices,
                flags: BufferStorageFlags.MapWriteBit);

            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                0,
                4,
                VertexAttribType.Float,
                false,
                0);

            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                1,
                4,
                VertexAttribType.Float,
                false,
                16);



            #endregion


            //GL.GenVertexArrays(1, out _vertexArray);
            //GL.BindVertexArray(_vertexArray);
            Closed += OnClosed;

            base.OnLoad();
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Close();
        }

        public override void Close()
        {
            GL.DeleteVertexArrays(1, ref _vertexArray);
            GL.DeleteProgram(_program);
            base.Close();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += e.Time;

            Title = $"Time: {MathHelper.Round(_time, 2)}";

            var backColor = new Color4(0.1f, 0.1f, 0.3f, 1f);

            GL.ClearColor(backColor);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.UseProgram(_program);

            #region Shader Attributes

            int timeIndex = GL.GetAttribLocation(_program, "time");
            GL.VertexAttrib1(timeIndex, _time);

            Vector3 position;
            position.X = (float)Math.Sin(_time) * 0.5f;
            position.Y = (float)Math.Cos(_time) * 0.5f;
            position.Z = 0.0f;

            int positionIndex = GL.GetAttribLocation(_program, "position");
            GL.VertexAttrib3(positionIndex, position);

            #endregion

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, Vertex.Size);

            GL.BindVertexArray(_vertexArray);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.PointSize(10);
            SwapBuffers();
        }

        private int CreateProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            var program = GL.CreateProgram();
            var shaders = new List<int>
            {
                CompileShader(ShaderType.VertexShader, vertexShaderPath),
                CompileShader(ShaderType.FragmentShader, fragmentShaderPath)
            };

            foreach (var shader in shaders)
                GL.AttachShader(program, shader);

            GL.LinkProgram(program);

            var info = GL.GetProgramInfoLog(program);
            if (!string.IsNullOrWhiteSpace(info))
                Debug.WriteLine($"GL.LinkProgram had info log: {info}");

            foreach (var shader in shaders)
            {
                GL.DetachShader(program, shader);
                GL.DeleteShader(shader);
            }

            return program;
        }

        private int CompileShader(ShaderType type, string path)
        {
            var shader = GL.CreateShader(type);
            var src = File.ReadAllText(path);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);
            var info = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(info))
                Debug.WriteLine($"GL.CompileShader [{type}] had info log: {info}");
            return shader;
        }
    }
}
