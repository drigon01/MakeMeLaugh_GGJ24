using UnityEngine;

public class ComedianController : MonoBehaviour
{
    [SerializeField] private Animator _headController;
    [SerializeField] private Animator _bodyController;

    public void BodyWalk() => _bodyController.SetTrigger("walking");
    public void BodyIdle() => _bodyController.SetTrigger("idle");
    public void BodyTurn() => _bodyController.SetTrigger("turning");
    public void HeadTalking() => _headController.SetBool("isTalking", true);
    public void HeadIdle() => _headController.SetBool("isTalking", false);
}
