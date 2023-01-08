using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    GameObject SeciliObje;
    GameObject SeciliSoket;
    public bool HareketVar;//Çoklu fiþ seçimini engellemek için oluþturuldu.
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
    [Header("-----DÝGER OBJELER")]
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
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))// Farenin sol tuþuna basýnca kameradan sahneye ýþýn gönderir.
            {
                if (hit.collider != null)
                {
                    //##SON FÝÞ
                    if (SeciliObje == null && !HareketVar)//Daha önceden bir objenin seçilmediði ve hareketin olmadýðý durumda çalýþýr
                    {
                        if (hit.collider.CompareTag("Mavi_Fis") || hit.collider.CompareTag("Sari_Fis") || hit.collider.CompareTag("Kirmizi_Fis"))
                        {//Iþýn null deðilse ve fiþlere deymiþse koþula girer.
                            FisSesi.Play();
                            _SonFis = hit.collider.GetComponent<SonFis>(); //Iþýnýn dokunmuþ olduðu fiþin scriptini oluþturduðum deðiþkene atadým.
                            _SonFis.FisHareketleri("Secim", _SonFis.MevcutSoket, _SonFis.MevcutSoket.GetComponent<Soket>().HareketPozisyonu);
                            SeciliObje = hit.collider.gameObject;
                            SeciliSoket = _SonFis.MevcutSoket;
                            HareketVar = true;
                        }
                    }
                    //##SON FÝÞ

                    //##SOKET
                    if (hit.collider.CompareTag("Soket"))
                    {//Seçilmiþ bir fiþ varsa, týklanan soket dolu deðilse ve Secili soket, fiþin çýkarýldýðý soketle ayný deðilse fiþi yeni sokete yönlendirir 
                        if(SeciliObje != null && !hit.collider.GetComponent<Soket>().Doluluk && SeciliSoket != hit.collider.gameObject)
                        {
                            SeciliSoket.GetComponent<Soket>().Doluluk = false;//Fiþin çekildiði soketin artýk boþ olduðu anlamýna gelir
                            Soket _Soket = hit.collider.GetComponent<Soket>();
                            _SonFis.FisHareketleri("PosDegis", hit.collider.gameObject, _Soket.HareketPozisyonu);
                            _Soket.Doluluk = true;//Fisin yeni takýldýðý soketi dolu yapar.
                            HamleHakki--;//Bir soketten baþka sokete geçtiðinde hamle hakký 1 azalýr
                            HamleHakkiText.text = "MOVES : " + HamleHakki.ToString();
                            SeciliObje = null;
                            SeciliSoket = null;
                        }
                        else if (SeciliSoket == hit.collider.gameObject)//Ayný sokete tekrar dokunursak bulunduðu sokete geri döner
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
    {//Takýlan fisler ile prizlerin rengi ayný olup olmadýðýný kontrol eder. Ayný ise TamamlanmaSayisini 1 arttýrýr
        foreach (var item in Fisler)
        {
            if (item.GetComponent<SonFis>().MevcutSoket.name == item.GetComponent<SonFis>().SoketRengi)
            {
                TamamlanmaSayisi++;
            }
        }
        if (TamamlanmaSayisi == HedefSoketSayisi)//Levelde istenen hedef priz sayýsýna ulaþýlmýþsa CarpismaKontrolObjelerini aktifleþtirir
        {//Kablolar düðümlenmemiþse oyuncu Leveli kazanýr.
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
    { /*Fisler prizlere dogru sekilde yerleþtirildikten 4 saniye sonra, fisler dugum deðilse oyuncu leveli kazanýr
    Eger fisler dugum olmuþsa oyuncu tekrardan doðru bir biçimde yerelþtirdiðinde metot çalýþýr*/
        Isiklar[0].SetActive(false);//Kýrmýzý ýþýk kapanýr
        Isiklar[1].SetActive(true);//Sarý ýþýk açýlýr
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
        Isiklar[1].SetActive(false);//Sarý ýþýk kapanýr
        Isiklar[2].SetActive(true);//Yeþil ýþýk açýlýr
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
