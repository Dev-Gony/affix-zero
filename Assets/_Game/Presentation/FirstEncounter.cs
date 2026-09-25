using UnityEngine;

namespace AffixZero.Presentation
{
    // Minimal U1 runtime UI. No fabricated art, currencies, save reads or victory claims.
    public sealed class FirstEncounter : MonoBehaviour
    {
        [SerializeField] private MeleeActor hero;
        [SerializeField] private MeleeActor enemy;
        public void Configure(MeleeActor player, MeleeActor opponent) { hero = player; enemy = opponent; }
        private void OnGUI()
        {
            if (hero == null || enemy == null) return;
            GUILayout.BeginArea(new Rect(16, 16, 330, 105), GUI.skin.box);
            GUILayout.Label("AFFIX: ZERO | First Encounter");
            GUILayout.Label("Hero " + hero.Hp + "/" + hero.MaxHp + "    Enemy " + enemy.Hp + "/" + enemy.MaxHp);
            if (hero.IsDead || enemy.IsDead)
            {
                GUILayout.Label(hero.IsDead ? "Defeat" : "Encounter cleared");
                if (GUILayout.Button("Restart encounter"))
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            GUILayout.EndArea();
        }
    }
}
