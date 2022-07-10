// Uduino Default Board
#include<Uduino.h>
Uduino uduino("uduinoBoard"); // Declare and name your object

// Servo
#include <Servo.h>
#define MAXSERVOS 8


void setup()
{
  Serial.begin(9600);

#if defined (__AVR_ATmega32U4__) // Leonardo
  while (!Serial) {}
#elif defined(__PIC32MX__)
  delay(1000);
#endif

  uduino.addCommand("s", SetMode);
  uduino.addCommand("d", WritePinDigital);
  uduino.addCommand("a", WritePinAnalog);
  uduino.addCommand("rd", ReadDigitalPin);
  uduino.addCommand("r", ReadAnalogPin);
  uduino.addCommand("br", BundleReadPin);
  uduino.addCommand("b", ReadBundle);
  uduino.addInitFunction(InitializeServos);
  uduino.addDisconnectedFunction(DisconnectAllServos);
  uduino.addInitFunction(DisconnectAllServos);
}

void ReadBundle() {
  char *arg;
  char *number;
  number = uduino.next();
  int len ;
  if (number != NULL)
    len = atoi(number);
  for (int i = 0; i < len; i++) {
    uduino.launchCommand(arg);
  }
}

void SetMode() {
  int pinToMap;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToMap = atoi(arg);
  }
  int type;
  arg = uduino.next();
  if (arg != NULL)
  {
    type = atoi(arg);
    PinSetMode(pinToMap, type);
  }
}

void PinSetMode(int pin, int type) {
  //TODO : vérifier que ça, ça fonctionne
  if (type != 4)
    DisconnectServo(pin);

  switch (type) {
    case 0: // Output
      pinMode(pin, OUTPUT);
      break;
    case 1: // PWM
      pinMode(pin, OUTPUT);
      break;
    case 2: // Analog
      pinMode(pin, INPUT);
      break;
    case 3: // Input_Pullup
      pinMode(pin, INPUT_PULLUP);
      break;
    case 4: // Servo
      SetupServo(pin);
      break;
  }
}

void WritePinAnalog() {
  int pinToMap;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToMap = atoi(arg);
  }

  int value;
  arg = uduino.next();
  if (arg != NULL)
  {
    value = atoi(arg);

    if (ServoConnectedPin(pinToMap)) {
      UpdateServo(pinToMap, value);
    } else {
      analogWrite(pinToMap, value);
    }
  }
}

void WritePinDigital() {
  int pinToMap;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToMap = atoi(arg);
  }
  int value;
  arg = uduino.next();
  if (arg != NULL)
  {
    value = atoi(arg);
    digitalWrite(pinToMap, value);
  }
}

void ReadAnalogPin() {
  int pinToRead;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToRead = atoi(arg);
    printValue(pinToRead, analogRead(pinToRead));
  }
}

void ReadDigitalPin() {
  int pinToRead;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToRead = atoi(arg);
  }
  printValue(pinToRead, digitalRead(pinToRead));
}

void BundleReadPin() {
  int pinToRead;
  char *arg;
  arg = uduino.next();
  if (arg != NULL)
  {
    pinToRead = atoi(arg);
  }
  printValue(pinToRead, analogRead(pinToRead));
}

Servo myservo;
void loop()
{
  uduino.update();
}

void printValue(int pin, int value) {
  Serial.print(pin);
  Serial.print(" ");
  Serial.println(value);
  // TODO : Here we could put bundle read multiple pins if(Bundle) { ... add delimiter // } ...
}

/* SERVO CODE */



Servo servos[MAXSERVOS];
int servoPinMap[MAXSERVOS];

void InitializeServos() {
  //Serial.println("Init all servos");
  for (int i = 0; i < MAXSERVOS - 1; i++ ) {
    servoPinMap[i] = -1;
    servos[i].detach();
  }
}

void SetupServo(int pin) {
  if (ServoConnectedPin(pin))
    return;

  int nextIndex = GetAvailableIndexByPin(-1);
  if (nextIndex == -1)
    nextIndex = 0;

  //Serial.print("Starting servo on ");
  //Serial.print(pin);
  //Serial.print(" index ");
  //Serial.println(nextIndex);
  servoPinMap[nextIndex] = pin;
  servos[nextIndex].attach(pin);
}


void DisconnectServo(int pin) {
  servos[GetAvailableIndexByPin(pin)].detach();
  servoPinMap[GetAvailableIndexByPin(pin)] = -1;
}

bool ServoConnectedPin(int pin) {
  if (GetAvailableIndexByPin(pin) == -1) return false;
  else return true;
}



int GetAvailableIndexByPin(int pin) {
  for (int i = 0; i < MAXSERVOS - 1; i++ ) {
    //Serial.print("Finding pin  ");
    //Serial.print(pin);
    //Serial.print(" in ");
    //Serial.print(i);
    //Serial.print(" value is  ");
    //Serial.print(servoPinMap[i]);
    if (servoPinMap[i] == pin) {
      //Serial.println(" -> Found");
      return i;
    }
    //Serial.println();
  }
  return -1; // return first index, but this should not happend
}


void UpdateServo(int pin, int value) {
  int index = GetAvailableIndexByPin(pin);
  //Serial.print("Updating servo on ");
  //Serial.print(pin);
  //Serial.print(" index ");
  //Serial.print(index);
  //Serial.print(" value ");
  //Serial.println(value);
  servos[index].write(value);
  delay(10);
}

void DisconnectAllServos() {
  for (int i = 0; i < MAXSERVOS; i++) {
    servos[i].detach();
    digitalWrite(servoPinMap[i], LOW);
    servoPinMap[i] = -1;
  }
}
