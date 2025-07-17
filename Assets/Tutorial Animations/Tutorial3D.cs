using UnityEngine;

public class Tutorial3D : TutorialBase
{
    [SerializeField] private GameObject animator;
    public override void StopTutorial()
    {
        animator.gameObject.SetActive(false);
    }

    public override void PlayTutorial()
    {
        animator.gameObject.SetActive(true);
    }
}