using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gota.EditorTools
{
    /// <summary>
    /// Ferramenta de desenvolvimento: grava quadros da partida (PNG) para montar o GIF do README.
    /// Menu: Gota > Gravar quadros de gameplay (em Play Mode).
    /// Um "piloto automático" simples troca de parede quando um obstáculo se aproxima.
    /// </summary>
    public static class GameplayCapture
    {
        private const int Fps = 12;
        private const float Seconds = 12f;
        private const int Width = 270;
        private const int Height = 480;

        private static int frame;
        private static float nextCaptureTime;
        private static string folder;
        private static bool recording;

        [MenuItem("Gota/Gravar quadros de gameplay")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Entre em Play Mode antes de gravar.");
                return;
            }

            folder = Path.Combine(Application.dataPath, "../Temp/gif");
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            frame = 0;
            nextCaptureTime = 0f;
            recording = true;
            PlayerPrefs.SetInt("Gota.HintShown", 0); // mostra a dica no GIF
            GameStateMachine.Instance.StartGame();
            EditorApplication.update += Tick;
            Debug.Log("Gravando quadros em " + folder);
        }

        [MenuItem("Gota/Parar gravação")]
        public static void Stop()
        {
            recording = false;
            EditorApplication.update -= Tick;
            Debug.Log("Gravação parada. Quadros: " + frame);
        }

        private static void Tick()
        {
            if (!recording || !EditorApplication.isPlaying) { Stop(); return; }

            AutoPilot();

            if (Time.time < nextCaptureTime) return;
            nextCaptureTime = Time.time + 1f / Fps;

            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            var small = Downscale(tex, Width, Height);
            File.WriteAllBytes(Path.Combine(folder, $"frame_{frame:0000}.png"), small.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(small);
            frame++;

            if (frame >= Fps * Seconds) Stop();
        }

        /// <summary>Troca de parede quando há obstáculo subindo do lado da gota.</summary>
        private static void AutoPilot()
        {
            var fsm = GameStateMachine.Instance;
            if (fsm == null || fsm.Current != GameState.Playing) return;
            var player = fsm.Player;

            float px = player.transform.position.x;
            float py = player.transform.position.y;
            foreach (var obstacle in Object.FindObjectsByType<Obstacle>(FindObjectsSortMode.None))
            {
                if (!obstacle.gameObject.activeInHierarchy) continue;
                var pos = obstacle.transform.position;
                bool sameSide = Mathf.Sign(pos.x) == Mathf.Sign(px) || obstacle.Kind == ObstacleKind.Mover;
                bool approaching = pos.y > py - 4.2f && pos.y < py - 1.2f;
                if (sameSide && approaching)
                {
                    if (obstacle.Kind == ObstacleKind.Mover && Mathf.Abs(pos.x - px) > 1.5f) continue;
                    player.SwitchWall();
                    return;
                }
            }
        }

        private static Texture2D Downscale(Texture2D src, int w, int h)
        {
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var dst = new Texture2D(w, h, TextureFormat.RGB24, false);
            dst.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            dst.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return dst;
        }
    }
}
