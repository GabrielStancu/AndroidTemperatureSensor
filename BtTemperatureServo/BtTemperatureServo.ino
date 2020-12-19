#include <Servo.h>

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

Servo servo;
void setup() {
  pinMode(digitalPin, INPUT);
  pinMode(analogPin, INPUT);

  Serial.begin(9600);
}

void loop() {
  if(Serial.available() <= 0)
  {
    temp = analogRead(analogPin);
    Serial.print(sendDataPackageStartByte);
    Serial.print(temp); 
    Serial.println();
  }
  else 
  {
    readData();
    delay(100);
  }
}

void readData()
{
  if (Serial.available())
  {       
    char temp[3];
    while(Serial.available() > 0)
    {
      char crtByte = Serial.read();
      if (crtByte == receiveDataPackageStartByteVal) 
      {
        temp[0] = Serial.read();
        temp[1] = Serial.read();
        temp[2] = Serial.read();

        if(Serial.read() != receiveDataPackageEndByteVal)
        {
          return;
        }
        
        break;
      } 
    }
    
    int desiredTemp = atoi(temp);
    int desiredAngle = map(desiredTemp, minTemp, maxTemp, minAngle, maxAngle);
    turnServo(desiredAngle);
  }
}

void turnServo(int angle)
{
  servo.attach(servoPin);
  delay(servoWriteDelay);
  servo.write(angle);
  servo.detach();
  delay(servoWriteDelay);
}
