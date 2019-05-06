using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Veldrid;
using Veldrid.Fluent;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Velrid.Fluent
{
    public static class VeldridExtensions
    {
        public static void Render(this Sdl2Window window, GraphicsDevice device, params Action<GraphicsDevice, CommandList, long>[] renderFuncs)
        {
            var stopwatch = new Stopwatch();
            long elapsedTime = 0;
            long lastUpdateTime = stopwatch.ElapsedMilliseconds;
            long gameTime = 0;
            var commandListPool = new Pool<CommandList>(generator: () => device.ResourceFactory.CreateCommandList());
            while (window.Exists)
            {
                window.PumpEvents();
                gameTime = stopwatch.ElapsedMilliseconds;
                elapsedTime = lastUpdateTime - gameTime;

                using (var clDisposable = commandListPool.Take())
                    foreach (var renderFunc in renderFuncs)
                        renderFunc(device, clDisposable.Value, elapsedTime);

                device.SwapBuffers();

                lastUpdateTime = gameTime;
            }
        }

        public static GraphicsDevice CreateTexture(this GraphicsDevice device,
            TextureDescription desc,
            out Texture texture)
        {
            texture = device.ResourceFactory.CreateTexture(desc);
            return device;
        }

        public static GraphicsDevice CreateTextureView(this GraphicsDevice device,
            Texture texture,
            out TextureView view)
        {
            view = device.ResourceFactory.CreateTextureView(texture);

            return device;
        }

        public static GraphicsDevice CreateStagingTexture(this GraphicsDevice device,
            Texture texture,
            out Texture stagingTexture)
        {
            switch (texture.Type)
            {
                case TextureType.Texture1D:
                    stagingTexture = device.ResourceFactory.CreateTexture(
                        TextureDescription.Texture1D(texture.Width,
                                                     texture.MipLevels,
                                                     texture.ArrayLayers, texture.Format, TextureUsage.Staging));
                    break;
                case TextureType.Texture2D:
                    stagingTexture = device.ResourceFactory.CreateTexture(
                        TextureDescription.Texture2D(texture.Width, texture.Height,
                                                     texture.MipLevels,
                                                     texture.ArrayLayers, texture.Format, TextureUsage.Staging));
                    break;
                case TextureType.Texture3D:
                    stagingTexture = device.ResourceFactory.CreateTexture(
                        TextureDescription.Texture2D(texture.Width, texture.Height, texture.Depth,
                                                     texture.MipLevels, texture.Format, TextureUsage.Staging));
                    break;

                default:
                    throw new NotImplementedException($"Don't know how to create staging texture for {texture.Type}");
            }

            return device;
        }

        public static GraphicsDevice CreateTextureFromFile(this GraphicsDevice device,
            string filename, out Texture texture)
        {
            var img = new ImageSharpTexture(filename);
            texture = img.CreateDeviceTexture(device, device.ResourceFactory);
            return device;
        }
    }
}