using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class EffectTail:MonoBehaviour
    {

        public float RotationX;
        public float RotationY;
        public float RotationZ;


        private void Update()
        {
            transform.Rotate(new Vector3(RotationX, RotationY, RotationZ)*GameTime.deltaTime);
        }
    }
}