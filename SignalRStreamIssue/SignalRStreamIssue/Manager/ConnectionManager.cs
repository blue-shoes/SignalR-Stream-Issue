namespace SignalRStreamIssue.Manager
{
    public class ConnectionManager
    {
        private List<DeviceConnection> devices = new List<DeviceConnection>();

        public void AddDevice(string connectionId, string deviceId)
        {
            devices.Add(new DeviceConnection(connectionId, deviceId));
        }

        public string GetDeviceConnection(string deviceId)
        {
           return devices.Find(i => i.DeviceId.Equals(deviceId))?.ConnectionId;
        }

        public string GetDeviceId(string connectionId)
        {
            return devices.Find(i => i.ConnectionId.Equals(connectionId))?.DeviceId;
        }
    }

    internal class DeviceConnection
    {
        public string ConnectionId { get; set; }
        public string DeviceId { get; set; }

        public DeviceConnection(string connectionId, string deviceId)
        {
            ConnectionId = connectionId;
            DeviceId = deviceId;
        }
    }
}
