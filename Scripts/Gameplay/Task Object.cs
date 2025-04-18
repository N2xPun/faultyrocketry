using ExtraMath;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TaskObject : MonoBehaviour
{
    [SerializeField] private string taskName;
    [SerializeField] private string taskDescription;
    [SerializeField] private float range = 15f;
    [SerializeField] private Transform displayText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private GameObject crossMark;
    [SerializeField] private RectTransform progressBar;
    private Guid tskID = Guid.NewGuid();

    private ChecksManager checksManager;
    private Transform playerTransform;

    private PlayerInput input;
    private InputAction interact;

    private bool isComplete = false;
    private bool inRange = false;

    private void Start()
    {
        try
        {
            checksManager = GameObject.FindGameObjectWithTag("Checks Manager").GetComponent<ChecksManager>();

            playerTransform = checksManager.PlayerTransform;

            input = playerTransform.GetComponent<PlayerInput>();
            interact = input.actions.FindAction("Interact");

            interact.started += StartProgressBar;
            interact.performed += UnsyncedCompletion;

            checksManager.AddCheck(tskID);

            displayText = gameObject.GetComponentInChildren<Canvas>().transform;

            nameText.text = taskName;
            descriptionText.text = taskDescription;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void Update()
    {
        inRange = ExMath.TaxicabDistance(transform.position, playerTransform.position) < 1.733f * range && Vector3.Distance(transform.position, playerTransform.position) < range;
        displayText.gameObject.SetActive(inRange);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, .5f);
        Gizmos.DrawSphere(transform.position, range);
    }

    private IEnumerator Progressor()
    {
        float time = 0.5f;
        while (time > 0 && interact.inProgress)
        {
            if (isComplete)
            {
                break;
            }
            time -= Time.deltaTime;

            progressBar.sizeDelta = new Vector2(Mathf.Clamp(1400f * (.5f - time), 0f, 700f), 25f);

            yield return null;
        }
        if (time < 0 || isComplete)
        {
            checksManager.CheckTask(tskID);
            isComplete = true;
            checkMark.SetActive(true);
            crossMark.SetActive(false);
            progressBar.gameObject.SetActive(false);
        }
        else if (time > 0)
        {
            progressBar.sizeDelta = Vector2.zero;
        }
    }

    private void StartProgressBar(InputAction.CallbackContext callbackContext)
    {
        if (!isComplete && inRange)
        {
            StartCoroutine(Progressor());
        }
    }

    private void UnsyncedCompletion(InputAction.CallbackContext callbackContext)
    {
        if (!isComplete && inRange)
        {
            isComplete = true;
        }
    }
}
