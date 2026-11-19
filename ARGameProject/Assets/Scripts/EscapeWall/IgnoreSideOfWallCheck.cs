using UnityEngine;

namespace EscapeWall
{
    public class IgnoreSideOfWallCheck : MonoBehaviour
    {

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<Player>(out var device))
                GameManager.Instance.isCheckingSideOfWall = false;
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent<Player>(out var device))
            {
                var side = GetSide(device.transform.position);

                if(side == Side.Back)
                    GameManager.Instance.isCheckingSideOfWall = true;
            }
        }

        public Side GetSide(Vector3 position)
        {
            var localPosition = transform.InverseTransformPoint(position);
            if (localPosition.z == 0)
                return Side.None;
            else if (localPosition.z > 0)
                return Side.Front;
            else
                return Side.Back;

        }
    }
}