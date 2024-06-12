using System;
using Pancake.Scriptable;

namespace Pancake.Localization
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "event_localetext.asset", menuName = "Pancake/Localization/Events/locale text")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventLocaleText : ScriptableEvent<LocaleText>
    {
    }
}