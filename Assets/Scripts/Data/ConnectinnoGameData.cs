using System;
using System.Collections.Generic;

[Serializable]
public class ConnectinnoGameData
{
    public int level = 1;
    public int coinAmount;
    public List<int> openableChests = new List<int>();
    public float MusicLevel = 0;
    public float SoundLevel= 0;
    public bool HapticIsOn = true;
}
