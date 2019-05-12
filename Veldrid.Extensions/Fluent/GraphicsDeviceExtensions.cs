using System;
using Veldrid.ImageSharp;

namespace Veldrid.Extensions.Fluent
{
    public static class GraphicsDeviceExtensions
    {
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

        public static GraphicsDevice CreateBuffer(this GraphicsDevice device, uint sizeInBytes, BufferUsage usage, out DeviceBuffer buffer)
        {
            buffer = device.ResourceFactory.CreateBuffer(new BufferDescription
            {
                SizeInBytes = sizeInBytes,
                Usage = usage
            });
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

        public static Pool<CommandList> CreateCommandListPool(this GraphicsDevice device)
        {
            return new Pool<CommandList>(generator: () => device.ResourceFactory.CreateCommandList());
        }
    }
}
