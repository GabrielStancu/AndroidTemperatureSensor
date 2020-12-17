using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace TemperatureSetterAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TextView crtTempTextView;
        TextView desiredTempTextView;
        private BluetoothSocket _socket;
        private byte[] buffer;
        private bool _initialized = false;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Button increaseTempButton = FindViewById<Button>(Resource.Id.IncreaseTempButton);
            Button decreaseTempButton = FindViewById<Button>(Resource.Id.DecreaseTempButton);
            crtTempTextView = FindViewById<TextView>(Resource.Id.CrtTemperatureTextView);
            desiredTempTextView = FindViewById<TextView>(Resource.Id.DesiredTemperatureTextView);

            increaseTempButton.Click += async (sender, e) =>
            {
                double crtTemp = double.Parse(desiredTempTextView.Text.Substring(0, desiredTempTextView.Text.Length - 2));
                crtTemp += 0.1;
                desiredTempTextView.Text = $"{crtTemp} °C";
                await SendDesiredTemp();
            };

            decreaseTempButton.Click += async (sender, e) =>
            {
                double crtTemp = double.Parse(desiredTempTextView.Text.Substring(0, desiredTempTextView.Text.Length - 2));
                crtTemp -= 0.1;
                desiredTempTextView.Text = $"{crtTemp} °C";
                await SendDesiredTemp();
            };

            buffer = new byte[20];
            await ConnectToBluetooth();
            await DisplayCurrentTemp();
        }

        private async Task ConnectToBluetooth()
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
        }

        private async Task DisplayCurrentTemp()
        {
            while (await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length) != 0)
            {
                int packageStart = 0;

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == Encoding.ASCII.GetBytes("T")[0])
                    {
                        packageStart = i + 1;
                        break;
                    }
                }

                int temp = 0;
                for (int i = packageStart; i < packageStart + 3; i++)
                {
                    temp = temp * 10 + Int32.Parse(((char)buffer[i]).ToString());
                }

                crtTempTextView.Text = $"{temp / 10}.{temp % 10}";

                if(!_initialized)
                {
                    _initialized = true;
                    desiredTempTextView.Text = crtTempTextView.Text;
                }

                await Task.Delay(2000);
            }
        }

        private async Task SendDesiredTemp()
        {
            //await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await Task.Delay(10);
        }
    }
}
