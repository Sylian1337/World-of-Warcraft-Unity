using Mirror;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum Stats
{
    Strength = 2,
    Stamina = 100,
    Intellect = 50,
    Agility = 2
}

public class Unit : MonoBehaviour
{
    // Reference to the NetworkIdentity component
    public NetworkIdentity Identity { get; private set; }
    public LocationHandler location {  get; private set; }

    private List<Spell> spellList = new List<Spell>();
    //private List<Aura> auraList = new List<Aura>();
    private List<Unit> combatList = new List<Unit>();

    // Data
    public string m_name = "Voreli";
    public float m_health = 1000;
    public float m_maxHealth = 1000;
    public float m_mana = 100;
    public float m_maxMana = 100;
    private float m_manaTickTimer = 1;
    private float m_manaTickAmount = 7;
    private float m_manaTickAmountCombat = 3;
    private bool m_isAlive = true;
    private float m_combatTimer = 0;

    public Player player { get; private set; }
    public bool IsCasting { get; protected set; }

    public Unit m_target { get; protected set; }

    // Initialize the NetworkIdentity reference
    void Start()
    {
        // Get the NetworkIdentity component attached to the same GameObject
        Identity = GetComponent<NetworkIdentity>();

        if (Identity == null)
        {
            print("NetworkIdentity component is missing on the Unit's GameObject.");
        }

       
    }

    public float GetMana()
    {
        return m_mana;
    }

    public void SetMana(float val)
    {
        m_mana = val;
    }

    public float GetMaxMana()
    {
        return m_maxMana;
    }

    public bool IsInCombat()
    {
        return m_combatTimer > 0; 
    }

    public void DropCombat()
    {
        m_combatTimer = 0;
    }

    public void SetInCombat()
    {
        m_combatTimer = 6;
    }

    public void SetInCombatWith(Unit unit)
    {
        if (!IsHostileTo(unit))
            print("ERROR: Unit is not an enemy!");

        SetInCombat();
        unit.SetInCombat();

        // We add eachother to our combat lists so both go into combat, not just one unit
        combatList.Add(unit);
        unit.combatList.Add(this);

    }

    public void Update()
    {
        if (GetHealth() <= 0)
        {
            SetHealth(0);
            m_isAlive = false;
            StopCasting();
            //WipeAuras();
            //if (ToCreature())
              //  ToCreature().Died();
        }

        

        //DRHandler().Update();

        //target stealth update
        /*if (HasTarget())
            if (GetTarget().IsInStealth())
                DropTarget();*/

        // Update Schools

        bool preparing = false;
        /*for (int i = 0; i < spellList.Count; i++)
        {
            if (spellList[i].GetSpellState() == SpellManager.SPELL_STATE_PREPARING)
            {
                preparing = true;
                IsCasting = true;
                break;
            }
        }*/

        if (!preparing)
        {
            StopCasting();
            IsCasting = false;
        }

        // Creature Update
        /*if (ToCreature())
        {
            ToCreature().Update();
        }*/

        /* Aura Update
        for (int i = 0; i < auraList.Count; i++)
        {
            auraList[i].Update();
        }*/

        //ToLocation().Update();

        // Mana Tick
        if (GetMana() < GetMaxMana())
        {
            if (m_manaTickTimer > 0)
            {
                m_manaTickTimer -= Time.deltaTime;
            }
            else
            {
                if (IsInCombat())
                {
                    // Check if adding combat mana tick amount exceeds max mana
                    if (GetMana() + m_manaTickAmountCombat < GetMaxMana())
                    {
                        SetMana(GetMana() + m_manaTickAmountCombat);
                    }
                    else
                    {
                        SetMana(GetMaxMana());
                    }
                }
                else
                {
                    // Check if adding normal mana tick amount exceeds max mana
                    if (GetMana() + m_manaTickAmount < GetMaxMana())
                    {
                        SetMana(GetMana() + m_manaTickAmount);
                    }
                    else
                    {
                        SetMana(GetMaxMana());
                    }
                }

                // Reset the mana tick timer
                m_manaTickTimer = 1f;
            }
        }

        if (IsInCombat())
        {
            // If there are entries in the combat list
            if (combatList.Count > 0)
            {
                // Iterate through the combat list
                for (int i = combatList.Count - 1; i >= 0; i--)
                {
                    if (!combatList[i].IsAlive())
                    {
                        combatList.RemoveAt(i);
                    }
                }

                // If the combat list is empty after cleaning up
                if (combatList.Count == 0)
                {
                    DropCombat();
                }
            }

            // Decrement the combat timer
            m_combatTimer -= Time.deltaTime;

            // If the combat timer has run out
            if (m_combatTimer <= 0f)
            {
                DropCombat();
                print("Out of combat!");
            }
        }



    }
    public void AddSpellToList(Spell spell)
    {
        spellList.Add(spell);
    }

