using System;
using UnityEngine;
using System.IO;
using UnityEditor.Recorder.Input;
using System.Collections;
using UnityEngine.UI;
using UIs.Base;

namespace UnityEditor.Recorder.Examples
{
    [Serializable]
    public class ScreenShotData
    {
        public string name;
        public int width;
        public int height;
    }

    public class CaptureScreenShot : MonoBehaviour
    {
        RecorderController m_RecorderController;

        [SerializeField]
        ScreenShotData[] ScreenShotDatas;

        private void Setting(string name, int width, int height)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            m_RecorderController = new RecorderController(settings);
            var mediaOutputFolder = Path.Combine(Application.dataPath, "../", "ScreenShot"); // 스크린샷 저장 경로 지정

            var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorder.name = name;
            imageRecorder.Enabled = true;
            imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorder.CaptureAlpha = false;

            imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, name + "_" + width + "_" + height);
            imageRecorder.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height,
            };

            settings.AddRecorderSettings(imageRecorder);
            settings.SetRecordModeToSingleFrame(0);

            Debug.Log($"{imageRecorder.OutputFile}에 스크린샷이 저장되었습니다.");
        }

        private void OnCapture()
        {
            StartCoroutine(Capture());
        }

        IEnumerator Capture()
        {
            foreach (ScreenShotData data in ScreenShotDatas)
            {
                // 원래 해상도와 캔버스스케일러 상태 저장
                int originalWidth = Screen.width;
                int originalHeight = Screen.height;
                var canvasScaler = FindFirstObjectByType<CanvasScaler>();
                var safeArea = FindFirstObjectByType<SafeArea>();
                float originalMatch = 0.5f;
                if (canvasScaler != null)
                    originalMatch = canvasScaler.matchWidthOrHeight;

                // 임시 해상도 및 캔버스스케일러 비율 설정
                Screen.SetResolution(data.width, data.height, false);
                if (canvasScaler != null)
                    canvasScaler.matchWidthOrHeight = (float)data.width / data.height > 1.7f ? 1f : 0f;

                Setting(data.name, data.width, data.height);
                m_RecorderController.PrepareRecording();
                safeArea?.SetSafeArea();
                Canvas.ForceUpdateCanvases();
                m_RecorderController.StartRecording();
                yield return null;

                // 캡쳐 후 원복
                if (canvasScaler != null)
                    canvasScaler.matchWidthOrHeight = originalMatch;
                Screen.SetResolution(originalWidth, originalHeight, false);
            }
        }
    }
}