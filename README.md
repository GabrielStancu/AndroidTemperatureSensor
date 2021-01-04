# AndroidTemperatureSensor

Arduino:

HC-05 Bluetooth module
KY-028 Digital temperature sensor
SG90 Servomotor
Arduino UNO board 

The temperature sensor collects data about the temperature of the room, sends it to the Arduino board which then sends it to eventual receivers using the Bluetooth module. 

Android:

Android device

The application allows the user to read the current temperature and adjust it in a given interval. The set desired temperature is sent back over Bluetooth to the Arduino board, which simulates the configuration of a thermostat by rotating the servomotor with respect to the received desired temperature value. 

Application developed with C# in Visual Studio, using Xamarin Android. 
