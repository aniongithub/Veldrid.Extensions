using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.Extensions
{
    public static class MemoryExtensions
    {
        public static IDisposable Pin<T>(this T[] data)
            where T: struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return new DisposableWrapper<IntPtr>(handle.AddrOfPinnedObject(), ptr => handle.Free());
        }
        public static IDisposable Pin<T>(this T[] data, out IntPtr ptr)
            where T: struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            ptr = handle.AddrOfPinnedObject();
            return new DisposableWrapper<IntPtr>(handle.AddrOfPinnedObject(), p => handle.Free());
        }

        public static DisposableWrapper<IntPtr> Pin<T>(this T data)
            where T : struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return new DisposableWrapper<IntPtr>(handle.AddrOfPinnedObject(), ptr => handle.Free());
        }

        public static DisposableWrapper<IntPtr> Pin<T>(this T data, out IntPtr ptr)
            where T: struct
        {
            var result = data.Pin();
            ptr = result.Value;
            return result;
        }

        unsafe public static IDisposable Pin<T>(this Memory<T> memory, out IntPtr ptr)
            where T: struct
        {
            var result = memory.Pin();
            ptr = new IntPtr(result.Pointer);
            return result;
        }
    }
}