using UnityEngine;

namespace Gota
{
    public enum PaletteSlot
    {
        Background, // fundo do cano
        Wall,       // paredes e marcas
        Accent,     // gota, coletáveis, botões, destaques
        Light       // obstáculos e texto
    }

    /// <summary>
    /// Paleta de 4 cores do jogo. Trocar aqui recolore tudo que usa PaletteColor.
    /// </summary>
    [CreateAssetMenu(fileName = "GamePalette", menuName = "Gota/Game Palette")]
    public class GamePalette : ScriptableObject
    {
        public Color background = new Color(0.05f, 0.11f, 0.17f);
        public Color wall = new Color(0.11f, 0.29f, 0.40f);
        public Color accent = new Color(0.38f, 0.71f, 0.80f);
        public Color light = new Color(0.88f, 0.98f, 0.99f);

        public Color Get(PaletteSlot slot)
        {
            switch (slot)
            {
                case PaletteSlot.Background: return background;
                case PaletteSlot.Wall: return wall;
                case PaletteSlot.Accent: return accent;
                default: return light;
            }
        }
    }
}
