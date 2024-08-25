using UnityEngine;

public abstract class SpellScript : MonoBehaviour
{
    // Called when the spell is cast
    public virtual void OnCast(Spell spell, Unit caster, Unit target)
    {
        // Default behavior (can be empty if there's no default)
    }

    // Called when the spell hits the target
    public virtual void OnHit(Spell spell, Unit caster, Unit target)
    {
        // Default behavior
    }

    // Called when the spell is interrupted
    public virtual void OnInterrupt(Spell spell, Unit caster, Unit target)
    {
        // Default behavior
    }

    // Called when the spell is finished (whether successful or not)
    public virtual void OnFinish(Spell spell, Unit caster, Unit target)
    {
        // Default behavior
    }

    // Called early on to change based on situations, rather than hard-coding it
    public virtual void Modify(Spell spell, Unit caster, Unit target)
    {
        // Default behaviour
    }
}
