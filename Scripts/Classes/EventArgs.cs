using System;

public class RocketLaunchedArgs : EventArgs
{
    public float Timer { get; set; }
    public bool isSuccessful { get; set; }
}