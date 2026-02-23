using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_VestActivation : MonoBehaviour
{
    //script that shows the activation of vest-motors on UI for debugging purposes

    public Sensor correspondingSensor;

    public Color zeroColor;
    public Color fullColor;

    private TextMeshProUGUI[] texts;
    private Image[] images;

    // Start is called before the first frame update
    void Start()
    {
        texts = new TextMeshProUGUI[40];
        images = new Image[40];
        for (int i = 0; i < 40; i++)
        {
            //first 20 from torso; final 20 from to back
            if (i < 20)
            {
                texts[i] = transform.GetChild(0).GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                images[i] = transform.GetChild(0).GetChild(i).GetComponent<Image>();
            }
            else
            {
                texts[i] = transform.GetChild(1).GetChild(i - 20).GetComponentInChildren<TextMeshProUGUI>();
                images[i] = transform.GetChild(1).GetChild(i - 20).GetComponent<Image>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (correspondingSensor != null)
        {
            SetValues(correspondingSensor.outputArray);
        }
    }

    public void SetValues(int[] values)
    {
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < 40; i++)
        {
            texts[i].text = values[i].ToString();
            images[i].color = Color.Lerp(zeroColor, fullColor, ((float) values[i]) / 100);
        }

    }
}
