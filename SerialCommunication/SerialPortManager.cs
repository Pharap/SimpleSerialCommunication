using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

/*
   Copyright (C) 2018 Pharap (@Pharap)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace SerialCommunication
{
    // WIP
    public class SerialPortManager : IDisposable
    {
        private readonly Dictionary<string, SerialPort> activePorts = new Dictionary<string, SerialPort>();
        private readonly DeviceChangeWatcher watcher = new DeviceChangeWatcher();

        public SerialPortManager()
        {
            this.watcher.DeviceArrived += HandleDeviceArrived;
            this.watcher.DeviceRemoved += HandleDeviceRemoved;
        }

        public void Dispose()
        {
            this.watcher.DeviceArrived -= HandleDeviceArrived;
            this.watcher.DeviceRemoved -= HandleDeviceRemoved;

            this.activePorts.Clear();
            this.watcher.Dispose();
        }

        public void Run()
        {

        }

        private void HandleDeviceRemoved(object sender, DeviceChangeEventArgs e)
        {
            Thread.Sleep(100); // Wait for port to go stale
            var availablePorts = SerialPort.GetPortNames();
            var portHash = new HashSet<string>(availablePorts);

            var removedPorts = new List<string>(this.activePorts.Keys.Where(portHash.Contains));
            foreach (var portName in removedPorts)
            {
                SerialPort port = null;
                if (this.activePorts.TryGetValue(portName, out port))
                {
                    port.Dispose();
                    this.activePorts.Remove(portName);
                }
            }
        }

        private void HandleDeviceArrived(object sender, DeviceChangeEventArgs e)
        {
            Thread.Sleep(100); // Wait for port to go stale
            var availablePorts = SerialPort.GetPortNames();
            foreach (var portName in availablePorts.Where(p => !this.activePorts.ContainsKey(p)))
            {
                this.activePorts.Add(portName, CreateSerialPort(portName));
            }
        }

        private SerialPort CreateSerialPort(string portName)
        {
            var serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One) { RtsEnable = true };

            /*serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            serial.ErrorReceived += new SerialErrorReceivedEventHandler(serial_ErrorReceived);
            serial.PinChanged += new SerialPinChangedEventHandler(serial_PinChanged);
            serial.Disposed += new EventHandler(serial_Disposed);*/
            serial.Open();

            return serial;
        }
    }
}
