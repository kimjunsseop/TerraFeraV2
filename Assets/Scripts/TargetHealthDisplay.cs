using System.Collections;

using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TargetHealthDisplay : MonoBehaviour
{
    public UnityEngine.UI.Image healthFillImage;
    public TextMeshProUGUI hpText;
    private Coroutine hideCoroutine;
    public static TargetHealthDisplay Instance;
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    public void ShowHealth(int current, int max)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        float pct = (float)current / max;
        healthFillImage.fillAmount = pct;
        hpText.text = $"HP : {current} / max";
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }
    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        hideCoroutine = null;
    }
}
