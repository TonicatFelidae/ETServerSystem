using System;
using System.Collections.Generic;

namespace ETServerSystem.Sample
{
    [Serializable]
    public class SampleData
    {
        public string playerId;
        public int level;
        public int exp;
        public int coins;
        public List<string> unlockedItems;
        public Dictionary<string, int> stats;

        public SampleData()
        {
            playerId = string.Empty;
            level = 1;
            exp = 0;
            coins = 100;
            unlockedItems = new List<string>();
            stats = new Dictionary<string, int>();
        }
    }
}
