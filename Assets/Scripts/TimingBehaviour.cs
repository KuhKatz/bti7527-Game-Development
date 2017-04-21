using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingBehaviour : MonoBehaviour
{
    public int countMax = 3;
    public CarBehaviour _carScript;
    private int _countDown;
    public GUIText guiTime;
    public GUIText countdown;
    private float _pastTime = 0;
    private bool _isFinished = false;
    private bool _isStarted = false;

    // Use this for initialization
    void Start()
    {
        print("Begin Start:" + Time.time);
        StartCoroutine(GameStart());
        print("End Start:" + Time.time);
        guiTime.text = "";
    }
    // GameStart CoRoutine
    IEnumerator GameStart()
    {
        for (_countDown = countMax; _countDown >= 0; _countDown--)
        {
            if (_countDown > 0)
            {
                countdown.text = _countDown.ToString("0");
                yield return new WaitForSeconds(1);
            }
            else
            {
                _carScript.enabled = true;
                countdown.text = "GO!";
                countdown.fontSize = 48;
                yield return new WaitForSeconds(2);
                countdown.enabled = false;
            }
        }
    }

    void Update()
    {
        if (_carScript.thrustEnabled)
        {
            if (_isStarted && !_isFinished)
                _pastTime += Time.deltaTime;
            guiTime.text = _pastTime.ToString("0.0 sec");
            print("Game Time: +_pastTime");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        { if (!_isStarted)
        _isStarted = true;
        else _isFinished = true;
        }
        }


}
