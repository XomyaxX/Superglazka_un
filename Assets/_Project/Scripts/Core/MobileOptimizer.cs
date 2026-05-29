using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Superglazka.Core
{
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Mobile Optimizations")]
        [SerializeField] private bool _applyOnStart = true;
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private UniversalRenderPipelineAsset _mobileAsset;
        [SerializeField] private UniversalRenderPipelineAsset _desktopAsset;

        [Header("Quality")]
        [SerializeField] private int _mobileQualityLevel = 1;
        [SerializeField] private int _desktopQualityLevel = 2;

        private void Start()
        {
            if (!_applyOnStart) return;
            ApplyOptimizations();
        }

        public void ApplyOptimizations()
        {
            bool isMobile = Application.isMobilePlatform;

            // Frame rate
            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Quality settings
            QualitySettings.SetQualityLevel(isMobile ? _mobileQualityLevel : _desktopQualityLevel, true);

            // URP asset swap
            if (isMobile && _mobileAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = _mobileAsset;
                QualitySettings.renderPipeline = _mobileAsset;
            }
            else if (!isMobile && _desktopAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = _desktopAsset;
                QualitySettings.renderPipeline = _desktopAsset;
            }

            // Memory
            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            // Disable unused modules
            if (isMobile)
            {
                // Reduce physics iterations
                Physics.defaultSolverIterations = 4;
                Physics2D.defaultSolverIterations = 4;
                // Disable real-time GI
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.softParticles = false;
                QualitySettings.softVegetation = false;
            }
        }

        public void SetLowPowerMode(bool enabled)
        {
            if (enabled)
            {
                Application.targetFrameRate = 30;
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            else
            {
                Application.targetFrameRate = _targetFrameRate;
            }
        }
    }
}
