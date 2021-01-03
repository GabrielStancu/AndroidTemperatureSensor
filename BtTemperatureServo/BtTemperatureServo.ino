#include <Servo.h>
#include <SoftwareSerial.h>

int txPin = 1;
int rxPin = 0;
int digitalPin = 7; // KY-028 digital interface
int analogPin = A2; // KY-028 analog interface
int servoPin = 11;

int angle = 0;
int temp;

const int minTemp = 150;
const int maxTemp = 350;
const int minAngle = 0;
const int maxAngle = 180;
const char sendDataPackageStartByte = 'T';
const int receiveDataPackageStartByteVal = 83; //"S"
const int receiveDataPackageEndByteVal = 13; 
const int servoWriteDelay = 100;

SoftwareSerial BT(2,3);

Servo servo;
void setup() {
  pinMode(digitalPin, INPUT);
  pinMode(analogPin, INPUT);

  Serial.begin(9600);
  BT.begin(9600);
}

void loop() {
  if(BT.available() <= 0)
  {
    temp = analogRead(analogPin);
    BT.print(sendDataPackageStartByte);
    BT.print(temp); 
    BT.println();
    delay(1000);
  }
  else 
  {
    readData();
  }
}

void readData()
{
  if (BT.available())
  {       
    char temp[3];
    while(BT.available() > 0)
    {
      char crtByte = BT.read();
      if (crtByte == receiveDataPackageStartByteVal) 
      {
        temp[0] = BT.read();
        temp[1] = BT.read();
        temp[2] = BT.read();

        if(BT.read() != receiveDataPackageEndByteVal)
        {
          return;
        }
        
        break;
      } 
    }
    
    int desiredTemp = atoi(temp);
    int desiredAngle = map(desiredTemp, minTemp, maxTemp, minAngle, maxAngle);

    Serial.println("Desired Temperature:");
    Serial.println(desiredTemp);

    Serial.println("Desired Angle:");
    Serial.println(desiredAngle);
    
    turnServo(desiredAngle);
    delay(1000);
  }
}

void turnServo(int angle)
{
  servo.attach(servoPin);
  delay(servoWriteDelay);
  servo.write(angle);
  delay(1000);
  servo.detach();
  delay(servoWriteDelay);
}
