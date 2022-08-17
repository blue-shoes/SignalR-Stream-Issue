namespace SignalRStreamIssue.Hub
{
    using Microsoft.AspNetCore.SignalR;
    using SignalRStreamIssue.Manager;

    public class StreamHub : Hub
    {
        private readonly ConnectionManager connManager;

        public StreamHub(ConnectionManager connMangaer)
        {
            this.connManager = connMangaer;
        }

        public async Task AddDevice(string deviceId)
        {
            connManager.AddDevice(Context.ConnectionId, deviceId);
            await Clients.Client(Context.ConnectionId).SendCoreAsync("InitDevice", new object[] { });
        }

        public async Task SendStreamLocal(string deviceId)
        {
            System.Diagnostics.Debug.WriteLine("SendStreamLocal");
            connManager.AddDevice(Context.ConnectionId, deviceId);
            Func<IAsyncEnumerable<string>> stream = delegate()
            {
                return LocalStream();
            };
            await Clients.All.SendAsync("ReceiveBadStream", stream);
            connManager.RemoveDevice(Context.ConnectionId);
        }

        async IAsyncEnumerable<string> LocalStream()
        {
            for (var i = 0; i < 5; i++)
            {
                var data = "left " + i;
                yield return data;
            }
        }

        public async Task AddJsDevice(string deviceId)
        {
            connManager.AddDevice(Context.ConnectionId, deviceId);
            System.Diagnostics.Debug.WriteLine("Added Js Device");

        }

        public async Task RemoveJsDevice(string deviceId)
        {
            connManager.RemoveDevice(deviceId);
            System.Diagnostics.Debug.WriteLine("Removed Js Device");
        }

        public async Task AddViewer(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId);
        }

        public async Task ReceiveInit(string data)
        {
            System.Diagnostics.Debug.WriteLine("StartBadStream");
            var group = connManager.GetDeviceId(Context.ConnectionId);
            await Clients.Group(group).SendAsync("ConnInit", data);
        }

        public async Task StartBadStream(string deviceId)
        {
            System.Diagnostics.Debug.WriteLine("StartBadStream");
            var conn = connManager.GetDeviceConnection(deviceId);
            if (conn != null)
            {
                await Clients.Client(conn).SendAsync("StartStream", "bad", "left");
                await Clients.Client(conn).SendAsync("StartStream", "bad", "right");
            }
        }

        public async Task StartGoodStream(string deviceId)
        {
            System.Diagnostics.Debug.WriteLine("StartGoodStream");
            var conn = connManager.GetDeviceConnection(deviceId);
            if (conn != null)
            {
                await Clients.Client(conn).SendAsync("StartStream", "good", "left");
                await Clients.Client(conn).SendAsync("StartStream", "good", "right");
            }
        }

        public async Task ReceiveBadStream(IAsyncEnumerable<string> data)
        {
            System.Diagnostics.Debug.WriteLine("ReceiveStreamBad");
            var group = connManager.GetDeviceId(Context.ConnectionId);
            await foreach (var item in data)
            {
                var side = item[..5];
                if (side.Equals("left "))
                {
                    await Clients.Group(group).SendAsync("SendLeftStream", item[5..]);
                }
                else
                {
                    await Clients.Group(group).SendAsync("SendRightStream", item[5..]);
                }
            }
        }

        public async Task<IAsyncEnumerable<string>> ReceiveGoodStream(IAsyncEnumerable<string> data)
        {
            System.Diagnostics.Debug.WriteLine("ReceiveStreamGood");
            var group = connManager.GetDeviceId(Context.ConnectionId);
            await foreach (var item in data)
            {
                var side = item[..5];
                if (side.Equals("left "))
                {
                    await Clients.Group(group).SendAsync("SendLeftStream", item[5..]);
                }
                else
                {
                    await Clients.Group(group).SendAsync("SendRightStream", item[5..]);
                }
            }
            return data;
        }

        public async Task StopStream(string deviceId)
        {
            string? conn = connManager.GetDeviceConnection(deviceId);
            if (conn != null)
            {
                await Clients.Client(conn).SendAsync("StopStream");
            }
        }
    }
}
