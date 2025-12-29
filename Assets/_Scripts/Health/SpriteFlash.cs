using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    private List<SpriteRenderer> weaponRenderers = new List<SpriteRenderer>();
    private SpriteRenderer legsRenderer;

    private void Awake()
    {
        Transform torso = transform.Find("Weapons");
        Transform legs = transform.Find("Legs");

        //Lấy các component SpriteRenderer trong child của Weapons và thêm vào List
        weaponRenderers.AddRange(torso.GetComponentsInChildren<SpriteRenderer>(true));
        legsRenderer = legs.GetComponent<SpriteRenderer>();
    }

    public void StartFlash(float flashDuration, Color flashColor, int numberOfFlashes)
    {
        StartCoroutine(FlashCoroutine(flashDuration, flashColor, numberOfFlashes));
    }

    public IEnumerator FlashCoroutine(float flashDuration, Color flashColor, int numberOfFlashes)
    {
        //Lưu lại màu gốc để sau này trả về
        List<Color> startColors = new List<Color>();

        foreach (var renderer in weaponRenderers)
            startColors.Add(renderer.color);

        Color legsColor = legsRenderer.color;

        float elapsedFlashTime = 0;
        float elapsedFlashPercentage = 0; //tỉ lệ thời gian trôi qua từ 0 đến 1

        while (elapsedFlashTime < flashDuration)
        {
            elapsedFlashTime += Time.deltaTime;
            elapsedFlashPercentage = elapsedFlashTime / flashDuration; //Tính % tiến độ

            if (elapsedFlashPercentage > 1) //không vượt quá 1 (100%)
            {
                elapsedFlashPercentage = 1;
            }

            //Tạo giá trị nhảy từ 0 -> 1 -> 0 để tạo hiệu ứng nhấp nháy
            float pingPongPercentage = Mathf.PingPong(elapsedFlashPercentage * 2 * numberOfFlashes, 1);

            //Đổi màu cho tất cả sprites
            for (int i = 0; i < weaponRenderers.Count; i++)
                weaponRenderers[i].color = Color.Lerp(startColors[i], flashColor, pingPongPercentage);

            legsRenderer.color = Color.Lerp(legsColor, flashColor, pingPongPercentage);

            yield return null;
        }
    }
}