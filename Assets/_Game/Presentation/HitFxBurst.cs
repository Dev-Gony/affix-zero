using System;
using System.Linq;
using UnityEngine;

namespace AffixZero.Presentation
{
    public sealed class HitFxBurst : MonoBehaviour
    {
        private const float FramesPerSecond = 20f;
        private static Sprite[] cachedFrames;
        private static int liveCount;
        private SpriteRenderer view;
        private float elapsed;

        public static void Spawn(Vector3 position, int sortingOrder,bool critical=false,bool killed=false,bool area=false)
        {
            if(liveCount>=32)return;
            Sprite[] frames = Frames();
            if (frames.Length != 8)
            {
                Debug.LogError("AFFIX impact FX requires exactly eight imported frames.");
                return;
            }
            var effect = new GameObject("Melee Impact FX", typeof(SpriteRenderer), typeof(HitFxBurst));
            effect.transform.position = position;
            var renderer = effect.GetComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sortingOrder = sortingOrder;
            float scale=killed?1.5f:critical?1.28f:area?1.12f:1f;
            effect.transform.localScale=Vector3.one*scale;
            renderer.color=killed?new Color(1f,.48f,.22f,1):critical?new Color(1f,.96f,.55f,1):Color.white;
            liveCount++;
        }

        private static Sprite[] Frames()
        {
            if (cachedFrames == null || cachedFrames.Length == 0)
                cachedFrames = Resources.LoadAll<Sprite>("AffixOriginal/ImpactFxAtlas-v1")
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
            return cachedFrames;
        }

        private void Awake() => view = GetComponent<SpriteRenderer>();

        private void OnDestroy(){liveCount=Mathf.Max(0,liveCount-1);}

        private void Update()
        {
            Sprite[] frames = Frames();
            elapsed += Time.deltaTime;
            int index = Mathf.FloorToInt(elapsed * FramesPerSecond);
            if (index >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }
            view.sprite = frames[index];
        }
    }
}
