using UnityEngine;

namespace Hyperplasia.Utils
{
    public class GOBase
    {
        public static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        public static void SetActive(Component component, bool active)
        {
            if (component != null)
            {
                component.gameObject.SetActive(active);
            }
        }
    }
}