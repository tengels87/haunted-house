using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public PlayerController player;

    public AudioSource soundCollectTreasure;
    public AudioSource soundCollectHeart;
    public AudioSource soundHitGhost;
    public AudioSource soundWalking;

    public int stage = 0;

    private int score;
    private int gems;

    void Awake() {
        //DontDestroyOnLoad(this);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public static void loadScene(string sceneName) {
        Scene current = SceneManager.GetActiveScene();

        if (current != null) {
            SceneManager.UnloadSceneAsync(current);
        }

        SceneManager.LoadSceneAsync(sceneName);
    }

    public int changeScore(int val) {
        score = score + val;

        return score;
    }

    public int addGem() {
        gems = gems + 1;

        return gems;
    }

    public void showMainManu() {
        loadScene("scene_mainmenu");
    }

    public void Die() {
        showMainManu();
    }

    public void loadNextLevel() {
        mapGenerator.generateLevel();
    }
}
