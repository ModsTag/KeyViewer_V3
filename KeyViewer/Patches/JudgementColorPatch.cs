using HarmonyLib;
using KeyViewer.API;
using KeyViewer.ModsTagVarArray;
using SkyHook;
using System.Linq;
using UnityEngine;

namespace KeyViewer.Patches
{
    [HarmonyPatch]
    public static class JudgementColorPatch
    {
        static bool _initialized = false;
        static SPSCCircularQueue<Key> _keys = new SPSCCircularQueue<Key>(4);
        public static void Init()
        {
            if (_initialized) return;
            SkyHookManager.KeyUpdated.AddListener(she =>
            {
                try
                {
                    if (!AsyncInputManager.isActive) return;
                    if (Main.KeyManager == null) return;
                    if (she.Type == SkyHook.EventType.KeyReleased) return;
                    if (Main.IsEnabled && (scrController.instance?.gameworld ?? false))
                    {
                        var code = AsyncInputCompat.Convert(she.Label);
                        if (Main.KeyManager.Codes.Contains(code))
                        {
                            _keys.Enqueue(Main.KeyManager[code]);
                        }
                    }
                }
                catch { }
            });
            _initialized = true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scrPlayer), "ValidInputWasTriggered")]
        public static void SyncInput_StackPushPatch(scrController __instance)
        {
            if (!_initialized) return;
            if (AsyncInputManager.isActive) return;
            foreach (var code in Main.KeyManager.Codes)
                if (Input.GetKeyDown(code))
                    _keys.Enqueue(Main.KeyManager[code]);
        }
        static bool stackFlushed = false;
        static bool rainRestored = false;
        static States prevState = States.None;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scrController), "Update")]
        public static void StackFlushPatch(scrController __instance)
        {
            if (!_initialized) return;
            var state = __instance.state;
            if (!stackFlushed && state == States.PlayerControl)
            {
                _keys.Clear();
                stackFlushed = true;
                rainRestored = false;
            }
            if (!rainRestored && state != prevState)
            {
                foreach (var activeKey in Main.KeyManager.Keys)
                {
                    if (activeKey.config.RainEnabled)
                    {
                        int count = activeKey.rains.Count;
                        for (int i = 0; i < count; i++)
                            activeKey.rains[i].image.color = activeKey.config.RainConfig.RainColor;
                    }
                }
                rainRestored = true;
                prevState = state;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scrController), "Awake_Rewind")]
        public static void StackFlushPatch2(scrController __instance)
        {
            if (!_initialized) return;
            stackFlushed = false;
        }
        [Comment("Change Key's Background Color")]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scrMarginTracker), "AddHit")]
        public static void ChangeColorPatch(HitMargin hit)
        {
            if (!_initialized) return;
            while (_keys.TryDequeue(out var key))
            {
                key.ChangeHitMarginColor(hit);
            }
        }
    }
}
