using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MPSpriteFlash : MonoBehaviourPun
{
    private List<SpriteRenderer> weaponRenderers = new List<SpriteRenderer>();
    private SpriteRenderer legsRenderer;

    private void Awake()
    {
        Transform torso = transform.Find("Weapons");
        Transform legs = transform.Find("Legs");

        weaponRenderers.AddRange(torso.GetComponentsInChildren<SpriteRenderer>(true));
        legsRenderer = legs.GetComponent<SpriteRenderer>();
    }

    public void StartFlash(float flashDuration, Color flashColor, int numberOfFlashes)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //gửi mã số vì photon không cho RPC Color
            photonView.RPC(nameof(RPC_StartFlash)
                ,
                RpcTarget.All,
                flashDuration,
                flashColor.r,
                flashColor.g,
                flashColor.b,
                flashColor.a,
                numberOfFlashes
            );
        }
    }

    [PunRPC]
    private void RPC_StartFlash(
        float flashDuration,
        float r, float g, float b, float a,
        int numberOfFlashes
    )
    {
        Color flashColor = new Color(r, g, b, a);
        StartCoroutine(FlashCoroutine(flashDuration, flashColor, numberOfFlashes));
    }

    public IEnumerator FlashCoroutine(float flashDuration, Color flashColor, int numberOfFlashes)
    {
        List<Color> startColors = new List<Color>();
        foreach (var renderer in weaponRenderers)
            startColors.Add(renderer.color);

        Color legsColor = legsRenderer.color;

        float elapsedFlashTime = 0f;

        while (elapsedFlashTime < flashDuration)
        {
            elapsedFlashTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedFlashTime / flashDuration);
            float ping = Mathf.PingPong(percent * 2 * numberOfFlashes, 1f);

            for (int i = 0; i < weaponRenderers.Count; i++)
                weaponRenderers[i].color = Color.Lerp(startColors[i], flashColor, ping);

            legsRenderer.color = Color.Lerp(legsColor, flashColor, ping);

            yield return null;
        }
    }
}
