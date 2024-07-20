using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    public PlayerController playerController;

    public Sprite spriteHpBackground;
    public Sprite spriteHp;
    public Sprite spriteKey;

    private int maxHP = 3;
    private List<GameObject> _goHPlist = new List<GameObject>();
    private GameObject _goKey;

    void Start()
    {
        for (int i = 0; i < maxHP; i++) {
            createSpriteInstance(spriteHpBackground, i * 1.0f, 0);
            _goHPlist.Add(createSpriteInstance(spriteHp, i * 1.0f, 0));
        }

        _goKey = createSpriteInstance(spriteKey, 0f, -1f);
    }

    void Update()
    {
        drawHP();
        drawKey();
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
}
