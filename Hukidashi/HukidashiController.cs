using BeatSaberMarkupLanguage;
using HMUI;
using Hukidashi.Configuration;
using Hukidashi.SimpleJson;
using Hukidashi.WebSockets;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Hukidashi
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class HukidashiController : MonoBehaviour, IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        private void Update()
        {
            if (!this._lookTarget) {
                return;
            }
            this._hukidashiCanvas.transform.LookAt(this._lookTarget.transform.position);
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{this.name}: OnDestroy()");
            if (this._hukidashiCanvas != null) {
                Destroy(this._hukidashiCanvas.gameObject);
            }
        }

        protected void OnDisable()
        {
            if (this._hukidashiCanvas != null) {
                this._hukidashiCanvas.gameObject.SetActive(false);
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            try {
                this._server.OnMessageRecived -= this.OnMessageRecived;
                this._server.OnMessageRecived += this.OnMessageRecived;
                PluginConfig.Instance.OnChanged -= this.OnConfigChanged;
                PluginConfig.Instance.OnChanged += this.OnConfigChanged;
                SceneManager.activeSceneChanged -= this.SceneManager_activeSceneChanged;
                SceneManager.activeSceneChanged += this.SceneManager_activeSceneChanged;
                this._isInGame = SceneManager.GetActiveScene().name == "GameCore";
                if (this._hukidashiCanvas == null) {
                    var go = new GameObject("HukidashiCanvas", typeof(Canvas), typeof(CurvedCanvasSettings));
                    this._hukidashiCanvas = go.GetComponent<Canvas>();
                    this._hukidashiCanvas.gameObject.layer = 5;
                    this._hukidashiCanvas.renderMode = RenderMode.WorldSpace;
                    this._hukidashiCanvas.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    (this._hukidashiCanvas.transform as RectTransform).sizeDelta = new Vector2(160, 90);
                    var curveSetting = go.GetComponent<CurvedCanvasSettings>();
                    curveSetting.SetRadius(0);
                    var imagego = new GameObject("HukidashiImage", typeof(ImageView));
                    this._hukidashiImage = imagego.GetComponent<ImageView>();
                    var assembly = Assembly.GetExecutingAssembly();
                    using (var st = assembly.GetManifestResourceStream("Hukidashi.Resouces.hukidasi.png")) {
                        var tex = new Texture2D(2, 2);
                        var rowData = new byte[st.Length];
                        st.Read(rowData, 0, (int)st.Length);
                        tex.LoadImage(rowData);
                        var rect = new Rect(Vector2.zero, new Vector2(tex.width, tex.height));
                        this._hukidashiImage.sprite = Sprite.Create(tex, rect, Vector2.one / 2);
                        this._hukidashiImage.material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlow");

                        this._hukidashiImage.transform.SetParent(this._hukidashiCanvas.transform as RectTransform, false);

                        this._hukidashiImage.rectTransform.sizeDelta = new Vector2(160, 90);
                        var vertical = new GameObject().AddComponent<VerticalLayoutGroup>();
                        vertical.transform.SetParent(this._hukidashiImage.transform as RectTransform, false);

                        this._text = new GameObject().AddComponent<CurvedTextMeshPro>();
                        if (FontManager.TryGetTMPFontByFamily("Segoe UI", out var font)) {
                            this._text.font = GameObject.Instantiate(font);
                        }
                        this._text.transform.SetParent(vertical.transform as RectTransform, false);
                        this._text.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                        this._text.rectTransform.sizeDelta = new Vector2(130, 10);
                        this._text.font.material.shader = BeatSaberUI.MainTextFont.material.shader;
                        this._text.enableWordWrapping = true;
                        this._text.color = Color.black;
                        this._text.enableAutoSizing = true;
                        this._text.autoSizeTextContainer = false;
                        this._text.fontSizeMin = 0.3f;
                        this._text.fontSizeMax = 20;
                        this._text.margin = new Vector4(3, 14, 3, 14);
                        this._text.ForceMeshUpdate(true);

                        this._hukidashiCanvas.transform.SetParent(Camera.main.transform, false);
                        this.UpdateHukidashi(this._isInGame);

                        this._hukidashiCanvas.gameObject.SetActive(false);

                        HMMainThreadDispatcher.instance.Enqueue(this.SerchCamera());
                    }
                }
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
            }
        }
        [Inject]
        public void Constractor(HukidashiWebSocketServer server)
        {
            this._server = server;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnMessageRecived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (!this.isActiveAndEnabled) {
                return;
            }

            var json = JSON.Parse(e.Data);
            if (!this._text) {
                return;
            }
            if (json?.IsNull == false) {
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    if (json["type"].Value == "Show") {
                        this._text.text = json["text"].Value;
                        this._text.SetAllDirty();
                        this._hukidashiCanvas.gameObject.SetActive(true);

                    }
                    else {
                        this._text.text = json["text"].Value;
                        this._hukidashiCanvas.gameObject.SetActive(false);
                    }
                });
            }
        }

        private IEnumerator SerchCamera(bool isInGame = false)
        {
            if (isInGame) {
                if (string.IsNullOrEmpty(PluginConfig.Instance.GameTargetCameraName)) {
                    this._lookTarget = Camera.main;
                }
                else {
                    yield return new WaitWhile(() => Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.GameTargetCameraName) == null);
                    this._lookTarget = this._lookTarget = Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.GameTargetCameraName);
                }
            }
            else {
                if (string.IsNullOrEmpty(PluginConfig.Instance.MenuTargetCameraName)) {
                    this._lookTarget = Camera.main;
                }
                else {
                    yield return new WaitWhile(() => Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.MenuTargetCameraName) == null);
                    this._lookTarget = this._lookTarget = Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.MenuTargetCameraName);
                }
            }
        }
        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            this._isInGame = arg1.name == "GameCore";
            HMMainThreadDispatcher.instance.Enqueue(this.SerchCamera(this._isInGame));
        }
        private void OnConfigChanged(PluginConfig obj)
        {
            this.UpdateHukidashi(this._isInGame);
        }

        private void UpdateHukidashi(bool isInGame = false)
        {
            if (isInGame) {
                this._hukidashiCanvas.transform.localScale = Vector3.one * PluginConfig.Instance.GameHukidashiScale;
                this._hukidashiCanvas.transform.localScale = new Vector3(-this._hukidashiCanvas.transform.localScale.x, this._hukidashiCanvas.transform.localScale.y, this._hukidashiCanvas.transform.localScale.z);
                this._hukidashiCanvas.transform.localPosition = new Vector3(PluginConfig.Instance.GameHukidashiPosX, PluginConfig.Instance.GameHukidashiPosY, PluginConfig.Instance.GameHukidashiPosZ);
            }
            else {
                this._hukidashiCanvas.transform.localScale = Vector3.one * PluginConfig.Instance.MenuHukidashiScale;
                this._hukidashiCanvas.transform.localScale = new Vector3(-this._hukidashiCanvas.transform.localScale.x, this._hukidashiCanvas.transform.localScale.y, this._hukidashiCanvas.transform.localScale.z);
                this._hukidashiCanvas.transform.localPosition = new Vector3(PluginConfig.Instance.MenuHukidashiPosX, PluginConfig.Instance.MenuHukidashiPosY, PluginConfig.Instance.MenuHukidashiPosZ);
            }
            HMMainThreadDispatcher.instance.Enqueue(this.SerchCamera(isInGame));
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private HukidashiWebSocketServer _server;
        private Canvas _hukidashiCanvas;
        private ImageView _hukidashiImage;
        private CurvedTextMeshPro _text;
        private Camera _lookTarget;
        private bool _isInGame;
        private bool _disposedValue;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    PluginConfig.Instance.OnChanged -= this.OnConfigChanged;
                    SceneManager.activeSceneChanged -= this.SceneManager_activeSceneChanged;
                    if (this._server != null) {
                        this._server.OnMessageRecived -= this.OnMessageRecived;
                    }
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
        #endregion
    }
}
