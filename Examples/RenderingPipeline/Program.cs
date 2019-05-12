using Veldrid;
using Veldrid.Extensions.Fluent;
using Veldrid.Extensions.SDL2;
using Veldrid.Extensions.Reactive;
using Veldrid.StartupUtilities;
using System.Numerics;
using System;
using System.Reactive.Linq;

namespace RenderingPipeline
{
    struct VertexPositionTexture
    {
        [VertexElement(nameof(Position), VertexElementSemantic.Position, VertexElementFormat.Float3)] 
        public Vector3 Position;
        [VertexElement(nameof(TexCoords), VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)]
        public Vector2 TexCoords;

        public VertexPositionTexture(Vector3 pos, Vector2 texCoords)
        {
            Position = pos;
            TexCoords = texCoords;
        }
    }

    class Program
    {
        private static readonly VertexPositionTexture[] CubeVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1)),
            // Bottom                                                             
            new VertexPositionTexture(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1)),
            // Left                                                               
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
            // Right                                                              
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
            // Back                                                               
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
            // Front                                                              
            new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1))
        };

        private static readonly ushort[] CubeIndices = new ushort[] {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

        static void Main(string[] args)
        {
            var windowCreateInfo = new WindowCreateInfo
            {
                WindowInitialState = WindowState.Maximized
            };
            var window = VeldridStartup.CreateWindow(ref windowCreateInfo);

            using (var device = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGL))
            using (var commandListPool = device.CreateCommandListPool())
            using (device.CreateTextureFromFile("textures/spnza_bricks_a_diff.png", out var texture))
            using (var pipeline = device.RenderingPipelineFrom(commandListPool, 
                "shaders/simple.vert", "shaders/simple.frag",
                null, null, null, null, null, null,
,                resources: 
                    Observable.Never<Memory<VertexPositionTexture>>()
                    .StartWith(new Memory<VertexPositionTexture>(CubeVertices))
                    .AsVertexBufferResource((uint)CubeVertices.Length, 0, device, commandListPool),
                    texture.AsTextureReadonlyResource(device)))
                window.Render(device,
                    pipeline.Render,
                    (dev, cl, elapsed) => dev.SubmitCommands(
                        cl.BeginFluent()
                        .SetFramebuffer(dev.SwapchainFramebuffer)
                        .ClearColorTarget(0, RgbaFloat.CornflowerBlue)
                        .End()));
        }
    }
}
