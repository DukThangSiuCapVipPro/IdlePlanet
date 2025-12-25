using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Voodoo.UI.Particles.Utils
{
    internal static class ParticleSystemExtensions
    {
        public static void SortForRendering(this List<ParticleSystem> self, Transform transform, bool sortByMaterial)
        {
            self.Sort((a, b) =>
            {
                var tr = transform;
                var aRenderer = a.GetComponent<ParticleSystemRenderer>();
                var bRenderer = b.GetComponent<ParticleSystemRenderer>();

                // Render queue: ascending
                var aMat = aRenderer.sharedMaterial ?? aRenderer.trailMaterial;
                var bMat = bRenderer.sharedMaterial ?? bRenderer.trailMaterial;
                if (!aMat && !bMat) return 0;
                if (!aMat) return -1;
                if (!bMat) return 1;

                if (sortByMaterial)
                    return aMat.GetInstanceID() - bMat.GetInstanceID();

                if (aMat.renderQueue != bMat.renderQueue)
                    return aMat.renderQueue - bMat.renderQueue;

                // Sorting layer: ascending
                if (aRenderer.sortingLayerID != bRenderer.sortingLayerID)
                    return aRenderer.sortingLayerID - bRenderer.sortingLayerID;

                // Sorting order: ascending
                if (aRenderer.sortingOrder != bRenderer.sortingOrder)
                    return aRenderer.sortingOrder - bRenderer.sortingOrder;

                // Z position & sortingFudge: descending
                var aTransform = a.transform;
                var bTransform = b.transform;
                var aPos = tr.InverseTransformPoint(aTransform.position).z + aRenderer.sortingFudge;
                var bPos = tr.InverseTransformPoint(bTransform.position).z + bRenderer.sortingFudge;
                if (!Mathf.Approximately(aPos, bPos))
                    return (int) Mathf.Sign(bPos - aPos);

                return (int) Mathf.Sign(GetIndex(self, a) - GetIndex(self, b));
            });
        }

        private static int GetIndex(IList<ParticleSystem> list, Object ps)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].GetInstanceID() == ps.GetInstanceID()) return i;
            }

            return 0;
        }

        public static long GetMaterialHash(this ParticleSystem self, bool trail)
        {
            if (!self) return 0;

            var r = self.GetComponent<ParticleSystemRenderer>();
            var mat = trail ? r.trailMaterial : r.sharedMaterial;

            if (!mat) return 0;

            var tex = trail ? null : self.GetTextureForSprite();
            return ((long) mat.GetHashCode() << 32) + (tex ? tex.GetHashCode() : 0);
        }

        public static Texture2D GetTextureForSprite(this ParticleSystem self)
        {
            if (!self) return null;

            // Get sprite's texture.
            var tsaModule = self.textureSheetAnimation;
            if (!tsaModule.enabled || tsaModule.mode != ParticleSystemAnimationMode.Sprites) return null;

            for (var i = 0; i < tsaModule.spriteCount; i++)
            {
                var sprite = tsaModule.GetSprite(i);
                if (!sprite) continue;

                return sprite.GetActualTexture();
            }

            return null;
        }

        public static void Exec(this List<ParticleSystem> self, Action<ParticleSystem> action)
        {
            self.RemoveAll(p => !p);
            self.ForEach(action);
        }
    }
}
