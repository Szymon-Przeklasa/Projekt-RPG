using System;

public class UpgradeData
{
    public string Name;
    public Action Apply;

    public UpgradeData(string name, Action apply)
    {
        Name = name;
        Apply = apply;
    }
}
