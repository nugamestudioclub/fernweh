using System.Linq;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private GameObject counter;
    private int FPS;
    private float[] stableFPSArray = new float[60];
    private int lastFPSFrameChecked = 0;
    private float FPSUpdatesPerSecond = 1;
    private float FPSUpdateTimer;
    public void updateFPS() {
        FPSUpdateTimer += Time.unscaledDeltaTime;

        stableFPSArray[lastFPSFrameChecked] = 1/Time.unscaledDeltaTime;
        lastFPSFrameChecked = (1 + lastFPSFrameChecked) % stableFPSArray.Length;
        FPS = Mathf.RoundToInt(stableFPSArray.Average());
        if (FPSUpdateTimer > 1 / FPSUpdatesPerSecond)
            FPSUpdateTimer = 0;
            counter.GetComponent<TMP_Text>().text = "FPS: " + FPS;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateFPS();
    }
}
