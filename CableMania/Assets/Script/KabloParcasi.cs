using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KabloParcasi : MonoBehaviour
{
    [SerializeField] private GameManager _GameManager;
    [SerializeField] private ParticleSystem[] KopmaEfektleri;
    private void OnCollisionEnter(Collision collision)
    {//Kopan parça zemine ya da prizlere dokunursa oyuncu leveli kaybeder.
        if (collision.gameObject.CompareTag("Zemin") || collision.gameObject.CompareTag("Soket"))
        {
            _GameManager.Kaybettin();
            KopmaEfektleri[0].gameObject.SetActive(true);
            KopmaEfektleri[1].gameObject.SetActive(true);
            KopmaEfektleri[0].Play();
            KopmaEfektleri[1].Play();
        }
    }
}
