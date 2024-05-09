using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using System;
using System.Diagnostics;
using System.Threading;

namespace ESP32App.Tests
{
    public class BluetoothTest
    {
        private static BluetoothLEServer server;

        private static GattLocalCharacteristic _readCharacteristic;
        private static GattLocalCharacteristic _readWriteCharacteristic;

        // Read/Write Characteristic value
        static byte _redValue = 128;
        static byte _greenValue = 128;
        static byte _blueValue = 128;

        public static void Test()
        {
            server = BluetoothLEServer.Instance;

            server.DeviceName = "nanoDevice";

            // Define some custom Uuids
            Guid serviceUuid = new Guid("6A67D5BC-1F87-11EF-BC15-AFD3B90CADFA");
            Guid readStaticCharUuid = new Guid("6A67D5BC-1F89-11EF-BC15-AFD3B90CADFA");
            Guid readWriteCharUuid = new Guid("6A67D5BC-1F8A-11EF-BC15-AFD3B90CADFA");

            //The GattServiceProvider is used to create and advertise the primary service definition.
            //An extra device information service will be automatically created.
            GattServiceProviderResult result = GattServiceProvider.Create(serviceUuid);
            if (result.Error != BluetoothError.Success)
            {
                return;
            }

            GattServiceProvider serviceProvider = result.ServiceProvider;

            // Get created Primary service from provider
            GattLocalService service = serviceProvider.Service;


            #region Static read characteristic
            // Now we add an characteristic to service
            // If the read value is not going to change then you can just use a Static value
            DataWriter sw = new DataWriter();
            sw.WriteString("Hello, World!");

            GattLocalCharacteristicResult characteristicResult = service.CreateCharacteristic(readStaticCharUuid,
                 new GattLocalCharacteristicParameters()
                 {
                     CharacteristicProperties = GattCharacteristicProperties.Read,
                     UserDescription = "Hello, World!",
                     StaticValue = sw.DetachBuffer()
                 });
            ;

            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return;
            }
            #endregion

            #region Create Characteristic for RGB read/write
            characteristicResult = service.CreateCharacteristic(readWriteCharUuid,
                new GattLocalCharacteristicParameters()
                {
                    CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Write,
                    UserDescription = "Read/Write Characteristic"
                });

            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return;
            }

            // Get reference to our read Characteristic  
            _readWriteCharacteristic = characteristicResult.Characteristic;

            // Every time the value is requested from a client this event is called 
            _readWriteCharacteristic.WriteRequested += _readWriteCharacteristic_WriteRequested;
            _readWriteCharacteristic.ReadRequested += _readWriteCharacteristic_ReadRequested;

            #endregion

            #region Start Advertising
            // Once all the Characteristics have been created you need to advertise the Service so 
            // other devices can see it. Here we also say the device can be connected too and other
            // devices can see it. 
            serviceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters()
            {
                IsConnectable = true,
                IsDiscoverable = true
            });
            #endregion 
        }

        /// <summary>
        /// Timer callback for notifying any connected clients who have subscribed to this 
        /// characteristic of changed value.
        /// </summary>
        /// <param name="state">Not used</param>
        private static void NotifyCallBack(object state)
        {
            if (_readCharacteristic.SubscribedClients.Length > 0)
            {
                _readCharacteristic.NotifyValue(GetTimeBuffer());
            }
        }

        /// <summary>
        /// Event handler for Read characteristic.
        /// </summary>
        /// <param name="sender">GattLocalCharacteristic object</param>
        /// <param name="ReadRequestEventArgs"></param>
        private static void ReadCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs ReadRequestEventArgs)
        {
            GattReadRequest request = ReadRequestEventArgs.GetRequest();

            // Get Buffer with hour/minute/second
            request.RespondWithValue(GetTimeBuffer());
        }

        // Separate out the Creation of Time buffer so it can be used by read characteristic and Notify
        private static Buffer GetTimeBuffer()
        {
            // Create DataWriter and write the data into buffer
            // Write Hour/minute/second of current time
            DateTime dt = DateTime.UtcNow;

            // Write data in a Buffer object using DataWriter
            DataWriter dw = new DataWriter();
            dw.WriteByte((Byte)dt.Hour);
            dw.WriteByte((Byte)dt.Minute);
            dw.WriteByte((Byte)dt.Second);

            // Detach Buffer object from DataWriter
            return dw.DetachBuffer();
        }

        /// <summary>
        /// Read event handler for Read/Write characteristic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ReadRequestEventArgs"></param>
        private static void _readWriteCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs ReadRequestEventArgs)
        {
            GattReadRequest request = ReadRequestEventArgs.GetRequest();

            DataWriter dw = new DataWriter();
            dw.WriteByte((Byte)_redValue);
            dw.WriteByte((Byte)_greenValue);
            dw.WriteByte((Byte)_blueValue);

            request.RespondWithValue(dw.DetachBuffer());

            Debug.WriteLine($"RGB read");
        }

        /// <summary>
        /// Write handler for  Read/Write characteristic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="WriteRequestEventArgs"></param>
        private static void _readWriteCharacteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs WriteRequestEventArgs)
        {
            GattWriteRequest request = WriteRequestEventArgs.GetRequest();

            // Check expected data length, we are expecting 3 bytes
            if (request.Value.Length != 3)
            {
                request.RespondWithProtocolError((byte)BluetoothError.NotSupported);
                return;
            }

            // Unpack data from buffer
            DataReader rdr = DataReader.FromBuffer(request.Value);
            _redValue = rdr.ReadByte();
            _greenValue = rdr.ReadByte();
            _blueValue = rdr.ReadByte();

            // Respond if Write requires response
            if (request.Option == GattWriteOption.WriteWithResponse)
            {
                request.Respond();
            }

            // Print new values, better to set a RGB led
            Debug.WriteLine($"Received RGB={_redValue}/{_greenValue}/{_blueValue}");
        }

    }
}
