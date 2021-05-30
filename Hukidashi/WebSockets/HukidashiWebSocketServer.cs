using Hukidashi.SimpleJson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Zenject;

namespace Hukidashi.WebSockets
{
    public class HukidashiWebSocketServer : IDisposable
    {
        private const int _port = 4443;
        private const int _obsPort = 4444;
        private WebSocketServer _webSocketServer;
        private WebSocket _obsWebSocket;
        private HashSet<SocketBehavior> behaviors = new HashSet<SocketBehavior>();
        private ConcurrentQueue<KeyValuePair<SocketBehavior, byte[]>> _queue = new ConcurrentQueue<KeyValuePair<SocketBehavior, byte[]>>();

        private Thread _sendThread;

        public event OnRecieveMessageHandler OnMessageRecived;
        private readonly static object _lockobject = new object();

        public HukidashiWebSocketServer()
        {
            this._webSocketServer = new WebSocketServer($"ws://127.0.0.1:{_port}");
            this._webSocketServer.AddWebSocketService<SocketBehavior>("/", this.BehavierInit);
            this._obsWebSocket = new WebSocket($"ws://127.0.0.1:{_obsPort}/");
            this._obsWebSocket.OnMessage += this.OnObsWebSocketOnMessage;
            this._sendThread = new Thread(new ThreadStart(() =>
            {
                while (true) {
                    try {
                        Plugin.Log.Debug($"{this._obsWebSocket?.ReadyState}");
                        if (this._obsWebSocket?.ReadyState != WebSocketState.Open) {
                            return;
                        }
                        while (this._queue.TryDequeue(out var pair)) {
                            Plugin.Log.Debug("Send Data.");
                            this._obsWebSocket?.Send(pair.Value);
                            Plugin.Log.Debug("Sended Data.");
                        }
                    }
                    catch (Exception e) {
                        Plugin.Log.Error(e);
                    }
                    Thread.Sleep(1);
                }
            }));
            this._sendThread.Start();
            this._webSocketServer.Start();
        }
        private void OnObsWebSocketOnMessage(object sender, MessageEventArgs e)
        {
            Plugin.Log.Debug(e.Data);
            var json = JSON.Parse(e.Data);
            if (!string.IsNullOrEmpty(json["message-id"])) {
                var id = json["message-id"].Value;
                foreach (var b in this.behaviors) {
                    b?.Respomce(e.RawData, comp => { });
                }
            }
        }

        private void BehavierInit(SocketBehavior wb)
        {
            this.behaviors.Add(wb);
            wb.Init((s, e) =>
            {
                Plugin.Log.Debug(e.Data);
                var json = JSON.Parse(e.Data);
                _queue.Enqueue(new KeyValuePair<SocketBehavior, byte[]>(wb, e.RawData));
                OnMessageRecived?.Invoke(this, e);
                if (this._obsWebSocket.ReadyState == WebSocketState.Closed || this._obsWebSocket.ReadyState == WebSocketState.Closing) {
                    lock (_lockobject) {
                        this._obsWebSocket.ConnectAsync();
                    }
                }
            });
        }

        private bool disposedValue;
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this._webSocketServer?.Stop();
                    this._obsWebSocket.Close();
                    this._sendThread.Abort();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~WebSocketServer()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
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

            public void Respomce(byte[] data, Action<bool> completed)
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
                this.OnRecievedMessage = null;
                base.OnClose(e);
            }
        }
    }

    public delegate void OnRecieveMessageHandler(object sender, MessageEventArgs e);
}
