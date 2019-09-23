using UnityEngine;
using UnityEngine.UI;

namespace guiraffe.SubstanceOrb
{
    [RequireComponent(typeof(Image))]
    public class OrbBehaviour : MonoBehaviour
    {
        protected float timeScaleOffset = 1f;
        
        Material material;

        protected Material Material
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (material == null)
                    {
                        Image.material = Instantiate(Image.material);

                        OrbBehaviour[] behaviours = GetComponents<OrbBehaviour>();
                        foreach (OrbBehaviour behaviour in behaviours)
                        {
                            behaviour.material = Image.material;
                        }
                    }

                    return material;
                }

                return Image.material;
            }
        }

        Image Image
        {
            get { return GetComponent<Image>(); }
        }
        
        public void setTimeScaleOffset(float offset) {
            this.timeScaleOffset = offset;
        }

        public void removeTimeScaleOffset() {
            this.timeScaleOffset = 1f;
        }
    }
}