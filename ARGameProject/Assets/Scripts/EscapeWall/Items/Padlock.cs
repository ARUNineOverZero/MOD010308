using System.Collections;
using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall.Items
{
    public class Padlock : MonoBehaviour
    {
        [SerializeField] private int _keyId = -1;

        [SerializeField] private Transform _keyAnchor;

        [SerializeField] private TransformAnimation _incorrectKey;
        [SerializeField] private TransformAnimation _correctKey;
        [SerializeField] private TransformAnimation _baseUnlock;
        private bool usingKey = false;

        public UnityEvent OnUnlocked;

        private void OnTriggerEnter(Collider other)
        {
            if (usingKey) return;

            Key key = other.gameObject.GetComponentInParent<Key>();

            if (key == null) return;

            UseKey(key);
        }

        public void UseKey(Key key)
        {
            if (key.ID == _keyId)
                PlayCorrectKey(key);
            else
                PlayIncorrectKey(key);
        }
        private void PlayIncorrectKey(Key key)
        {
            usingKey = true;
            Transform t = key.transform;
            Rigidbody rb = key.Rigidbody;
            rb.isKinematic = true;
            t.parent = _keyAnchor;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            StartCoroutine(_incorrectKey.Play(null, () =>
            {
                t.parent = null;
                rb.isKinematic = false;
                StartCoroutine(RemoveKey());
            }));
        }

        private IEnumerator RemoveKey()
        {
            yield return new WaitForSeconds(5);
            usingKey = false;
        }

        private void PlayCorrectKey(Key key)
        {
            usingKey = true;
            Transform t = key.transform;
            Destroy(key.Rigidbody);
            t.parent = _keyAnchor;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            StartCoroutine(_correctKey.Play(null, PlayUnlock));
        }

        private void PlayUnlock()
        {
            StartCoroutine(_baseUnlock.Play(null, Drop));
        }

        private void Drop()
        {
            GetComponent<Rigidbody>().isKinematic = false;
            OnUnlocked?.Invoke();
            Destroy(this);
        }
    }
}
