using UnityEngine;
using UnityEngine.SceneManagement;  
using System.Collections; 

public class MainMenu : MonoBehaviour
{
    public Animator animator; 

    public void PlayGame()
    {
        if (animator != null) 
        {
            animator.SetTrigger("Close"); 
            StartCoroutine(LoadGameSceneAfterDelay(1f)); 
        }
        else
        {
            Debug.LogError("Animator is not assigned in the MainMenu script!"); 
            SceneManager.LoadScene("GameScene"); 
        }
    }

    private IEnumerator LoadGameSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        SceneManager.LoadScene("GameScene"); 
    }


    public void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    public void OpenSupportLink()
    {
        StartCoroutine(OpenURLWithDelay("https://tyumen.hh.ru/employer/3095178?hhtmFrom=vacancy"));
    }


    private IEnumerator OpenURLWithDelay(string url)
    {

        yield return null;
        Application.OpenURL(url);
    }
}