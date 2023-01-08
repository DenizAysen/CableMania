using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarpismaKontrol : MonoBehaviour
{
    public GameManager _GameManager;
    public int CarpismaIndex;
    Collider[] HitColl;
    void Update()
    {
        HitColl = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity);

        for (int i = 0; i < HitColl.Length; i++)
        {//Eger kablolar çizilen obje ile temas ederse false deðeri gönderir
            if (HitColl[i].CompareTag("KabloParcasi"))
            {
                _GameManager.CarpismayiKontrolEt(CarpismaIndex,false);
            }
            else
            {
                _GameManager.CarpismayiKontrolEt(CarpismaIndex, true);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale/2 );
    }
}
