using ESP32App.Common;
using System;
using System.Device.Gpio;
using System.Device.Wifi;
using System.Diagnostics;
using System.Threading;

namespace ESP32App
{
    public class Program
    {
        private static GpioController s_GpioController;

        public static void Main()
        {
            //WiFiHelper.Connect();

            s_GpioController = new GpioController();
            GpioPin led = s_GpioController.OpenPin(2, PinMode.Output);
            led.Write(PinValue.Low);

            BlinkLED(led);
        }


        private static void BlinkLED(GpioPin led)
        {
            var random = new Random();
            while (true)
            {
                led.Blink();

                var interval = random.Next(800);
                Thread.Sleep(interval);
            }
        }

    }
}
