using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Person
{
    public string Name { get; set; }
    public bool Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public int Generation { get; set; }
    public int? Dad { get; set; }
    public int? Mom { get; set; }
    public int? Spouse { get; set; }
    public List<int> Children { get; set; } = new List<int>();
    public byte[]? Pfp { get; set; }
    public Person() { }
    public Person(string name, bool gender, DateTime date, int generation = 0, int? dad = null, int? mom = null, int? spouse = null, byte[]? imageData = null)
    {
        Name = name;
        Gender = gender;
        BirthDate = date;
        Generation = generation;
        Dad = dad;
        Mom = mom;
        Spouse = spouse;
        Pfp = imageData;
    }
}

public class SaveData
{
    public Dictionary<int, Person> Tree { get; set; }
    public int UniqueId { get; set; }
}

static class FamilyTree
{
    private static Dictionary<int, Person> Tree = new();
    private static int UniqueId = 0;

    private static readonly string SavePath = "familytree.json";

    public static void Load()
    {
        if (!File.Exists(SavePath))
            return;

        string json = File.ReadAllText(SavePath);

        var data = JsonSerializer.Deserialize<SaveData>(json);

        if (data != null)
        {
            Tree = data.Tree ?? new();
            UniqueId = data.UniqueId;
        }
    }

    private static void Save()
    {
        var data = new SaveData
        {
            Tree = Tree,
            UniqueId = UniqueId
        };

        File.WriteAllText(SavePath,
            JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    public static int AddPerson(string name, bool gender, DateTime birthDate, int generation, int? dad = null, int? mom = null, int? spouse = null, byte[]? imageData = null)
    {
        int id = UniqueId++;
        var newPerson = new Person(name, gender, birthDate, generation, dad, mom, spouse, imageData);
        Tree[id] = newPerson;
        if (dad.HasValue && Tree.ContainsKey(dad.Value))
            Tree[dad.Value].Children.Add(id);

        if (mom.HasValue && Tree.ContainsKey(mom.Value))
            Tree[mom.Value].Children.Add(id);

        if (spouse.HasValue && Tree.ContainsKey(spouse.Value))
            Tree[spouse.Value].Spouse = id;
        Save();
        return id;
    }

    public static void RemovePerson(int id)
    {
        if (Tree.ContainsKey(id))
        {
            if (Tree[id].Dad != null) Tree[Tree[id].Dad.Value].Children.Remove(id);
            if (Tree[id].Mom != null) Tree[Tree[id].Mom.Value].Children.Remove(id);
            if (Tree[id].Spouse != null) Tree[Tree[id].Spouse.Value].Spouse = null;
            foreach (int childID in Tree[id].Children)
            {
                if (Tree[childID].Dad == id) Tree[childID].Dad = null;
                else if (Tree[childID].Mom == id) Tree[childID].Mom = null;
            }
            Tree.Remove(id);
            Save();
        }
    }

    public static List<(int id, Person person)> SearchByName(string query)
    {
        query = query.ToLower();

        var results = new List<(int id, Person person)>();

        foreach (var kvp in Tree)
        {
            if (kvp.Value.Name.ToLower().Contains(query))
            {
                results.Add((kvp.Key, kvp.Value));
            }
        }

        return results;
    }
}