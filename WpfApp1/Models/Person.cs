using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

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

    public static void Save()
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

    public static Person GetPerson(int id)
    {
        if (Tree.ContainsKey(id))
            return Tree[id];
        return null;
    }
    public static void combineChildren(int dad, int mom)
    {
        var combinedChildren = Tree[dad].Children
        .Union(Tree[mom].Children)
        .Distinct()
        .ToList();
        Tree[dad].Children = combinedChildren;
        Tree[mom].Children = combinedChildren;
    }
    public static int addFather(int id, int dad)
    {
        Tree[id].Dad = dad;
        if (!Tree[dad].Children.Contains(id)) Tree[dad].Children.Add(id);
        if (Tree[dad].Spouse.HasValue)
        {
            Tree[id].Mom = Tree[dad].Spouse;
            combineChildren(dad, Tree[dad].Spouse.Value);
        }
        else if (Tree[id].Mom.HasValue)
        {
            Tree[dad].Spouse = Tree[id].Mom;
            Tree[Tree[id].Mom.Value].Spouse = dad;
            combineChildren(dad, Tree[id].Mom.Value);
        }
        Save();
        return 0;
    }
    public static int addMother(int id, int mom)
    {
        Tree[id].Mom = mom;
        if (!Tree[mom].Children.Contains(id)) Tree[mom].Children.Add(id);
        if (Tree[mom].Spouse.HasValue)
        {
            Tree[id].Dad = Tree[mom].Spouse;
            combineChildren(mom, Tree[mom].Spouse.Value);
        }
        else if (Tree[id].Dad.HasValue)
        {
            Tree[mom].Spouse = Tree[id].Dad;
            Tree[Tree[id].Dad.Value].Spouse = mom;
            combineChildren(mom, Tree[id].Dad.Value);
        }
        Save();
        return 0;
    }
    public static int addSpouse(int id, int spouse)
    {
        Tree[id].Spouse = spouse;
        Tree[spouse].Spouse = id;
        combineChildren(id, spouse);
        foreach (int childId in Tree[id].Children)
        {
            if (Tree[id].Gender)
            {
                Tree[childId].Dad = id;
                Tree[childId].Mom = spouse;
            }
            else
            {
                Tree[childId].Dad = spouse;
                Tree[childId].Mom = id;
            }
        }
        Save();
        return 0;
    }
    public static int addChildren(int id, int child)
    {
        if (!Tree[id].Children.Contains(child))
        {
            Tree[id].Children.Add(child);
        }
        if (Tree[id].Gender)
        {
            Tree[child].Dad = id;
            if (Tree[id].Spouse.HasValue) Tree[child].Mom = Tree[id].Spouse.Value;
        }
        else
        {
            Tree[child].Mom = id;
            if (Tree[id].Spouse.HasValue) Tree[child].Dad = Tree[id].Spouse.Value;
        }
        Save();
        return 0;
    }
    public static void updatePfp(int id, byte[]? image)
    {
        Tree[id].Pfp = image;
        Save();
    }
}