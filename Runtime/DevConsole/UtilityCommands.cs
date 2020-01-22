using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UtilityCommands
{
    [Command("reload")]
    public static void ReloadScene()
    {
        Debug.Log("Reloading current scene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [Command("echo", "e")]
    public static void Echo(string inp)
    {
        Debug.Log($"Echo: {inp}");
    }
}