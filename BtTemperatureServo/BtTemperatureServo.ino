#include <Servo.h>
//#include <SoftwareSerial.h>

int txPin = 1;
int rxPin = 0;
int digitalPin = 7; // KY-028 digital interface
int analogPin = A2; // KY-028 analog interface
int angle = 0;
int temp;
bool incTemp = false, decTemp = false;

Servo servo;
//SoftwareSerial BT(txPin,rxPin);

void setup() {
  pinMode(digitalPin, INPUT);
  pinMode(analogPin, INPUT);

  //BT.begin(9600);
  Serial.begin(9600);
}

void loop() {
  temp = analogRead(analogPin);
  Serial.print("T");
  Serial.print(temp); 
  Serial.println();

  readData();

  /*if (BT.available())
  {
    //Serial.println("here");
    char bytesToSend[3];
    bytesToSend[0] = 42;
    bytesToSend[1] = temp/10;
    bytesToSend[2] = temp%10;
    bytesToSend[3] = 48;
    BT.println(bytesToSend);
    /*Serial.print((int)bytesToSend[1]);
    Serial.print(".");
    Serial.print((int)bytesToSend[2]);
    Serial.println();

    BT.write(temp/10);
    BT.write(temp%10);
  }
  else 
  {
    Serial.println("unavailable");
  }*/

  /*if(incTemp)
  {
    servo.attach(11);
    delay(100);
    
    servo.write(90);
    delay(100);
    servo.write(135);
    delay(100);
    servo.write(180);
    delay(100);
    servo.write(135);
    delay(100);

    servo.detach();
    delay(100);
  }
  else if(decTemp)
  {
    servo.attach(11);
    delay(100);
    
    servo.write(90);
    delay(100);
    servo.write(45);
    delay(100);
    servo.write(0);
    delay(100);
    servo.write(45);
    delay(100);

    servo.detach();
    delay(100);
  }*/
}

void readData()
{
  if (Serial.available())
  {   
      String btInput = Serial.readString();   
      int desiredTemp = btInput.toInt();
      int desiredAngle = map(desiredTemp, 15, 30, 0, 180);

      servo.attach(11);
      delay(100);
      servo.write(desiredAngle);
      servo.detach();
      delay(100);
  }
}
