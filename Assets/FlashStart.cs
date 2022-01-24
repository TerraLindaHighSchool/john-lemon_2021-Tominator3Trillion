using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class FlashStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FlashBangCoroutine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FlashBangCoroutine()
    {
        AutoExposure autoExposureLayer;
        PostProcessVolume volume = gameObject.GetComponent<PostProcessVolume>();
        //set the exposure to 1000 for a 3 second duration and then lerp it to 0 over the duration multiplied by the falloff
        volume.profile.TryGetSettings(out autoExposureLayer);
        autoExposureLayer.active = true;
        Debug.Log("hello: ");
        float value = 100f;

        autoExposureLayer.keyValue.value = value;
        
        //wait for the duration
        yield return new WaitForSeconds(1f);
        
        Debug.Log("waited for 1 second");
        while(value > 1f) {
            Debug.Log("value: " + value);
            value-=1f;
            autoExposureLayer.keyValue.value = value;
            yield return new WaitForSeconds(0.05f);
        }
        
         


        autoExposureLayer.active = false;
    }
}
