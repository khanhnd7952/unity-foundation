using UnityEngine;

namespace Pancake.DebugView
{
    public readonly struct Flick
    {
        public Flick(Vector2 touchStartScreenPosition, Vector2 flickStartScreenPosition, Vector2 endScreenPosition, Vector2 deltaInchPosition)
        {
            TouchStartScreenPosition = touchStartScreenPosition;
            FlickStartScreenPosition = flickStartScreenPosition;
            EndScreenPosition = endScreenPosition;
            DeltaInchPosition = deltaInchPosition;
        }

        public Vector2 TouchStartScreenPosition { get; }

        public Vector2 FlickStartScreenPosition { get; }
        public Vector2 EndScreenPosition { get; }
        public Vector2 DeltaInchPosition { get; }
    }
}