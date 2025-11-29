using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_change : MonoBehaviour
{
    private AudioSource finishSound;

    [SerializeField]private bool levelCompleted = false;
    [SerializeField] private float delayBeforeLoading = 1f;
    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !levelCompleted)
        {
            //finishSound.Play();
            levelCompleted = true;
            Invoke("CompleteLevel", delayBeforeLoading);
        }
    }

    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
