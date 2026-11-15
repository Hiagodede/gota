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
