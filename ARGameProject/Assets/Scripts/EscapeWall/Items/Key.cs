using UnityEngine;

namespace EscapeWall.Items
{
    public class Key : PhysicalItem
    {
        [SerializeField] private int _id = -1;
        public int ID => _id;
    }
}
