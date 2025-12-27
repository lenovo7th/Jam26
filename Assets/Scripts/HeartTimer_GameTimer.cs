using UnityEngine;
using TMPro;
using UnityEngine.UIElements;




public class NewMonoBehaviourScript : MonoBehaviour
{
public int Nabiz;
public int geriSayim;
public int geriSayimBitisi;
public int geriSayimArtisHizi;
public int MaxNabiz;
public int NabizArtisHizi;
public AudioSource sakinKalpAtisi;
public AudioSource NormalKalpAtisi;
public AudioSource HizliKalpAtisi;
public AudioSource DahaHizliKalpAtisi;
public AudioSource AsiriHizliKalpAtisi;
public GameObject GameOverPanel;

public TextMeshProUGUI sayacYazisi;
public TextMeshProUGUI geriSayimYazisi;

void Start()
{
    InvokeRepeating("zamanlayici", 0f, 1f);
    InvokeRepeating("GeriSayimDongu", 0f, 1f);
    InvokeRepeating("SesKontrol", 0f, 10f);

}

    void FixedUpdate()
    {
        if (Nabiz >= 60)
        {
          geriSayimArtisHizi = 1; 
        }

      if (Nabiz >= 80)
        {
          geriSayimArtisHizi = 2;
        }

        if (Nabiz >= 120)
        {
          geriSayimArtisHizi = 3;   
        }

          if (Nabiz >= 160)
        {
          geriSayimArtisHizi = 4;     
        }
            if (Nabiz >= 200)
        {
          geriSayimArtisHizi = 5;
          
        }
        if (Nabiz == MaxNabiz)
        {
          geriSayimArtisHizi = 90;
        }
        if (geriSayim == geriSayimBitisi)
    {
      GameOverPanel.SetActive(true);
      UnityEngine.Cursor.lockState = CursorLockMode.None;
      UnityEngine.Cursor.visible = true;
    }

    }

    void zamanlayici ()
{

     if (Nabiz < MaxNabiz)
    {
        Nabiz += NabizArtisHizi;
        
        Nabiz = Mathf.Clamp(Nabiz, 0, MaxNabiz);

        sayacYazisi.text = Nabiz.ToString();
            
        Debug.Log(Nabiz);
    }
}

void GeriSayimDongu()
{
    if (geriSayim > geriSayimBitisi)
    {
        geriSayim -= geriSayimArtisHizi;

        geriSayim = Mathf.Clamp(geriSayim, geriSayimBitisi, geriSayim);

        geriSayimYazisi.text = geriSayim.ToString();

        Debug.Log(geriSayim);
}

}

void SesKontrol()
{
    if (Nabiz < 60)
    {
        sakinKalpAtisi.Play();
        NormalKalpAtisi.Stop();
        HizliKalpAtisi.Stop();
        DahaHizliKalpAtisi.Stop();
        AsiriHizliKalpAtisi.Stop();
    }

    if (Nabiz >= 60 && Nabiz < 80)
    {
        sakinKalpAtisi.Stop();
        NormalKalpAtisi.Play();
        HizliKalpAtisi.Stop();
        DahaHizliKalpAtisi.Stop();
        AsiriHizliKalpAtisi.Stop();
    }

    if (Nabiz >= 80 && Nabiz < 120)
    {
        sakinKalpAtisi.Stop();
        NormalKalpAtisi.Stop();
        HizliKalpAtisi.Play();
        DahaHizliKalpAtisi.Stop();
        AsiriHizliKalpAtisi.Stop();
    }

    if (Nabiz >= 120 && Nabiz < 160)
    {
        sakinKalpAtisi.Stop();
        NormalKalpAtisi.Stop();
        HizliKalpAtisi.Stop();
        DahaHizliKalpAtisi.Play();
        AsiriHizliKalpAtisi.Stop();
    }

    if (Nabiz >= 160)
    {
        sakinKalpAtisi.Stop();
        NormalKalpAtisi.Stop();
        HizliKalpAtisi.Stop();
        DahaHizliKalpAtisi.Stop();
        AsiriHizliKalpAtisi.Play();
    }

}


}