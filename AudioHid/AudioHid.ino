//#include "Encoder.h"
//#include <GyverEncoder.h>


#define ENCODER_OPTIMIZE_INTERRUPTS
#define ENCODER_USE_INTERRUPTS

#include <Adafruit_NeoPixel.h>
#include <Encoder.h>
#include <HID-Project.h>


#define PIN            10
#define NUMPIXELS      8


Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUMPIXELS, PIN, NEO_GRB + NEO_KHZ800);

Encoder myEnc(2, 3);


int oldPosition = -999;
int g[2][NUMPIXELS];

uint8_t hidDataReceive[64];
uint8_t hidDataSend[64];

const int pinLed = LED_BUILTIN;



int grad[2][NUMPIXELS] = {
	{ 0,70,90,130,156,182,219,255 }, //Red
	{ 255,190,170,146,130,90,70,0 }  //Green
};

void setup() {
	Serial.begin(9600);
	pixels.begin();
	pixels.setBrightness(100);
	RawHID.begin(hidDataReceive, sizeof(hidDataReceive));
	pinMode(pinLed, OUTPUT);
}


void func(int j) {
	double total = j / 12.5;
	for (int i = 0; i<NUMPIXELS; i++) {
		if (total>1) {
			g[0][i] = grad[0][i];
			g[1][i] = grad[1][i];
			total -= 1;
		}
		else {
			g[0][i] = round(grad[0][i] * total);
			g[1][i] = round(grad[1][i] * total);
			total = 0;
		}
	}
}

void reDraw() {
	for (int i = 0; i<NUMPIXELS; i++) {
		pixels.setPixelColor(i, g[0][i], g[1][i], 0); // Moderately bright green color. 
		pixels.show(); // This sends the updated pixel color to the hardware.
	}
}

void sendVolume(uint8_t rotation, uint8_t numberEncoder) {
	for (uint8_t i = 0; i < sizeof(hidDataSend); i++) {
		hidDataSend[i] = 0;
	}
	hidDataSend[0] = 1;
	hidDataSend[1] = numberEncoder;
	hidDataSend[2] = rotation; //0 - left, 1 - right

	RawHID.write(hidDataSend, 64);
	RawHID.flush();
}

int val = 0;
void loop() {
	if (Serial.available() > 0) {
		val = Serial.parseInt();
		Serial.println(val);
		pixels.setBrightness(val);
		reDraw();
	}


	
	int newPosition = myEnc.read() / 4;
	if (newPosition) {
		if (newPosition >= 1) {
			sendVolume(1, 1); //right
		} else {
			sendVolume(0, 1); //left
		}
		myEnc.write(0);
    if (newPosition>1) {
		Serial.print("pos: ");
		Serial.println(newPosition);
    }

	}
	
	auto bytesAvailable = RawHID.available();
	if (bytesAvailable)
	{
		digitalWrite(pinLed, HIGH);
		uint8_t hidData[64];
		int i = 0;
		while (bytesAvailable--) {
			hidData[i] = RawHID.read();
			i++;
			//Serial.println(RawHID.read());
		}
		if (hidData[2] == 255) {
			func(0);
		}
		else {
			func(hidData[2]);
		}
		Serial.println(hidData[2]);
		reDraw();

		digitalWrite(pinLed, LOW);
	}
}
