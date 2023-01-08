using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonFis : MonoBehaviour
{
    public GameObject MevcutSoket;//Fi�in bulundu�u soket/priz
    public string SoketRengi;
    [SerializeField] private GameManager _GameManager;

    bool Secildi; 
    bool PosDegistir;    
    public bool SoketeOtur;

    GameObject HareketPozisyonu;
    GameObject SoketinKendisi;

    public void FisHareketleri(string islem, GameObject Soket, GameObject GidilecekObje = null)
    {//SoketeOtur isleminde GidilecekObje parametresine ihtiya� olmad��� i�in opsiyonel yapt�m
        switch (islem)
        {
            case "Secim":
                HareketPozisyonu = GidilecekObje;
                Secildi = true;
                break;
            case "PosDegis":
                HareketPozisyonu = GidilecekObje;
                SoketinKendisi = Soket;
                PosDegistir = true;
                break;
            case "SoketeOtur":
                SoketinKendisi = Soket;
                SoketeOtur = true;
                break;
        }
    }
    void Update()
    {
        
        if (Secildi)
        {
            transform.position = Vector3.Lerp(transform.position, HareketPozisyonu.transform.position, 0.08f);
            if (Vector3.Distance(transform.position, HareketPozisyonu.transform.position) < 0.01f)
            {
                Secildi = false;
            }
        }
        else if (PosDegistir)
        {
            transform.position = Vector3.Lerp(transform.position, HareketPozisyonu.transform.position, 0.04f);
            if (Vector3.Distance(transform.position, HareketPozisyonu.transform.position) < 0.01f)
            {
                PosDegistir = false;
                SoketeOtur = true;
            }
        }
        if (Time.timeScale == 0)
        {
            SoketeOtur = false;
        }
        else if (SoketeOtur)
        {
            transform.position = Vector3.Lerp(transform.position, SoketinKendisi.transform.position, 0.08f);
            if (Vector3.Distance(transform.position, SoketinKendisi.transform.position) < 0.01f)
            {
                _GameManager.FisSesiCal();
                SoketeOtur = false;
                _GameManager.HareketVar = false;//Fi� prize tak�l�nca hareket biter.B�ylece yeni fi� se�ilebilir.
                MevcutSoket = SoketinKendisi;
                _GameManager.FisleriKontrolEt();
            }
        }
    }
}
