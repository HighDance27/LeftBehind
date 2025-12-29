using System.Collections;
using TMPro;
using UnityEngine;

public class ScrollingText : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private string[] itemInfo;
    [SerializeField] private float textSpeed = 0.1f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemInfoText;
    private int currentDisplayingText = 0;

    private bool isWriting = false;
    private bool skip = false;

    private void Start()
    {
        StartCoroutine(AnimateText());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Skip();
        }
    }

    public void Skip()
    {
        if (isWriting)
        {
            skip = true;
        }
        else
        {
            NextText();
        }
    }

    public void NextText()
    {
        // Kiểm tra xem còn đoạn tiếp theo trong mảng không
        if (currentDisplayingText < itemInfo.Length - 1)
        {
            currentDisplayingText++;
            StartCoroutine(AnimateText());
        }
    }

    IEnumerator AnimateText()
    {
        isWriting = true;
        skip = false;
        string fullText = itemInfo[currentDisplayingText]; //Lấy nội dung đoạn hiện tại

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (skip)
            {
                itemInfoText.text = fullText;
                break;
            }
            //Cắt chuỗi từ ký tự đầu tiên đến ký tự thứ i, xong đợi một lúc
            itemInfoText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(textSpeed);
        }
        isWriting = false;
    }

    public void PlayScrolling()
    {
        StopAllCoroutines();
        currentDisplayingText = 0;
        StartCoroutine(AnimateText());
    }

}
