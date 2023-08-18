using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KorMod.Modules
{
    public class OnDamageDealt : MonoBehaviour, IOnDamageDealtServerReceiver
    {
        float time;
        public void OnDamageDealtServer(DamageReport damageReport)
        {
            time = 0;
        }
        void Update()
        {
            time += Time.deltaTime;
        }
        public float GetTime()
        {
            return time;
        }
    }
}
