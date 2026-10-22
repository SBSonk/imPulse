using UnityEngine;

namespace ImPulse.Helpers
{
    public class Rotate : MonoBehaviour
    {
        public Vector3 rotationSpeed = new Vector3(15f, 45f, 15f);

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}