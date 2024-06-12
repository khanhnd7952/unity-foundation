﻿using UnityEngine;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Base classes of all ScriptableObjects
    /// </summary>
    [Searchable]
    public abstract class ScriptableBase : ScriptableObject
    {
        public virtual void Reset() { }
        public System.Action repaintRequest;
        [HideInInspector] public int categoryIndex;
        [HideInInspector] public string description;
        public virtual System.Type GetGenericType { get; }
    }
}