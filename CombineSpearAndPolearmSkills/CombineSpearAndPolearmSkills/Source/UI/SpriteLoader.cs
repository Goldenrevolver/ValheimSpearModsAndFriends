using System.IO;
using System.Reflection;
using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    internal class SpriteLoader
    {
        internal static Sprite archerySprite;
        internal static Sprite rogueSprite;
        internal static Sprite slashSprite;
        internal static Sprite polearmSprite;

        internal static void LoadSprites()
        {
            var path = "CombineSpearAndPolearmSkills.Sprites";

            archerySprite = LoadIconSprite($"{path}.archery.png");
            rogueSprite = LoadIconSprite($"{path}.rogue.png");
            slashSprite = LoadIconSprite($"{path}.slash.png");
            polearmSprite = LoadIconSprite($"{path}.polearm.png");
        }

        public static Sprite LoadIconSprite(string path)
        {
            return LoadSprite(path, new Rect(0, 0, 64, 64), new Vector2(32, 32));
        }

        // originally from 'Trash Items' mod, as allowed in their permission settings on nexus
        // https://www.nexusmods.com/valheim/mods/441
        // https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems
        // modified because the original texture2D settings make the sprite brighter than default
        public static Sprite LoadSprite(string path, Rect size, Vector2 pivot)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream imageStream = assembly.GetManifestResourceStream(path);

            Texture2D texture = new Texture2D(0, 0);

            using (MemoryStream mStream = new MemoryStream())
            {
                imageStream.CopyTo(mStream);
                texture.LoadImage(mStream.ToArray());
                texture.Apply();
                return Sprite.Create(texture, size, pivot);
            }
        }
    }
}