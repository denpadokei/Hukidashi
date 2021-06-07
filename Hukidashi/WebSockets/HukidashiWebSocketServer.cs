using Hukidashi.Configuration;
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
        private WebSocketServer _webSocketServer;
        public event OnRecieveMessageHandler OnMessageRecived;
        private readonly static object _lockobject = new object();

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

        private bool disposedValue;
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this._webSocketServer?.Stop();
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
