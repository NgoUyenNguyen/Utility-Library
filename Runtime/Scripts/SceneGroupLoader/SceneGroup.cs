using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;

namespace NgoUyenNguyen
{
    [Serializable]
    public struct SceneGroup
    {
        public string groupName;
        public SceneReference activeScene;
        public List<SceneReference> additiveScenes;

        public IEnumerable<SceneReference> AllScenes
        {
            get
            {
                yield return activeScene;
                foreach (var additiveScene in additiveScenes) yield return additiveScene;
            }
        }

        public bool HasScene(string sceneName)
        {
            if (activeScene != null && activeScene.Name == sceneName) return true;
            return additiveScenes.Any(additiveScene => additiveScene != null && additiveScene.Name == sceneName);
        }
    }
}