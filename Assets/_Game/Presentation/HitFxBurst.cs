using System;
using System.Linq;
using UnityEngine;

namespace AffixZero.Presentation
{
    public sealed class HitFxBurst : MonoBehaviour
    {
        private const float FramesPerSecond = 20f;
        private static Sprite[] cachedFrames;
        private SpriteRenderer view;
        private float elapsed;

        public static void Spawn(Vector3 position, int sortingOrder)
        {
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
        }

        private static Sprite[] Frames()
        {
            if (cachedFrames == null || cachedFrames.Length == 0)
                cachedFrames = Resources.LoadAll<Sprite>("AffixOriginal/ImpactFxAtlas-v1")
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
            return cachedFrames;
        }

        private void Awake() => view = GetComponent<SpriteRenderer>();

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
