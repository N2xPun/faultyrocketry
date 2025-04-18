using ExtraMath;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class RocketManager : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] bool usePositionAsCenter = false;
    [SerializeField] Vector3 zoneCenter = new(0, 0, 0);
    [SerializeField] bool useScaleAsDimensions = false;
    [SerializeField] Vector3 zoneDimensions = new(3, 1, 3);
    [SerializeField] float launchTimer = 5f;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI untilLaunchText;

    private ChecksManager checksManager;

    bool isPlayerIn = false;
    bool launched = false;
    bool launching = false;

    public event EventHandler RocketLaunched;

    private void Start()
    {
        checksManager = GameObject.FindGameObjectWithTag("Checks Manager").GetComponent<ChecksManager>();
        StartCoroutine(ForcedLauncher());
    }

    private void Update()
    {
        if (!launched)
        {
            isPlayerIn = ExMath.WithinCuboid(playerTransform.position, usePositionAsCenter ? transform.position : zoneCenter, useScaleAsDimensions ? transform.localScale : zoneDimensions);
            if (isPlayerIn && !launching)
            {
                StartCoroutine(Launcher());
            }
        }
    }

    private IEnumerator Launcher()
    {
        launching = true;
        float timeleft = launchTimer;
        while (timeleft > 0 && isPlayerIn)
        {
            timeleft -= Time.deltaTime;
            untilLaunchText.text = ("Stand here for 5 seconds to launch rocket early " + timeleft.ToString("0.0"));
            yield return null;
        }
        if (isPlayerIn)
        {
            StartCoroutine(LaunchSequence());
            untilLaunchText.text = "Rocket Launched";
            launching = false;
        }
        else
        {
            print("Canceled Launch");
            untilLaunchText.text = "Stand here for 5 seconds to launch rocket early";
            launching = false;
        }
    }

    private IEnumerator ForcedLauncher()
    {
        float timeleft = 300f;
        while (timeleft > 0 && !launched)
        {
            timeleft -= Time.deltaTime;

            timerText.text = ("Time Left till Scheduled Launch: " + timeleft.ToString("0.0") + " s");

            yield return null;
        }
        if (!launched)
        {
            launching = false;
            StartCoroutine(LaunchSequence());
        }
    }

    private IEnumerator LaunchSequence()
    {
        launched = true;
        timerText.text = "Rocket Launched";
        bool isSuccessful = checksManager.ChecksStatus();
        float timer = 10f + UnityEngine.Random.Range(-4f, 2f);
        RocketLaunchedArgs args = new()
        {
            isSuccessful = isSuccessful
        };
        if (isSuccessful)
        {
            timer = 10f;
            args.Timer = timer;
            OnRocketLaunched(args);
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            timerText.text = "Rocket Launch Successful - The End";
        }
        else
        {
            args.Timer = timer;
            OnRocketLaunched(args);
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            timerText.text = "Rocket Launch Failed - The End";
        }
    }

    protected virtual void OnRocketLaunched(RocketLaunchedArgs e)
    {
        RocketLaunched?.Invoke(this, e);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerIn ? new(0, 1, 0, .5f) : new(1, 0, 0, .5f);
        Gizmos.DrawCube(usePositionAsCenter ? transform.position : zoneCenter, useScaleAsDimensions ? transform.localScale : zoneDimensions);
    }
}
