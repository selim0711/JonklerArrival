using UnityEngine.SceneManagement;

public enum JoinklerScenes
{
    MainMenu,
    Cutscene,
    MainHouse
}
public static class JoinklerSceneManager
{
    public static void SwitchScene(JoinklerScenes jScene)
    {
        switch(jScene)
        {
            case JoinklerScenes.MainMenu:
                //SceneManager.LoadScene("");
                break;
            case JoinklerScenes.Cutscene:
                //SceneManager.LoadScene("");
                break;
            case JoinklerScenes.MainHouse:
                SceneManager.LoadScene("MainScene");
                break;
        }
    }
}