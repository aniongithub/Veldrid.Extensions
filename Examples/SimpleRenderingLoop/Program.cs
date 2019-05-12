using Veldrid;
using Veldrid.Extensions.Fluent;
using Veldrid.Extensions.SDL2;
using Veldrid.StartupUtilities;

namespace SimpleRenderingLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            var windowCreateInfo = new WindowCreateInfo
            {
                WindowInitialState = WindowState.Maximized
            };
            var window = VeldridStartup.CreateWindow(ref windowCreateInfo);

            using (var device = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGL))
                window.Render(device,
                    (dev, cl, elapsed) => dev.SubmitCommands(
                        cl.BeginFluent()
                        .SetFramebuffer(dev.SwapchainFramebuffer)
                        .ClearColorTarget(0, RgbaFloat.CornflowerBlue)
                        .End()));
        }
    }
}