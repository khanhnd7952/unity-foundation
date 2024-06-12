﻿using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_float.asset", menuName = "Pancake/Scriptable/Variables/float")]
    [EditorIcon("scriptable_variable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        [Tooltip("Clamps the value of this variable to a minimum and maximum.")] [SerializeField]
        private bool isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField, ShowIf(nameof(isClamped)), Indent]
        private Vector2 minMax = new(0, 100);

        public bool IsClamped => isClamped;
        public Vector2 MinMax { get => minMax; set => minMax = value; }
        public float Min { get => minMax.x; set => minMax.x = value; }
        public float Max { get => minMax.y; set => minMax.y = value; }

        /// <summary>
        /// Returns the percentage of the value between the minimum and maximum.
        /// </summary>
        public float Ratio => Mathf.InverseLerp(minMax.x, minMax.y, value);

        public override void Save()
        {
            Data.Save(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            Value = Data.Load(Guid, InitialValue);
        }

        public void Add(float value) { Value += value; }

        public override float Value
        {
            get => value;
            set
            {
                float clampedValue = isClamped ? Mathf.Clamp(value, minMax.x, minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (isClamped)
            {
                float clampedValue = Mathf.Clamp(value, minMax.x, minMax.y);
                if (value < clampedValue || value > clampedValue) value = clampedValue;
            }

            if (value.Approximately(PreviousValue)) return;
            ValueChanged();
        }
#endif
    }
}