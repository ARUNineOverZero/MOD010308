using EscapeWall.PlayerComponents;
using EscapeWall.PlayerComponents.EWInput;
using UnityEngine;

namespace EscapeWall
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerController))]
    public class Player : MonoBehaviour
    {
        private InputSystem _inputSystem;
        public InputSystem InputSystem => _inputSystem;
        [SerializeField] private PlayerInputHandler _inputHandler;
        public PlayerInputHandler InputHandler => _inputHandler;
        [SerializeField] private PlayerController _controller;
        public PlayerController Controller => _controller;
    }
}