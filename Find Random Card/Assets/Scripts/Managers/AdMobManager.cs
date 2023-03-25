using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdMobManager : MonoBehaviour
{
    //string adUnitId = "ca-app-pub-4348469999914971/4134480658";
#if UNITY_ANDROID
    // production ID
    // app ID:                      ca-app-pub-4348469999914971~9022741604

    // test ID
    // app ID:                      ca-app-pub-3940256099942544~3347511713
    string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
    //string adUnitId = "unused";
#endif

    BannerView bannerView;

    private void Awake()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) => { });

    }

    void CreateBannerView()
    {
#if UNITY_EDITOR
        print("Creating banner view");
#endif

        if (bannerView != null)
        {
            DestroyAd();
        }

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
    }

    public void LoadAd()
    {
        if (bannerView == null)
        {
            CreateBannerView();
        }

        AdRequest adRequest = new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();

#if UNITY_EDITOR
        print("Loading banner Ad.");
#endif
        bannerView.LoadAd(adRequest);
        bannerView.Hide();
    }

    public void DestroyAd()
    {
        if (bannerView != null)
        {
#if UNITY_EDITOR
            print("Destroying banner Ad.");
#endif
            bannerView.Destroy();
            bannerView = null;
        }
    }

    public void ShowAd()
    {
        bannerView.Show();
    }

    public void HideAd()
    {
        bannerView.Hide();
    }
}
