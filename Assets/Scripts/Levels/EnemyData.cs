using System.Collections.Generic;

[System.Serializable]
public class EnemyData
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

[System.Serializable]
public class SpawnData
{
    public string enemy;
    public string count = "1";
    public List<int> sequence = new List<int> { 1 };
    public string delay = "2";
    public string location = "random";
    public string hp = "base";
    public string speed = "base";
    public string damage = "base";
}

[System.Serializable]
public class LevelData
{
    public string name;
    public int waves = -1; // -1 for endless mode
    public List<SpawnData> spawns = new List<SpawnData>();
} 