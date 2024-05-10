using ESP32App.Services;

namespace ESP32App
{
    public class Program
    {
        

        private static void Initialize()
        {
            ApplePopupsService.Initialize();



        }

        public static void Main()
        {
            Initialize();

            ApplePopupsService.Popups();


        }
    }
}
