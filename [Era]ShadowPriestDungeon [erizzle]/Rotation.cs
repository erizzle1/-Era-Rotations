using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.WowBots;
using wShadow.WowBots.PartyInfo;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class EraShadowPriestDungeon : Rotation
{
    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };
    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
    }
    private bool HasItem(object item) => Api.Inventory.HasItem(item);

    private DateTime lastDebugTime = DateTime.MinValue;
    private int debugInterval = 5; // Set the debug interval in seconds

    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }

    private double GetDistance(WowUnit unit1, WowUnit unit2)
    {
        var deltaX = unit1.Position.X - unit2.Position.X;
        var deltaY = unit1.Position.Y - unit2.Position.Y;
        var deltaZ = unit1.Position.Z - unit2.Position.Z;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }


    public override void Initialize()
    {
         if (!PartyBot.IsEnabled())
        {
            PartyBot.Enable();
        }
        // Can set min/max levels required for this rotation.


        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 750;
        FastTick = 500;

        // You can also use this method to add to various action lists.

        // This will add an action to the internal passive tick.
        // bool: needTarget -> If true action will not fire if player does not have a target
        // Func<bool>: function -> Action to attempt, must return true or false.
        PassiveActions.Add((true, () => false));

        // This will add an action to the internal combat tick.
        // bool: needTarget -> If true action will not fire if player does not have a target
        // Func<bool>: function -> Action to attempt, must return true or false.
        CombatActions.Add((true, () => false));

    }

    public override bool PassivePulse()
    {
        var me = Api.Player;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targethealth = target.HealthPercent;
      

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources

        // Target distance from the player

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.Auras.Contains("Drink") || me.Auras.Contains("Food"))
        {
            return false;
        }

         // Buff party members
    var members = PartyBot.GetMemberUnits();
    if (members.Length > 5)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("NO MORE THAN 5 PARTY MEMBERS - BOT MIGHT GO CRAZY");
        Console.ResetColor();
        Thread.Sleep(2000); //Prevent  
        return false;
    }

    for (int i = 0; i < members.Length; i++)
    {
        var member = members[i];
        // Check if the member is within 40 yards before attempting to cast
        if (GetDistance(me, member) <= 30 && me.ManaPercent >= 60)
        {
            // Check if the member already has the auras before attempting to cast
            if (Api.Spellbook.CanCast("Power Word: Fortitude") && !member.Auras.Contains("Power Word: Fortitude", false))
            {
                TargetAndBuffMember((PartyMember)i, member.Name, "Power Word: Fortitude");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }

            if (Api.Spellbook.CanCast("Fear Ward") && !member.Auras.Contains("Fear Ward", false))
            {
                TargetAndBuffMember((PartyMember)i, member.Name, "Fear Ward");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }

            if (Api.Spellbook.CanCast("Shadow Protection") && !member.Auras.Contains("Shadow Protection", false))
            {
                TargetAndBuffMember((PartyMember)i, member.Name, "Shadow Protection");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }

            if (Api.Spellbook.CanCast("Heal") && member.HealthPercent < 90)
            {
                TargetAndBuffMember((PartyMember)i, member.Name, "Heal");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }
        }
    }
        
        var reaction = me.GetReaction(target);
        
        if (Api.Spellbook.CanCast("Renew") && target.IsValid() && !me.Auras.Contains("Renew",false) && healthPercentage < 80 &&
        (reaction == UnitReaction.Friendly || reaction == UnitReaction.Honored || reaction == UnitReaction.Revered || reaction == UnitReaction.Exalted))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Renew");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Renew"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Power Word: Fortitude") && !me.Auras.Contains("Power Word: Fortitude",false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Fortitude");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Fortitude"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Shadow Protection") && !me.Auras.Contains("Shadow Protection", false) && !target.Auras.Contains("Shadow Protection",false)) 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Shadow Protection"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Inner Fire") && !me.Auras.Contains("Inner Fire",false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Inner Fire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Inner Fire"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield",false) && !me.Auras.Contains("Weakened Soul", false) && me.InCombat() && me.ManaPercent > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Shield"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Shadowform") && !me.Auras.Contains("Shadowform", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadowform");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Shadowform"))
            {
                return true;
            }
        }
        
        if (target.IsValid())
        {
            if (!target.IsDead())
            {
                if (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted)
                {
                    if (mana >= 5)
                    {
                        if (!IsNPC(target))
                        {
                            if (Api.Spellbook.CanCast("Vampiric Embrace"))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Casting Mind Blast or Vampiric Embrace as STARTER");
                                Console.ResetColor();
                                if (Api.Spellbook.Cast("Vampiric Embrace"))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Vampiric Embrace is not ready to be cast. Casting MindBlast");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Target is an NPC.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Mana is not above 20%.");
                    }
                }
                else
                {
                    Console.WriteLine("Target is friendly, honored, revered, or exalted.");
                }
            }
            else
            {
                Console.WriteLine("Target is dead.");
            }
        }

        return base.PassivePulse();
    }

    private void TargetAndBuffMember(PartyMember memberIndex, string memberName, string spellName)
    {
        // Target the party member using the PartyBot method
        PartyBot.TargetMember(memberIndex);
        Thread.Sleep(1000); // Wait for the target to switch

        // Verify the target has switched
        if (Api.Target != null && Api.Target.Name == memberName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Casting {spellName} on {memberName}!!");
            Console.ResetColor();
            Api.Spellbook.Cast(spellName);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"**Failed to target {memberName}!!");
            Console.ResetColor();
        }
    }

    public override bool CombatPulse()
    {
        var me = Api.Player;
        var target = Api.Target;
        var mana = me.ManaPercent;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;
        if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };


        if (me.HealthPercent <= 70 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
        {
            foreach (string hpot in HP)
            {
                if (HasItem(hpot))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Using Healing potion");
                    Console.ResetColor();
                    if (Api.Inventory.Use(hpot))
                    {
                        return true;
                    }
                }
            }
        }

        if (me.ManaPercent <= 50 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
        {
            foreach (string manapot in MP)
            {
                if (HasItem(manapot))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Using mana potion");
                    Console.ResetColor();
                    if (Api.Inventory.Use(manapot))
                    {
                        return true;
                    }
                }
            }
        }

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);
        if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield",true) && mana > 15 && !me.Auras.Contains("Weakened Soul",true))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Shield"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Silence") && !Api.Spellbook.OnCooldown("Silence") && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Silence");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Silence"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Shadow Word: Pain") && !target.Auras.Contains("Shadow Word: Pain",false) && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Word: Pain");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Shadow Word: Pain"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Vampiric Embrace") && !target.Auras.Contains("Vampiric Embrace",false) && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Vampiric Embrace");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Vampiric Embrace"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Mind Blast") && targethealth >= 30 && mana > 10 && !Api.Spellbook.OnCooldown("Mind Blast"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mind Blast");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mind Blast"))
            {
                return true;
            }
        }


        if (Api.Equipment.HasItem(EquipmentSlot.Extra) && Api.HasMacro("Shoot") && !me.IsShooting())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ranged weapon is equipped. Attempting to cast Shoot.");
            Console.ResetColor();

            if (Api.UseMacro("Shoot"))
            {
                return true;
            }
        }

        return base.CombatPulse();
    }


    private bool IsNPC(WowUnit unit)
    {
        if (!IsValid(unit))
        {
            // If the unit is not valid, consider it not an NPC
            return false;
        }

        foreach (var condition in npcConditions)
        {
            switch (condition)
            {
                case "Innkeeper" when unit.IsInnkeeper():
                case "Auctioneer" when unit.IsAuctioneer():
                case "Banker" when unit.IsBanker():
                case "FlightMaster" when unit.IsFlightMaster():
                case "GuildBanker" when unit.IsGuildBanker():
                case "StableMaster" when unit.IsStableMaster():
                case "Trainer" when unit.IsTrainer():
                case "Vendor" when unit.IsVendor():
                case "QuestGiver" when unit.IsQuestGiver():
                    return true;
            }
        }

        return false;
    }
    private void LogPlayerStats()
    {
        var me = Api.Player;

        var mana = me.ManaPercent;
        var healthPercentage = me.HealthPercent;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();
        if (Api.HasMacro("Shoot"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Wand macro 'Shoot' is present.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("INGAME ..... Create Macro ");
            Console.WriteLine("Macro name : Shoot");
            Console.WriteLine("Macro code : /cast !Shoot");

            Console.WriteLine("Save macro, exit options and when ingame RELOAD UI");
            Console.ResetColor();
        }
    }




}