using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid.SPIRV;
using System.Reflection;
using System.Reactive;
using System.Reactive.Linq;

using Veldrid.Extensions.Fluent;
using System.Collections.Generic;

namespace Veldrid.Extensions.Reactive
{
    public interface IRenderingPipeline: IDisposable
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

    public class BindableResourceInfo: IDisposable
    {
        public BindableResource Resource { get; internal set; }

        internal Action Disposer { get; set; }

        public void Dispose()
        {
            Disposer?.Invoke();
        }
    }

    public abstract class ShaderResourceInfo: BindableResourceInfo 
    {
        public string Name { get; internal set; }
        public ShaderStages Stage { get; internal set; }
        public uint Set { get; internal set; }
        public abstract ResourceKind ResourceKind { get; }
    }

    public class UniformBufferResourceInfo: ShaderResourceInfo
    {
        internal Type UnderlyingType { get; set; }
        public override ResourceKind ResourceKind => ResourceKind.UniformBuffer;
    }

    public class TextureReadOnlyResourceInfo: ShaderResourceInfo
    {
        public override ResourceKind ResourceKind => ResourceKind.TextureReadOnly;
    }

    public class TextureReadWriteResourceInfo: ShaderResourceInfo
    {
        public override ResourceKind ResourceKind => ResourceKind.TextureReadWrite;
    }

    public class DrawResourceInfo: BindableResourceInfo
    {
    }

    public class VertexBufferResourceInfo: DrawResourceInfo
    {
        public uint Index { get; internal set; }
        public Type VertexType { get; internal set; }
    }

    public class IndexBufferResourceInfo: DrawResourceInfo
    {
        internal Type IndexType { get; set; }
    }

    public static class RenderingExtensions
    {
        internal sealed class BasicRenderingPipeline: IRenderingPipeline
        {
            internal IEnumerable<IDisposable> Disposables { get; set; }

            public Pipeline Pipeline { get; set; }
            public Framebuffer FrameBuffer { get; set; }

            public IEnumerable<KeyValuePair<uint, DeviceBuffer>> VertexBuffers { get; set; }
            public DeviceBuffer IndexBuffer { get; set; }
            public IndexFormat IndexFormat { get; set; }
            public IEnumerable<KeyValuePair<uint, ResourceSet>> ResourceSets { get; set; }

            public RgbaFloat ClearColor { get; set; }

            public uint IndexCount { get; set; }
            public uint InstanceCount { get; set; }
            public uint IndexStart { get; set; }
            public int VertexOffset { get; set; }
            public uint InstanceStart { get; set; }

            public void Render(GraphicsDevice device, CommandList commandList, long elapsed)
            {
                device.SubmitCommands(
                commandList
                    .BeginFluent()
                    .SetFramebuffer(FrameBuffer)
                    .ClearColorTarget(0, ClearColor)
                    .SetPipeline(Pipeline)
                    .SetVertexBuffers(VertexBuffers)
                    .SetIndexBuffer(IndexBuffer, IndexFormat)
                    .SetGraphicsResourceSets(ResourceSets)
                    .DrawIndexed(
                   indexCount: IndexCount,
                   instanceCount: InstanceCount,
                   indexStart: IndexStart,
                   vertexOffset: VertexOffset,
                   instanceStart: InstanceStart)
                   .End());
            }

            public void Dispose()
            {
                // Destroy in reverse order of creation
                foreach (var disposable in Disposables.Reverse())
                    disposable?.Dispose();
                Disposables = Enumerable.Empty<IDisposable>();
            }
        }

        public static BindableResourceInfo AsUniformBufferResource<T>(this IObservable<T> observable, 
            string name, GraphicsDevice device, Pool<CommandList> commandListPool,
            ShaderStages stage, uint set = 0)
            where T: struct
        {
            device.CreateBuffer((uint)Marshal.SizeOf<T>(), BufferUsage.UniformBuffer, out var buffer);
            observable.Subscribe(val =>
            {
                using (commandListPool.Take(out var cl))
                using (val.Pin(out var ptr))
                    device.SubmitCommands(cl
                        .BeginFluent()
                        .UpdateBuffer(buffer, 0, ptr, (uint)Marshal.SizeOf<T>())
                        .End());
            });
            return new UniformBufferResourceInfo
            {
                Name = name,
                Set = set,
                Stage = stage,
                Resource = buffer,
                Disposer = () => buffer.Dispose(),
                UnderlyingType = typeof(T)
            };
        }

