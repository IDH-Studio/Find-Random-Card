using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdMobManager : MonoBehaviour
{
    // production ID
    // app ID:                      ca-app-pub-4348469999914971~9022741604

    // test ID
    // app ID:                      ca-app-pub-3940256099942544~3347511713

    string _adUnitId;

    BannerView _bannerView;

    private void Awake()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) => { });

#if UNITY_ANDROID
        // 제품 ID
        //_adUnitId = "ca-app-pub-4348469999914971/4134480658";

        // 테스트 ID
        _adUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
        _adUnitId = "unused";
#endif

    }

    void CreateBannerView()
    {
#if UNITY_EDITOR
        print("Creating banner view");
#endif

        if (_bannerView != null)
        {
            DestroyAd();
        }

        _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);
    }

    public void LoadAd()
    {
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        AdRequest adRequest = new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();

#if UNITY_EDITOR
        print("Loading banner Ad.");
#endif
        _bannerView.LoadAd(adRequest);
    }

    public void DestroyAd()
    {
        if (_bannerView != null)
        {
#if UNITY_EDITOR
            print("Destroying banner Ad.");
#endif
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void ShowAd()
    {
        _bannerView.Show();
    }

    public void HideAd()
    {
        _bannerView.Hide();
    }
}
