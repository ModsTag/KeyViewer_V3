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
            SkyHookManager.KeyUpdated.AddListener(she => InputAPI.SetKeyState(AsyncInputCompat.Convert(she.Label), she.Type == EventType.KeyPressed, she.GetTimeInTicks()));
        }
    }
}
