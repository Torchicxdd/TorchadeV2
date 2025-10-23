using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Graphics.Capture;
using WinRT.Interop;

namespace TorchadeV2
{
    class ScreenCapture
    {
        public bool OnInitialization()
        {
            if (!GraphicsCaptureSession.IsSupported())
            {
                return false;
            }
            return true;
        }

        public async Task StartCaptureAsync(Window window)
        {
            var picker = new GraphicsCapturePicker();

            InitializeWithWindow.Initialize(picker, new System.Windows.Interop.WindowInteropHelper(window).Handle);

            GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            if (item != null)
            {
                //StartCaptureInternal(item);
            }
        }
    }
}
