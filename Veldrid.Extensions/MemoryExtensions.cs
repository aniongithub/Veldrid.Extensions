using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.Extensions
{
    public static class MemoryExtensions
    {
        public static DisposableWrapper<IntPtr> Pin<T>(this T[] data)
            where T: struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return new DisposableWrapper<IntPtr>(handle.AddrOfPinnedObject(), ptr => handle.Free());
        }
        public static DisposableWrapper<IntPtr> Pin<T>(this T data)
            where T : struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return new DisposableWrapper<IntPtr>(handle.AddrOfPinnedObject(), ptr => handle.Free());
        }
    }
}