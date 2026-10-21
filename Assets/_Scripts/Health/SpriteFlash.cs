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
            elapsedFlashTime += Time.deltaTime;//0.1s , 0.2s...
            elapsedFlashPercentage = elapsedFlashTime / flashDuration; //Tính % tiến độ. Ví dụ: 0.5s / 1s = 0.5 (50%)

            if (elapsedFlashPercentage > 1) //không vượt quá 1 (100%)
            {
                elapsedFlashPercentage = 1;
            }

            // Input: Biến này sẽ tăng dần từ 0 lên (2 * số lần nháy).
            //        Ví dụ: 5 lần nháy -> Input chạy từ 0 đến 10.
            // Output (pingPongPercentage): Luôn dao động trong khoảng 0 đến 1.
            //        0 -> 1 (Sáng dần) -> 0 (Tắt dần) -> 1 (Sáng lại)...
            float pingPongPercentage = Mathf.PingPong(elapsedFlashPercentage * 2 * numberOfFlashes, 1);

            // Dùng con số dao động ở trên (0 -> 1 -> 0) để điều khiển việc pha màu.
            // 0: Lấy 100% màu Gốc. 1: Lấy 100% màu Flash. 0.5: Pha trộn 50-50.
            for (int i = 0; i < weaponRenderers.Count; i++)
                weaponRenderers[i].color = Color.Lerp(startColors[i], flashColor, pingPongPercentage);

            legsRenderer.color = Color.Lerp(legsColor, flashColor, pingPongPercentage);

            yield return null;
        }
        //trả màu gốc
        for (int i = 0; i < weaponRenderers.Count; i++)
            weaponRenderers[i].color = startColors[i];

        legsRenderer.color = legsColor;
    }
}