    public Spell CreateSpellAndPrepare(int spellId, GameObject spellPrefab)
    {
        SpellInfo info = SpellDataHandler.Instance.Spells.FirstOrDefault(spell => spell.Id == spellId);

        // Instantiate the prefab instead of creating a new GameObject
        GameObject spellObject = Instantiate(spellPrefab);

        // Set the name of the cloned object if needed
        spellObject.name = $"{spellId}";

        // Get the Spell component from the instantiated prefab
        Spell newSpell = spellObject.GetComponent<Spell>();

        DontDestroyOnLoad(spellObject);

        newSpell.Initialize(spellId, this, info);
        //SpellManager.Instance.AddSpell(newSpell);

        newSpell.prepare();

        return newSpell;

        // Network-related: If you need the spell object to be networked
        //NetworkServer.Spawn(spellObject);
    }
    public float GetHealth()
    {
        return m_health;
    }

    public void SetHealth(float val)
    {
        m_health = val; 
    }

    public float GetMaxHealth()
    {
        return m_maxHealth;
    }

    public void RemoveSpellFromList(Spell spell)
    {
        for (int i = 0; i < spellList.Count; i++)
        {
            if (spellList[i] == spell)
            {
                spellList.RemoveAt(i);
                return;
            }
        }
    }

    // Method to set the unit as casting
    public virtual void SetCasting()
    {
        IsCasting = true;
    }

    // Method to stop the unit from casting
    public virtual void StopCasting()
    {
        IsCasting = false;
    }

    // Get the target unit (this can be adapted based on your actual target acquisition logic)
    public Unit GetTarget()
    {
        return m_target;
    }

    public Unit SetTarget(Unit target)
    {
        return m_target = target; 
    }

    public bool HasTarget()
    { return m_target != null; }

    // Check if this unit is hostile to the given target
    public bool IsHostileTo(Unit target)
    {
        return true; // Replace with actual hostility logic
    }

    // Convert this Unit to a Player if applicable
    public Player ToPlayer()
    {
        return player;
    }

    public LocationHandler ToLocation()
    {
        return location;
    }

    // Check if this unit is the local player
    public bool IsLocalPlayer()
    {
        return Identity != null && Identity.isLocalPlayer;
    }

    public bool IsAlive()
    {
        return m_isAlive;
    }

    public virtual Unit ToUnit()
    {
        return this;
    }


    public void DealDamage(Spell spell, Unit target)
    {
        if (spell == null || target == null)
        {
            if (spell == null)
                print("spellNull");
            else if (target == null)
                print("targetNull");

            print("Nope1"); 
            return; 
        }

        SpellInfo m_spellInfo = spell.m_spellInfo; // Assuming spell has a spellInfo property
        if (m_spellInfo == null)
        {
            print("Nope2");
            return;
        }

        SpellSchoolMask spellClass = m_spellInfo.SchoolMask;

        SpellType spellType = m_spellInfo.Type;

        if (spellType != SpellType.Damage)
        {
            print("Nope3");
            return;
        }

        // Calculate the damage and check if it's a critical hit
        bool crit;
        float damage = SpellDamageBonusDone(target, spell, m_spellInfo, out crit);

        bool absorb = false;

        // target.RemoveAurasWithInterruptFlags(SD.SpellAuraInterruptFlags.Damage, spell);

        /*float abamt = target.GetAbsorbAmount();
        if (abamt > 0)
        {
            if (abamt >= damage)
            {
                abamt -= damage;
                damage = 0;
                absorb = true;
            }
            else
            {
                damage -= abamt;
                abamt = 0;
            }
            target.SetAbsorbAmount(abamt);
        }*/

        

        if (!spell.isPositive && damage > 0)
        {
            float newHealth = target.GetHealth() - damage;
            newHealth = Mathf.Round(newHealth);
            target.SetHealth(newHealth);
            print($"Hit {target.name} with {spell.name}!");
            print($"{newHealth} is his new health!");
            return;
        }
        else if (spell.isPositive)
        {
            float newHealth = target.GetHealth() + damage;
            newHealth = Mathf.Round(newHealth);
            target.SetHealth(newHealth);
            print($"Hit {target.name} with {spell.name}!");
            print($"{newHealth} is his new health!");
            return;
        }

        /* TODO: Handle stealth removal
        if (target.IsInStealth())
        {
            target.BreakStealth();
        }*/

        // TODO: Send opcode to client about health loss
    }

