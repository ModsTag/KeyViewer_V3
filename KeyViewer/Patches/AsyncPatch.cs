using HarmonyLib;
using KeyViewer.API;
using SkyHook;

namespace KeyViewer.Patches
{
    [HarmonyPatch(typeof(AsyncInputManager), "Setup")]
    public static class AsyncPatch
    {
        public static void Postfix()
        {
            SkyHookManager.KeyUpdated.AddListener((SkyHookEvent she) => { InputAPI.UpdateKeyState(AsyncInputCompat.Convert(she.Label), she.Type == EventType.KeyPressed); });
        }
    }
}
