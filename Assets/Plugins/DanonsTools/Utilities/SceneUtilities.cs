using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace DanonsTools.Utilities
{
    public static class SceneUtilities
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
        
        public static GameObject InstantiateIntoScene(in GameObject original, in Vector3 position, in Quaternion rotation, in string sceneName)
        {
            var obj = Object.Instantiate(original, position, rotation);
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName(sceneName));
            return obj;
        }
        
        public static GameObject InstantiateIntoScene(in GameObject original, in string sceneName)
        {
            return InstantiateIntoScene(original, default, Quaternion.identity, sceneName);
        }

        public static GameObject CreateGameObjectInScene(in string objName, in string sceneName)
        {
            var obj = new GameObject(objName);
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName(sceneName));
            return obj;
        }
        
        public static GameObject CreateGameObjectInScene(in string sceneName)
        {
            var obj = new GameObject();
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName(sceneName));
            return obj;
        }
    }
}