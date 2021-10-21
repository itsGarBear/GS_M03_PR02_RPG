using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Builder : PlayerController
{
    [Header("Builder")]
    public SpriteRenderer currBlockSprite;
    public Sprite[] blockTypes;
    public bool[] blockNeedsTrigger;
    public string[] tags;
    public GameObject[] rectangleBG;
    private int blockNdx = 0;


    private void Start()
    {
        for(int i = 0; i < blockTypes.Length; i++)
        {
            rectangleBG[i] = GameObject.Find(i.ToString());
            rectangleBG[i].SetActive(false);
        }

        currBlockSprite.gameObject.transform.localScale = Vector3.one;
        rectangleBG[blockNdx].SetActive(true);
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

        if (Input.GetMouseButtonDown(0))
        {
            if (blockNdx == 0)
            {
                if (Time.time - lastAttackTime > attackRate)
                {
                    Attack();
                }
            }    
            else
            {
                PlaceBlock();
            }
        }  
        else if (Input.GetMouseButtonDown(1))
            ToggleBlockChoice();
        
    }

    public void PlaceBlock()
    {
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        if (hit.collider.isTrigger && hit.collider.gameObject.CompareTag("Background"))
        {
            weaponAnim.SetTrigger("Attack");
            photonView.RPC("ChangeBlock", RpcTarget.AllBuffered, hit.collider.gameObject.name, blockNdx);
        }
        else if (hit.collider != null && hit.collider.gameObject.CompareTag("Collision"))
        {
            weaponAnim.SetTrigger("Attack");
            photonView.RPC("ChangeBlock", RpcTarget.AllBuffered, hit.collider.gameObject.name, blockNdx);
        }
    }

    [PunRPC]
    public void ChangeBlock(string name, int block)
    {
        GameObject go = GameObject.Find(name);

        go.tag = tags[blockNdx];
        go.GetComponent<SpriteRenderer>().sprite = blockTypes[block];
        go.GetComponent<BoxCollider2D>().isTrigger = blockNeedsTrigger[block];
    }

    public void ToggleBlockChoice()
    {
        rectangleBG[blockNdx].SetActive(false);
        blockNdx = (blockNdx + 1) % blockTypes.Length;
        rectangleBG[blockNdx].SetActive(true);
        currBlockSprite.sprite = blockTypes[blockNdx];
        if (blockNdx == 0)
        {
            currBlockSprite.gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            currBlockSprite.gameObject.transform.localScale = new Vector3(.33f, .33f, .33f);
        }
    }
}
