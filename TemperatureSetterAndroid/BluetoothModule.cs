using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace TemperatureSetterAndroid
{
    internal class BluetoothModule
    {
        private BluetoothSocket _socket;
        private byte[] _buffer;

        public async Task ConnectToBluetooth()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new Exception("Bluetooth adapter is not enabled.");

            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "HC-05"
                                      select bd).FirstOrDefault();

            if (device == null)
                throw new Exception("Named device not found.");

            _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
            await _socket.ConnectAsync();

            _buffer = new byte[20];
        }  

        public async Task<int> ReadCurrentTemp()
        {
            await _socket.InputStream.ReadAsync(_buffer, 0, _buffer.Length);
            int packageStart = GetPackageStartByteIndex();

            if(packageStart == -1)
            {
                throw new Exception("No data received.");
            }

            return ComputeCurrentTemp(packageStart);
        }

        private int GetPackageStartByteIndex()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                if (_buffer[i] == Encoding.ASCII.GetBytes("T")[0])
                {
                    return (i + 1);
                }
            }

            return -1;
        }

        private int ComputeCurrentTemp(int packageStart)
        {
            int temp = 0;
            for (int i = packageStart; i < packageStart + 3; i++)
            {
                temp = temp * 10 + Int32.Parse(((char)_buffer[i]).ToString());
            }

            return temp;
        }

        public async Task SendDesiredTemp()
        {
            //await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await Task.Delay(10);
        }
    }
}