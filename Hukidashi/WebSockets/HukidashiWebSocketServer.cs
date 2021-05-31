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
        private ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

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
                        if (this._obsWebSocket?.ReadyState != WebSocketState.Open) {
                            continue;
                        }
                        while (this._queue.TryDequeue(out var data)) {
                            Plugin.Log.Debug("Send Data.");
                            Plugin.Log.Debug($"{data}");
                            this._obsWebSocket?.Send(data);
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
            foreach (var b in this.behaviors) {
                b?.Respomce(e.Data, comp => { });
            }
        }

        private void BehavierInit(SocketBehavior wb)
        {
            this.behaviors.Add(wb);
            wb.Init((s, e) =>
            {
                Plugin.Log.Debug(e.Data);
                var json = JSON.Parse(e.Data);
                _queue.Enqueue(e.Data);
                OnMessageRecived?.Invoke(this, e);
                if (this._obsWebSocket?.ReadyState != WebSocketState.Open) {
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
