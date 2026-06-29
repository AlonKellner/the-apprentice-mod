using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public static class TensionHelper
{
    // Pure math — testable in isolation.
    // Order: Cadence multiplier (1=none, 2=double, 3=triple) → Fortissimo triples → Tuning/Dynamics bonus added last.
    public static int ComputeTensionGain(int baseAmount, int tuningBonus, int cadenceMultiplier, bool fortissimoTriple)
    {
        int result = baseAmount;
        if (cadenceMultiplier > 1) result *= cadenceMultiplier;
        if (fortissimoTriple) result *= 3;
        result += tuningBonus;
        return result;
    }

    public static int ComputePartialRemoval(int current, int cap) => Math.Min(current, cap);

    // Adds Tension to a creature, applying all active modifier powers.
    // DynamicsNextCardPower is consumed on first Tension gain (flat bonus, added after multipliers).
    public static async Task AddTension(PlayerChoiceContext ctx, Creature creature, int amount, CardModel? card)
    {
        int tuningBonus = (int)creature.GetPowerAmount<TuningPower>();
        int cadencePower = (int)creature.GetPowerAmount<CadencePower>();
        int cadenceMultiplier = cadencePower >= 2 ? 3 : cadencePower >= 1 ? 2 : 1;
        bool fortissimoTriple = creature.GetPowerAmount<FortissimoPower>() >= 2;

        int dynamicsBonus = (int)creature.GetPowerAmount<DynamicsNextCardPower>();
        if (dynamicsBonus > 0)
            await PowerCmd.Apply<DynamicsNextCardPower>(ctx, creature, -dynamicsBonus, creature, card, false);

        int finalAmount = ComputeTensionGain(amount, tuningBonus + dynamicsBonus, cadenceMultiplier, fortissimoTriple);
        await PowerCmd.Apply<TensionPower>(ctx, creature, finalAmount, creature, card, false);
    }

    public static int GetTension(Creature creature) => (int)creature.GetPowerAmount<TensionPower>();

    public static async Task<int> RemoveAllTension(PlayerChoiceContext ctx, Creature creature, CardModel? card)
    {
        int current = GetTension(creature);
        if (current <= 0) return 0;
        await PowerCmd.Apply<TensionPower>(ctx, creature, -current, creature, card, false);
        return current;
    }

    public static async Task<int> RemoveTension(PlayerChoiceContext ctx, Creature creature, int cap, CardModel? card)
    {
        int removed = ComputePartialRemoval(GetTension(creature), cap);
        if (removed <= 0) return 0;
        await PowerCmd.Apply<TensionPower>(ctx, creature, -removed, creature, card, false);
        return removed;
    }
}
