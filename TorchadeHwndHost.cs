using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace TorchadeV2
{
    class TorchadeHwndHost : HwndHost
    {
        private IntPtr _hwndHost;
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
        private const int HOST_ID = 0x00000002;

        private ID3D11Device _device;
        private ID3D11DeviceContext _context;
        private IDXGISwapChain1 _swapChain;
        private ID3D11RenderTargetView _renderTargetView;
        private ID3D11Texture2D _backBuffer;

        public ID3D11Device Device => _device;
        public ID3D11DeviceContext Context => _context;

        private bool isInitialized = false;

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

            InitializeDirectX();

            return new HandleRef(this, _hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            CleanUpDirectX();
            DestroyWindow(_hwndHost);
        }

        protected override nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            const int WM_SIZE = 0x0005;
            const int WM_PAINT = 0x000F;

            switch (msg)
            {
                case WM_SIZE:
                    OnResize();
                    break;
                case WM_PAINT:
                    Render();
                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        private void InitializeDirectX()
        {
            if (_hwndHost == IntPtr.Zero)
            {
                return;
            }

            try
            {
                var featureLevels = new[]
                {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
                    FeatureLevel.Level_10_0,
                };

                D3D11.D3D11CreateDevice(
                    null,
                    DriverType.Hardware,
                    DeviceCreationFlags.BgraSupport,
                    featureLevels,
                    out _device,
                    out var selectedFeatureLevel,
                    out _context);

                using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
                using var adapter = dxgiDevice.GetAdapter();
                using var factory = adapter.GetParent<IDXGIFactory2>();

                SwapChainDescription1 swapChainDesc = new SwapChainDescription1
                {
                    Width = (uint)Width,
                    Height = (uint)Height,
                    Format = Format.B8G8R8A8_UNorm,
                    BufferCount = 2,
                    BufferUsage = Usage.RenderTargetOutput,
                    SampleDescription = new SampleDescription(1, 0),
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.FlipDiscard,
                    AlphaMode = AlphaMode.Ignore
                };

                _swapChain = factory.CreateSwapChainForHwnd(_device, _hwndHost, swapChainDesc);

                factory.MakeWindowAssociation(_hwndHost, WindowAssociationFlags.IgnoreAltEnter);

                CreateRenderTargetView();
                SetViewport();

                isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectX initialization failed: {ex.Message}");
                CleanUpDirectX();
            }
        }

        private void CreateRenderTargetView()
        {
            _backBuffer?.Dispose();
            _renderTargetView?.Dispose();

            _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _renderTargetView = _device.CreateRenderTargetView(_backBuffer);
        }

        private void SetViewport()
        {
            var viewport = new Viewport(0, 0, (float)Width, (float)Height, 0.0f, 1.0f);
            _context.RSSetViewport(viewport);
        }

        private void OnResize()
        {
            if (!isInitialized || _swapChain == null)
            {
                return;
            }

            // Release references to swap chain buffers
            _context.ClearState();
            _renderTargetView?.Dispose();
            _backBuffer?.Dispose();

            _swapChain.ResizeBuffers(
                2,
                (uint)Width,
                (uint)Height,
                Format.B8G8R8A8_UNorm,
                SwapChainFlags.None);

            CreateRenderTargetView();
            SetViewport();
        }

        private void Render()
        {
            if (!isInitialized || _context == null)
            {
                return;
            }

            try
            {
                _context.OMSetRenderTargets(_renderTargetView, null);
                _context.ClearRenderTargetView(_renderTargetView, new Color4(1.0f, 0.0f, 0.0f, 1.0f));

                // CODE

                _swapChain.Present(1, PresentFlags.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
            }
        }

        private void CleanUpDirectX()
        {
            isInitialized = false;

            _renderTargetView?.Dispose();
            _backBuffer?.Dispose();
            _swapChain?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
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
