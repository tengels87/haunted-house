using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int hp;
    public int maxHP = 3;
    public bool hasKey;

    private List<Vector2> waypointList = new List<Vector2>();
    private List<Vector2> targetList = new List<Vector2>();
    private bool canMove = true;


    void Awake() {
        hp = maxHP;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) {
            //Vector2 targetPos = Input.mousePosition;
            //waypointList.Add(MapGenerator.pixelPos2WorldPos(targetPos) - Vector2.one * 0.5f);
        }

        // walk along path
        if (waypointList.Count > 0) {
            Vector2 targetWaypoint = waypointList[0];
            float dist = Vector2.Distance(targetWaypoint, getPosition());
            if (dist > 0.1f) {
                if (canMove) {
                    Vector3 moveDIr = (targetWaypoint - getPosition()).normalized;

                    this.transform.position = transform.position + moveDIr * 4 * Time.deltaTime;
                }
            } else {
                waypointList.RemoveAt(0);
            }
        }
    }

    public Vector2 getPosition() {
        return new Vector2(this.transform.position.x, this.transform.position.y);
    }

    public void changeHP(int val) {
        hp = hp + val;

        if (hp > maxHP) {
            hp = maxHP;
        }
        if (hp < 0) {
            hp = 0;
        }
    }

    public void setWaypoints(List<Vector2> waypoints) {
        waypointList = new List<Vector2>(waypoints);
    }

    public void addWaypoints(List<Vector2> waypoints) {
        waypointList.AddRange(waypoints);
    }

    public void addTarget(Vector2 target) {
        targetList.Add(target);
    }

    public void addTargetFirst(Vector2 target) {
        targetList.Insert(0, target);
    }

    public void removeTarget(Vector2 target) {
        targetList.Remove(target);
    }

    public List<Vector2> getTargetList() {
        return targetList;
    }

    public void clearWaypoints() {
        waypointList.Clear();
    }

    public void clearTargets() {
        targetList.Clear();
    }

    public void kill() {
        UnityEngine.Object.Destroy(this.gameObject);
    }

    IEnumerator CoroutineFreeze(Action callback = null) {
        if (canMove) {
            canMove = false;

            yield return new WaitForSeconds(0.3f);

            canMove = true;

            callback?.Invoke();
         } else {
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (this.gameObject != collision.gameObject) {
            if (this.gameObject.tag == MapGenerator.TAG_PLAYER) {
                if (collision.gameObject.tag == MapGenerator.TAG_FINISH) {

                    // generate next level
                    StartCoroutine(CoroutineFreeze(() => {
                        hasKey = false;
                        WorldConstants.Instance.getGameManager().mapGenerator.generateLevel();
                    }));
                } else if (collision.gameObject.tag == "Treasure") {

                    // collect treasure
                    StartCoroutine(CoroutineFreeze(() => {
                        PlayerController enemyController = collision.gameObject.GetComponent<PlayerController>();
                        if (enemyController != null) {
                            enemyController.kill();
                            WorldConstants.Instance.getGameManager().changeScore(100);
                            WorldConstants.Instance.getGameManager().soundCollectTreasure.Play();
                        }
                    }));
                } else if (collision.gameObject.tag == "Key") {

                    // collect key
                    StartCoroutine(CoroutineFreeze(() => {
                        hasKey = true;
                        addTarget(WorldConstants.Instance.getGameManager().mapGenerator.getExitTile().getPosition());
                        WorldConstants.Instance.getGameManager().mapGenerator.checkPlayerPath();
                        PlayerController enemyController = collision.gameObject.GetComponent<PlayerController>();
                        if (enemyController != null) {
                            enemyController.kill();
                            WorldConstants.Instance.getGameManager().soundCollectTreasure.Play();
                        }
                    }));
                } else if (collision.gameObject.tag == "Enemy") {
                    changeHP(-1);
                    WorldConstants.Instance.getGameManager().soundHitGhost.Play();

                    // kill enemy
                    PlayerController enemyController = collision.gameObject.GetComponent<PlayerController>();
                    if (enemyController != null) {
                        enemyController.kill();
                    }

                    if (hp <= 0) {
                        StartCoroutine(CoroutineFreeze(() => {
                            kill();
                            WorldConstants.Instance.getGameManager().Die();
                        }));
                    }
                } else if (collision.gameObject.tag == "Heart") {

                    // restore HP
                    StartCoroutine(CoroutineFreeze(() => {
                        changeHP(1);
                        WorldConstants.Instance.getGameManager().soundCollectHeart.Play();

                        // kill enemy
                        PlayerController heartController = collision.gameObject.GetComponent<PlayerController>();
                        if (heartController != null) {
                            heartController.kill();
                        }
                    }));
                }
            }
        }
    }
}
