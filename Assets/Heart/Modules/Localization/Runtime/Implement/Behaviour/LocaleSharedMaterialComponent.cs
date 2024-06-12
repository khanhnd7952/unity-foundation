using TMPro;

namespace Pancake.Localization
{
    using UnityEngine;

    [EditorIcon("csharp")]
    public class LocaleSharedMaterialComponent : LocaleComponentGeneric<LocaleMaterial, Material>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Renderer>("sharedMaterial");
            TrySetComponentAndPropertyIfNotSet<TMP_Text>("fontSharedMaterial");
        }
    }

}