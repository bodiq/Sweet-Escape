using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AspectRatioController : MonoBehaviour
{
    [SerializeField] private List<AspectRatioFitter> aspectRatioFitters;
    

    private void Start()
    {
#if UNITY_WEBGL
        foreach (var filter in aspectRatioFitters)
        {
            filter.enabled = true;
        }
#endif
    }

}
