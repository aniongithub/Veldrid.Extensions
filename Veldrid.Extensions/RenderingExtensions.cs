using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid.SPIRV;
using Velrid;
using System.Reflection;
using System.Reactive;
using System.Collections.Concurrent;

namespace Veldrid.Fluent
{
    // TODO: Implement IDisposable
    public interface IRenderingPipeline
    {
        void Render(GraphicsDevice device, CommandList commandList, long elapsed);
    }

    public sealed class VertexElementAttribute: Attribute
    {
        public VertexElementAttribute(string name, VertexElementSemantic semantic, VertexElementFormat format)
        {
            Name = name;
            Semantic = semantic;
            Format = format;
        }

        public string Name { get; private set; }
        public VertexElementSemantic Semantic { get; private set; }
        public VertexElementFormat Format { get; private set; }

        public VertexElementDescription Description => new VertexElementDescription
        {
            Name = this.Name,
            Semantic = this.Semantic,
            Format = this.Format
        };
    }

    public struct RenderingResource
    {
        public DeviceResource Resource { get; set; }
        public ResourceKind ResourceKind { get; set; }
    }

    public static class RenderingExtensions
    {
        internal interface IInternalRenderingPipeline : IRenderingPipeline
        {
            ResourceLayout Layout{ get; set; }
            ResourceSet ResourceSet{ get; set; }
            Shader[] Shaders{ get; set; }
            ShaderSetDescription ShaderSet{ get; set; }
            GraphicsPipelineDescription PipelineDescription{ get; set; }
            Pipeline Pipeline{ get; set; }
            Framebuffer FrameBuffer { get; set; }

            DeviceBuffer VSParamsBuffer { get; }
            DeviceBuffer PSParamsBuffer { get; }
            DeviceBuffer VertexBuffer { get; }
            DeviceBuffer IndexBuffer { get; }
            TextureView[] TextureViews { get; }
        }

        internal sealed class BasicRenderingPipeline : IInternalRenderingPipeline
        {
            public ResourceLayout Layout { get; set; }
            public ResourceSet ResourceSet { get; set; }
            public Shader[] Shaders { get; set; }
            public ShaderSetDescription ShaderSet { get; set; }
            public GraphicsPipelineDescription PipelineDescription { get; set; }
            public Pipeline Pipeline { get; set; }
            public Framebuffer FrameBuffer { get; set; }

            public DeviceBuffer VSParamsBuffer { get; set; }
            public DeviceBuffer PSParamsBuffer { get; set; }
            public DeviceBuffer VertexBuffer { get; set; }
            public DeviceBuffer IndexBuffer { get; set; }
            public TextureView[] TextureViews { get; set; }

            public void Render(GraphicsDevice device, CommandList commandList, long elapsed)
            {
                commandList.Begin();

                commandList.SetPipeline(Pipeline);
                commandList.SetGraphicsResourceSet(0, ResourceSet);
                commandList.SetFramebuffer(FrameBuffer);

                // commandList.SetFullScissorRects();
                // commandList.SetFullViewports();

                commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

                commandList.SetVertexBuffer(0, VertexBuffer);
                commandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);

                commandList.DrawIndexed(
                    indexCount: 6,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);
                commandList.End();

                device.SubmitCommands(commandList);
            }
        }

        public static RenderingResource ReadOnly(this Texture texture)
        {
            return new RenderingResource { Resource = texture, ResourceKind = ResourceKind.TextureReadOnly };
        }

        public static RenderingResource ReadWrite(this Texture texture)
        {
            return new RenderingResource { Resource = texture, ResourceKind = ResourceKind.TextureReadWrite };
        }

