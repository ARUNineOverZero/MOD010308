using UnityEngine;

namespace EscapeWall
{
    public class TheWall : MonoBehaviour
    {
        [SerializeField] private GameObject _playerOne;
        [SerializeField] private GameObject _playerTwo;

        public void ActivatePlayerOne() => _playerOne.SetActive(true);
        public void DeactivePlayerOne()  => _playerOne.SetActive(false);

        public void ActivatePlayerTwo() => _playerTwo.SetActive(true);
        public void DeactivePlayerTwo()  => _playerTwo.SetActive(false);

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
