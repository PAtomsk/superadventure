using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Monster : LivingCreature
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaxDMG { get; set; }
        public int RewardExp { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }

        public Monster(int currentHP, int maxHP, int id, string name, int maxDMG, int rewardExp, int rewardGold):base(currentHP,maxHP)
        {
            ID = id;
            Name = name;
            MaxDMG = maxDMG;
            RewardExp = rewardExp;
            RewardGold = rewardGold;
            LootTable = new List<LootItem>();
        }

    }
}
