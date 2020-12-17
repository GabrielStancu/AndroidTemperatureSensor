using System;
using System.ComponentModel;
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
using Java.Lang;
using Java.Util;

namespace TemperatureSetterAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView _crtTempTextView;
        private TextView _desiredTempTextView;
        private Button _increaseTempButton;
        private Button _decreaseTempButton;
        private BluetoothModule _bluetoothModule;
        private bool _initialized = false;
        private double _temperature = 0.0f;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            GetViewsIds();
            AddEventHandlers();
            await EstablishBluetoothCommunication();
        }

        private void GetViewsIds()
        {
            _increaseTempButton = FindViewById<Button>(Resource.Id.IncreaseTempButton);
            _decreaseTempButton = FindViewById<Button>(Resource.Id.DecreaseTempButton);
            _crtTempTextView = FindViewById<TextView>(Resource.Id.CrtTemperatureTextView);
            _desiredTempTextView = FindViewById<TextView>(Resource.Id.DesiredTemperatureTextView);
        }

        private void AddEventHandlers()
        {
            _increaseTempButton.Click += async (sender, e) =>
            {
                double crtTemp = double.Parse(_desiredTempTextView.Text.Substring(0, _desiredTempTextView.Text.Length - 2));
                crtTemp += 0.1;
                _desiredTempTextView.Text = $"{crtTemp} °C";
                await _bluetoothModule.SendDesiredTemp();
            };

            _decreaseTempButton.Click += async (sender, e) =>
            {
                double crtTemp = double.Parse(_desiredTempTextView.Text.Substring(0, _desiredTempTextView.Text.Length - 2));
                crtTemp -= 0.1;
                _desiredTempTextView.Text = $"{crtTemp} °C";
                await _bluetoothModule.SendDesiredTemp();
            };
        }

        private async Task EstablishBluetoothCommunication()
        {
            _bluetoothModule = new BluetoothModule();
            await _bluetoothModule.ConnectToBluetooth();

            while (true)
            {
                _temperature = 1.0 * (await _bluetoothModule.ReadCurrentTemp()) / 10;
                await Task.Delay(500);
                DisplayCurrentTemperature();
            }        
        }

        private void DisplayCurrentTemperature()
        {
            _crtTempTextView.Text = $"{_temperature}";

            if (!_initialized)
            {
                _initialized = true;
                _desiredTempTextView.Text = _crtTempTextView.Text;
            }
        }
    }
}
