using System;
using System.Collections.Generic;

namespace Veldrid.Extensions.Fluent
{
    public static class CommandListExtensions
    {
        public static FluentWrapper<CommandList> BeginFluent(this CommandList commandList)
        {
            commandList.Begin();
            return new FluentWrapper<CommandList>(commandList);
        }
        public static CommandList End(this FluentWrapper<CommandList> commandList)
        {
            commandList.Value.End();
            return commandList.Value;
        }

        public static FluentWrapper<CommandList> SetPipeline(this FluentWrapper<CommandList> commandList, Pipeline pipeline)
        {
            commandList.SetPipeline(pipeline);
            return commandList;
        }

        public static FluentWrapper<CommandList> SetVertexBuffer(this FluentWrapper<CommandList> commandList, UInt32 index, DeviceBuffer buffer)
        {
            commandList.SetVertexBuffer(index, buffer);
            return commandList;
        }

        public static FluentWrapper<CommandList> SetVertexBuffers(this FluentWrapper<CommandList> commandList, IEnumerable<KeyValuePair<uint, DeviceBuffer>> buffers)
        {
            foreach (var kvp in buffers)
                commandList.SetVertexBuffer(kvp.Key, kvp.Value);
            return commandList;
        }

        public static FluentWrapper<CommandList> SetVertexBuffer(this FluentWrapper<CommandList> commandList, UInt32 index, DeviceBuffer buffer, UInt32 offset)
        {
            commandList.SetVertexBuffer(index, buffer, offset);
            return commandList;
        }