        public static BindableResourceInfo AsIndexBufferResource<T>(this IObservable<Memory<T>> observable,
            uint maxElements,
            GraphicsDevice device, Pool<CommandList> commandListPool)
            where T : struct
        {
            device.CreateBuffer((uint)Marshal.SizeOf<T>() * maxElements, BufferUsage.IndexBuffer, out var buffer);
            observable.Subscribe(val =>
            {
                using (commandListPool.Take(out var cl))
                using (val.Pin(out var ptr))
                    device.SubmitCommands(cl
                        .BeginFluent()
                        .UpdateBuffer(buffer, 0, ptr, (uint)(Marshal.SizeOf<T>() * val.Length))
                        .End());
            });
            return new IndexBufferResourceInfo
            {
                IndexType = typeof(T),
                Resource = buffer,
                Disposer = () => buffer.Dispose()
            };
        }

        public static BindableResourceInfo AsVertexBufferResource<T>(this IObservable<Memory<T>> observable,
            uint maxElements, uint index,
            GraphicsDevice device, Pool<CommandList> commandListPool)
            where T : struct
        {
            device.CreateBuffer((uint)Marshal.SizeOf<T>() * maxElements, BufferUsage.VertexBuffer, out var buffer);
            observable.Subscribe(val =>
            {
                using (commandListPool.Take(out var cl))
                using (val.Pin(out var ptr))
                    device.SubmitCommands(cl
                        .BeginFluent()
                        .UpdateBuffer(buffer, 0, ptr, (uint)(Marshal.SizeOf<T>() * val.Length))
                        .End());
            });
            return new VertexBufferResourceInfo
            {
                VertexType = typeof(T),
                Resource = buffer,
                Disposer = () => buffer.Dispose(),
                Index = index
            };
        }

        public static BindableResourceInfo AsTextureReadonlyResource(this Texture texture, GraphicsDevice device, 
            ShaderStages stage = ShaderStages.Fragment, 
            uint set = 0)
        {
            device.CreateTextureView(texture, out var view);
            return new TextureReadOnlyResourceInfo
            {
                Resource = view,
                Disposer = () => view.Dispose(),
                Set = set,
                Stage = stage
            };
        }

        public static BindableResourceInfo AsTextureReadWriteResource(this Texture texture, GraphicsDevice device,
            ShaderStages stage = ShaderStages.Fragment,
            uint set = 0)
        {
            device.CreateTextureView(texture, out var view);
            return new TextureReadWriteResourceInfo
            {
                Resource = view,
                Disposer = () => view.Dispose(),
                Set = set,
                Stage = stage
            };
        }

