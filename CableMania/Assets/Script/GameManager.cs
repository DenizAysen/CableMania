using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    GameObject SeciliObje;
    GameObject SeciliSoket;
    public bool HareketVar;//�oklu fi� se�imini engellemek i�in olu�turuldu.
    [Header("-----LEVEL AYARLARI")]
    [SerializeField] private GameObject[] CarpismaKontrolObjeleri;
    [SerializeField] private GameObject[] Fisler;
    [SerializeField] private int HedefSoketSayisi;
    [SerializeField] private List<bool> CarpismaDurumlari;
    [SerializeField] private int HamleHakki;
    [SerializeField] private HingeJoint[] KopmaNoktalari;
    int TamamlanmaSayisi;
    int CarpmaKontrolSayisi;
    SonFis _SonFis;
    [Header("-----D�GER OBJELER")]
    [SerializeField] private GameObject[] Isiklar;
    [SerializeField] private AudioSource FisSesi;
    [Header("-----UI OBJELERI")]
    [SerializeField] private GameObject KontrolPaneli;
    [SerializeField] private TextMeshProUGUI KontrolText;
    [SerializeField] private TextMeshProUGUI HamleHakkiText;
    [SerializeField] private TextMeshProUGUI[] UITextleri;
    [SerializeField] private GameObject[] Paneller;
    void Start()
    {
        HamleHakkiText.text = "MOVES : " + HamleHakki.ToString();
        for (int i = 0; i < HedefSoketSayisi-1; i++)
        {
            CarpismaDurumlari.Add(false);
        }

        UITextleri[3].text = PlayerPrefs.GetInt("Para").ToString();
        
        Invoke("BaglantiNoktalariniAyarla",2f);
    }
    
    void BaglantiNoktalariniAyarla()
    {
        foreach (var item in KopmaNoktalari)
        {
            item.breakForce = 600;
            item.breakTorque = 500;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))// Farenin sol tu�una bas�nca kameradan sahneye ���n g�nderir.
            {
                if (hit.collider != null)
                {
                    //##SON F��
                    if (SeciliObje == null && !HareketVar)//Daha �nceden bir objenin se�ilmedi�i ve hareketin olmad��� durumda �al���r
                    {
                        if (hit.collider.CompareTag("Mavi_Fis") || hit.collider.CompareTag("Sari_Fis") || hit.collider.CompareTag("Kirmizi_Fis"))
                        {//I��n null de�ilse ve fi�lere deymi�se ko�ula girer.
                            FisSesi.Play();
                            _SonFis = hit.collider.GetComponent<SonFis>(); //I��n�n dokunmu� oldu�u fi�in scriptini olu�turdu�um de�i�kene atad�m.
                            _SonFis.FisHareketleri("Secim", _SonFis.MevcutSoket, _SonFis.MevcutSoket.GetComponent<Soket>().HareketPozisyonu);
                            SeciliObje = hit.collider.gameObject;
                            SeciliSoket = _SonFis.MevcutSoket;
                            HareketVar = true;
                        }
                    }
                    //##SON F��

                    //##SOKET
                    if (hit.collider.CompareTag("Soket"))
                    {//Se�ilmi� bir fi� varsa, t�klanan soket dolu de�ilse ve Secili soket, fi�in ��kar�ld��� soketle ayn� de�ilse fi�i yeni sokete y�nlendirir 
                        if(SeciliObje != null && !hit.collider.GetComponent<Soket>().Doluluk && SeciliSoket != hit.collider.gameObject)
                        {
                            SeciliSoket.GetComponent<Soket>().Doluluk = false;//Fi�in �ekildi�i soketin art�k bo� oldu�u anlam�na gelir
                            Soket _Soket = hit.collider.GetComponent<Soket>();
                            _SonFis.FisHareketleri("PosDegis", hit.collider.gameObject, _Soket.HareketPozisyonu);
                            _Soket.Doluluk = true;//Fisin yeni tak�ld��� soketi dolu yapar.
                            HamleHakki--;//Bir soketten ba�ka sokete ge�ti�inde hamle hakk� 1 azal�r
                            HamleHakkiText.text = "MOVES : " + HamleHakki.ToString();
                            SeciliObje = null;
                            SeciliSoket = null;
                        }
                        else if (SeciliSoket == hit.collider.gameObject)//Ayn� sokete tekrar dokunursak bulundu�u sokete geri d�ner
                        {
                            _SonFis.FisHareketleri("SoketeOtur", hit.collider.gameObject);
                            SeciliObje = null;
                            SeciliSoket = null;
                            HareketVar = true;
                        }
                    }
                    //##SOKET
                }
            }
        }
    }
    public void CarpismayiKontrolEt(int CarpismaIndex, bool durum)
    {
        CarpismaDurumlari[CarpismaIndex] = durum;
    }
    public void FisleriKontrolEt()
    {//Tak�lan fisler ile prizlerin rengi ayn� olup olmad���n� kontrol eder. Ayn� ise TamamlanmaSayisini 1 artt�r�r
        foreach (var item in Fisler)
        {
            if (item.GetComponent<SonFis>().MevcutSoket.name == item.GetComponent<SonFis>().SoketRengi)
            {
                TamamlanmaSayisi++;
            }
        }
        if (TamamlanmaSayisi == HedefSoketSayisi)//Levelde istenen hedef priz say�s�na ula��lm��sa CarpismaKontrolObjelerini aktifle�tirir
        {//Kablolar d���mlenmemi�se oyuncu Leveli kazan�r.
            Debug.Log("Tum soketler yerinde");

            foreach (var item in CarpismaKontrolObjeleri)
            {
                item.SetActive(true);
            }
            StartCoroutine(CarpismaVarmi());
        }
        else
        {
            if (HamleHakki <= 0)
            {
                Kaybettin();
            }
        }
        TamamlanmaSayisi = 0;
    }   
    IEnumerator CarpismaVarmi()
    { /*Fisler prizlere dogru sekilde yerle�tirildikten 4 saniye sonra, fisler dugum de�ilse oyuncu leveli kazan�r
    Eger fisler dugum olmu�sa oyuncu tekrardan do�ru bir bi�imde yerel�tirdi�inde metot �al���r*/
        Isiklar[0].SetActive(false);//K�rm�z� ���k kapan�r
        Isiklar[1].SetActive(true);//Sar� ���k a��l�r
        KontrolPaneli.SetActive(true);
        KontrolText.text = "CONTROLLING....";

        yield return new WaitForSeconds(2.5f);
        foreach (var item in CarpismaDurumlari)
        {
            if (item)
                CarpmaKontrolSayisi++;
        }

        if (CarpmaKontrolSayisi == CarpismaDurumlari.Count)
        {
            Invoke("Kazandin", 0.5f);
        }
        else
        {
            Isiklar[0].SetActive(true);
            Isiklar[1].SetActive(false);
            KontrolText.text = "THERE IS A COLLISON";
            Invoke("PaneliKapat", 2f);
            foreach (var item in CarpismaKontrolObjeleri)
            {
                item.SetActive(false);
            }
            if (HamleHakki <= 0)
            {
                Kaybettin();
            }
        }
        CarpmaKontrolSayisi = 0;
    }
    void PaneliKapat()
    {
        KontrolPaneli.SetActive(false);
    }
    public void Kaybettin()
    {
        UITextleri[1].text= "LEVEL : " + SceneManager.GetActiveScene().name;
        Paneller[2].SetActive(true);
        Time.timeScale = 0f;
    }
    void Kazandin()
    {
        Isiklar[1].SetActive(false);//Sar� ���k kapan�r
        Isiklar[2].SetActive(true);//Ye�il ���k a��l�r
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        UITextleri[0].text = "LEVEL : " + SceneManager.GetActiveScene().name;
        KontrolText.text = "YOU WIN";

        int randomPara = Random.Range(5, 20);
        PlayerPrefs.SetInt("Para", randomPara+PlayerPrefs.GetInt("Para"));
        UITextleri[2].text = "MONEY : +" + randomPara;
        Paneller[1].SetActive(true);
        Time.timeScale = 0f;
    }
    public void ButtonIslemleri(string Deger)
    {
        switch (Deger)
        {
            case "Durdur":
                Paneller[0].SetActive(true);
                Time.timeScale = 0f;
                break;
            case "DevamEt":
                Paneller[0].SetActive(false);
                Time.timeScale = 1f;
                break;
            case "SonrakiLevel":
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                Time.timeScale = 1f;
                break;
            case "Tekrar":
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                Time.timeScale = 1f;
                break;
            case "Cikis":
                Application.Quit();
                break;
        }
    }
    public void FisSesiCal()
    {
        FisSesi.Play();
    }
}
