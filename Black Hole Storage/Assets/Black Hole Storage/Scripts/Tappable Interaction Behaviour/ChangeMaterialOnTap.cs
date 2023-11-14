using UnityEngine;

namespace Leap.Unity.Interaction.Storage
{
    /// <summary>
    /// A simple class which toggles its tappable interaction behaviour's material on tap
    /// </summary>
    public class ChangeMaterialOnTap : MonoBehaviour
    {
        public Material a;
        public Material b;
        public TappableInteractionBehaviour tappableInteractionBehaviour;
        public MeshRenderer meshRenderer;

        private bool materialAEnabled = true;

        // Start is called before the first frame update
        void Start()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (tappableInteractionBehaviour == null)
            {
                tappableInteractionBehaviour = GetComponent<TappableInteractionBehaviour>();
            }

            tappableInteractionBehaviour.OnTap += ChangeMaterial;
            meshRenderer.material = a;
        }

        void ChangeMaterial()
        {
            if (materialAEnabled)
            {
                meshRenderer.material = b;
            }
            else
            {
                meshRenderer.material = a;
            }
            materialAEnabled = !materialAEnabled;
        }
    }
}
