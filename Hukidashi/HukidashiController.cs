﻿using BeatSaberMarkupLanguage;
using HMUI;
using Hukidashi.Configuration;
using Hukidashi.SimpleJson;
using Hukidashi.WebSockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Hukidashi
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class HukidashiController : MonoBehaviour, IInitializable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            Plugin.Log?.Debug($"{name}: Awake()");
        }

        private void Update()
        {
            if (!_lookTarget) {
                return;
            }
            this._hukidashiCanvas.transform.LookAt(_lookTarget.transform.position);
#if DEBUG
            if (Input.GetKeyDown(KeyCode.N)) {
                if (this.shaders.Count <= (uint)this.shaderIndex) {
                    this.shaderIndex = 0;
                }
                var shader = this.shaders.ElementAt(shaderIndex);
                Plugin.Log.Debug($"index {shaderIndex}, {shader}");
                this._hukidashiImage.material = shader;
                shaderIndex++;
            }
#endif
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            PluginConfig.Instance.OnChanged -= this.OnConfigChanged;
            this._server.OnMessageRecived -= this.OnMessageRecived;
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
#if DEBUG
                        this.shaders = Resources.FindObjectsOfTypeAll<Material>().OrderBy(x => x.name).ToList();
#endif
                        this._hukidashiImage.material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlow");
                        
                        this._hukidashiImage.transform.SetParent(_hukidashiCanvas.transform as RectTransform, false);
                        
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
                        this._hukidashiCanvas.transform.localScale = Vector3.one * PluginConfig.Instance.HukidashiScale;
                        this._hukidashiCanvas.transform.localScale = new Vector3(-this._hukidashiCanvas.transform.localScale.x, this._hukidashiCanvas.transform.localScale.y, this._hukidashiCanvas.transform.localScale.z);
                        this._hukidashiCanvas.transform.localPosition = new Vector3(PluginConfig.Instance.HukidashiPosX, PluginConfig.Instance.HukidashiPosY, PluginConfig.Instance.HukidashiPosZ);

                        this._hukidashiCanvas.gameObject.SetActive(false);

                        PluginConfig.Instance.OnChanged += this.OnConfigChanged;

                        HMMainThreadDispatcher.instance.Enqueue(this.SerchCamera());
                    }
                }
            }
            catch (Exception e) {

            }
        }
        [Inject]
        public void Constractor(HukidashiWebSocketServer server)
        {
            this._server = server;
            this._server.OnMessageRecived += this.OnMessageRecived;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnMessageRecived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            var json = JSON.Parse(e.Data);
            if (json["request-type"].Value != "SetTextGDIPlusProperties") {
                return;
            }
            if (json["source"] != PluginConfig.Instance.OBSSouceName) {
                return;
            }
            if (this._text) {
                if (string.IsNullOrEmpty(json["text"])) {
                    this._hukidashiCanvas.gameObject.SetActive(false);
                }
                else {
                    this._text.text = json["text"].Value;
                    this._text.SetAllDirty();
                    this._hukidashiCanvas.gameObject.SetActive(true);
                }
            }
        }

        private IEnumerator SerchCamera()
        {
            if (string.IsNullOrEmpty(PluginConfig.Instance.TargetCameraName)) {
                this._lookTarget = Camera.main;
            }
            yield return new WaitWhile(() => Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.TargetCameraName) == null);
            this._lookTarget = this._lookTarget = Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == PluginConfig.Instance.TargetCameraName);
        }

        private void OnConfigChanged(PluginConfig obj)
        {
            this._hukidashiCanvas.transform.localScale = Vector3.one * obj.HukidashiScale;
            this._hukidashiCanvas.transform.localScale = new Vector3(-this._hukidashiCanvas.transform.localScale.x, this._hukidashiCanvas.transform.localScale.y, this._hukidashiCanvas.transform.localScale.z);
            this._hukidashiCanvas.transform.localPosition = new Vector3(PluginConfig.Instance.HukidashiPosX, PluginConfig.Instance.HukidashiPosY, PluginConfig.Instance.HukidashiPosZ);
            HMMainThreadDispatcher.instance.Enqueue(this.SerchCamera());

        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private HukidashiWebSocketServer _server;
        private Canvas _hukidashiCanvas;
        private ImageView _hukidashiImage;
        private CurvedTextMeshPro _text;
        private Camera _lookTarget;
#if DEBUG
        private List<Material> shaders;
        private int shaderIndex;
#endif
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        #endregion
    }
}
