using UnityEngine;
using Hyperplasia.Runtime;

namespace Hyperplasia.Presenter
{
    public class GameObjectRegistry : MonoBehaviour
    {
        [Tooltip("如果有对应数据层对象，请填写对应的ID，否则填写大于1000的唯一ID")]
        [SerializeField] private int _id;

        void Start()
        {
            GameBootstrap.Register(_id, gameObject);
        }

        void OnDestroy()
        {
            GameBootstrap.Unregister(_id, gameObject);
        }
    }
}