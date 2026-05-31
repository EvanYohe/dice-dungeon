using System;
using System.Collections.Generic;
using DiceDungeon.scripts.Combat;
using DiceDungeon.scripts.Entities.Abilities;

namespace DiceDungeon.scripts.Entities;

public interface I_Entity {
    
    public Guid id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int initiative { get; set; }
    public int healthCurrent { get; set; }
    public int healthMaximum { get; set; }
    public ResourceType resourceType { get; set; }
    public int resourceCurrent { get; set; }
    public int resourceMaximum { get; set; }
    public int rerollsCurrent { get; set; }
    public int rerollsMaximum { get; set; }
    public int currency { get; set; }
    
    public CreatureType creatureType { get; set; }
    public Character character { get; set; }
    
    public List<Ability> abilities { get; set; }
    
    public Dictionary<DamageType, bool> immunities { get; set; }
    public Dictionary<DamageType, int> resistances { get; set; }
    public Dictionary<DamageType, int> vulnerabilities { get; set; }
    
    public Dictionary<DamageType, int> damageBonusesByDamageType { get; set; }
    public Dictionary<DamageType, int> damageReductionsByDamageType { get; set; }
    public Dictionary<DiceType, int> damageBonusesByDiceType{ get; set; }
    public Dictionary<DiceType, int> damageReductionsByDiceType { get; set; }
    
    public bool isAlive { get; set; }
    public bool isHostileToPlayer { get; set; }
    public bool isHostileToEnemies { get; set; }
    public bool isVendor { get; set; }
    public bool isVisible { get; set; }
    public bool isInvincible { get; set; }
    public bool isPlayer { get; set; }
    public bool isAffectedByConditions { get; set; }
    
}