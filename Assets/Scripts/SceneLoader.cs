using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour // Zmieniona nazwa klasy!
{
    public void ChangeScene(int sceneIndex)
    {
        SaveManager.Instance.ZapiszGre();
        SceneManager.LoadScene(sceneIndex);
    }
}