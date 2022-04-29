using Hukidashi.Configuration;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Hukidashi.WebSockets
{
    public class HukidashiWebSocketServer : IDisposable
    {
        private readonly WebSocketServer _webSocketServer;
        public event OnRecieveMessageHandler OnMessageRecived;

        public HukidashiWebSocketServer()
        {
            this._webSocketServer = new WebSocketServer($"ws://127.0.0.1:{PluginConfig.Instance.ModPort}");
            this._webSocketServer.AddWebSocketService<SocketBehavior>("/", this.BehavierInit);

            this._webSocketServer.Start();
        }

        private void BehavierInit(SocketBehavior wb)
        {
            wb.Init((s, e) =>
            {
                Plugin.Log.Debug(e.Data);
                OnMessageRecived?.Invoke(this, e);
            });
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    this._webSocketServer?.Stop();
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public class SocketBehavior : WebSocketBehavior
        {
            public Action<object, MessageEventArgs> OnRecievedMessage;

            public SocketBehavior Init(Action<object, MessageEventArgs> onMessage)
            {
                this.OnRecievedMessage = onMessage;
                return this;
            }

            public void Respomce(string data, Action<bool> completed)
            {
                this.SendAsync(data, completed);
            }

            protected override void OnOpen()
            {
                base.OnOpen();
                Plugin.Log.Debug($"On open!");
            }
            protected override void OnMessage(MessageEventArgs e)
            {
                base.OnMessage(e);
                this.OnRecievedMessage?.Invoke(this, e);
            }
            protected override void OnClose(CloseEventArgs e)
            {
                Plugin.Log.Debug($"On close!");
                this.OnRecievedMessage = null;
                base.OnClose(e);
            }
        }
    }

    public delegate void OnRecieveMessageHandler(object sender, MessageEventArgs e);
}
