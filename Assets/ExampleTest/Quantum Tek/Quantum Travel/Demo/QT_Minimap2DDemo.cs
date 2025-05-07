using UnityEngine;

namespace QuantumTek.QuantumTravel.Demo
{
    public class QT_Minimap2DDemo : MonoBehaviour
    {
        public float moveSpeed = 1;
        public float rotSpeed = 1;

        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
                transform.Rotate(0, 0, rotSpeed);
            if (Input.GetKey(KeyCode.D))
                transform.Rotate(0, 0, -rotSpeed);
            if (Input.GetKey(KeyCode.W))
                transform.localPosition += moveSpeed * Time.deltaTime * transform.up;
            if (Input.GetKey(KeyCode.S))
                transform.localPosition += moveSpeed * Time.deltaTime * -transform.up;
        }
    }
}