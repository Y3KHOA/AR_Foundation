using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này xử lý biểu ngữ tiêu đề nút trong trò chơi, cho phép người dùng mở cài đặt cấu hình cho vật phẩm hoặc mặt đất.
/// </summary>
public class OptionConfigController : MonoBehaviour
{
    [Header("Option")]
    public OptionConfig objectOption;
    public OptionConfig materialOption;

    [Header("Material Control")]
    public MaterialController materialController;

    public void OpenObjectCanvasOnClick()
    {
        if (!objectOption.isOpen)
        {
            materialOption.isOpen = false;

            //Canvas
            objectOption.canvas.SetActive(true);
            materialOption.canvas.SetActive(false);

            //Button
            objectOption.offBTN.gameObject.SetActive(false);
            objectOption.onBTN.gameObject.SetActive(true);
            materialOption.onBTN.gameObject.SetActive(false);
            materialOption.offBTN.gameObject.SetActive(true);
        }
    }

    public void OpenMaterialCanvasOnClick()
    {
        if (!materialOption.isOpen)
        {
            objectOption.isOpen = false;

            //Canvas
            objectOption.canvas.SetActive(false);
            materialOption.canvas.SetActive(true);

            //Button
            objectOption.offBTN.gameObject.SetActive(true);
            objectOption.onBTN.gameObject.SetActive(false);
            materialOption.onBTN.gameObject.SetActive(true);
            materialOption.offBTN.gameObject.SetActive(false);

            //Control font size
            StartCoroutine(materialController.DelayedControlFontSize());
        }
    }
}
