#ifndef ESP32_FIRMWARE_PERSISTENCE_H
#define ESP32_FIRMWARE_PERSISTENCE_H

/*
 * ENERLYTICS ESP32 FIRMWARE - DEVICE PERSISTENCE MODULE
 * This code implements hardware-side persistent storage using Preferences.h
 * (ESP32 NVS).
 */

#include <Arduino.h>
#include <Preferences.h>
#include <vector>

Preferences preferences;
const char *NAMESPACE = "enerlytics";
const int MAX_DEVICES = 50;

struct DeviceConfig {
  int id;
  int period;
  char sensors[32];
};

// Internal state
std::vector<int> registeredDevices;
String wifi_ssid = "";
String wifi_pass = "";

void loadDevicesFromMemory() {
  preferences.begin(NAMESPACE, true);

  // Load IDs
  size_t len = preferences.getBytesLength("ids");
  if (len > 0) {
    int ids[len / sizeof(int)];
    preferences.getBytes("ids", ids, len);
    registeredDevices.clear();
    for (int i = 0; i < len / sizeof(int); i++) {
      registeredDevices.push_back(ids[i]);
    }
  }

  // Load WiFi
  wifi_ssid = preferences.getString("ssid", "");
  wifi_pass = preferences.getString("pass", "");

  preferences.end();
}

void saveDevicesToMemory() {
  preferences.begin(NAMESPACE, false);
  int ids[registeredDevices.size()];
  for (size_t i = 0; i < registeredDevices.size(); i++) {
    ids[i] = registeredDevices[i];
  }
  preferences.putBytes("ids", ids, registeredDevices.size() * sizeof(int));
  preferences.end();
}

void handleSerialCommand(String cmd) {
  if (cmd.startsWith("REG_ID:")) {
    // Format: REG_ID:12|AA:BB:CC:...
    int id = cmd.substring(7, cmd.indexOf('|')).toInt();

    bool exists = false;
    for (int rid : registeredDevices)
      if (rid == id)
        exists = true;

    if (!exists && registeredDevices.size() < MAX_DEVICES) {
      registeredDevices.push_back(id);
      saveDevicesToMemory();
      Serial.println("REG_OK:" + String(id));
    } else if (exists) {
      Serial.println("REG_OK:" + String(id)); // Already there
    } else {
      Serial.println("ERR_FULL");
    }
  } else if (cmd.startsWith("DEL_ID:")) {
    int id = cmd.substring(7).toInt();
    for (auto it = registeredDevices.begin(); it != registeredDevices.end();
         ++it) {
      if (*it == id) {
        registeredDevices.erase(it);
        saveDevicesToMemory();
        Serial.println("DEL_OK:" + String(id));
        break;
      }
    }
  } else if (cmd.startsWith("WIFI_SET:")) {
    String data = cmd.substring(9);
    int sep = data.indexOf('|');
    if (sep != -1) {
      wifi_ssid = data.substring(0, sep);
      wifi_pass = data.substring(sep + 1);

      preferences.begin(NAMESPACE, false);
      preferences.putString("ssid", wifi_ssid);
      preferences.putString("pass", wifi_pass);
      preferences.end();
      Serial.println("WIFI_OK");
    }
  } else if (cmd.startsWith("CFG_SET:")) {
    // Format: CFG_SET:id|period|sensors
    Serial.println("CFG_OK:" + cmd.substring(8, cmd.indexOf('|', 8)));
  } else if (cmd.startsWith("CMD_PING:")) {
    Serial.println("PONG:" + cmd.substring(9));
  } else if (cmd == "NET_SCAN") {
    Serial.println("SCAN_COMPLETE:Master + " +
                   String(registeredDevices.size()) + " Nodes");
  }
}

void setup() {
  Serial.begin(115200);
  loadDevicesFromMemory();
}

void loop() {
  if (Serial.available() > 0) {
    String cmd = Serial.readStringUntil('\n');
    handleSerialCommand(cmd);
  }

  // Simulate data sending only for registered devices
  for (int id : registeredDevices) {
    // MQTT publish logic here...
  }
}

#endif
