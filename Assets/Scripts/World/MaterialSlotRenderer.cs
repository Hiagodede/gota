using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Liga um MeshRenderer a um papel visual (MaterialSlot). No Start pega o
    /// material de execução em TunnelMaterials. É o equivalente 3D do PaletteColor.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MaterialSlotRenderer : MonoBehaviour
    {
        [SerializeField] private MaterialSlot slot = MaterialSlot.Concrete;

        public MaterialSlot Slot => slot;

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            var materials = TunnelMaterials.Instance;
            if (materials == null) return;
            GetComponent<Renderer>().sharedMaterial = materials.Get(slot);
        }
    }
}
