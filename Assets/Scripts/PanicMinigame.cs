﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanicMinigame : MonoBehaviour
{
    //target variables
    [SerializeField] Transform leftPivot;
    [SerializeField] Transform rightPivot;//limits of targets' position
    [SerializeField] Transform target;//the moving target itself

    SoundManager soundManager = SoundManager.instance;
    
    float targetPosition;
    float targetDestination;
    float targetTimer;
    [SerializeField] float timeMultiplier = 3f;
    float targetSpeed;
    [SerializeField] float smoothMotion = 1f;
    //end of target variables

    //Panic control variables
    [SerializeField] Transform playerControl;//referência a barra que vai ser movida
    float barPosition;
    [SerializeField] float barSize;
    float barMoveVelocity;//a velocidade da barra em si
    [SerializeField] float barMovePower = 0.03f;//a "força" aplicada para a barra se mexer
    //end of panic control variables

    //Variáveis de progresso de panico
    [SerializeField] Transform progressBar;//referência a barra de progresso
    float barProgress;
    [SerializeField] float barPower = 0.0000001f;//determina a velocidade de progresso
    [SerializeField] float barDegradationPower = 0.1f;//a velocidade que a barra de progresso retrocede
    //fim das variáveis de progresso de panico

    //variáveis auxiliares
    public bool pause;
    [SerializeField] float failTimer = 1000f;
    public GameObject Player;
    public GameObject Enemy;

    void Start()
    {
        barSize = playerControl.localScale.x;
        pause = true;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!pause)
        {
            if (!soundManager.IsPlaying("Breathing"))
            {
                soundManager.PlayLooped("Breathing");
            }
            Target();
            PanicControl();
            ProgressCheck();
        }else if (soundManager.IsPlaying("Breathing"))
        {
            soundManager.StopSound("Breathing");
        }
    }

    void PanicControl()
    {
        if (Input.GetMouseButton(0))//move para a direita
        {
            barMoveVelocity -= barMovePower * Time.deltaTime;
        } 
        if (Input.GetMouseButton(1))//move para a esquerda
        {
            barMoveVelocity += barMovePower * Time.deltaTime;
        }


        barPosition += barMoveVelocity;

        if((barPosition - (barSize / 2) <= 0 && barMoveVelocity < 0) || (barPosition + (barSize / 2) >= 1 && barMoveVelocity > 0))//se está tentando mover a barra além dos limites
        {
            barMoveVelocity = 0;//limita velocidade
        }

        barPosition = Mathf.Clamp(barPosition, barSize / 2, 1 - (barSize / 2));
        playerControl.position = Vector2.Lerp(leftPivot.position, rightPivot.position, barPosition);
    }

    void Target()
    {
        targetTimer -= Time.deltaTime;
        if (targetTimer < 0f)
        {
            targetTimer = UnityEngine.Random.value * timeMultiplier;//move o peixe em uma direção por uma quantidade aleatória de tempo

            targetDestination = UnityEngine.Random.value;
        }
        targetPosition = Mathf.SmoothDamp(targetPosition, targetDestination, ref targetSpeed, smoothMotion);
        target.position = Vector2.Lerp(leftPivot.position, rightPivot.position, targetPosition);
    }

    void ProgressCheck()
    {
        Vector2 localScale = progressBar.localScale;
        localScale.x = barProgress;
        progressBar.localScale = localScale;

        float min = barPosition - barSize / 2;
        float max = barPosition + barSize / 2;//limites de onde a barra alcança

        if(min < targetPosition && targetPosition < max)//se alvo está dentro da barra controlada
        {
            barProgress += barPower * Time.deltaTime;//acalma
        }
        else
        {
            barProgress -= barDegradationPower * Time.deltaTime;//se não, panica

            failTimer -= Time.deltaTime;
            if (failTimer < 0)
            {
                Panic();
            }

        }

        if(barProgress >= 1)
        {
            StayCalm();
        }

        barProgress = Mathf.Clamp(barProgress, 0, 1);
    }

    void StayCalm()
    {
        pause = true;
        barProgress = 0;
        gameObject.SetActive(false);
    }

    void Panic()
    {
        pause = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
