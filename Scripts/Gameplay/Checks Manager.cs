using System;
using System.Collections.Generic;
using UnityEngine;

public class ChecksManager : MonoBehaviour
{
    private readonly Dictionary<Guid, bool> checks = new();
    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform { get { return playerTransform; } }

    public void AddCheck(Guid taskID)
    {
        if (!checks.ContainsKey(taskID))
        {
            checks.Add(taskID, false);
        }

        print("New Task Added: " + taskID);
        string combined = "";
        foreach (Guid n in checks.Keys)
        {
            combined += $", {n}";
        }
        print("Current List of Tasks: " + combined);
    }

    public void CheckTask(Guid taskID)
    {
        if (!checks[taskID])
        {
            checks[taskID] = true;
            print("Checked Task: " + taskID);
        }
    }

    public bool ChecksStatus()
    {
        bool res = true;
        foreach (bool check in checks.Values)
        {
            res &= check;
        }
        return res;
    }
}
