using MelonLoader;
using UnityEngine;
using System.Reflection;

[assembly: MelonInfo(typeof(CleanScreenMod.Main), "Clean Screen & Shader Killer", "1.9.0", "w3ntr")]
[assembly: MelonGame(null, null)]

namespace CleanScreenMod
{
    public class Main : MelonMod
    {
        private bool isHudHidden = false;
        private bool areHandsHidden = false;
        private bool areShadersDisabled = false;

        private KeyCode toggleHudKey = KeyCode.F5;       // Скрыть HUD
        private KeyCode toggleHandsKey = KeyCode.F6;    // Скрыть руки
        private KeyCode killShadersKey = KeyCode.F7;   // Вырубить кастомные шейдеры (по итогу не сработало)

        public override void OnUpdate()
        {
            // 1. HUD
            if (Input.GetKeyDown(toggleHudKey))
            {
                isHudHidden = !isHudHidden;
                ToggleAllCanvases(isHudHidden);
                MelonLogger.Msg($"[CleanScreen] HUD: {(isHudHidden ? "Hidden" : "Visible")}");
            }

            // 2. Руки
            if (Input.GetKeyDown(toggleHandsKey))
            {
                areHandsHidden = !areHandsHidden;
                ToggleCameraRenderers(areHandsHidden);
                MelonLogger.Msg($"[CleanScreen] Hands: {(areHandsHidden ? "Hidden" : "Visible")}");
            }

            // 3. Убийца кастомных визуальных эффектов (OnRenderImage / не обращать на это внимание, это неудачная попытка сделать игру оптимизированее))
            if (Input.GetKeyDown(killShadersKey))
            {
                areShadersDisabled = !areShadersDisabled;
                ToggleRenderImageEffects(areShadersDisabled);
                string status = areShadersDisabled ? "Disabled (RGB Shaders Killed)" : "Enabled";
                MelonLogger.Msg($"[CleanScreen] Render Image Shaders: {status}");
            }
        }

        private void ToggleAllCanvases(bool hide)
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas != null) canvas.enabled = !hide;
            }
        }

        private void ToggleCameraRenderers(bool hide)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Renderer[] renderers = mainCam.GetComponentsInChildren<Renderer>(true);
                foreach (var rend in renderers)
                {
                    if (rend.gameObject != mainCam.gameObject) rend.enabled = !hide;
                }
            }
        }

        private void ToggleRenderImageEffects(bool disable)
        {
            MonoBehaviour[] allScripts = Object.FindObjectsOfType<MonoBehaviour>();
            int killedCount = 0;

            foreach (var script in allScripts)
            {
                if (script != null)
                {
                    if (script.GetType().FullName.Contains("CleanScreenMod")) continue;

                    MethodInfo renderMethod = script.GetType().GetMethod("OnRenderImage",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (renderMethod != null)
                    {
                        var enabledProp = script.GetType().GetProperty("enabled");
                        if (enabledProp != null)
                        {
                            enabledProp.SetValue(script, !disable);
                            killedCount++;
                            MelonLogger.Msg($"[Shader Killer] Найден и переключен эффект OnRenderImage на объекте '{script.gameObject.name}' (Скрипт: {script.GetType().FullName}) -> {!disable}");
                        }
                    }
                }
            }

            MelonLogger.Msg($"[Shader Killer] Всего обработано экранных шейдеров: {killedCount}");
        }
    }
}