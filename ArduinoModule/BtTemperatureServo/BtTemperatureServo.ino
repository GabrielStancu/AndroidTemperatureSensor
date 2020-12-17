#include <Servo.h>

int txPin = 1;
int rxPin = 0;
int digitalPin = 7; // KY-028 digital interface
int analogPin = A2; // KY-028 analog interface
int angle = 0;
int temp;
bool incTemp = false, decTemp = false;

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
    Serial.print("T");
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
      if (crtByte == 83) //"S"
      {
        temp[0] = Serial.read();
        temp[1] = Serial.read();
        temp[2] = Serial.read();

        if(Serial.read() != 13)
        {
          return;
        }
        
        break;
      } 
    }
    
    int desiredTemp = (temp[0] - '0') * 100 + (temp[1] - '0') * 10 + (temp[2] - '0');
    desiredTemp *= 100;
    int desiredAngle = map(desiredTemp, 15000, 35000, 0, 180);

    turnServo(desiredAngle);
  }
}

void turnServo(int angle)
{
  servo.attach(11);
  delay(100);
  servo.write(angle);
  servo.detach();
  delay(100);
}
