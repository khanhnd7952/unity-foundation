using Pancake.Monetization;
using Pancake.Scriptable;
using UnityEngine;

namespace _Project.Scripts.Advertisement
{
    public class SampleSceneManager : MonoBehaviour
    {
        [SerializeField] private ApplovinInterVariable interVariable;
        [SerializeField] private ScriptableEventString changeNetworkEvent;

        [ContextMenu("Show Inter")]
        public void ShowInter()
        {
            Debug.Log("Show Interstitial");
            interVariable.Show();
        }

        [ContextMenu("Change Admob")]
        public void ChangeAdmob()
        {
            changeNetworkEvent.Raise("admob");
        }
    }
}