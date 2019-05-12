using System;
using System.Diagnostics;
using Veldrid.Sdl2;

namespace Veldrid.Extensions.SDL2
{
    public static class WindowExtensions
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
    }
}