using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Presentation
{
    public sealed class FirstEncounter : MonoBehaviour
    {
        [SerializeField] private MeleeActor hero;
        [SerializeField] private MeleeActor enemy;
        private EncounterRewards rewards;
        private GUIStyle title, caption, body, value, button;
        private readonly Color ink = new Color(0.13f, 0.25f, 0.20f);
        private readonly Color paper = new Color(0.98f, 0.97f, 0.89f, 0.97f);
        private readonly Color green = new Color(0.18f, 0.47f, 0.30f);
        public int Experience => rewards == null ? 0 : rewards.Experience;
        public int Gold => rewards == null ? 0 : rewards.Gold;
        public int RewardCollectionCount => rewards == null ? 0 : rewards.CollectionCount;
        public MeleeActor Hero => hero;
        public MeleeActor Enemy => enemy;

        public void Configure(MeleeActor player, MeleeActor opponent) { hero = player; enemy = opponent; }

        private void Start()
        {
            rewards = new EncounterRewards(Guid.NewGuid().ToString("N"));
            if (enemy != null) enemy.Damaged += OnEnemyDamaged;
        }

        private void OnDestroy()
        {
            if (enemy != null) enemy.Damaged -= OnEnemyDamaged;
        }

        private void OnEnemyDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (receipt.Killed && hero != null && !hero.IsDead)
                rewards.TryCollect(rewards.EncounterId, actor.ActorId, actor.DeathCount, 25, 8);
        }

        private void OnGUI()
        {
            if (hero == null || enemy == null) return;
            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = Mathf.Min(Screen.width / 1200f, Screen.height / 720f);
            float offsetX = (Screen.width - 1200 * scale) * 0.5f;
            float offsetY = (Screen.height - 720 * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0), Quaternion.identity, Vector3.one * scale);
            try
            {
                Panel(new Rect(24, 24, 400, 89));
                GUI.Label(new Rect(42, 34, 360, 37), "AFFIX: ZERO", title);
                GUI.Label(new Rect(43, 77, 350, 24), "GREENFIELDS  /  FIRST HUNT", caption);
                DrawHealth(new Rect(24, 130, 270, 83), "YOUR HERO", hero, green);
                DrawHealth(new Rect(906, 24, 270, 83), "MELEE ENEMY", enemy, new Color(0.76f, 0.35f, 0.22f));

                Panel(new Rect(24, 598, 1152, 98));
                bool ended = hero.IsDead || enemy.IsDead;
                string status = !hero.IsReady || !enemy.IsReady ? "Preparing the field..."
                    : hero.IsDead ? "A new hunt awaits" : enemy.IsDead ? "Field cleared!" : "Auto hunt is active";
                string detail = hero.IsDead ? "Regroup and try again."
                    : enemy.IsDead && RewardCollectionCount > 0 ? "+25 XP and +8 gold collected automatically."
                    : "Your hero approaches and attacks automatically.";
                GUI.Label(new Rect(44, 611, 510, 32), status, value);
                GUI.Label(new Rect(44, 651, 540, 27), detail, body);
                GUI.Label(new Rect(605, 615, 200, 27), "XP  " + Experience, value);
                GUI.Label(new Rect(605, 650, 200, 27), "GOLD  " + Gold, body);
                if (ended && GUI.Button(new Rect(905, 623, 247, 46), "HUNT AGAIN", button))
                    RestartEncounter();
            }
            finally { GUI.matrix = previousMatrix; GUI.color = previousColor; }
        }

        public void RestartEncounter()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else Debug.LogError("Add the encounter scene to Build Settings before restarting.", this);
        }

        private void DrawHealth(Rect area, string name, MeleeActor actor, Color fill)
        {
            Panel(area);
            GUI.Label(new Rect(area.x + 16, area.y + 10, 144, 23), name, caption);
            GUI.Label(new Rect(area.x + 164, area.y + 10, 92, 23), actor.Hp + " / " + actor.MaxHp, body);
            Rect bar = new Rect(area.x + 16, area.y + 43, area.width - 32, 17);
            Fill(bar, new Color(0.83f, 0.84f, 0.74f));
            bar.width *= Mathf.Clamp01(actor.Hp / (float)actor.MaxHp);
            Fill(bar, fill);
        }

        private void Panel(Rect area)
        {
            Fill(new Rect(area.x, area.y + 4, area.width, area.height), new Color(0.15f, 0.26f, 0.17f, 0.18f));
            Fill(area, paper);
            Fill(new Rect(area.x, area.y, 4, area.height), green);
        }

        private static void Fill(Rect area, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private GUIStyle TextStyle(int size, FontStyle weight = FontStyle.Normal)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight };
            style.normal.textColor = ink;
            return style;
        }

        private void EnsureStyles()
        {
            if (title != null) return;
            title = TextStyle(28, FontStyle.Bold);
            caption = TextStyle(13, FontStyle.Bold);
            body = TextStyle(16);
            value = TextStyle(22, FontStyle.Bold);
            button = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            button.normal.textColor = ink;
        }
    }
}
