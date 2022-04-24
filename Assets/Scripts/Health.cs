using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int curHealth = 0;
    public int maxHealth = 100;

    public HealthBar healthBar;
    public Text DeathCounter;
    public Animator DeathAnimator;

    private int AmountDeaths = 0;

    void Start()
    {
        curHealth = maxHealth;
    }

    public void DamagePlayer(int damage)
    {
        curHealth -= Mathf.Clamp(damage, 0, 100);
        healthBar.SetHealth(curHealth);

        if(curHealth <= 0)
            StartCoroutine(OneMoarDeath());
    }

    IEnumerator OneMoarDeath()
    {
        DeathAnimator.SetBool("death", true);
        AmountDeaths++;
        yield return new WaitForSeconds(0.1f);
        DeathAnimator.SetBool("death", false);
        DeathCounter.text = AmountDeaths.ToString();
        curHealth = maxHealth;
        healthBar.SetHealth(curHealth);
        yield break;
    }
}