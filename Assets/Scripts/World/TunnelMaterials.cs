using System.Collections.Generic;
using UnityEngine;

namespace Gota
{
    /// <summary>Papel visual de um objeto no túnel. Cada papel tem um único material em tempo de execução.</summary>
    public enum MaterialSlot
    {
        Concrete,     // chão, teto e paredes
        Neon,         // aros e frisos emissivos (seguro)
        Danger,       // obstáculos (branco = perigo)
        Collectible,  // gotinhas
        Glass         // a gota
    }

    /// <summary>
    /// Dono dos materiais do túnel. Na partida cria uma cópia de cada material
    /// (para não sujar o asset) e entrega a mesma cópia a todos os renderers do
    /// mesmo papel. Assim o SRP Batcher agrupa tudo e trocar de bioma é
    /// mudar cinco cores em cinco materiais.
    /// </summary>
    public class TunnelMaterials : MonoBehaviour
    {
        public static TunnelMaterials Instance { get; private set; }

        [SerializeField] private Material concrete;
        [SerializeField] private Material neon;
        [SerializeField] private Material danger;
        [SerializeField] private Material collectible;
        [SerializeField] private Material glass;

        private readonly Dictionary<MaterialSlot, Material> runtime = new Dictionary<MaterialSlot, Material>();

        private void Awake()
        {
            Instance = this;
            runtime[MaterialSlot.Concrete] = new Material(concrete);
            runtime[MaterialSlot.Neon] = new Material(neon);
            runtime[MaterialSlot.Danger] = new Material(danger);
            runtime[MaterialSlot.Collectible] = new Material(collectible);
            runtime[MaterialSlot.Glass] = new Material(glass);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            foreach (var m in runtime.Values) Destroy(m);
            runtime.Clear();
        }

        /// <summary>Material de execução do papel pedido (sempre a mesma instância).</summary>
        public Material Get(MaterialSlot slot) => runtime[slot];

        [Header("Como a paleta vira material")]
        [Tooltip("Brilho HDR do neon (acima de 1 o bloom acende).")]
        [SerializeField] private float neonIntensity = 1.4f;
        [Tooltip("Quanto clarear a cor Wall da paleta para o concreto.")]
        [Range(0f, 1f)] [SerializeField] private float concreteLift = 0.3f;
        [SerializeField] private Camera targetCamera;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        /// <summary>
        /// Aplica uma mistura de duas paletas (t = 0 → from, t = 1 → to) a todos os
        /// materiais, à névoa, à luz ambiente e ao fundo da câmera.
        /// </summary>
        public void Apply(GamePalette from, GamePalette to, float t)
        {
            Color background = Color.Lerp(from.background, to.background, t);
            Color wall = Color.Lerp(from.wall, to.wall, t);
            Color accent = Color.Lerp(from.accent, to.accent, t);
            Color light = Color.Lerp(from.light, to.light, t);

            runtime[MaterialSlot.Concrete].SetColor(BaseColor, Color.Lerp(wall, Color.white, concreteLift));
            runtime[MaterialSlot.Neon].SetColor(BaseColor, accent * neonIntensity);
            runtime[MaterialSlot.Danger].SetColor(BaseColor, light * 0.88f);
            runtime[MaterialSlot.Danger].SetColor(EmissionColor, light * 0.12f);
            runtime[MaterialSlot.Collectible].SetColor(BaseColor, accent);
            runtime[MaterialSlot.Collectible].SetColor(EmissionColor, accent * 1.2f);
            runtime[MaterialSlot.Glass].SetColor(EmissionColor, accent * 0.35f);

            RenderSettings.fogColor = background;
            // Ambiente entre a parede e o neon: escuro, mas com a cor do bioma.
            RenderSettings.ambientLight = Color.Lerp(wall, accent, 0.5f);
            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null) cam.backgroundColor = background;
        }

        /// <summary>Material original (asset) do papel, para quem precisa do valor de referência.</summary>
        public Material Source(MaterialSlot slot)
        {
            switch (slot)
            {
                case MaterialSlot.Neon: return neon;
                case MaterialSlot.Danger: return danger;
                case MaterialSlot.Collectible: return collectible;
                case MaterialSlot.Glass: return glass;
                default: return concrete;
            }
        }
    }
}
