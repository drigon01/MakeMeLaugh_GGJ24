using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ComedianController : MonoBehaviour
{
    [SerializeField] private Animator _headController;
    [SerializeField] private Animator _bodyController;
    [SerializeField] private AudioSource _audioSource;

    public void BodyWalk() => _bodyController.SetTrigger("walking");
    public void BodyIdle() => _bodyController.SetTrigger("idle");
    public void BodyTurn() => _bodyController.SetTrigger("turning");
    private void HeadTalking() => _headController.SetBool("isTalking", true);
    private void HeadIdle() => _headController.SetBool("isTalking", false);

    public AudioClip[] mumbles;

    public IEnumerator Start()
    {
        while (true)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    private bool _isTalking;
    public bool IsTalking
    {
        get => _isTalking;
        set
        {
            if (_isTalking == value)
                return;
            
            _isTalking = value;
            if (!_isTalking)
            {
                HeadIdle();
                _audioSource.Stop();
            }
            else
            {
                HeadTalking();
                _audioSource.clip = mumbles[Random.Range(0, mumbles.Length)];
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }
    }
}
