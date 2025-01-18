using System;
using System.Threading;
using System.Collections.Generic;
using wShadow.Templates;
using wShadow.WowBots;
using wShadow.WowBots.PartyInfo;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class EraShadowPriest : Rotation
{
    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };

    private DateTime lastDebugTime = DateTime.MinValue;
    private int debugInterval = 5; // Set the debug interval in seconds

    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
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
        LogPlayerStats();
        SlowTick = 750;
        FastTick = 500;
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

    // Check if the player is in a state where actions cannot be performed
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

            if (Api.Spellbook.CanCast("Divine Spirit") && !member.Auras.Contains("Divine Spirit", false))
            {
                TargetAndBuffMember((PartyMember)i, member.Name, "Divine Spirit");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }
        }
    }

    // Self buffs
    if (Api.Spellbook.CanCast("Renew") && !me.Auras.Contains("Renew", true) && healthPercentage < 85)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Casting Renew on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Renew"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Power Word: Fortitude") && !me.Auras.Contains("Power Word: Fortitude", false) && mana > 50)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Power Word: Fortitude on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Power Word: Fortitude"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Shadow Protection") && !me.Auras.Contains("Shadow Protection", false) && mana > 50)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Shadow Protection on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Shadow Protection"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }


    if (Api.Spellbook.CanCast("Fear Ward") && !me.Auras.Contains("Fear Ward", false) && mana > 50)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting FEAR WARD on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Fear Ward"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }
    if (Api.Spellbook.CanCast("Inner Fire") && !me.Auras.Contains("Inner Fire", false) & mana > 55)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Inner Fire on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Inner Fire"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

        if (Api.Spellbook.CanCast("Divine Spirit") && !me.Auras.Contains("Divine Spirit", false) & mana > 55)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting DIVINE SPIRIT on self!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Divine Spirit"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield", false) && me.InCombat())
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Passive Pulse - Casting Power Word: Shield!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Power Word: Shield"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    if (!me.Auras.Contains("Shadowform", false) && Api.Spellbook.CanCast("Shadowform", true))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Shadowform!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Shadowform"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    var reaction = me.GetReaction(target);
    if (target.IsValid())
    {
        if (!target.IsDead())
        {
            if (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted)
            {
                if (mana >=90)
                {
                    if (!IsNPC(target))
                    {
                        if (Api.Spellbook.CanCast("Mind Blast"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Casting Mind Blast as Starterrrr");
                            Console.ResetColor();
                            if (Api.Spellbook.Cast("Mind Blast"))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            Api.UseMacro("Shoot");
                            Console.WriteLine("Mindblast on GetReation is not ready to be cast. Casting Shoot!");
                            Thread.Sleep(500); // Add a 500ms pause after a successful cast
                        }
                    }
                    else
                    {
                        Console.WriteLine("Target is an NPC.");
                        Thread.Sleep(500); // Add a 500ms pause after a successful cast
                    }
                }
                else
                {
                    Api.Spellbook.Cast("Mind Blast");
                    Console.WriteLine("Mana is not above 60%.....");
                    Thread.Sleep(500); // Add a 500ms pause after a successful cast
                }
            }
            else
            {
                Console.WriteLine("Target is friendly, honored, revered, or exalted.");
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
            }
        }
        else
        {
            Console.WriteLine("Target is dead.");
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
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

    if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
    {
        LogPlayerStats();
        lastDebugTime = DateTime.Now;
    }

    if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

    string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
    string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

    if (me.HealthPercent <= 25 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
    {
        foreach (string hpot in HP)
        {
            if (HasItem(hpot))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Using Healing potion!!!");
                Console.ResetColor();
                if (Api.Inventory.Use(hpot))
                {
                    return true;
                }
            }
        }
    }

    if (me.ManaPercent <= 05 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
    {
        foreach (string manapot in MP)
        {
            if (HasItem(manapot))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Using mana potion!!!");
                Console.ResetColor();
                if (Api.Inventory.Use(manapot))
                {
                    return true;
                }
                
            }
        }
    }

    var reaction = me.GetReaction(target);
    if (Api.Spellbook.CanCast("Renew") && target.IsValid() && me.ManaPercent > 30 && target.HealthPercent <90 && !target.Auras.Contains("Renew", false) &&
        (reaction == UnitReaction.Friendly || reaction == UnitReaction.Honored || reaction == UnitReaction.Revered || reaction == UnitReaction.Exalted))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Renew on target");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Renew"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Power Word: Shield") && me.HealthPercent <99 && me.ManaPercent >= 20 && !me.Auras.Contains("Power Word Shield", false) && !me.Auras.Contains("Weakened Soul", false))
    {
        Api.UseMacro("TargetSelf");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Power Word: Shield on ME!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Power Word: Shield"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }
if (Api.Spellbook.CanCast("Renew") && !me.Auras.Contains("Renew", false) && me.HealthPercent < 85 && me.ManaPercent > 20)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Attempting to cast Renew on self!");
    Console.ResetColor();

    // Use the macro to target self
    if (Api.UseMacro("TargetSelf"))
    {
        Thread.Sleep(100); // Short pause to ensure the macro execution

        // Verify the target has switched to self
        if (Api.Target == me)
        {
            if (Api.Spellbook.Cast("Renew"))
            {
                Thread.Sleep(500); // Add a 500ms pause after a successful cast
                return true;
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to verify target switch to self");
            Console.ResetColor();
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Failed to target self using macro");
        Console.ResetColor();
    }
}

    
    if (Api.Spellbook.CanCast("Power Word: Shield") && me.HealthPercent <90 && me.Auras.Contains("Power Word: Shield", false) && me.ManaPercent > 15 && !me.Auras.Contains("Weakened Soul", false))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Power Word: Shield on Target!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Power Word: Shield"))
        {
            Thread.Sleep(250); // Add a (ms) pause after a successful cast
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Lesser Heal") && me.HealthPercent < 90 && me.HealthPercent > 80 && me.ManaPercent > 15)
    {
    
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Casting Lesser Heal on Target!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Lesser Heal"))
        {
            Thread.Sleep(250); // Add a (ms) pause after a successful cast
            return base.CombatPulse();
        }
    }

        if (Api.Spellbook.CanCast("Flash Heal") && me.HealthPercent < 80 && me.HealthPercent > 10 )
    {
    
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Casting Flash Heal on Target!");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Flash Heal"))
        {
            Thread.Sleep(250); // Add a 500ms pause after a successful cast
            return base.CombatPulse();
        }
    }

    if (Api.Spellbook.CanCast("Silence") && target.IsValid() && !Api.Spellbook.OnCooldown("Silence") && (target.IsCasting() || target.IsChanneling()))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Silence");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Silence"))
        {
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Shadow Word: Pain") && target.IsValid() && !target.Auras.Contains("Shadow Word: Pain", false) && target.HealthPercent >= 1 && me.ManaPercent > 10)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Shadow Word: Pain");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Shadow Word: Pain"))
        {
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Vampiric Embrace") && target.IsValid() && !target.Auras.Contains("Vampiric Embrace", true) && target.HealthPercent >= 30 && me.ManaPercent > 90)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Vampiric Embrace");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Vampiric Embrace"))
        {
            return true;
        }
    }

    if (Api.Spellbook.CanCast("Mind Blast") && target.IsValid() && target.HealthPercent >= 30 && me.ManaPercent > 35 && !Api.Spellbook.OnCooldown("Mind Blast"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Mind Blast");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Mind Blast"))
        {
            Thread.Sleep(500); // Add a 500ms pause after a successful cast
            return true;
        }
    }

    //if (Api.Spellbook.CanCast("Smite") && me.ManaPercent > 95 && !me.Auras.Contains("Shadowform", true))
    //{
    //    Console.ForegroundColor = ConsoleColor.Green;
    //    Console.WriteLine("Casting Smite");
    //    Console.ResetColor();
    //    if (Api.Spellbook.Cast("Smite"))
    //    {
    //        return true;
    //    }
    //}

    // Directly check distance 
    if (target.Position.Distance2D(me.Position) < 10) 
    // Example distance threshold 
    { 
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Target is within 10 units distance. Continuing to cast shoot...");
        Console.ResetColor();
        Thread.Sleep(800); // Add a 500ms pause after a successful cast 
    } 
    else 
    { 
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Target is farther than 10 units distance. Continuing to cast shoot....");
        Console.ResetColor(); 
        Thread.Sleep(800); // Add a 500ms pause after a successful cast 
    }

    if (Api.Equipment.HasItem(EquipmentSlot.Extra) && Api.HasMacro("Shoot") && !me.IsShooting())
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("End CombatPulse - Attempting to cast Shoot.");
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
