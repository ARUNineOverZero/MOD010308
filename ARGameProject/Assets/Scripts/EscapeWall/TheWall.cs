using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall
{
    public class TheWall : MonoBehaviour
    {
        [SerializeField] private GameObject _playerOne;
        [SerializeField] private GameObject _playerTwo;
        [SerializeField] private GameObject _playerHiddenOne;
        [SerializeField] private GameObject _playerHiddenTwo;

        public UnityEvent OnDeactivatePlayerOne;
        public UnityEvent OnDeactivatePlayerTwo;

        public void ActivatePlayerOne()
        {
            _playerOne.SetActive(true);
            _playerHiddenTwo.SetActive(false);
        }

        public void DeactivePlayerOne()
        {
            _playerOne.SetActive(false);
            _playerHiddenTwo.SetActive(true);
            OnDeactivatePlayerOne?.Invoke();

        }

        public void ActivatePlayerTwo()
        {
            _playerTwo.SetActive(true);
            _playerHiddenOne.SetActive(false);
        }

        public void DeactivePlayerTwo()
        {
            _playerTwo.SetActive(false);
            _playerHiddenOne.SetActive(true);
            OnDeactivatePlayerTwo?.Invoke();

        }

        public void DeactivateNonPlayerSide()
        {
            var side = GameManager.Instance.CameraSideOfWall;

            if(side == Side.Back)
                DeactivePlayerTwo();
            else
                DeactivePlayerOne();
        }
    }
}