    public float SpellDamageBonusDone(Unit victim, Spell spell, SpellInfo m_spellInfo, out bool crit)
    {
        crit = false;
        float doneTotal = 0f;
        // float doneTotalMod = SpellDamagePctDone(spell); // Assuming this method exists
        float doneTotalMod = 1f;                                               
        // float doneAdvertisedBenefit = SpellBaseDamageBonusDone(); // Placeholder for future implementation

        // Check if spell amount is 0
        if (spell.GetAmount() == 0)
            return 0f;

        // TODO: Check for immunities
       // if (victim.m_immunities.ContainsKey(spell.m_school) && victim.m_immunities[spell.m_school])
        //    return 0f;

        // Default damage calculation
        doneTotal = UnityEngine.Random.Range(spell.GetAmount(), spell.GetAmount() * 1.5f);

        // Damage modifiers based on damage class
        string damageClass = m_spellInfo.damageClass;
        if (damageClass == "Intellect")
        {
            Stats intellect = Stats.Intellect;
            doneTotal += UnityEngine.Random.Range(doneTotal + ((int)intellect / 8f), doneTotal + ((int)intellect / 7f));
        }
        else if (damageClass == "Strength")
        {
            Stats strength = Stats.Strength;
            doneTotal += UnityEngine.Random.Range(doneTotal + ((int)strength / 10f), doneTotal + ((int)strength / 9f));
        }

        /* TODO: Check for damage-increasing auras
        foreach (Aura aura in GetAuraList())
        {
            if (aura.spell.spellId == 46) // Rune of Power
            {
                doneTotal *= 1.4f;
            }
        }*/

        /* TODO: Crit Handler
        float critPercentage = spell.GetCritChance();
        if (spell.HasFlag("SPELL_FLAG_ALWAYS_CRIT") ||
            HasAura(24) || // Combustion hard check
            (victim.IsFrozen() && GetClass() == "Mage" && GetSpecialization() == "Frost"))
        {
            doneTotal *= 2f;
            crit = true;
        }
        else
        {
            if (UnityEngine.Random.Range(0, 100) <= critPercentage)
            {
                doneTotal *= 2f;
                crit = true;
            }
        }*/

        /* TODO: We don't want DoTs to trigger crit procs
        bool isDot = false;
        AuraInfo auraInfo = spell.GetAuraInfo();
        if (auraInfo != null && auraInfo.Periodic)
        {
            isDot = true;
        }

        // Handle Crit Procs
        if (crit)
        {
            if (GetClass() == "Mage" && GetSpecialization() == "Fire")
            {
                if (!isDot && spell.GetSchool() == "Fire")
                {
                    const int SPELL_MAGE_HEATING_UP = 21;
                    const int SPELL_MAGE_HOT_STREAK = 22;

                    if (HasAura(SPELL_MAGE_HEATING_UP))
                    {
                        RemoveAura(SPELL_MAGE_HEATING_UP);
                        if (HasAura(SPELL_MAGE_HOT_STREAK))
                        {
                            RemoveAura(SPELL_MAGE_HOT_STREAK);
                        }
                        CastSpell(SPELL_MAGE_HOT_STREAK, this); // Cast Hot Streak
                    }
                    else
                    {
                        CastSpell(SPELL_MAGE_HEATING_UP, this); // Cast Heating Up!
                    }
                }
            }
        }
        else
        {
            if (!isDot && HasAura(21)) // Assuming 21 is SPELL_MAGE_HEATING_UP
            {
                RemoveAura(21);
            }
        }*/

        float tmpDamage = doneTotal * doneTotalMod;
        /* TODO: if (spell.m_customPCT != null)
        {
            return spell.m_customPCT.Value; // Assuming m_customPCT is nullable
        }*/

        return tmpDamage;
    }


}
