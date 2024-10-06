using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public PlayerController playerController;

    public Sprite spriteHpBackground;
    public Sprite spriteHp;
    public Sprite spriteKey;
    public Object prefabGemCounter;

    private int maxHP = 3;
    private List<GameObject> _goHPlist = new List<GameObject>();
    private GameObject _goKey;
    private GameObject _goGem;

    void Start()
    {
        for (int i = 0; i < maxHP; i++) {
            createSpriteInstance(spriteHpBackground, i * 1.0f, 0);
            _goHPlist.Add(createSpriteInstance(spriteHp, i * 1.0f, 0));
        }

        _goKey = createSpriteInstance(spriteKey, 0f, -1f);
        _goGem = createPrefabInstance(prefabGemCounter, 0f, -2f);
    }

    void Update()
    {
        drawHP();
        drawKey();
    }

    public GameObject createPrefabInstance(Object _prefab, float posX, float posY) {
        GameObject go = (GameObject)Object.Instantiate(_prefab);
        go.transform.SetParent(this.transform);
        go.transform.localPosition = new Vector2(posX, posY);

        return go;
    }

    public GameObject createSpriteInstance(Sprite spr, float posX, float posY) {
        GameObject go = new GameObject("sprite_hud");
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = spr;
        go.transform.SetParent(this.transform);
        go.transform.localPosition = new Vector2(posX, posY);

        return go;
    }

    private void drawHP() {
        for (int i = 0; i < maxHP; i++) {
            _goHPlist[i].SetActive(i <= playerController.hp-1);
        }
    }

    private void drawKey() {
        _goKey.SetActive(playerController.hasKey);
    }

    public void setGemCounter(int val) {
        Text text_gemcounter = _goGem.GetComponentInChildren<Text>();
        if (text_gemcounter != null) {
            text_gemcounter.text = "" + val;
        }
    }
}
