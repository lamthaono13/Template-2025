using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataService : IDataService
{
    public DataService()
    {

    }

    public string LoadData(string key)
    {
        throw new System.NotImplementedException();
    }

    public void SaveData(string key, string value)
    {
        throw new System.NotImplementedException();
    }
}

public interface IDataService
{
    public void SaveData(string key, string value);
    public string LoadData(string key);
}
