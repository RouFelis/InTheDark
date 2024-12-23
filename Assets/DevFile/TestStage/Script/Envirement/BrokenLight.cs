using UnityEngine;
using System.Collections;

public class BrokenLight : MonoBehaviour
{
    public Light lightSource; // Á¶¸í ¼Ò½º
    public float minIntensity = 0.0f; // ÃÖ¼Ò ¹à±â
    public float maxIntensity = 2.0f; // ÃÖ´ë ¹à±â
    public int minFlickerCount = 2; // ÃÖ¼Ò ±ôºıÀÓ È½¼ö
    public int maxFlickerCount = 5; // ÃÖ´ë ±ôºıÀÓ È½¼ö
    public float flickerTotalDuration = 1.0f; // ±ôºıÀÓ ÃÑ Áö¼Ó ½Ã°£
    public float delayAfterFlicker = 2.0f; // ±ôºıÀÓ ÈÄ ´ë±â ½Ã°£

    void Start()
    {
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
        }

        StartCoroutine(FlickerLightRoutine());
    }

    private IEnumerator FlickerLightRoutine()
    {
        while (true)
        {
            // ·£´ı ±ôºıÀÓ È½¼ö
            int flickerCount = Random.Range(minFlickerCount, maxFlickerCount);

            // °¢ ±ôºıÀÓÀÇ °£°İ °è»ê (ÃÑ Áö¼Ó ½Ã°£ / ±ôºıÀÓ È½¼ö)
            float flickerInterval = flickerTotalDuration / (flickerCount * 2); // On/Off °£°İ Æ÷ÇÔ

            for (int i = 0; i < flickerCount; i++)
            {
                // Á¶¸íÀ» ÄÑ°í ·£´ı ¹à±â·Î ¼³Á¤
                lightSource.intensity = Random.Range(minIntensity, maxIntensity);
                yield return new WaitForSeconds(flickerInterval);

                // Á¶¸íÀ» ²û
                lightSource.intensity = 0f;
                yield return new WaitForSeconds(flickerInterval);
            }

            // ±ôºıÀÓ ÈÄ ´ë±â
            lightSource.intensity = maxIntensity; // ¾ÈÁ¤µÈ ¹à±â·Î ¼³Á¤
            yield return new WaitForSeconds(delayAfterFlicker);
        }
    }
}
