#pragma once

#define DHT_REQUEST_DELAY 5000

#include "Arduino.h"
#include <Ethernet.h>
#include "DHT.h"

class DHTSensor
{
public:
	DHTSensor(byte pin);
	void Loop();
	void ReadTemperatureSensor();
	void RenderJSON(EthernetClient* client);
	int temperature = 0;
	int humidity = 0;

private:
	void RenderPageHeader(EthernetClient* client);
	DHT *dht;
	unsigned long  readSensorTimer = 0;
};

