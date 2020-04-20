using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMessageService : MonoBehaviour
{
    [SerializeField] GameObject messageBubble;
    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] float showTime = 5f;
    float lastMessage = 0f;
    float lastRobotMessage = 0f;
    [SerializeField] string[] randomMessages;

    private void OnEnable()
    {
        RobotController.OnRobotMessage += RobotController_OnRobotMessage;
    }

    private void OnDisable()
    {
        RobotController.OnRobotMessage -= RobotController_OnRobotMessage;
    }

    private void RobotController_OnRobotMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;
        if (Time.timeSinceLevelLoad - lastRobotMessage > showTime * 0.5f)
        {
            lastRobotMessage = Time.timeSinceLevelLoad;
            StartCoroutine(ShowMessage(msg));
        }
    }

    private void Start()
    {
        ClearMessage();
        StartCoroutine(RandomMessageGenerator());
    }

    void ClearMessage()
    {
        text.text = "";
        messageBubble.SetActive(false);
    }

    IEnumerator<WaitForSeconds> RandomMessageGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(showTime * 1.5f, showTime * 2.5f));
            if (Time.timeSinceLevelLoad - lastMessage > showTime)
            {
                var msg = randomMessages[Random.Range(0, randomMessages.Length)];
                text.text = msg;
                messageBubble.SetActive(true);
                lastMessage = Time.timeSinceLevelLoad;
                yield return new WaitForSeconds(showTime);
                if (text.text == msg) ClearMessage();
            }
        }
    }

    IEnumerator<WaitForSeconds> ShowMessage(string msg)
    {
        lastMessage = Time.timeSinceLevelLoad;
        text.text = msg;
        messageBubble.SetActive(true);
        yield return new WaitForSeconds(showTime);
        if (text.text == msg) ClearMessage();
    }
}
