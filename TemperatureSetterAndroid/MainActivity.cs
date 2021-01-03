﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using AlertDialog = Android.App.AlertDialog;

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
            var connRes = await _bluetoothModule.ConnectToBluetooth();

            if(connRes != BluetoothStatus.NO_ERROR)
            {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }

            while (true)
            {
                try
                {
                    _temperature = Math.Round((double)(await _bluetoothModule.ReadCurrentTemp()) / 10, decimalPlaces);
                    await Task.Delay(BluetoothModule.CommunicationDelay);
                    DisplayCurrentTemperature();
                }
                catch(Exception)
                {
                    DisplayError();
                }
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

        private void DisplayError()
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            AlertDialog alert = dialog.Create();
            alert.SetTitle("Connection lost");
            alert.SetMessage("Connection to Arduino was lost. Check the connection wires and your Bluetooth connection.");

            alert.SetButton("OK", (c, ev) =>
            {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });
        }
    }
}
