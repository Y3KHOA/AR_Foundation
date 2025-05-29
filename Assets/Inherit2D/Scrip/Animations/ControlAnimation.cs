using System.Collections;
using UnityEngine;

/// <summary>
/// Lớp này xử lý hoạt ảnh điều khiển cho các thành phần UI, cho phép chúng xuất hiện và biến mất theo thời gian đếm ngược.
/// </summary>
public class ControlAnimation : MonoBehaviour
{
    private Animator popupAnimator;
    private float waitTime = 2f;
    private float tempTime;

    private void Start()
    {
        popupAnimator = GetComponent<Animator>();
        tempTime = waitTime;
    }

    public void EndOfFrameAppear()
    {
        popupAnimator.SetInteger("state", 1);
        StartCoroutine(CountDown());
    }

    public void EndOfFrameDisappear()
    {
        popupAnimator.SetInteger("state", 0);
        popupAnimator.gameObject.SetActive(false);
    }

    private IEnumerator CountDown()
    {
        while (tempTime > 0)
        {
            tempTime -= Time.deltaTime * 2;
            yield return null;
        }

        tempTime = waitTime;
        popupAnimator.SetInteger("state", 2);
    }
}
