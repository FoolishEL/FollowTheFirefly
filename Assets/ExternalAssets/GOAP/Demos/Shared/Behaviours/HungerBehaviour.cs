using UnityEngine;

namespace Demos.Shared.Behaviours
{
    public class HungerBehaviour : MonoBehaviour
    {
        public float hunger = 50;

        private void Awake()
        {
            hunger = Random.Range(0, 100f);
            // this.hunger = 80f;
        }

        private void FixedUpdate()
        {
            hunger += Time.fixedDeltaTime * 2f;
        }
    }
}