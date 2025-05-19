using System;
using UnityEngine;

namespace Utils
{
    public static class AndroidAESBidge
    {
        // private const string JavaClassPath = "com.HosuStudio.keystore";
        private const string JavaClassPath = "com.hosustudio.aeskeyhelper.AESKeystoreHelper";

        public static string Encrypt(string plainText)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using AndroidJavaClass javaClass = new(JavaClassPath);
            return javaClass.CallStatic<string>("encrypt", plainText);
#else
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
#endif
        }

        public static string Decrypt(string cipherText)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using AndroidJavaClass javaClass = new(JavaClassPath);
            return javaClass.CallStatic<string>("decrypt", cipherText);
#else
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
#endif
        }
    }
}