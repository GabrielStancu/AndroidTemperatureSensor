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
        private const int BufferLength = 20;
        private const string Socket = "00001101-0000-1000-8000-00805f9b34fb";
        private const string DeviceName = "HC-05";
        private const string ReceivedDataPackageStart = "T";
        private const string SentDataPackageStart = "S";
        private const int PackageEnd = 13;
        private const int PackageLength = 5;
        private const int ShortPackageLength = 3;
        private const int NoDataReceivedCode = -1;
        public const int CommunicationDelay = 1000;

        public async Task<BluetoothStatus> ConnectToBluetooth()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                return BluetoothStatus.NO_ADAPTER;

            if (!adapter.IsEnabled)
                return BluetoothStatus.NO_CONNECTION;

            BluetoothDevice device = adapter.BondedDevices.Where(bd => bd.Name.Equals(DeviceName)).FirstOrDefault();

            if (device == null)
                return BluetoothStatus.NO_DEVICE;

            try
            {
                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(Socket));
                await _socket.ConnectAsync();
            }
            catch (Exception)
            {
                return BluetoothStatus.NO_DEVICE;
            }

            _buffer = new byte[BufferLength];

            return BluetoothStatus.NO_ERROR;
        }  

        public async Task<int> ReadCurrentTemp()
        {
            try
            {
                await _socket.InputStream.ReadAsync(_buffer, 0, _buffer.Length);
                int packageStart = GetPackageStartByteIndex();

                if (packageStart == NoDataReceivedCode)
                {
                    throw new Exception("No data received.");
                }

                return ComputeCurrentTemp(packageStart);
            }
            catch(Exception)
            {
                return -1;
            }        
        }

        private int GetPackageStartByteIndex()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                if (_buffer[i] == Encoding.ASCII.GetBytes(ReceivedDataPackageStart)[0])
                {
                    return ++i;
                }
            }

            return NoDataReceivedCode;
        }

        private int ComputeCurrentTemp(int packageStart)
        {
            int temp = 0;
            for (int i = packageStart; i < packageStart + ShortPackageLength; i++)
            {
                temp = temp * 10 + Int32.Parse(((char)_buffer[i]).ToString());
            }

            return temp;
        }

        public async Task SendDesiredTemp(int temp)
        {
            byte[] sendBuffer = new byte[PackageLength];
          
            sendBuffer[0] = Encoding.ASCII.GetBytes(SentDataPackageStart)[0];
            sendBuffer[1] = Encoding.ASCII.GetBytes((temp / 100).ToString())[0];
            sendBuffer[2] = Encoding.ASCII.GetBytes((temp / 10 % 10).ToString())[0];
            sendBuffer[3] = Encoding.ASCII.GetBytes((temp % 10).ToString())[0];
            sendBuffer[4] = PackageEnd;

            await _socket.OutputStream.WriteAsync(sendBuffer, 0, sendBuffer.Length);
            await Task.Delay(CommunicationDelay);
        }
    }
}