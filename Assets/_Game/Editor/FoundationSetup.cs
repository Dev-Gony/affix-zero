using UnityEditor;
using UnityEngine;
using AffixZero.Presentation;

namespace AffixZero.Editor
{
    public static class FoundationSetup
    {
        // Keep the existing menu entry, but do not create an empty scene as a game deliverable.
        [MenuItem("AFFIX/Setup/Create First Field Scene")]
        public static void CreateScene() => ProjectDashboard.Open();

        [MenuItem("AFFIX/Validate Selected Animation Set")]
        public static void ValidateAnimation()
        {
            var set = Selection.activeObject as ActorAnimationSet;
            if (set == null) { Debug.LogWarning("Select an ActorAnimationSet asset first."); return; }
            string problem = set.ValidateSet();
            if (problem == null) Debug.Log("Animation data checks passed. Source license, pivot, poses and visual approval remain separate.", set);
            else Debug.LogError(problem, set);
        }
    }
}
