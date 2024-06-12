﻿using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_string.asset", menuName = "Pancake/Scriptable/Constants/string")]
    [EditorIcon("scriptable_const")]
    [HideMonoScript]
    [System.Serializable]
    public class StringConstant : ScriptableConstant<string>
    {
    }
}