        public static IRenderingPipeline RenderingPipelineFrom<TVSParams, TPSParams, TVertex, TIndex>(this GraphicsDevice device, Pool<CommandList> commandListPool, string vsFilename, string psFilename,
            IObservable<TVertex[]> vertices, int numVertices, IObservable<TIndex[]> indices, int numIndices,
            IObservable<TVSParams> vsParams, IObservable<TPSParams> psParams,
            Framebuffer frameBuffer = null,
            params RenderingResource[] resources)
            where TVertex: struct
            where TIndex: struct
            where TVSParams: struct
            where TPSParams : struct
        {
            var readonlyTextures = from resource in resources
                               let texture = resource.Resource as Texture
                               where (texture != null) && resource.ResourceKind == ResourceKind.TextureReadOnly
                               select texture;

            var readWriteTexures = from resource in resources
                                   let texture = resource.Resource as Texture
                                   where (texture != null) && resource.ResourceKind == ResourceKind.TextureReadWrite
                                   select texture;
            var samplers = from resource in resources
                           let sampler = resource.Resource as Sampler
                           where (sampler != null)
                           select sampler;

            var result = new BasicRenderingPipeline
            {
                VSParamsBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<TVSParams>(), BufferUsage.UniformBuffer)),
                PSParamsBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<TPSParams>(), BufferUsage.UniformBuffer)),
                VertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Marshal.SizeOf<TVertex>() * numVertices), BufferUsage.VertexBuffer)),
                IndexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Marshal.SizeOf<TIndex>() * numIndices), BufferUsage.IndexBuffer)),
                TextureViews = (from texture in readonlyTextures select device.ResourceFactory.CreateTextureView(texture))
                               .Append(from texture in readWriteTexures select device.ResourceFactory.CreateTextureView(texture))
                               .ToArray(),
            };

            result.Layout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription[] {
                        new ResourceLayoutElementDescription("VSParams", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                        new ResourceLayoutElementDescription("PSParams", ResourceKind.UniformBuffer, ShaderStages.Fragment)
                    }
                    .Append(from texture in readonlyTextures select new ResourceLayoutElementDescription(string.Empty, ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                    .Append(from texture in readWriteTexures select new ResourceLayoutElementDescription(string.Empty, ResourceKind.TextureReadWrite, ShaderStages.Fragment))
                    .Append(from texture in samplers select new ResourceLayoutElementDescription(string.Empty, ResourceKind.Sampler, ShaderStages.Fragment))
                    .ToArray()));
            result.ResourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                result.Layout,
                new BindableResource[] {
                    result.VSParamsBuffer,
                    result.PSParamsBuffer,
                }
                .Append(result.TextureViews)
                .Append(samplers)
                .ToArray()));
            result.Shaders = device.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    UTF8Encoding.UTF8.GetBytes(File.ReadAllText(vsFilename)),
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    UTF8Encoding.UTF8.GetBytes(File.ReadAllText(psFilename)),
                    "main"));
            result.ShaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription((from field in typeof(TVertex).GetFields()
                                              from attr in field.GetCustomAttributes<VertexElementAttribute>()
                                              select attr.Description).ToArray())
                },
                result.Shaders);

            result.FrameBuffer = frameBuffer ?? device.SwapchainFramebuffer;

            result.PipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                result.ShaderSet,
                result.Layout,
                result.FrameBuffer.OutputDescription);
            result.Pipeline = device.ResourceFactory.CreateGraphicsPipeline(result.PipelineDescription);

            vertices.Subscribe(Observer.Create<TVertex[]>(
                verts => 
                {
                    using (var disposableCl = commandListPool.Take())
                    {
                        var commandList = disposableCl.Value;
                        commandList.Begin();

                        using (var pinned = verts.Pin())
                            commandList.UpdateBuffer(result.VertexBuffer, 0, pinned.Value, (uint)(Marshal.SizeOf<TVertex>() * verts.Length));

                        commandList.End();

                        device.SubmitCommands(commandList);
                    }
                }));
            indices.Subscribe(Observer.Create<TIndex[]>(
                inds =>
                {
                    using (var disposableCl = commandListPool.Take())
                    {
                        var commandList = disposableCl.Value;
                        commandList.Begin();

                        using (var pinned = inds.Pin())
                            commandList.UpdateBuffer(result.IndexBuffer, 0, pinned.Value, (uint)(Marshal.SizeOf<TIndex>() * inds.Length));

                        commandList.End();

                        device.SubmitCommands(commandList);
                    }
                }));
            vsParams.Subscribe(Observer.Create<TVSParams>(
                vsp =>
                {
                    using (var disposableCl = commandListPool.Take())
                    {
                        var commandList = disposableCl.Value;
                        commandList.Begin();

                        commandList.UpdateBuffer(result.VSParamsBuffer, 0, vsp);

                        commandList.End();

                        device.SubmitCommands(commandList);
                    }
                }));
            psParams.Subscribe(Observer.Create<TPSParams>(
                psp =>
                {
                    using (var disposableCl = commandListPool.Take())
                    {
                        var commandList = disposableCl.Value;
                        commandList.Begin();

                        commandList.UpdateBuffer(result.PSParamsBuffer, 0, psp);

                        commandList.End();

                        device.SubmitCommands(commandList);
                    }
                }));

            return result;
        }
    }
}