﻿using com.tinylabproductions.TLPLib.Android;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class Platform {
    public const string ANDROID = "android";
    public const string IOS = "ios";
    public const string WP8 = "wp8";
    public const string METRO = "metro";
    public const string BLACKBERRY = "blackberry";
    public const string WEB = "web";
    public const string PC = "pc";
    public const string OTHER = "other";

    public static string fullName { get {
      var sub = subname;
      return sub == "" ? name : name + "-" + subname;
    } }

    public static string name { get {
      switch (Application.platform) {
        case RuntimePlatform.Android: 
          return ANDROID;
        case RuntimePlatform.IPhonePlayer: 
          return IOS;
        case RuntimePlatform.WP8Player: 
          return WP8;
#if UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			  case RuntimePlatform.MetroPlayerX86:
			  case RuntimePlatform.MetroPlayerX64:
			  case RuntimePlatform.MetroPlayerARM:
				  return METRO;
#else
        case RuntimePlatform.WSAPlayerX86:
        case RuntimePlatform.WSAPlayerX64:
        case RuntimePlatform.WSAPlayerARM:
          return METRO;
#endif
        case RuntimePlatform.BlackBerryPlayer:
          return BLACKBERRY;
        case RuntimePlatform.WindowsWebPlayer:
        case RuntimePlatform.OSXWebPlayer:
          return WEB;
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.OSXPlayer:
        case RuntimePlatform.OSXDashboardPlayer:
        case RuntimePlatform.LinuxPlayer:
          return PC;
        default: 
          return OTHER;
      }
    } }

    public static string subname { get {
#if UNITY_ANDROID
      if (name == ANDROID) {
#if UNITY_AMAZON
        return "amazon";
#elif UNITY_OUYA
        return "ouya";
#elif UNITY_GAMESTICK
        return "gamestick";
#elif UNITY_OPERA
        return "opera";
#endif
        if (!Droid.hasSystemFeature("android.hardware.touchscreen")) {
          return "tv";
        }
      }
#endif
      if (name == PC) {
        switch (Application.platform) {
          case RuntimePlatform.WindowsPlayer:
            return "windows";
          case RuntimePlatform.OSXPlayer:
            return "osx";
          case RuntimePlatform.OSXDashboardPlayer:
            return "osx-dashboard";
          case RuntimePlatform.LinuxPlayer:
            return "linux";
        }
      }
      if (name == WEB) {
        switch (Application.platform) {
          case RuntimePlatform.WindowsWebPlayer:
            return "windows";
          case RuntimePlatform.OSXWebPlayer:
            return "osx";
        }
      }

      return "";
    } }
  }
}
