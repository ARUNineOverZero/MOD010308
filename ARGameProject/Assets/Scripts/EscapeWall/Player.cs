using System;
using EscapeWall.PlayerComponents;
using EscapeWall.PlayerComponents.EWInput;
using EscapeWall.PlayerComponents.Interaction;
using EscapeWall.WallPlacer;
using JetBrains.Annotations;
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
        [SerializeField] private WallPlacerController _wallPlacerController;
        public WallPlacerController WallPlacerController => _wallPlacerController;
        [SerializeField] private InteractionManager _interactionManager;
        public InteractionManager InteractionManager => _interactionManager;
        [SerializeField] private PlayerController _controller;
        public PlayerController Controller => _controller;
        
        [SerializeField] private PlayerHand _playerHand;
        public PlayerHand PlayerHand => _playerHand;
        [SerializeField] private Camera _camera;
        public Camera Camera => _camera;

        public void Initialize()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Enable();
        }

        public void EnterWallPlacementControllerMode()
        {
            _wallPlacerController.enabled = true;
            _controller.enabled = false;
            _wallPlacerController.BeginPlacement();
        }

        public void EnterPlayerControllerMode()
        {
            _wallPlacerController.enabled = false;
            _controller.enabled = true;
        }
    }
}