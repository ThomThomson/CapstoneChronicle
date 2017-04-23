using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    public GameObject spawnPoint;

    //private struct HealthRegeneration
    //{
    //    public HealthRegeneration(PlayerHealth context, int delay, int speed, int amount)
    //    {
    //        Delay = delay;
    //        Speed = speed;
    //        Amount = amount;
    //        delayStopwatch = new Stopwatch();
    //        delayStopwatch.Start();
    //        speedStopwatch = new Stopwatch();
    //        playerHealth = context;
    //        isActive = true;
    //    }
    //    public int Delay { get; }
    //    public int Speed { get; }
    //    public int Amount { get; }
    //    private Stopwatch delayStopwatch;
    //    private Stopwatch speedStopwatch;
    //    private PlayerHealth playerHealth;
    //    private bool isActive;
    //    public void TryRegenerate()
    //    {
    //        if (isActive
    //            && playerHealth.CurrentHealth < playerHealth.StartingHealth
    //            && delayStopwatch.ElapsedMilliseconds >= Delay)
    //        {
    //            if (speedStopwatch.IsRunning)
    //            {
    //                int speedStopwatchElapsedTime = (int)speedStopwatch.ElapsedMilliseconds;
    //                if (speedStopwatchElapsedTime >= Speed)
    //                {
    //                    UnityEngine.Debug.Log("speed stopwatch: " + speedStopwatchElapsedTime);
    //                    int multiplier = (int)(speedStopwatchElapsedTime / Speed);
    //                    UnityEngine.Debug.Log("multiplier: " + multiplier);
    //                    playerHealth.AddHealth(Amount * multiplier);
    //                    speedStopwatch.Reset();
    //                    speedStopwatch.Start();
    //                }
    //            }
    //            else
    //            {
    //                UnityEngine.Debug.Log("speed stopwatch reset");
    //                speedStopwatch.Start();
    //            }
    //        }
    //    }
    //    public void Reset()
    //    {
    //        delayStopwatch.Reset();
    //        delayStopwatch.Start();
    //        speedStopwatch.Reset();
    //        if (!isActive)
    //            isActive = true;
    //    }
    //    public void Stop()
    //    {
    //        delayStopwatch.Reset();
    //        speedStopwatch.Reset();
    //        isActive = false;
    //    }
    //}
    private struct HealthRegeneration {
        public HealthRegeneration(PlayerHealth context, float delay, float speed, int amount) {
            Delay = delay;
            Speed = speed;
            Amount = amount;
            playerHealth = context;
            _IsActive = true;
            lastDamageTakenTime = Time.time;
            lastRegenerationTime = 0f;
        }
        public float Delay;
        public float Speed;
        public int Amount;
        private float lastDamageTakenTime;
        private float lastRegenerationTime;
        private PlayerHealth playerHealth;
        private bool _IsActive;
        public bool IsActive {
            get {
                return _IsActive;
            }
        }
        public void TryRegenerate() {
            if (IsActive && playerHealth.CurrentHealth < playerHealth.StartingHealth && Time.time - lastDamageTakenTime >= Delay) {
                if (lastRegenerationTime == 0f) {
                    lastRegenerationTime = Time.time;
                } else {
                    int multiplier = (int)((Time.time - lastRegenerationTime) / Speed);
                    if (multiplier > 0) {
                        playerHealth.AddHealth(Amount * multiplier);
                        lastRegenerationTime = Time.time;
                    }
                }
            }
        }
        public void Reset() {
            lastDamageTakenTime = Time.time;
            lastRegenerationTime = 0f;
            _IsActive = true;
        }
        public void Stop() {
            lastDamageTakenTime = 0f;
            lastRegenerationTime = 0f;
            _IsActive = false;
        }
    }

    public int Lives { get; set; }
    public int StartingHealth { get; set; }
    public int CurrentHealth { get; set; }
    private HealthRegeneration healthRegeneration;
    public bool IsImmobilized { get; set; }
    public bool IsDead { get; set; }
    public Slider HealthSlider;
    public Text LivesText;
    public Text GameOver;
    public GameObject otherPlayer;
    public float sleepTime = 5f;

    private void UpdateUI() {
        HealthSlider.value = CurrentHealth;
        LivesText.text = Lives.ToString();
        //UnityEngine.Debug.Log("Current Health: " + CurrentHealth);
        //UnityEngine.Debug.Log("Lives: " + Lives);
    }

    void Start() {
        Lives = 3;
        StartingHealth = 1000;
        IsImmobilized = false;
        IsDead = false;
        CurrentHealth = StartingHealth;
        healthRegeneration = new HealthRegeneration(this, 5f, 0.01f, 5);
        UpdateUI();
    }

    void Update() {
        UpdateUI();
        if (!IsDead) {
            healthRegeneration.TryRegenerate();
        } else {
            sleepTime += Time.deltaTime;
            if (sleepTime > 5) {
                IsDead = false;
                GameOver.text = "";
                PlayerHealth otherPlayerHealth = otherPlayer.GetComponent<PlayerHealth>();
                Lives = 3;
                CurrentHealth = StartingHealth;
                otherPlayer.transform.position = new Vector3(-2, 2, 0);
                otherPlayerHealth.Lives = 3;
                otherPlayerHealth.CurrentHealth = otherPlayerHealth.StartingHealth;
                gameObject.transform.position = new Vector3(2, 2, 0);
            }
        }
    }

    //void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (hit.collider == GameObject.Find("sword1").GetComponent<Collider>())
    //    {
    //        TakeDamage(10);
    //    }
    //}

    void Die() {
        sleepTime = 0f;
        IsDead = true;
        GameOver.text = "Game Over. Restarting in 5 seconds.";
        float currentTime = Time.time;
        //TODO: Implement a die method
    }

    void Respawn() {
        gameObject.transform.position = spawnPoint.transform.position;
        CurrentHealth = StartingHealth;
    }

    void RemoveLife() {
        Lives -= 1;
        if (Lives >= 1)
            Respawn();
        else
            Die();
    }

    public void TakeDamage(int damageAmount) {
        if (!IsDead) {
            CurrentHealth -= damageAmount;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
            if (CurrentHealth == 0) {
                RemoveLife();
                //TODO: Implement immobilization feature
                //if (IsImmobilized)
                //    RemoveLife();
                //else
                //    IsImmobilized = true;
            }
            if (IsDead)
                healthRegeneration.Stop();
            else
                healthRegeneration.Reset();
        }
    }

    public void AddHealth(int healthAmount) {
        CurrentHealth += healthAmount;
        if (CurrentHealth > StartingHealth)
            CurrentHealth = StartingHealth;
    }

}