﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Views;

namespace TemperatureSetterAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView _crtTempTextView;
        private TextView _desiredTempTextView;
        private TextView _crtTempLabel;
        private TextView _desiredTempLabel;
        private Button _increaseTempButton0;
        private Button _increaseTempButton1;
        private Button _increaseTempButton5;
        private Button _decreaseTempButton0;
        private Button _decreaseTempButton1;
        private Button _decreaseTempButton5;
        private BluetoothModule _bluetoothModule;
        private bool _initialized = false;
        private double _temperature = 0.0f;
        private BluetoothStatus _bluetoothStatus;
        private readonly string _temperatureDisplayFormat = "{0} °C";
        private readonly double[] _temperatureVariations = new double[] { 0.1, 1.0, 5.0 };
        private readonly int decimalPlaces = 1;
        private readonly double minTemp = 15.0, maxTemp = 35.0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            GetViewsIds();
            AddEventHandlers();
            HideControls(ViewStates.Invisible);
            EstablishBluetoothCommunication();
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

            _crtTempLabel = FindViewById<TextView>(Resource.Id.CrtTemperatureLabel);
            _desiredTempLabel = FindViewById<TextView>(Resource.Id.DesiredTemperatureLabel);
        }

        private void AddEventHandlers()
        {
            _increaseTempButton0.Click += (sender, e) =>
            {
                SetTemperature(_temperatureVariations[0]);
            };

            _increaseTempButton1.Click += (sender, e) =>
            {
                SetTemperature(_temperatureVariations[1]);
            };

            _increaseTempButton5.Click += (sender, e) =>
            {
                SetTemperature(_temperatureVariations[2]);
            };

            _decreaseTempButton0.Click += (sender, e) =>
            {
                SetTemperature(-_temperatureVariations[0]);
            };

            _decreaseTempButton1.Click += (sender, e) =>
            {
                SetTemperature(-_temperatureVariations[1]);
            };

            _decreaseTempButton5.Click += (sender, e) =>
            {
                SetTemperature(-_temperatureVariations[2]);
            };
        }

        private async void SetTemperature(double increase)
        {
            double crtTemp = double.Parse(_desiredTempTextView.Text.Substring(0, _desiredTempTextView.Text.Length - 2));
            crtTemp += increase;
            crtTemp = Math.Round(crtTemp, decimalPlaces);

            if (crtTemp < minTemp)
            {
                crtTemp = minTemp;
            }
            else if (crtTemp > maxTemp)
            {
                crtTemp = maxTemp;
            }

            _desiredTempTextView.Text = string.Format(_temperatureDisplayFormat, crtTemp);
            await _bluetoothModule.SendDesiredTemp((int)(crtTemp * 10));
        }

        private async void EstablishBluetoothCommunication()
        {
            _bluetoothModule = new BluetoothModule();
            _bluetoothStatus = await _bluetoothModule.ConnectToBluetooth();

            while (true)
            {
                int readTemp = await _bluetoothModule.ReadCurrentTemp();
                await Task.Delay(BluetoothModule.CommunicationDelay);
                if (readTemp < 0)
                {                   
                    DisplayCurrentStatus(true);
                    break;
                }
                else
                {
                    _temperature = Math.Round((double)readTemp / 10, decimalPlaces);
                    DisplayCurrentStatus(false);
                }
            }
        }

        private async void DisplayCurrentStatus(bool errorEncountered)
        {
            if(errorEncountered)
            {
                SetErrorMessage();
                HideControls(ViewStates.Invisible);
                await DisplayErrorSource();
                CloseApp();
            }
            else
            {
                if (!_initialized)
                {
                    HideControls(ViewStates.Visible);
                }
                DisplayCurrentTemperature();
            } 
        }

        private void DisplayCurrentTemperature()
        {
            _crtTempTextView.Text = string.Format(_temperatureDisplayFormat, _temperature);

            if (!_initialized)
            {
                _initialized = true;
                _desiredTempTextView.Text = _crtTempTextView.Text;
            }
        }

        private void HideControls(ViewStates viewState)
        {
            _desiredTempTextView.Visibility = viewState;
            _decreaseTempButton0.Visibility = viewState;
            _decreaseTempButton1.Visibility = viewState;
            _decreaseTempButton5.Visibility = viewState;
            _increaseTempButton0.Visibility = viewState;
            _increaseTempButton1.Visibility = viewState;
            _increaseTempButton5.Visibility = viewState;
            _crtTempLabel.Visibility = viewState;
            _desiredTempLabel.Visibility = viewState;
        }

        private void SetErrorMessage()
        {
            int errorTextSize = 28;

            _crtTempTextView.TextSize = errorTextSize;
            switch (_bluetoothStatus)
            {
                case BluetoothStatus.NO_ADAPTER:
                    _crtTempTextView.Text = "Your device has no Bluetooth adapter.";
                    break;
                case BluetoothStatus.NO_CONNECTION:
                    _crtTempTextView.Text = "Your Bluetooth is turned off. Please turn it on before running the app.";
                    break;
                case BluetoothStatus.NO_DEVICE:
                    _crtTempTextView.Text = "No temperature device was found.";
                    break;
                default:
                    break;
            }      
        }

        private async Task DisplayErrorSource()
        {
            int closeAppSeconds = 5;
            const int millisecondsInASecond = 1000;
            string baseText = _crtTempTextView.Text;

            for (int seconds = closeAppSeconds; seconds > 0; seconds--)
            {
                _crtTempTextView.Text = baseText + $" The application will close in {seconds} second(s).";
                await Task.Delay(millisecondsInASecond);
            }
        }

        private void CloseApp()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}