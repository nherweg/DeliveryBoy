﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DeliveryItems : MonoBehaviour
{
    [System.Serializable]
    public struct StoreAudio
    {
        public string storeName;
        public AudioClip[] englishAudio;
        public AudioClip[] germanAudio;
    }

    private static List<string> unused_store_names = new List<string>();

    private System.Random random;

    public StoreAudio[] storeNamesToItems;

    private static string RemainingItemsPath(string storeName)
    {
        return System.IO.Path.Combine(UnityEPL.GetParticipantFolder(), "remaining_items", storeName);
    }

    private void WriteRemainingItemsFiles()
    {
        foreach (StoreAudio storeAudio in storeNamesToItems)
        {
            string remainingItemsPath = RemainingItemsPath(storeAudio.storeName);
            if (!System.IO.File.Exists(remainingItemsPath))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(remainingItemsPath));
                System.IO.File.Create(remainingItemsPath).Close();
                AudioClip[] languageAudio;
                if (LanguageSource.current_language.Equals(LanguageSource.LANGUAGE.ENGLISH))
                {
                    languageAudio = storeAudio.englishAudio;
                }
                else
                {
                    languageAudio = storeAudio.germanAudio;
                }
                foreach (AudioClip clip in languageAudio)
                {
                    System.IO.File.AppendAllLines(remainingItemsPath, new string[] { clip.name });
                }
            }
        }
    }

    private void WriteAlphabetizedItemsFile()
    {
        string outputFilePath = System.IO.Path.Combine(UnityEPL.GetParticipantFolder(), "all_items.txt");
        List<string> allItems = new List<string>();
        foreach (StoreAudio storeAudio in storeNamesToItems)
        {
            AudioClip[] languageAudio;
            if (LanguageSource.current_language.Equals(LanguageSource.LANGUAGE.ENGLISH))
            {
                languageAudio = storeAudio.englishAudio;
            }
            else
            {
                languageAudio = storeAudio.germanAudio;
            }
            foreach (AudioClip clip in languageAudio)
            {
                allItems.Add(clip.name);
            }
        }
        allItems.Sort();
        System.IO.File.AppendAllLines(outputFilePath, allItems);
    }

    private void WriteStoreNamesFile()
    {
        string outputFilePath = System.IO.Path.Combine(UnityEPL.GetParticipantFolder(), "all_stores.txt");
        List<string> allStores = new List<string>();
        foreach (StoreAudio storeAudio in storeNamesToItems)
        {
            allStores.Add(LanguageSource.GetLanguageString(storeAudio.storeName));
        }
        allStores.Sort();
        System.IO.File.AppendAllLines(outputFilePath, allStores);
    }

    void Awake()
    {
        random = new System.Random(UnityEPL.GetParticipants()[0].GetHashCode());

        WriteRemainingItemsFiles();
        WriteAlphabetizedItemsFile();
        WriteStoreNamesFile();

        foreach (StoreAudio storeAudio in storeNamesToItems)
        {
            unused_store_names.Add(storeAudio.storeName);
        }
    }

    public string PopStoreName()
    {
        if (unused_store_names.Count < 1)
        {
            throw new UnityException("I ran out of store names!");
        }
        unused_store_names.Shuffle(random);
        string storeName = unused_store_names[0];
        unused_store_names.RemoveAt(0);
        return storeName;
    }

    public AudioClip PopItem(string storeName)
    {
        //get the item
        string remainingItemsPath = RemainingItemsPath(storeName);
        string[] remainingItems = System.IO.File.ReadAllLines(remainingItemsPath);
        int randomItemIndex = UnityEngine.Random.Range(0, remainingItems.Length);
        string randomItemName = remainingItems[randomItemIndex];
        AudioClip randomItem = null;
        foreach (StoreAudio storeAudio in storeNamesToItems)
        {
            AudioClip[] languageAudio;
            if (LanguageSource.current_language.Equals(LanguageSource.LANGUAGE.ENGLISH))
            {
                languageAudio = storeAudio.englishAudio;
            }
            else
            {
                languageAudio = storeAudio.germanAudio;
            }
            foreach (AudioClip clip in languageAudio)
            {
                if (clip.name.Equals(randomItemName))
                {
                    randomItem = clip;
                }
            }
        }
        if (randomItem == null)
            throw new UnityException("Possible language mismatch. I couldn't find an item for: " + storeName);

        //delete it from remaining items
        string[] remainingItemsMinusRandomItem = new string[remainingItems.Length - 1];
        for (int i = 0; i < remainingItems.Length; i++)
        {
            if (i < randomItemIndex)
                remainingItemsMinusRandomItem[i] = remainingItems[i];
            if (i > randomItemIndex)
                remainingItemsMinusRandomItem[i - 1] = remainingItems[i];
        }
        System.IO.File.WriteAllLines(remainingItemsPath, remainingItemsMinusRandomItem);

        Debug.Log("Items remaining: " + remainingItemsMinusRandomItem.Length.ToString());

        //return the item
        return randomItem;
    }

    public static bool ItemsExhausted()
    {
        bool itemsExhausted = false;
        string remainingItemsDirectory = RemainingItemsPath("");
        if (!System.IO.Directory.Exists(remainingItemsDirectory))
            return false;
        string[] remainingItemsPaths = System.IO.Directory.GetFiles(remainingItemsDirectory);
        foreach (string remainingItemsPath in remainingItemsPaths)
        {
            string[] itemsRemaining = System.IO.File.ReadAllLines(remainingItemsPath);
            bool storeExhausted = itemsRemaining.Length == 0;
            if (storeExhausted)
                itemsExhausted = true;
        }
        return itemsExhausted;
    }
}