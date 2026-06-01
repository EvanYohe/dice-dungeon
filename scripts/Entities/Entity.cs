using System;
using System.Collections.Generic;
using DiceDungeon.scripts.Combat;
using DiceDungeon.scripts.Entities.Abilities;

namespace DiceDungeon.scripts.Entities;

public interface I_Entity {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Initiative { get; set; }
    public int HealthCurrent { get; set; }
    public int HealthMaximum { get; set; }
    public ResourceType ResourceType { get; set; }
    public int ResourceCurrent { get; set; }
    public int ResourceMaximum { get; set; }
    public int RerollsCurrent { get; set; }
    public int RerollsMaximum { get; set; }
    public int Currency { get; set; }

    public CreatureType CreatureType { get; set; }
    public Character Character { get; set; }

    public List<Ability> Abilities { get; set; }

    public Dictionary<DamageType, bool> Immunities { get; set; }
    public Dictionary<DamageType, int> Resistances { get; set; }

    public Dictionary<DamageType, int> Vulnerabilities { get; set; }

    public Dictionary<DamageType, int> DamageBonusesByDamageType { get; set; }

    public Dictionary<DamageType, int>
        DamageReductionsByDamageType { get; set; }

    public Dictionary<DiceType, int> DamageBonusesByDiceType { get; set; }

    public Dictionary<DiceType, int> DamageReductionsByDiceType { get; set; }

    public bool IsAlive { get; set; }
    public bool IsHostileToPlayer { get; set; }
    public bool IsHostileToEnemies { get; set; }
    public bool IsVendor { get; set; }
    public bool IsVisible { get; set; }
    public bool IsInvincible { get; set; }
    public bool IsPlayer { get; set; }
    public bool IsAffectedByConditions { get; set; }
}