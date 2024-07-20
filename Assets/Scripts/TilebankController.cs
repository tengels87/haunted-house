using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TilebankController : MonoBehaviour
{
    public SpriteRenderer rendererTile;
    public SpriteRenderer rendererExtra;

    [SerializeField]
    public List<TileVariant> tileVariantList = new List<TileVariant>();

    private TileVariant nextTileVariant;
    private int lastTileVariantChildIdx;

    private System.Random rnd;

    void Awake() {
        rnd = new System.Random();

        // init tile bank
        nextRandomTileVariant();
    }
    
    void Start()
    {
        
    }

    void Update()
    {

    }

    public void nextRandomTileVariant() {
        int variantIdx = rnd.Next(tileVariantList.Count - 2) + 2; // -2 to exclude start and finish tile (which currently also reside inside tilevariants list)

        nextTileVariant = tileVariantList[variantIdx];
        nextTileVariant.hasExtra = rnd.Next(100) < 20;

        lastTileVariantChildIdx = 0;

        updateTilebank();
    }

    private void rotateTileVariant() {
        if (nextTileVariant != null) {
            lastTileVariantChildIdx++;

            if (lastTileVariantChildIdx > nextTileVariant.spriteList.Count - 1) {
                lastTileVariantChildIdx = 0;
            }

            updateTilebank();
        }
    }

    public TileVariant getCurrentVariant() {
        return nextTileVariant;
    }

    public int getRotationIndex() {
        return lastTileVariantChildIdx;
    }

    private void updateTilebank() {
        Sprite spr = nextTileVariant.spriteList[lastTileVariantChildIdx];
        rendererTile.sprite = spr;
        
        rendererExtra.enabled = nextTileVariant.hasExtra;
    }

    void OnMouseDown() {
        
        // rotate tile
        rotateTileVariant();
    }

    [System.Serializable]
    public class TileVariant {
        public List<Sprite> spriteList = new List<Sprite>();
        public List<string> canConnectAt = new List<string>();
        public bool hasExtra = false;

        public TileVariant() {

        }
    }
}
