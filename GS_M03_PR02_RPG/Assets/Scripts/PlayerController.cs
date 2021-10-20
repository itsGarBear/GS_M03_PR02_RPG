using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    [Header("Stats")]
    public float moveSpeed;
    public int gold;
    public float currHP;
    public float maxHP;
    public bool dead;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    protected float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rb;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;
    public HeaderInfo headerInfo;

    public static PlayerController me;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (player.IsLocal)
            me = this;
        else
            rb.isKinematic = true;

        headerInfo.Initialize(player.NickName, maxHP);

    }

    private void Update()
    {
        if (!photonView.IsMine || dead)
            return;

        Move();

        float mouseX = (Screen.width / 2) - Input.mousePosition.x;
        if (mouseX < 0)
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        else
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);

        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
            Attack();
        
    }

    protected void Attack()
    {
        lastAttackTime = Time.time;

        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }

        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        currHP -= damage;

        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, (int)currHP);

        if (currHP <= 0)
            Die();
        else
        {
            StartCoroutine(DamageFlash());

            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }

    }

    [PunRPC]
    protected void Die()
    {
        dead = true;
        rb.isKinematic = true;

        transform.position = new Vector3(0, 99, 0);

        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;

        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));

        IEnumerator Spawn(Vector3 spawnPosition, float timeToSpawn)
        {
            yield return new WaitForSeconds(timeToSpawn);

            dead = false;
            transform.position = spawnPosition;
            currHP = maxHP;
            rb.isKinematic = false;

            headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, currHP);
        }
    }

    [PunRPC]
    protected void Heal(int amountToHeal)
    {
        currHP = Mathf.Clamp(currHP + amountToHeal, 0, maxHP);
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, (int)currHP);
    }

    [PunRPC]
    protected void GiveGold(int goldToGive)
    {
        gold += goldToGive;
        GameUI.instance.UpdateGoldText(gold);
    }

    protected virtual void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        rb.velocity = new Vector2(x, y) * moveSpeed;
    }
}
