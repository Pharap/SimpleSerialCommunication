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
    class Program
    {
        private static readonly Dictionary<string, SerialPort> activePorts = new Dictionary<string, SerialPort>();

        static void Main(string[] args)
        {
            using (DeviceChangeWatcher watcher = new DeviceChangeWatcher())
            {
                watcher.DeviceArrived += new DeviceChangeEventHandler(watcher_DeviceArrived);
                watcher.DeviceRemoved += new DeviceChangeEventHandler(watcher_DeviceRemoved);

                watcher.Start();

                var availablePorts = SerialPort.GetPortNames();
                foreach (var portName in availablePorts)
                {
                    activePorts.Add(portName, CreateSerialPort(portName));
                }

                var line = Console.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    foreach (var serial in activePorts.Values)
                    {
                        serial.WriteLine(line);
                    }
                    line = Console.ReadLine();
                }

                foreach (var port in activePorts.Values)
                {
                    port.Dispose();
                }
                activePorts.Clear();

                watcher.Stop();
            }
        }

        public static void DestroySerialPort(SerialPort serialPort)
        {
            serialPort.Close();

            serialPort.DataReceived -= serial_DataReceived;
            serialPort.ErrorReceived -= serial_ErrorReceived;
            serialPort.PinChanged -= serial_PinChanged;
            serialPort.Disposed -= serial_Disposed;

            serialPort.Dispose();
        }

        public static SerialPort CreateSerialPort(string portName)
        {
            // https://www.arduino.cc/en/Serial/Begin
            SerialPort serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One) { RtsEnable = true };

            serialPort.DataReceived += serial_DataReceived;
            serialPort.ErrorReceived += serial_ErrorReceived;
            serialPort.PinChanged += serial_PinChanged;
            serialPort.Disposed += serial_Disposed;

            serialPort.Open();
            return serialPort;
        }

        static void watcher_DeviceRemoved(object sender, DeviceChangeEventArgs e)
        {
            Thread.Sleep(100); // Wait for port to go stale
            var availablePorts = SerialPort.GetPortNames();
            var portHash = new HashSet<string>(availablePorts);

            var removedPorts = new List<string>(activePorts.Keys.Where(p => !portHash.Contains(p)));
            foreach (var portName in removedPorts)
            {
                SerialPort port = null;
                if (activePorts.TryGetValue(portName, out port))
                {
                    port.Dispose();
                    activePorts.Remove(portName);
                }
            }
        }

        static void watcher_DeviceArrived(object sender, DeviceChangeEventArgs e)
        {
            Thread.Sleep(100); // Wait for port to go stale
            var availablePorts = SerialPort.GetPortNames();
            foreach (var portName in availablePorts.Where(p => !activePorts.ContainsKey(p)))
            {
                activePorts.Add(portName, CreateSerialPort(portName));
            }
        }

        static void serial_Disposed(object sender, EventArgs e)
        {
            Console.WriteLine("Serial disposed");
        }

        static void serial_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            Console.WriteLine("Pin changed");
            switch (e.EventType)
            {
                case SerialPinChange.Break: return;
                case SerialPinChange.CDChanged: return;
                case SerialPinChange.CtsChanged: return;
                case SerialPinChange.DsrChanged: return;
                case SerialPinChange.Ring: return;
            }                
        }
        
        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serial = sender as SerialPort;
            if(serial != null)
            {
                if (e.EventType == SerialData.Eof)
                {
                    return;
                    //Console.WriteLine("EOF");
                }
                else if (e.EventType == SerialData.Chars)
                {
                    var input = serial.ReadExisting();
                    Console.Write(input);
                }
            }
        }

        static void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("Error:");
            if (e.EventType.HasFlag(SerialError.RXOver))
                Console.WriteLine("An input buffer overflow has occurred.");
            if (e.EventType.HasFlag(SerialError.Overrun))
                Console.WriteLine("A character-buffer overrun has occurred.");
            if (e.EventType.HasFlag(SerialError.RXParity))
                Console.WriteLine("The hardware detected a parity error.");
            if (e.EventType.HasFlag(SerialError.Frame))
                Console.WriteLine("The hardware detected a framing error.");
            if (e.EventType.HasFlag(SerialError.TXFull))
                Console.WriteLine("The application tried to transmit a character, but the output buffer was full.");
        }
    }
}
