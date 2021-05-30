using HMUI;
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
            this._hukidashiCanvas.transform.LookAt(Camera.main.transform);
#if DEBUG
            if (Input.GetKeyDown(KeyCode.N)) {
                if (this.shaders.Count <= (uint)this.shaderIndex) {
                    this.shaderIndex = 0;
                }
                var shader = this.shaders.ElementAt(shaderIndex);
                Plugin.Log.Debug($"index {shaderIndex}, {shader}");
                this._hukidashiImage.material.shader = shader;
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
                        this.shaders = Resources.FindObjectsOfTypeAll<ImageView>().OrderBy(x => x.name).Select(x => x.material.shader).ToList();
#endif
                        this._hukidashiImage.material.shader = Resources.FindObjectsOfTypeAll<Shader>().FirstOrDefault(x => x.name.Contains("CustomUI"));
                        this._hukidashiImage.transform.SetParent(_hukidashiCanvas.transform as RectTransform, false);
                        var vertival = new GameObject().AddComponent<VerticalLayoutGroup>();
                        vertival.transform.SetParent(this._hukidashiImage.transform as RectTransform, false);
                        this._text = new GameObject().AddComponent<CurvedTextMeshPro>();
                        this._text.transform.SetParent(vertival.transform as RectTransform, false);
                        this._text.color = Color.black;

                        this._hukidashiCanvas.transform.SetParent(Camera.main.transform, false);
                        this._hukidashiCanvas.transform.localScale *= 0.01f;
                        this._hukidashiCanvas.transform.localPosition = new Vector3(0, 0, 1);
                    }
                }
            }
            catch (Exception e) {

            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        [Inject]
        public void Init(HukidashiWebSocketServer server)
        {
            this._server = server;
            this._server.OnMessageRecived += this.OnMessageRecived;
        }

        private void OnMessageRecived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            //Plugin.Log.Debug($"{e.Data}");
            //var json = JSON.Parse(e.Data);

        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private HukidashiWebSocketServer _server;
        private Canvas _hukidashiCanvas;
        private ImageView _hukidashiImage;
        private CurvedTextMeshPro _text;
#if DEBUG
        private List<Shader> shaders;
        private int shaderIndex;
#endif
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        #endregion
    }
}
