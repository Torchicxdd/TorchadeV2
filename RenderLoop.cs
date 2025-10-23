using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorchadeV2
{
    class RenderLoop : IDisposable
    {
        private Thread _renderThread;
        private volatile bool _isRunning;
        private readonly Action _renderCallback;

        public bool IsRunning => _isRunning;

        public RenderLoop(Action renderCallback)
        {
            _renderCallback = renderCallback ?? throw new ArgumentNullException(nameof(renderCallback));
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _renderThread = new Thread(Loop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal,
                Name = "RenderThread"
            };
            _renderThread.Start();
        }

        private void Loop()
        {
            while (_isRunning)
            {
                try
                {
                    _renderCallback();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Render loop error: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _renderThread.Join(1000);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
