using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Engine;


namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10,10,20,0,1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            lblHP.Text = _player.CurrentHP.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExp.Text = _player.Exp.ToString();
            lblLvl.Text = _player.Lvl.ToString();

        }
        
        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any item requirements to enter
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                return;
            }

            //update players location
            _player.CurrentLocation = newLocation;

            //show/hide available movement buttons;
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //completely heal the player
            _player.CurrentHP = _player.MaxHP;

            //update hp in ui
            lblHP.Text = _player.CurrentHP.ToString();

            //does the location have a quest?
            if(newLocation.QuestAvailableHere != null)
            {
                // See if the player already has the quest, and if they've completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                //see if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    //if the player has not completed quest yet
                    if(!playerAlreadyCompletedQuest)
                    {
                        // See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // the player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            //Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExp.ToString() + " experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExp.ToString() + " gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.Exp += newLocation.QuestAvailableHere.RewardExp;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                // what will happen if player doesnt already have a quest
                else
                {
                    //display the messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach(QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if(qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    //add the quest to the players quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }
            //does the location have a monster?
            if(newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                // make a new monster, using the values from the standard monster in the world.monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaxDMG, standardMonster.RewardExp, standardMonster.RewardGold, standardMonster.CurrentHP, 
                    standardMonster.MaxHP);

                foreach(LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            // Refresh player's inventory list
            UpdateInventoryListInUI();
            // Refresh player's quest list
            UpdateQuestListInUI();
            // Refresh player's weapons combobox
            UpdateWeaponListInUI();
            // Refresh player's potions combobox
            UpdatePotionListInUI();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";
            dgvInventory.Rows.Clear();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] {inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";
            dgvQuests.Rows.Clear();
            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] {playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }
            if (weapons.Count == 0)
            {
                // The player doesn't have any weapons, so hide the weapon combobox and "Use" button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";
                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add(
                        (HealingPotion)inventoryItem.Details);
                    }
                }
            }
            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
            }
        }

        private void RefreshUI()
        {
            lblHP.Text = _player.CurrentHP.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExp.Text = _player.Exp.ToString();
            lblLvl.Text = _player.Lvl.ToString();

            UpdateInventoryListInUI();
            UpdatePotionListInUI();
            UpdateQuestListInUI();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // get the currently selected weapon from the cboWeapons combobox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            PlayerAttack(currentWeapon);

            // check if the monster is dead
            if(_currentMonster.CurrentHP <= 0)
            {
                //monster is dead
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                GiveRewards();

                //refresh players information and inventory controls
                RefreshUI();

                //add a blank line to the messages box, just for DEM STYLE POINTS
                rtbMessages.Text += Environment.NewLine;

                //move player to current location to heal player and respawn the baddie
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //monster not dead. panik.
                //how hard did he spank you, daddy?
                MonsterAttack();
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            DrinkPotion(potion);

            // monster gets its turn to attack
            MonsterAttack();

            //refresh players information and inventory controls
            RefreshUI();
        }

        private void PlayerAttack(Weapon currentWeapon)
        {
            // determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinDMG, currentWeapon.MaxDMG);

            //apply the damage to the monsters current hp
            _currentMonster.CurrentHP -= damageToMonster;

            //display message
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;
        }

        private void GiveRewards()
        {
            //gib xp
            _player.Exp += _currentMonster.RewardExp;
            rtbMessages.Text += "You receive " + _currentMonster.RewardExp + " experience points!" + Environment.NewLine;

            //gib gold
            _player.Gold += _currentMonster.RewardGold;
            rtbMessages.Text += "You receive " + _currentMonster.RewardGold + " gold!" + Environment.NewLine;

            //gib loot
            List<InventoryItem> lootedItems = new List<InventoryItem>();

            //add items to the lootedItems list, comparing a random number to the drop percentage
            foreach (LootItem lootItem in _currentMonster.LootTable)
            {
                if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                {
                    lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                }
            }

            //if no items were randomly selected, then add the default loot items
            if (lootedItems.Count == 0)
            {
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (lootItem.IsDefaultItem)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }
            }

            // add booty to the player inventory!
            foreach (InventoryItem inventoryItem in lootedItems)
            {
                _player.AddItemToInventory(inventoryItem.Details);

                if (inventoryItem.Quantity == 1)
                {
                    rtbMessages.Text += "You loot " + inventoryItem.Details.Name + Environment.NewLine;
                }
                else
                {
                    rtbMessages.Text += "You loot " + inventoryItem.Details.NamePlural + Environment.NewLine;
                }
            }
        }

        private void MonsterAttack()
        {
            int damagetoPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaxDMG);

            //display message
            rtbMessages.Text += "You received " + damagetoPlayer.ToString() + " damage from " + _currentMonster.Name + Environment.NewLine;

            //subtract damage from players hp
            _player.CurrentHP -= damagetoPlayer;

            //refresh players hp in ui
            lblHP.Text = _player.CurrentHP.ToString();

            if (_player.CurrentHP <= 0)
            {
                //display message
                rtbMessages.Text += "You died." + Environment.NewLine;

                //move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }
        }

        private void DrinkPotion(HealingPotion potion)
        {
            // add healing amount to the current player hp
            _player.CurrentHP += potion.AmountToHeal;

            // current hp cant exceed max hp
            if (_player.CurrentHP > _player.MaxHP)
            {
                _player.CurrentHP = _player.MaxHP;
            }

            //remove pot from the inv
            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }
            //display message
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;
        }
  
    }
}
