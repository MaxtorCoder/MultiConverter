using MultiConverter.Lib.Readers.WMO;
using MultiConverter.Lib.RenderingObject;
using MultiConverter.WPF.Loaders;
using MultiConverter.WPF.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MultiConverter.WPF.OpenGL
{
    public class ModelPreview
    {
        public static Dictionary<uint, List<uint>> FilesXChildren = new Dictionary<uint, List<uint>>();

        private GLControl glControl;
        private System.Windows.Controls.Image blpView;

        private WorldObject currentObject;

        // Shaders
        private int wmoShaderProgram;
        private int m2ShaderProgram;
        private int bakeShaderProgram;

        private Camera activeCamera;

        public ModelPreview(GLControl glControl, System.Windows.Controls.Image blpView)
        {
            this.blpView = blpView;
            this.glControl = glControl;
            this.glControl.Paint += GlControl_Paint;
            this.glControl.Load += GlControl_Load;
            this.glControl.Resize += GlControl_Resize;

            activeCamera = new Camera(glControl.Width, glControl.Height, new Vector3(0, 0, -1), new Vector3(-11, 0, 0));
        }

        /// <summary>
        /// Set the camera position
        /// </summary
        public void SetCamera(float x, float y, float z, float rot)
        {
            activeCamera.Pos = new Vector3(x, y, z);
            activeCamera.rotationAngle = rot;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);

            wmoShaderProgram    = Shader.CompileShader("wmo");
            m2ShaderProgram     = Shader.CompileShader("m2");
            bakeShaderProgram   = Shader.CompileShader("baketexture");

            GL.ClearColor(Color.LightBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Update the GLControl.
        /// </summary>
        private void Update()
        {
            if (!glControl.Focused) 
                return;

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            activeCamera.processKeyboardInput(keyboardState);

            return;
        }

        /// <summary>
        /// Load a model based off a given filename.
        /// </summary>
        public void LoadModel(string filename)
        {
            try
            {
                if (filename.EndsWith(".wmo"))
                {
                    GL.ActiveTexture(TextureUnit.Texture0);

                    var wmoLoader = new WMOLoader();
                    wmoLoader.ReadWMO(filename, wmoShaderProgram);

                    currentObject = wmoLoader;
                }
                else if (filename.EndsWith(".blp"))
                {
                    var bitmapImage = BLPLoader.LoadBLP(filename);
                    blpView.Source = bitmapImage;
                    blpView.MaxWidth = bitmapImage.Width;
                    blpView.MaxHeight = bitmapImage.Height;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unknown error occured while loading {filename}\n{ex}");
            }

            activeCamera.ResetCamera();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.Enable(EnableCap.Texture2D);

            if (currentObject is WMOLoader wmo)
            {
                GL.UseProgram(wmoShaderProgram);

                activeCamera.setupGLRenderMatrix(wmoShaderProgram);
                activeCamera.flyMode = false;

                var alphaRefLoc = GL.GetUniformLocation(wmoShaderProgram, "alphaRef");
                for (var i = 0; i < wmo.WorldModel.Batches.Count; ++i)
                {
                    GL.BindVertexArray(wmo.WorldModel.GroupBatches[(int)wmo.WorldModel.Batches[i].GroupId].Vao);

                    switch (wmo.WorldModel.Batches[i].BlendType)
                    {
                        case 0:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            break;
                        case 1:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, 0.90393700787f);
                            break;
                        case 2:
                            GL.Enable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        default:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            break;
                    }

                    // Bind all textures
                    GL.BindTexture(TextureTarget.Texture2D, wmo.WorldModel.Batches[i].MaterialId[0]);
                    GL.BindTexture(TextureTarget.Texture2D, wmo.WorldModel.Batches[i].MaterialId[1]);
                    GL.BindTexture(TextureTarget.Texture2D, wmo.WorldModel.Batches[i].MaterialId[2]);

                    GL.DrawElements(PrimitiveType.Triangles, (int)wmo.WorldModel.Batches[i].NumFaces, DrawElementsType.UnsignedInt, (int)wmo.WorldModel.Batches[i].FirstFace * 4);
                }
            }

            var error = GL.GetError().ToString();
            if (error != "NoError")
                Console.WriteLine(error);

            GL.BindVertexArray(0);
            glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            if (glControl.Width > 0 && glControl.Height > 0)
                activeCamera.viewportSize(glControl.Width, glControl.Height);
        }

        public void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Update();
            glControl.Invalidate();
        }

        public void WFHost_Initialized(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
        }
    }
}