        public static IRenderingPipeline RenderingPipelineFrom(this GraphicsDevice device, 
            Pool<CommandList> commandListPool, 
            string vsFilename, string psFilename,
            IObservable<RgbaFloat> clearColor = null,
            IObservable<Framebuffer> frameBuffer = null,
            IObservable<uint> instanceStart = null,
            IObservable<uint> instanceCount = null,
            IObservable<uint> indexStart = null,
            IObservable<int> vertexOffset = null,
            params BindableResourceInfo[] resources)
        {
            var vertexBuffers = from resource in resources
                                 where resource is VertexBufferResourceInfo
                                 select (VertexBufferResourceInfo)resource;
            if (!vertexBuffers.Any())
                throw new ArgumentException("No vertex buffers bound!");
            var indexBuffer = (from resource in resources
                               where resource is IndexBufferResourceInfo
                               select (IndexBufferResourceInfo)resource).FirstOrDefault();
            if (indexBuffer == null)
                throw new ArgumentException("No index buffer bound!");

            var shaders = device.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(File.ReadAllText(vsFilename)),
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(File.ReadAllText(psFilename)),
                    "main"));
            var shaderSet = new ShaderSetDescription((from vertexBufferInfo in vertexBuffers
                                                      orderby vertexBufferInfo.Index
                                                      select
                                                         new VertexLayoutDescription(
                                                             (from field in vertexBufferInfo.VertexType.GetFields()
                                                              from attr in field.GetCustomAttributes<VertexElementAttribute>()
                                                              select attr.Description).ToArray())).ToArray(),
                                                        shaders);
            var layouts = from resource in resources
                          where resource is ShaderResourceInfo
                          let shaderResource = resource as ShaderResourceInfo
                          group resource by shaderResource.Set into g
                          select new
                          {
                              Layout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                              (from ShaderResourceInfo r in g
                               select new ResourceLayoutElementDescription(
                               r.Name, r.ResourceKind, r.Stage)).ToArray())),
                              Resources = g
                          };
            var resourceSets = from layout in layouts
                               select new KeyValuePair<uint, ResourceSet>(layout.Resources.Key, device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                                   layout.Layout,
                                   (from r in layout.Resources select r.Resource).ToArray())));
            return new BasicRenderingPipeline
            {
                VertexBuffers = from v in vertexBuffers select new KeyValuePair<uint, DeviceBuffer>(v.Index, (v.Resource as DeviceBuffer)),
                IndexBuffer = indexBuffer.Resource as DeviceBuffer,
                IndexFormat = indexBuffer.IndexType == typeof(short) ?
                    IndexFormat.UInt16 :
                    indexBuffer.IndexType == typeof(uint) ?
                        IndexFormat.UInt32 :
                        throw new InvalidDataException($"Unknown index format: {indexBuffer.IndexType.ToString()}"),
                ResourceSets = resourceSets
            };

            //result.ResourceSets = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            //    result.Layout,
            //    new BindableResource[] {
            //        result.VSParamsBuffer,
            //        result.PSParamsBuffer,
            //    }
            //    .Append(result.BindableResources)
            //    .Append(samplers)
            //    .ToArray()));
            //result.Disposables.Append(result.ResourceSet);

            //result.Shaders = device.ResourceFactory.CreateFromSpirv(
            //    new ShaderDescription(
            //        ShaderStages.Vertex,
            //        Encoding.UTF8.GetBytes(File.ReadAllText(vsFilename)),
            //        "main"),
            //    new ShaderDescription(
            //        ShaderStages.Fragment,
            //        Encoding.UTF8.GetBytes(File.ReadAllText(psFilename)),
            //        "main"));
            //result.Disposables.Append(result.Shaders);

            //result.ShaderSet = new ShaderSetDescription(
            //    new[]
            //    {
            //        new VertexLayoutDescription((from field in typeof(TVertex).GetFields()
            //                                  from attr in field.GetCustomAttributes<VertexElementAttribute>()
            //                                  select attr.Description).ToArray())
            //    },
            //    result.Shaders);
            //result.Disposables.Append(result.Shaders);

            //(frameBuffer ?? Observable.Never<Framebuffer>().StartWith(device.SwapchainFramebuffer))
            //    .Subscribe(fb => result.FrameBuffer = fb);

            //result.PipelineDescription = new GraphicsPipelineDescription(
            //    BlendStateDescription.SingleOverrideBlend,
            //    DepthStencilStateDescription.Disabled,
            //    new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
            //    PrimitiveTopology.TriangleList,
            //    result.ShaderSet,
            //    result.Layout,
            //    result.FrameBuffer.OutputDescription);
            //result.Pipeline = device.ResourceFactory.CreateGraphicsPipeline(result.PipelineDescription);
            //result.Disposables.Append(result.Pipeline);

            //clearColor.Subscribe(color => result.ClearColor = color);

            //(instanceCount ?? Observable.Never<uint>().StartWith(1u))
            //    .Subscribe(i => result.InstanceCount = i);
            //(instanceStart ?? Observable.Never<uint>().StartWith(0u))
            //    .Subscribe(i => result.InstanceStart = i);
            //(vertexOffset ?? Observable.Never<int>().StartWith(0))
            //    .Subscribe(o => result.VertexOffset = o);
            //(indexStart ?? Observable.Never<uint>().StartWith(0u))
            //    .Subscribe(i => result.IndexStart = i);
                
            //if (typeof(TIndex) == typeof(ushort))
            //    result.IndexFormat = IndexFormat.UInt16;
            //if (typeof(TIndex) == typeof(uint))
            //    result.Indexformat = IndexFormat.UInt32;

            //vertices.Subscribe(Observer.Create<Memory<TVertex>>(
            //    verts =>
            //    {
            //        using (var disposableCl = commandListPool.Take(out var cl))
            //        using (var pinned = verts.Pin(out var ptr))
            //            device.SubmitCommands(cl
            //            .BeginFluent()
            //            .UpdateBuffer(result.VertexBuffer, 0, ptr, (uint)(Marshal.SizeOf<TVertex>() * verts.Length))
            //            .End());
            //    }));
            //indices.Subscribe(Observer.Create<Memory<TIndex>>(
            //    inds =>
            //    {
            //        using (var disposableCl = commandListPool.Take(out var cl))
            //        using (var pinned = inds.Pin(out var ptr))
            //            device.SubmitCommands(cl
            //            .BeginFluent()
            //            .UpdateBuffer(result.IndexBuffer, 0, ptr, (uint)(Marshal.SizeOf<TIndex>() * inds.Length))
            //            .End());
            //        result.IndexCount = (uint)inds.Length;
            //    }));
            //vsParams.Subscribe(Observer.Create<TVSParams>(
            //    vsp =>
            //    {
            //        using (var disposableCl = commandListPool.Take(out var cl))
            //            device.SubmitCommands(cl
            //                .BeginFluent()
            //                .UpdateBuffer(result.VSParamsBuffer, 0, vsp)
            //                .End());
            //    }));
            //psParams.Subscribe(Observer.Create<TPSParams>(
            //    psp =>
            //    {
            //        using (var disposableCl = commandListPool.Take(out var cl))
            //            device.SubmitCommands(
            //                cl.BeginFluent()
            //                .UpdateBuffer(result.PSParamsBuffer, 0, psp)
            //                .End());
            //    }));

            //return result;
        }
    }
}