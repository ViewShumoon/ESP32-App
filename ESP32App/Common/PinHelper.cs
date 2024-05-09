using System.Device.Gpio;
using System.Threading;

namespace ESP32App.Common
{
    public static partial class PinHelper
    {
        public static void Blink(this GpioPin pin, int milliseconds = 100)
        {
            pin.Toggle();
            Thread.Sleep(milliseconds);
            pin.Toggle();
        }
    }
}
