#include <SoftwareSerial.h>
#include "DHT.h"

/* REFERENCES:
 * SHOCK http://henrysbench.capnfatz.com/henrys-bench/arduino-sensors-and-input/ky-002-arduino-vibration-shake-sensor-manual-and-tutorial/
/*

/* TODO: lastSigfoxTime - kontrolovat okno
 * zprávy ukládat do bufferu, postupně odesílat jinou funkcí
 * kontrola počtu zpráv
 * funkce pro zjisteni rozdilu mezi dvema casy (last a millis())
 * uspavani
 */


const int pinLed = BUILTIN_LED;
const int pinShock = D2;
const int pinButton = D3;
const int pinTemp = D4;
const int pinSigfox_TX = D5;
const int pinSigfox_RX = D6;

const boolean isDebug = false;

const String version = "0.1";

//Shock sensor
int shockVal = HIGH;
boolean isShock = false;
unsigned long lastShockTime; // Record the time that we measured a shock
int shockAlarmTime = 60000; // Number of milli seconds to keep the shock alarm high - 10 secs

//Button
int buttonState = 0;
int isButtonPressed = false;
int messageCount = 0;

//Temperature and huminidy
#define DHTTYPE DHT22   // DHT 22  (AM2302)
DHT dht(pinTemp, DHTTYPE);
float lastTemperature = 0;
float lastHumidity = 0;
boolean isTemp1 = true;
boolean isTemp2 = false;

//Weight
float lastWeight = 0;
boolean isWeight = false;

//GPS
boolean isGPS = false;
long lastLongitude = 0;
long lastLatitude = 0;

//Sigfox
SoftwareSerial sigfox(pinSigfox_RX, pinSigfox_TX);
unsigned long lastSigfoxTime;

//Info, whed message was sent
unsigned long lastAlarmSend = 0;
unsigned long lastStatusSend = 0;
unsigned long secsToAlarm = 600;   //10 mins
unsigned long secsToStatus = 3600; //60 mins

void setup() {
  pinMode(pinButton, INPUT);
  pinMode(pinLed, OUTPUT);
  pinMode(pinShock, INPUT) ; // input from the KY-002

  delay(500);

  //Startup started, switch on led
  digitalWrite(pinLed, LOW);  // LED on

  // start serial port at 9600 bps:
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
  Serial.println();
  Serial.println();
  Serial.println("STARTING BEEHIVE MONITOR - VERSION " + version);
  Serial.println("=======================================================================");

  // start temp sensor at DHT shield
  Serial.print("Starting DHT...");
  dht.begin();
  Serial.println("done");

  // start sigfox
  Serial.print("Starting Sigfox Serial...");
  delay(2000);
  sigfox.begin(9600);
  Serial.println("done");

  if (!isDebug) {
    //release version
    Serial.println("RELEASE VERSION");
    secsToAlarm = 120;   //should be 600 = 10 mins
    secsToStatus = 360;  //should be 3600 = 60 mins
  } else {
    //debug
    Serial.println("DEBUG VERSION");
    secsToAlarm = 60;   //1 min
    secsToStatus = 120; //2 mins
  }

  readSensors();

  //Startup finished, switch off led
  digitalWrite(pinLed, HIGH);
  Serial.println("READY, ARMED");
}

void loop() {
  readButton();
  readShock();

  if (elapsedMins(lastStatusSend) >= secsToStatus * 60) {
     Serial.println("TIME FOR STATUS");
     sendStatus();
  }

  if (isShock && elapsedMins(lastAlarmSend) >= secsToAlarm * 60) {
    Serial.println("TIME FOR ALARM");
    sendAlarm();
  }

  // Sleep for 5 seconds
  //System.sleep(SLEEP_MODE_DEEP, 30);
}

//=======================================
//  HELPER FUNCTIONS
//=======================================
void blinkLed(int wait) {
  delay(2000);
  digitalWrite(pinLed, LOW);  // LED on
  delay(wait);
  digitalWrite(pinLed, HIGH);  // LED off
  delay(wait);
}

int elapsedSecs(int oldTime) {
  return (millis() - oldTime)/1000;
}

int elapsedMins(int oldTime) {
  /*Serial.print("    Uplynulo " );
  Serial.print(String((millis() - oldTime)/1000/60));
  Serial.print(" minut, " );
  Serial.print(String((millis() - oldTime)/1000));  
  Serial.println(" sekund." );
  */
  
  return (millis() - oldTime)/1000/60;
}

int getStatusByte() {
  //vrat status zakodovany do bitu
  int status[15];
  int i;
  //clear array
  for (i = 0; i <= 15; i = i + 1) {status[i]=0;}

  //set status bits
  Serial.println("MY STATUS:");
  if (isShock) {status[0]=1; Serial.println("Alarm active");}
  if (isButtonPressed) {status[1]=1; Serial.println("Button down");}
  if (isTemp1) {status[2]=1; Serial.println("Temperature 1 active");}
  if (isTemp2) {status[3]=1; Serial.println("Temperature 2 active");}
  if (isWeight) {status[4]=1; Serial.println("Weight active");}
  if (isGPS) {status[5]=1; Serial.println("GPS active");}

  int result = 0;
  for (i = 0; i <= 15; i = i + 1) {
    if (status[i]==1) {
      int addValue = 0.5 + pow(2,i);  //Strong, right? But it use float, rounded down (3.9999 instead of 4, rounded to 3)
      result = result + addValue;
    }
  }

  return result;
}

