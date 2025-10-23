using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace TorchadeV2
{
    class Renderer : IDisposable
    {
        private ID3D11Device _device;
        private ID3D11DeviceContext _context;
        private IDXGISwapChain1 _swapChain;
        private ID3D11RenderTargetView _renderTargetView;
        private ID3D11Texture2D _backBuffer;

        public ID3D11Device Device => _device;
        public ID3D11DeviceContext Context => _context;

        private bool isInitialized = false;
        private int a = 0;

        public void InitializeDirectX(IntPtr hwnd, int width, int height)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new ArgumentException($"Invalid window handle {nameof(hwnd)}");
            }

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
                Width = (uint)width,
                Height = (uint)height,
                Format = Format.B8G8R8A8_UNorm,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = new SampleDescription(1, 0),
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore
            };

            _swapChain = factory.CreateSwapChainForHwnd(_device, hwnd, swapChainDesc);

            factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);

            CreateRenderTargetView();
            SetViewport(width, height);

            isInitialized = true;
        }

        public void CreateRenderTargetView()
        {
            _backBuffer?.Dispose();
            _renderTargetView?.Dispose();

            _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _renderTargetView = _device.CreateRenderTargetView(_backBuffer);
        }

        public void Resize(int width, int height)
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
                (uint)width,
                (uint)height,
                Format.B8G8R8A8_UNorm,
                SwapChainFlags.None);

            CreateRenderTargetView();
            SetViewport(width, height);
        }

        public void Render()
        {
            if (!isInitialized || _context == null)
            {
                return;
            }

            try
            {
                _context.OMSetRenderTargets(_renderTargetView, null);
                _context.ClearRenderTargetView(_renderTargetView, new Color4(0.0f, 0.0f, 0.0f, 1.0f));

                RenderScene();

                _swapChain.Present(1, PresentFlags.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
            }
        }

        private void SetViewport(int width, int height)
        {
            var viewport = new Viewport(0, 0, (float)width, (float)height, 0.0f, 1.0f);
            _context.RSSetViewport(viewport);
        }

        private void RenderScene()
        {
            // DO COOL CODE HERE
        }

        public void Dispose()
        {
            isInitialized = false;

            _renderTargetView?.Dispose();
            _backBuffer?.Dispose();
            _swapChain?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
        }
    }
}
