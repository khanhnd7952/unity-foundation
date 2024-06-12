using System;
using Pancake.Monetization;
using Pancake.Scriptable;
using UnityEngine;

namespace _Project.Scripts.Advertisement
{
    public class SampleSceneManager : MonoBehaviour
    {
        [SerializeField] private AdSettings adSettings;
        [SerializeField] private ScriptableEventString changeNetworkEvent;

        private void Start()
        {
        }

        public void ShowInter()
        {
            Debug.Log("Show Interstitial");
            adSettings.ApplovinInter.Show();
        }

        public void ShowReward()
        {
            Debug.Log("Show Reward");
            adSettings.ApplovinReward.Show();
        }

        public void ChangeAdmob()
        {
            changeNetworkEvent.Raise("admob");
        }
    }
}