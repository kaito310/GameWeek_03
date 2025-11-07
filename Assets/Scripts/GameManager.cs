using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const string TITLE_SCENE_NAME = "Title";
    const string MAIN_SCENE_NAME = "Main";

    bool isGameClear = false;
    bool isGameOver = false;

    public bool GetIsGameClear() { return isGameClear; }
    public bool GetIsGameOver() {  return isGameOver; }


    [SerializeField] Text stateText;
    [SerializeField] Text GuideText;
    [SerializeField] GameObject[] buttons;

    /// <summary>
    /// プレイヤーが置くブロックを表示している部分のテキストを設定する
    /// </summary>
    /// <param name="_text">入れたいテキスト</param>
    public void SetGuideText(string _text) { GuideText.text = _text; }

    void Start()
    {
        if (stateText != null)
        {
            stateText.text = "";
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Exit();
        }
    }

    /// <summary>
    /// メインゲームシーンへ遷移
    /// </summary>
    public void GameStart()
    {
        SceneManager.LoadScene(MAIN_SCENE_NAME);
    }

    public void GameClear()
    {
        isGameClear = true;
        stateText.text = "ゲームクリア！！";
        stateText.color = new Color32(0x3D, 0xFF, 0xC9, 0xFF);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(true);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        stateText.text = "ゲームオーバー...";
        stateText.color = new Color32(0xCA, 0x29, 0x17, 0xFF);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(true);
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(MAIN_SCENE_NAME);
    }

    /// <summary>
    /// タイトルシーンへ遷移
    /// </summary>
    public void BackTitle()
    {
        SceneManager.LoadScene(TITLE_SCENE_NAME);
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
