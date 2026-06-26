using KeyViewer.API;
using UnityEngine;

namespace KeyViewer
{
    public static class ReplayInput
    {
        public static void OnStartInputs() => InputAPI.Lock = true;
        public static void OnEndInputs() => InputAPI.Lock = false;
        public static void OnKeyPressed(KeyCode key) => InputAPI.UpdateLockKeyState(key, true);
        public static void OnKeyReleased(KeyCode key) => InputAPI.UpdateLockKeyState(key, false);
    }
}
