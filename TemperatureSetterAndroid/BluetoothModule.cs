using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
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
                    return ++i;
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

        public async Task SendDesiredTemp(int temp)
        {
            byte[] sendBuffer = new byte[5];
            sendBuffer[0] = Encoding.ASCII.GetBytes("S")[0];
            sendBuffer[1] = Encoding.ASCII.GetBytes((temp / 100).ToString())[0];
            sendBuffer[2] = Encoding.ASCII.GetBytes((temp / 10 % 10).ToString())[0];
            sendBuffer[3] = Encoding.ASCII.GetBytes((temp % 10).ToString())[0];
            sendBuffer[4] = 13;

            await _socket.OutputStream.WriteAsync(sendBuffer, 0, sendBuffer.Length);
            await Task.Delay(500);
        }
    }
}