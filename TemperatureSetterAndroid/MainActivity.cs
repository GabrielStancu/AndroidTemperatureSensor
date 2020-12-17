using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace TemperatureSetterAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView _crtTempTextView;
        private TextView _desiredTempTextView;
        private Button _increaseTempButton0;
        private Button _increaseTempButton1;
        private Button _increaseTempButton5;
        private Button _decreaseTempButton0;
        private Button _decreaseTempButton1;
        private Button _decreaseTempButton5;
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
            _increaseTempButton0 = FindViewById<Button>(Resource.Id.IncreaseTempButton0);
            _increaseTempButton1 = FindViewById<Button>(Resource.Id.IncreaseTempButton1);
            _increaseTempButton5 = FindViewById<Button>(Resource.Id.IncreaseTempButton5);

            _decreaseTempButton0 = FindViewById<Button>(Resource.Id.DecreaseTempButton0);
            _decreaseTempButton1 = FindViewById<Button>(Resource.Id.DecreaseTempButton1);
            _decreaseTempButton5 = FindViewById<Button>(Resource.Id.DecreaseTempButton5);

            _crtTempTextView = FindViewById<TextView>(Resource.Id.CrtTemperatureTextView);
            _desiredTempTextView = FindViewById<TextView>(Resource.Id.DesiredTemperatureTextView);
        }

        private void AddEventHandlers()
        {
            _increaseTempButton0.Click += (sender, e) =>
            {
                IncreaseTemperature(0.1);
            };

            _increaseTempButton1.Click += (sender, e) =>
            {
                IncreaseTemperature(1.0);
            };

            _increaseTempButton5.Click += (sender, e) =>
            {
                IncreaseTemperature(5.0);
            };

            _decreaseTempButton0.Click += (sender, e) =>
            {
                DecreaseTemperature(0.1);
            };

            _decreaseTempButton1.Click += (sender, e) =>
            {
                DecreaseTemperature(1.0);
            };

            _decreaseTempButton5.Click += (sender, e) =>
            {
                DecreaseTemperature(5.0);
            };
        }

        private async void IncreaseTemperature(double increase)
        {
            double crtTemp = double.Parse(_desiredTempTextView.Text.Substring(0, _desiredTempTextView.Text.Length - 2));
            crtTemp += increase;
            _desiredTempTextView.Text = $"{crtTemp} °C";
            await _bluetoothModule.SendDesiredTemp((int)(crtTemp * 10));
        }

        private async void DecreaseTemperature(double increase)
        {
            double crtTemp = double.Parse(_desiredTempTextView.Text.Substring(0, _desiredTempTextView.Text.Length - 2));
            crtTemp -= increase;
            _desiredTempTextView.Text = $"{crtTemp} °C";
            await _bluetoothModule.SendDesiredTemp((int)(crtTemp * 10));
        }

        private async Task EstablishBluetoothCommunication()
        {
            _bluetoothModule = new BluetoothModule();
            await _bluetoothModule.ConnectToBluetooth();

            while (true)
            {
                _temperature = 1.0 * (await _bluetoothModule.ReadCurrentTemp()) / 10;
                await Task.Delay(2000);
                DisplayCurrentTemperature();
            }        
        }

        private void DisplayCurrentTemperature()
        {
            _crtTempTextView.Text = $"{_temperature} °C";

            if (!_initialized)
            {
                _initialized = true;
                _desiredTempTextView.Text = _crtTempTextView.Text;
            }
        }
    }
}
