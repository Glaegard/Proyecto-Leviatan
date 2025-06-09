using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckSaveData
{
    public string deckName;
    public List<string> cards; // Lista de cardName_ES o EN
}

[System.Serializable]
public class DeckCollection
{
    public List<DeckSaveData> decks = new List<DeckSaveData>();
}

public static class DeckStorage
{
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "decks.json");

    public static void SaveAllDecks(DeckCollection all)
    {
        try
        {
            var json = JsonUtility.ToJson(all, true);
            File.WriteAllText(FilePath, json);
            Debug.Log($"Mazos guardados en {FilePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Error guardando mazos: {e.Message}");
        }
    }

    public static DeckCollection LoadAllDecks()
    {
        if (!File.Exists(FilePath)) return new DeckCollection();
        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<DeckCollection>(json);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error cargando mazos: {e.Message}");
            return new DeckCollection();
        }
    }
}
