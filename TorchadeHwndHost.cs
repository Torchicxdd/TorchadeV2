using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace TorchadeV2
{
    class TorchadeHwndHost : HwndHost
    {
        private IntPtr _hwndHost;
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
        private const int HOST_ID = 0x00000002;

        private Renderer _renderer;
        private RenderLoop _renderLoop;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hwndHost = CreateWindowEx(
                0,
                "static",
                "",
                WS_CHILD | WS_VISIBLE,
                0, 0,
                (int)Width, (int)Height,
                hwndParent.Handle,
                (IntPtr)HOST_ID,
                IntPtr.Zero,
                IntPtr.Zero);

            _renderer = new Renderer();

            try
            {
                _renderer.InitializeDirectX(_hwndHost, (int)Width, (int)Height);

                _renderLoop = new RenderLoop(_renderer.Render);
                _renderLoop.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectX initialization failed: {ex.Message}");
                _renderer?.Dispose();
                _renderer = null;
            }

            return new HandleRef(this, _hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _renderLoop?.Dispose();
            _renderer?.Dispose();
            DestroyWindow(_hwndHost);
        }

        protected override nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            const int WM_SIZE = 0x0005;

            if (msg == WM_SIZE && _renderer != null)
            {
                _renderer.Resize((int)Width, (int)Height);
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        #region Win32 API Imports

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            IntPtr lpParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        #endregion
    }
}
