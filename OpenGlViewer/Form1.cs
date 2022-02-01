using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace OpenGlViewer
{
    public partial class Form1 : Form
    {
        private Timer _timer = null!;
        private float _angle = 0.0f;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Make sure that when the GLControl is resized or needs to be painted,
            // we update our projection matrix or re-render its contents, respectively.
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;

            // Ensure that the viewport and projection matrix are set correctly initially.
            glControl_Resize(glControl, EventArgs.Empty);

            // Log WinForms keyboard/mouse events.
            glControl.MouseDown += (sender, e) =>
            {
                glControl.Focus();
                label1.Text = ($"WinForms Mouse down: ({e.X},{e.Y})");
            };
            glControl.MouseUp += (sender, e) =>
                label1.Text = ($"WinForms Mouse up: ({e.X},{e.Y})");
            glControl.MouseMove += (sender, e) =>
                label1.Text = ($"WinForms Mouse move: ({e.X},{e.Y})");
            glControl.KeyDown += (sender, e) =>
                label1.Text = ($"WinForms Key down: {e.KeyCode}");
            glControl.KeyUp += (sender, e) =>
                label1.Text = ($"WinForms Key up: {e.KeyCode}");
            glControl.KeyPress += (sender, e) =>
                label1.Text = ($"WinForms Key press: {e.KeyChar}");

        }

        private void glControl_Load(object? sender, EventArgs e)
        {
            // Make sure that when the GLControl is resized or needs to be painted,
            // we update our projection matrix or re-render its contents, respectively.
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;

            // Ensure that the viewport and projection matrix are set correctly initially.
            glControl_Resize(glControl, EventArgs.Empty);
        }

        private void glControl_Resize(object? sender, EventArgs e)
        {
            glControl.MakeCurrent();

            if (glControl.ClientSize.Height == 0)
                glControl.ClientSize = new System.Drawing.Size(glControl.ClientSize.Width, 1);

            GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);

            float aspect_ratio = Math.Max(glControl.ClientSize.Width, 1) / (float)Math.Max(glControl.ClientSize.Height, 1);
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            glControl.MakeCurrent();

            #region Load Image
            // Install SixLabors.ImageSharp

            //Load the image
            Image<Rgba32> image = Image.Load<Rgba32>(@"C:\Users\PROG5\Desktop\Images\1111.jpg");

            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            //image.Mutate(x => x.Flip(FlipMode.Vertical));

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



            GL.ClearColor(Color4.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            Matrix4 lookAt = Matrix4.LookAt(0, 0, 5, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookAt);


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.Texture2D);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.Begin(BeginMode.Quads);

            GL.Color4(Color4.White);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-1.0f, 1.0f, 0.0f);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(-1.0f, -1.0f, 0.0f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(1.0f, -1.0f, 0.0f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f);

            GL.End();

            glControl.SwapBuffers();
        }

    }
}
