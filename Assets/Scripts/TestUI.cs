using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TestUI : MonoBehaviour
{
    public void PressTestButton01()
    {
        EventService.Instance.TrackEvent("Button1", "down");
    }
    public void PressTestButton02()
    {
        EventService.Instance.TrackEvent("Button2", "down");
    }
}
