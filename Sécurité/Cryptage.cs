﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{

    public class rfc2898test
    {
        // Generate a key k1 with password pwd1 and salt salt1.
        // Generate a key k2 with password pwd1 and salt salt1.
        // Encrypt data1 with key k1 using symmetric encryption, creating edata1.
        // Decrypt edata1 with key k2 using symmetric decryption, creating data2.
        // data2 should equal data1.

        private const string usageText = "Usage: RFC2898 <password>\nYou must specify the password for encryption.\n";
        public static void Main(string[] passwordargs)
        {
            //If no file name is specified, write usage text.
            if (passwordargs.Length == 0)
            {
                Console.WriteLine(usageText);
            }
            else
            {
                string pwd1 = passwordargs[0];
                // Create a byte array to hold the random value.
                byte[] salt1 = new byte[8];
                using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
                {
                    // Fill the array with a random value.
                    rngCsp.GetBytes(salt1);
                }

                //data1 can be a string or contents of a file.
                string data1 = "Some test data";
                //The default iteration count is 1000 so the two methods use the same iteration count.
                int myIterations = 1000;
                try
                {
                    Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt1,
    myIterations);
                    Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt1);
                    // Encrypt the data.
                    Aes encAlg = Aes.Create();
                    encAlg.Key = k1.GetBytes(16);
                    MemoryStream encryptionStream = new MemoryStream();
                    CryptoStream encrypt = new CryptoStream(encryptionStream,
    encAlg.CreateEncryptor(), CryptoStreamMode.Write);
                    byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(
    data1);

                    encrypt.Write(utfD1, 0, utfD1.Length);
                    encrypt.FlushFinalBlock();
                    encrypt.Close();
                    byte[] edata1 = encryptionStream.ToArray();
                    k1.Reset();

                    // Try to decrypt, thus showing it can be round-tripped.
                    Aes decAlg = Aes.Create();
                    decAlg.Key = k2.GetBytes(16);
                    decAlg.IV = encAlg.IV;
                    MemoryStream decryptionStreamBacking = new MemoryStream();
                    CryptoStream decrypt = new CryptoStream(
    decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                    decrypt.Write(edata1, 0, edata1.Length);
                    decrypt.Flush();
                    decrypt.Close();
                    k2.Reset();
                    string data2 = new UTF8Encoding(false).GetString(
    decryptionStreamBacking.ToArray());

                    if (!data1.Equals(data2))
                    {
                        Console.WriteLine("Error: The two values are not equal.");
                    }
                    else
                    {
                        Console.WriteLine("The two values are equal.");
                        Console.WriteLine("k1 iterations: {0}", k1.IterationCount);
                        Console.WriteLine("k2 iterations: {0}", k2.IterationCount);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: ", e);
                }
            }
        }
    }
    public static class Cryptage
    {
        private const string password = "hyddhrii%2moi43Hd5%%";
        private static readonly byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        public static string Encrypte(string texte)
        {
            string[] passwordArgs = new string[] { password };
            rfc2898test.Main(passwordArgs);
            string encodé = null;
            byte[] bytes = Encoding.Unicode.GetBytes(texte);
            using (Aes encrypteur = Aes.Create())
            {
                Rfc2898DeriveBytes pbk = new Rfc2898DeriveBytes(password, salt);
                encrypteur.Key = pbk.GetBytes(32);
                encrypteur.IV = pbk.GetBytes(16);

                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encrypteur.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytes, 0, bytes.Length);
                    cs.Close();
                }
                encodé = Encoding.Unicode.GetString(ms.ToArray());

                string décodé = Décrypte(encodé);
            }
            return encodé;
        }

        public static string Décrypte(string texte)
        {
            texte = texte.Replace(" ", "+");
            byte[] bytes = Convert.FromBase64String(texte);
            using (Aes encrypteur = Aes.Create())
            {
                Rfc2898DeriveBytes pbk = new Rfc2898DeriveBytes(password, salt);
                encrypteur.Key = pbk.GetBytes(32);
                encrypteur.IV = pbk.GetBytes(16);

                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encrypteur.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytes, 0, bytes.Length);
                    cs.Close();
                }
                texte = Encoding.Unicode.GetString(ms.ToArray());


            }
            return texte;
        }

        public static string Encrypte(Object objet)
        {
            string texte = JsonSerializer.Serialize(objet);
            string encrypté = Encrypte(texte);
            return encrypté;
        }

        public static T Décrypte<T>(string texteCrypté)
        {
            string texte = Décrypte(texteCrypté);
            return JsonSerializer.Deserialize<T>(texte);
        }
    }
}
