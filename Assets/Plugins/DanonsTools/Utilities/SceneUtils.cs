using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace DanonsTools.Utilities
{
    public static class SceneUtils
    {
        public static Scene CreateScene(in string sceneName)
        {
            return SceneManager.CreateScene(sceneName);
        }
        
        public static async UniTask LoadSceneAsync(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
        }

        public static async UniTask LoadAddressableSceneAsync(string sceneAddress)
        {
            await Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive);
        }

        public static async UniTask UnloadSceneAsync(string sceneName)
        {
            await SceneManager.UnloadSceneAsync(sceneName).ToUniTask();
        }

        public static async UniTask SetActiveScene(string sceneName)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            await UniTask.Yield();
        }
    }
}