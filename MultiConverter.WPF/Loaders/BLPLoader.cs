using MultiConverter.Lib;
using MultiConverter.Lib.Readers.BLP;
using MultiConverter.WPF.Util;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace MultiConverter.WPF.Loaders
{
    public static class BLPLoader
    {
        /// <summary>
        /// Load a texture from a given Filename.
        /// </summary>
        public static int LoadTexture(string filename)
        {
            if (Listfile.TryGetFileDataId(filename, out var fileDataId))
                return LoadTexture(fileDataId);

            return 0;
        }

        /// <summary>
        /// Load a texture from a given filedataid.
        /// </summary>
        public static int LoadTexture(uint fileDataId)
        {
            if (fileDataId == 0)
                return 0;

            if (!CASC.CascHandler.FileExists((int)fileDataId))
                return 0;

            GL.ActiveTexture(TextureUnit.Texture0);

            var textureId = GL.GenTexture();
            using (var blp = new BlpFile(CASC.CascHandler.OpenFile((int)fileDataId)))
            {
                var bmp = blp.GetBitmap(0);
                if (bmp == null)
                    throw new Exception("BLP is Null!");

                GL.BindTexture(TextureTarget.Texture2D, textureId);

                var bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                bmp.UnlockBits(bmp_data);
            }

            return textureId;
        }

        /// <summary>
        /// Load a <see cref="BitmapImage"/> based from the given filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BitmapImage LoadBLP(string filename)
        {
            if (!Listfile.TryGetFileDataId(filename, out var fileDataId) && !CASC.CascHandler.FileExists((int)fileDataId))
                throw new Exception($"{filename} does not exist in CASC!");

            using (var stream = new MemoryStream())
            using (var blp = new BlpFile(CASC.CascHandler.OpenFile((int)fileDataId)))
            {
                var bmp = blp.GetBitmap(0);
                if (bmp == null)
                    throw new Exception("BLP is Null!");

                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
