using System;
using System.Management;

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


    public class DeviceChangeWatcher : IDisposable
    {
        private const string deviceChangeEventString = "SELECT * FROM Win32_DeviceChangeEvent";

        private ManagementEventWatcher watcher;

        public event DeviceChangeEventHandler DeviceChanged;
        public event DeviceChangeEventHandler DeviceArrived;
        public event DeviceChangeEventHandler DeviceRemoved;
        public event DeviceChangeEventHandler DeviceDocking;
        public event DeviceChangeEventHandler DeviceConfigurationChanged;

        public DeviceChangeWatcher()
        {
            this.watcher = new ManagementEventWatcher(deviceChangeEventString);
            this.watcher.EventArrived += HandleEventArrival;
        }

        public void Dispose()
        {
            if (this.watcher != null)
            {
                this.watcher.EventArrived -= HandleEventArrival;
                this.watcher.Dispose();
                this.watcher = null;
            }
        }

        public void Start()
        {
            if (this.watcher != null)
                this.watcher.Start();
        }

        public void Stop()
        {
            if (this.watcher != null)
                this.watcher.Stop();
        }

        private void HandleEventArrival(object sender, EventArrivedEventArgs e)
        {
            DeviceChangeEventType eventType = (DeviceChangeEventType)((ushort)e.NewEvent.Properties["EventType"].Value);            
            DateTime timeStamp = DateTime.FromFileTimeUtc((long)((ulong)e.NewEvent.Properties["TIME_CREATED"].Value));

            DeviceChangeEventArgs eventArgs = new DeviceChangeEventArgs(eventType, timeStamp);
            OnDeviceChanged(eventArgs);
            switch (eventType)
            {
                case DeviceChangeEventType.ConfigurationChanged:
                    OnDeviceConfigurationChanged(eventArgs);
                    break;
                case DeviceChangeEventType.DeviceArrival:
                    OnDeviceArrived(eventArgs);
                    break;
                case DeviceChangeEventType.DeviceRemoval:
                    OnDeviceRemoved(eventArgs);
                    break;
                case DeviceChangeEventType.Docking:
                    OnDeviceDocking(eventArgs);
                    break;
            }            
        }

        protected virtual void OnDeviceChanged(DeviceChangeEventArgs e)
        {
            // Avoid race conditions in multi-threaded environments
            var handler = this.DeviceChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeviceArrived(DeviceChangeEventArgs e)
        {
            // Avoid race conditions in multi-threaded environments
            var handler = this.DeviceArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeviceRemoved(DeviceChangeEventArgs e)
        {
            // Avoid race conditions in multi-threaded environments
            var handler = this.DeviceRemoved;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeviceDocking(DeviceChangeEventArgs e)
        {
            // Avoid race conditions in multi-threaded environments
            var handler = this.DeviceDocking;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeviceConfigurationChanged(DeviceChangeEventArgs e)
        {
            // Avoid race conditions in multi-threaded environments
            var handler = this.DeviceConfigurationChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

}