        public static FluentWrapper<CommandList> SetIndexBuffer(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, IndexFormat format)
        {
            commandList.SetIndexBuffer(buffer, format);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetIndexBuffer(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, IndexFormat format, UInt32 offset)
        {
            commandList.SetIndexBuffer(buffer, format, offset);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetGraphicsResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs)
        {
            commandList.SetGraphicsResourceSet(slot, rs);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetGraphicsResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs, UInt32[] dynamicOffsets)
        {
            commandList.SetGraphicsResourceSet(slot, rs, dynamicOffsets);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetGraphicsResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs, UInt32 dynamicOffsetsCount, ref UInt32 dynamicOffsets)
        {
            commandList.SetGraphicsResourceSet(slot, rs, dynamicOffsetsCount, ref dynamicOffsets);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetComputeResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs)
        {
            commandList.SetComputeResourceSet(slot, rs);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetComputeResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs, UInt32[] dynamicOffsets)
        {
            commandList.SetComputeResourceSet(slot, rs, dynamicOffsets);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetComputeResourceSet(this FluentWrapper<CommandList> commandList, UInt32 slot, ResourceSet rs, UInt32 dynamicOffsetsCount, ref UInt32 dynamicOffsets)
        {
            commandList.SetComputeResourceSet(slot, rs, dynamicOffsetsCount, ref dynamicOffsets);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetFramebuffer(this FluentWrapper<CommandList> commandList, Framebuffer fb)
        {
            commandList.SetFramebuffer(fb);
            return commandList;
        }
        public static FluentWrapper<CommandList> ClearColorTarget(this FluentWrapper<CommandList> commandList, UInt32 index, RgbaFloat clearColor)
        {
            commandList.ClearColorTarget(index, clearColor);
            return commandList;
        }
        public static FluentWrapper<CommandList> ClearDepthStencil(this FluentWrapper<CommandList> commandList, Single depth)
        {
            commandList.ClearDepthStencil(depth);
            return commandList;
        }
        public static FluentWrapper<CommandList> ClearDepthStencil(this FluentWrapper<CommandList> commandList, Single depth, Byte stencil)
        {
            commandList.ClearDepthStencil(depth, stencil);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetFullViewports(this FluentWrapper<CommandList> commandList)
        {
            commandList.SetFullViewports();
            return commandList;
        }
        public static FluentWrapper<CommandList> SetFullViewport(this FluentWrapper<CommandList> commandList, UInt32 index)
        {
            commandList.SetFullViewport(index);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetViewport(this FluentWrapper<CommandList> commandList, UInt32 index, Viewport viewport)
        {
            commandList.SetViewport(index, viewport);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetViewport(this FluentWrapper<CommandList> commandList, UInt32 index, ref Viewport viewport)
        {
            commandList.SetViewport(index, ref viewport);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetFullScissorRects(this FluentWrapper<CommandList> commandList)
        {
            commandList.SetFullScissorRects();
            return commandList;
        }
        public static FluentWrapper<CommandList> SetFullScissorRect(this FluentWrapper<CommandList> commandList, UInt32 index)
        {
            commandList.SetFullScissorRect(index);
            return commandList;
        }
        public static FluentWrapper<CommandList> SetScissorRect(this FluentWrapper<CommandList> commandList, UInt32 index, UInt32 x, UInt32 y, UInt32 width, UInt32 height)
        {
            commandList.SetScissorRect(index, x, y, width, height);
            return commandList;
        }
        public static FluentWrapper<CommandList> Draw(this FluentWrapper<CommandList> commandList, UInt32 vertexCount)
        {
            commandList.Draw(vertexCount);
            return commandList;
        }
        public static FluentWrapper<CommandList> Draw(this FluentWrapper<CommandList> commandList, UInt32 vertexCount, UInt32 instanceCount, UInt32 vertexStart, UInt32 instanceStart)
        {
            commandList.Draw(vertexCount, instanceCount, vertexStart, instanceStart);
            return commandList;
        }
        public static FluentWrapper<CommandList> DrawIndexed(this FluentWrapper<CommandList> commandList, UInt32 indexCount)
        {
            commandList.DrawIndexed(indexCount);
            return commandList;
        }
        public static FluentWrapper<CommandList> DrawIndexed(this FluentWrapper<CommandList> commandList, UInt32 indexCount, UInt32 instanceCount, UInt32 indexStart, Int32 vertexOffset, UInt32 instanceStart)
        {
            commandList.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
            return commandList;
        }
        public static FluentWrapper<CommandList> DrawIndirect(this FluentWrapper<CommandList> commandList, DeviceBuffer indirectBuffer, UInt32 offset, UInt32 drawCount, UInt32 stride)
        {
            commandList.DrawIndirect(indirectBuffer, offset, drawCount, stride);
            return commandList;
        }
        public static FluentWrapper<CommandList> DrawIndexedIndirect(this FluentWrapper<CommandList> commandList, DeviceBuffer indirectBuffer, UInt32 offset, UInt32 drawCount, UInt32 stride)
        {
            commandList.DrawIndexedIndirect(indirectBuffer, offset, drawCount, stride);
            return commandList;
        }
        public static FluentWrapper<CommandList> Dispatch(this FluentWrapper<CommandList> commandList, UInt32 groupCountX, UInt32 groupCountY, UInt32 groupCountZ)
        {
            commandList.Dispatch(groupCountX, groupCountY, groupCountZ);
            return commandList;
        }
        public static FluentWrapper<CommandList> DispatchIndirect(this FluentWrapper<CommandList> commandList, DeviceBuffer indirectBuffer, UInt32 offset)
        {
            commandList.DispatchIndirect(indirectBuffer, offset);
            return commandList;
        }
        public static FluentWrapper<CommandList> ResolveTexture(this FluentWrapper<CommandList> commandList, Texture source, Texture destination)
        {
            commandList.ResolveTexture(source, destination);
            return commandList;
        }
        public static FluentWrapper<CommandList> UpdateBuffer<T>(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, UInt32 bufferOffsetInBytes, T source) where T : struct
        {
            commandList.UpdateBuffer(buffer, bufferOffsetInBytes, source);
            return commandList;
        }
        public static FluentWrapper<CommandList> UpdateBuffer<T>(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, UInt32 bufferOffsetInBytes, ref T source) where T : struct
        {
            commandList.UpdateBuffer(buffer, bufferOffsetInBytes, ref source);
            return commandList;
        }
        public static FluentWrapper<CommandList> UpdateBuffer<T>(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, UInt32 bufferOffsetInBytes, ref T source, UInt32 sizeInBytes) where T : struct
        {
            commandList.UpdateBuffer(buffer, bufferOffsetInBytes, ref source, sizeInBytes);
            return commandList;
        }
        public static FluentWrapper<CommandList> UpdateBuffer<T>(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, UInt32 bufferOffsetInBytes, T[] source) where T : struct
        {
            commandList.UpdateBuffer(buffer, bufferOffsetInBytes, source);
            return commandList;
        }
        public static FluentWrapper<CommandList> UpdateBuffer(this FluentWrapper<CommandList> commandList, DeviceBuffer buffer, UInt32 bufferOffsetInBytes, IntPtr source, UInt32 sizeInBytes)
        {
            commandList.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
            return commandList;
        }
        public static FluentWrapper<CommandList> CopyBuffer(this FluentWrapper<CommandList> commandList, DeviceBuffer source, UInt32 sourceOffset, DeviceBuffer destination, UInt32 destinationOffset, UInt32 sizeInBytes)
        {
            commandList.CopyBuffer(source, sourceOffset, destination, destinationOffset, sizeInBytes);
            return commandList;
        }
        public static FluentWrapper<CommandList> CopyTexture(this FluentWrapper<CommandList> commandList, Texture source, Texture destination)
        {
            commandList.CopyTexture(source, destination);
            return commandList;
        }
        public static FluentWrapper<CommandList> CopyTexture(this FluentWrapper<CommandList> commandList, Texture source, Texture destination, UInt32 mipLevel, UInt32 arrayLayer)
        {
            commandList.CopyTexture(source, destination, mipLevel, arrayLayer);
            return commandList;
        }
        public static FluentWrapper<CommandList> CopyTexture(this FluentWrapper<CommandList> commandList, Texture source, UInt32 srcX, UInt32 srcY, UInt32 srcZ, UInt32 srcMipLevel, UInt32 srcBaseArrayLayer, Texture destination, UInt32 dstX, UInt32 dstY, UInt32 dstZ, UInt32 dstMipLevel, UInt32 dstBaseArrayLayer, UInt32 width, UInt32 height, UInt32 depth, UInt32 layerCount)
        {
            commandList.CopyTexture(source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer, destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer, width, height, depth, layerCount);
            return commandList;
        }
        public static FluentWrapper<CommandList> GenerateMipmaps(this FluentWrapper<CommandList> commandList, Texture texture)
        {
            commandList.GenerateMipmaps(texture);
            return commandList;
        }
        public static FluentWrapper<CommandList> PushDebugGroup(this FluentWrapper<CommandList> commandList, String name)
        {
            commandList.PushDebugGroup(name);
            return commandList;
        }
        public static FluentWrapper<CommandList> PopDebugGroup(this FluentWrapper<CommandList> commandList)
        {
            commandList.PopDebugGroup();
            return commandList;
        }
        public static FluentWrapper<CommandList> InsertDebugMarker(this FluentWrapper<CommandList> commandList, String name)
        {
            commandList.InsertDebugMarker(name);
            return commandList;
        }

        internal static FluentWrapper<CommandList> SetGraphicsResourceSets(this FluentWrapper<CommandList> commandList, IEnumerable<KeyValuePair<uint, ResourceSet>> resourceSetsBySlot)
        {
            foreach (var kvp in resourceSetsBySlot)
                commandList.SetGraphicsResourceSet(kvp.Key, kvp.Value);
            return commandList;
        }
    }
}
