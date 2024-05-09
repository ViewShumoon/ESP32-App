using ESP32App.Common;
using ESP32App.Tests;
using System;
using System.Collections;
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
            Initialize();

            BluetoothTest.Test();
            //WiFiHelper.Connect();

            GpioPin led = s_GpioController.OpenPin(2, PinMode.Output);
            led.Write(PinValue.Low);

            RandomBlinkLED(led);
        }

        private static void Initialize()
        {
            s_GpioController = new GpioController();

        }


        private static void RandomBlinkLED(GpioPin led)
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
