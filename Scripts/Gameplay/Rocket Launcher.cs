using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [SerializeField] private RocketManager rktm;
    [SerializeField] private GameObject hangarRocket;
    [SerializeField] private GameObject launchRocket;
    [SerializeField] private GameObject launchRocketContainer;
    [SerializeField] private ParticleSystem thrust;
    [SerializeField] private List<ParticleSystem> waterVapors;
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private TextMeshProUGUI timertext;

    float timer = 0f;
    bool isSuccessful = false;

    private void Start()
    {
        rktm.RocketLaunched += LaunchPort;
    }

    private void LaunchPort(object sender, EventArgs args)
    {
        print("hello");
        RocketLaunchedArgs rktLA = (RocketLaunchedArgs)args;
        timer = rktLA.Timer - 5f;
        isSuccessful = rktLA.isSuccessful;

        hangarRocket.SetActive(false);
        launchRocket.SetActive(true);

        StartCoroutine(LaunchSequence());
    }

    private IEnumerator LaunchSequence()
    {
        float tminus = 5f;
        while (tminus > 0f)
        {
            tminus -= Time.deltaTime;
            timertext.text = $"Rocket Launching in {tminus:0.0}";
            yield return null;
        }
        timertext.text = "Rocket Launched";
        float rtime = timer;
        thrust.Play();
        foreach(var wvp in waterVapors)
        {
            wvp.Play();
        }
        while (rtime > 0f)
        {
            rtime -= Time.deltaTime;

            launchRocketContainer.transform.position += .2f * (timer - rtime) * Vector3.up;

            yield return null;
        }
        if (isSuccessful)
        {
            while (launchRocketContainer.transform.position.y < 100000)
            {
                rtime -= Time.deltaTime;

                launchRocketContainer.transform.position += .2f * (timer - rtime) * Vector3.up;

                yield return null;
            }
        }
        else
        {
            thrust.Stop();
            explosion.Play();
            launchRocket.SetActive(false);
        }
    }
}
