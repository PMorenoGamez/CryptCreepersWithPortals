﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float h;
    float v;
    Vector3 moveDirection;
    public float speed = 3;
    [SerializeField] Transform aim;
    [SerializeField] Camera camera;
    Vector2 facingDirection;
    [SerializeField] Transform bulletPrefab;
    bool gunLoaded = true;
    [SerializeField] float fireRate = 1;
    [SerializeField] int health = 10;
    bool powerShotEnabled;
    bool invulnerable;
    [SerializeField] float invulnerableTime = 3;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float blinkRate = 1;
    CameraController camController;
    [SerializeField] AudioClip powerUp;

    public int Health
    {
        get => health;
        set
        {
            health = value;
            UIManager.Instance.UpdateUIHealth(health);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        camController = FindObjectOfType<CameraController>();
        UIManager.Instance.UpdateUIHealth(health);
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        moveDirection.x = h;
        moveDirection.y = v;

        transform.position += moveDirection * Time.deltaTime * speed;

        //Movimiento de la mira
        facingDirection = camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        aim.position = transform.position + (Vector3)facingDirection.normalized;

        if (Input.GetMouseButton(0) && gunLoaded)
        {
            gunLoaded = false;
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Transform bulletClone = Instantiate(bulletPrefab, transform.position, targetRotation);
            if (powerShotEnabled)
            {
                bulletClone.GetComponent<Bullet>().powerShot = true;
            }
            StartCoroutine(ReloadGun());
        }

        anim.SetFloat("Speed", moveDirection.magnitude);
        if(aim.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;
        }

        else if(aim.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
    }


    IEnumerator ReloadGun()
    {
        yield return new WaitForSeconds(1/fireRate);
        gunLoaded = true;
    }

    public void TakeDamage()
    {
        if (invulnerable)
            return;

        Health--;
        invulnerable = true;
        fireRate = 1;
        powerShotEnabled = false;
        camController.Shake();
        StartCoroutine(MakeVulnerableAgain());
        if (Health <= 0)
        {
            GameManager.Instance.gameOver = true;
            UIManager.Instance.ShowGameOverScreen();
        }
    }

    IEnumerator BlinkRoutine()
    {
        int t = 10;
        while(t > 0)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(t * blinkRate);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(t * blinkRate);
            t--;
        }
    }

    IEnumerator MakeVulnerableAgain()
    {
        StartCoroutine(BlinkRoutine());
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PowerUp"))
        {

            switch (collision.GetComponent<PowerUp>().powerUpType)
            {
                case PowerUp.PowerUpType.FireRateIncrease:
                    fireRate++;
                    break;

                case PowerUp.PowerUpType.PowerShot:
                    powerShotEnabled = true;
                    break;
            }

            AudioSource.PlayClipAtPoint(powerUp, transform.position);
            Destroy(collision.gameObject, 0.1f);

        }
    }

}