void readSensors()
{
  if (isWeight) {
    readWeight();
  }
  if (isGPS) {
    readGPS();
  }
  if (isTemp1) {
    readTemp();
  }
}

//=======================================
//  MESSAGE FUNCTIONS
//=======================================

void sendStatus() {
  int data[] = {1, getStatusByte(), lastTemperature + 100, lastHumidity, 0, 0, lastWeight};
  sendMessageArray(data, 7);

  lastStatusSend = millis();
}


void sendAlarm() {
  if (!isGPS) {
    int data[] = {2, getStatusByte()};
    sendMessageArray(data, 2);    
  } else {    
    int data[] = {2, getStatusByte(), 1, 2, 3, 4, 5, 6};  //dummy data, replace by real values, bytes encoded
    sendMessageArray(data, 8);        
  }

  lastAlarmSend = millis();
}


// convert array of int to hex string and send message
void sendMessageArray(int data[], int sizeOfArray) {
  String messageHEX = "";
  String messageDEC = "";

  int i;
  for (i = 0; i < sizeOfArray; i = i + 1) {
    if (data[i] < 10) {
      messageHEX = messageHEX + "0"; // add 0 for number < 10
    }

    messageHEX = messageHEX + String(data[i], HEX) + " ";
    messageDEC = messageDEC + String(data[i]) + " ";

    //Serial.println(String(i) + "=" + String(data[i]));
  }

  //Serial.print("message computed: ");
  //Serial.println(messageHEX + "(" + messageDEC + ")");

  sendMessageString(messageHEX);
}

// send message encoded to HEX
void sendMessageString(String message) {
    message.trim();

    Serial.println();
    Serial.print("SENDING MESSAGE '" + message + "'...");
    sigfox.print("AT");
    sigfox.write("\r\n");
    sigfox.print("AT");
    sigfox.write("\r\n");
    if (!isDebug) {sigfox.print("AT$SS=" + message);}
    sigfox.write("\r\n");

    lastSigfoxTime = millis();

    Serial.println("DONE");
    blinkLed(1000);

    //read data from SigFox
    while (sigfox.available() > 0) {
        int ret = sigfox.read();    //throw data away
        //Serial.print(sigfox.read());
    }
    Serial.println();
    Serial.println();
}



//=======================================
//  SENSORS
//=======================================
void readWeight() {
  isWeight = false;
  lastWeight = random(20,90);
}

void readGPS() {
  if (!isDebug) {
    isGPS = false;
    lastLongitude = 14.45;
    lastLatitude = 50.05;
  }
  else
  {
    isGPS = true;
    lastLongitude = 14.45;
    lastLatitude = 50.05;
  }
}

void readShock() {
  shockVal = digitalRead (pinShock) ; // read the value from our sensor

  if (shockVal == LOW) // If we're in an alarm state
  {
    lastShockTime = millis(); // record the time of the shock
    // The following is so you don't scroll on the output screen
    if (!isShock) {
      Serial.println("");
      Serial.println("Shock Alarm");
      isShock = true;

      sendAlarm();
    }
  }
  else
  {
    if ( (millis() - lastShockTime) > shockAlarmTime  &&  isShock) {
      Serial.println("");
      Serial.println("alarm ended");
      isShock = false;
      sendStatus();
    }
  }
}

void readButton() {
  // read button state, HIGH when pressed, LOW when not
  buttonState = digitalRead(pinButton);

  if (buttonState == LOW) 
  {
    // if the push button pressed, send message
    if (!isButtonPressed) 
    {
      Serial.print("CLICK ");
      Serial.println(millis());
      isButtonPressed = true;
      blinkLed(250);

      readTemp();
      sendStatus();
    }
  }
  else
  {
      isButtonPressed = false;
  }
}

void readTemp() {
  Serial.println("Reading temperature and humidity - sensor 1 ...");

  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
  float h = dht.readHumidity();

  // Read temperature as Celsius (the default)
  float t = dht.readTemperature();

  // Check if any reads failed and exit early (to try again).
  if (isnan(h) || isnan(t)) {
    Serial.println("Failed to read from DHT sensor!");
    isTemp1 = false;
    return;
  }

  //Store values
  lastTemperature = t;
  lastHumidity = h;
  isTemp1 = true;

  // Compute heat index in Celsius (isFahreheit = false)
  float hic = dht.computeHeatIndex(t, h, false);

  Serial.print("Humidity: ");
  Serial.print(h);
  Serial.println(" %\t");
  Serial.print("Temperature: ");
  Serial.print(t);
  Serial.println(" *C ");
  Serial.print("Heat index: ");
  Serial.print(hic);
  Serial.println(" *C ");
}
