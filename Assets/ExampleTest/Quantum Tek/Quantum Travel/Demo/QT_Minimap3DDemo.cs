using UnityEngine;

namespace QuantumTek.QuantumTravel.Demo
{
    public class QT_Minimap3DDemo : MonoBehaviour
    {
        public float moveSpeed = 1;
        public float rotSpeed = 1;

        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
                transform.Rotate(0, -rotSpeed, 0);
            if (Input.GetKey(KeyCode.D))
                transform.Rotate(0, rotSpeed, 0);
            if (Input.GetKey(KeyCode.W))
                transform.localPosition += moveSpeed * Time.deltaTime * transform.forward;
            if (Input.GetKey(KeyCode.S))
                transform.localPosition += moveSpeed * Time.deltaTime * -transform.forward;
        }
    }
}