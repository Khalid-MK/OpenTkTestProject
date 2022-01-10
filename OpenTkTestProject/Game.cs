using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTkTestProject
{
    class Game : GameWindow
    {
        private int _verticesBufferHandle;
        private int _shaderProgramHandle;
        private int _vertexArrayHandle;

        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(720, 480));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            GL.ClearColor(Color.AliceBlue);

            #region Load Image
            // Install SixLabors.ImageSharp

            //Load the image
            Image<Rgba32> image = Image.Load<Rgba32>(@"C:\Users\PROG5\Desktop\Images\1.jpeg");

            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }

            #endregion

            #region Load Image Using Bitmap

            //Bitmap bitmap = new Bitmap(@"C:\Users\Khalid\Desktop\Test\wall.jpg");
            //Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            //BitmapData bitmapdata = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly,
            //    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //bitmap.UnlockBits(bitmapdata);

            #endregion

            #region Applying textures

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //float[] vertices =
            //{
            //    //Position        //Texture coordinates
            //    1f,  1f, 0.0f, 1.0f, 1.0f, // 0 top right
            //    1f, -1f, 0.0f, 1.0f, 0.0f, // 1 bottom right
            //    -1f, -1f, 0.0f, 0.0f, 0.0f, // 2 bottom left
            //    -1f,  1f, 0.0f, 0.0f, 1.0f  // 3 top left
            //};

            float[] vertices =
            {
                //Position        //Texture coordinates
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // 0 top right
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // 1 bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // 2 bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // 3 top left
            };

            #endregion

            #region Vertices Buffer

            //float[] vertices = {
            //    0.5f,  0.5f, 0.0f,  // 0
            //    0.5f, -0.5f, 0.0f,  // 1
            //    -0.5f, -0.5f, 0.0f, // 2
            //    -0.5f,  0.5f, 0.0f  // 3
            //};

            uint[] indices = {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            this._verticesBufferHandle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferHandle);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            #endregion

            #region Vertex Array

            this._vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(this._vertexArrayHandle);

            // 5 <=> 3
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._verticesBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            //
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            #region EBO

            // To use indexing 
            var elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            #endregion

            GL.BindVertexArray(0);

            #endregion

            #region Vertex Shader

            // Save this as shader.vert
            string vertexShaderCode =
                @"
                    #version 330 core

                    layout(location = 0) in vec3 aPosition;

                    layout(location = 1) in vec2 aTexCoord;

                    out vec2 texCoord;

                    void main(void)
                    {
                        texCoord = aTexCoord;

                        gl_Position = vec4(aPosition, 1.0);
                    }                    
                ";

            var vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);

            var infoLogVertex = GL.GetShaderInfoLog(vertexShaderHandle);
            if (infoLogVertex != string.Empty)
                System.Console.WriteLine(infoLogVertex);

            #endregion

            #region Fragment Shader

            // Save this as shader.frag
            string pixelShaderCode =
                @"
                    #version 330

                    out vec4 outputColor;

                    in vec2 texCoord;

                    uniform sampler2D texture0;

                    void main()
                    {
                        outputColor = texture(texture0, texCoord);
                    }                    
                ";

            var pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            var infoLogFrag = GL.GetShaderInfoLog(pixelShaderHandle);

            if (infoLogFrag != string.Empty)
                System.Console.WriteLine(infoLogFrag);

            #endregion

            #region Program

            this._shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(this._shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(this._shaderProgramHandle, pixelShaderHandle);

            GL.LinkProgram(this._shaderProgramHandle);

            #endregion

            #region Cleanup

            GL.DetachShader(this._shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(this._shaderProgramHandle, pixelShaderHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(vertexShaderHandle);

            #endregion

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            #region Clean Buffer

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.DeleteBuffer(this._verticesBufferHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(this._shaderProgramHandle);

            #endregion

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this._shaderProgramHandle);

            GL.BindVertexArray(this._vertexArrayHandle);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DrawElements(PrimitiveType.Triangles, /*indices.Length*/ 6, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
        }
    }
}
