using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class IntenseModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Intense";

    // How many times Intense has been applied to this card (its "level").
    public int Stacks { get; private set; }

    // True once the card has been played; adds Unplayable keyword like Expend.
    public bool IsSpent { get; private set; }

    // ── Combat-scoped counter ────────────────────────────────────────────────────────────────
    // Counts distinct cards that have received at least one Intense application this combat.
    // Applying Intense 3× to the same card still counts as 1 distinct card.
    // Bonus per card = Stacks × _intenseCreated (read at damage/block calculation time).
    private static ICombatState? _lastCombat;
    private static int _intenseCreated;

    private static void MaybeResetForCombat(ICombatState combat)
    {
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _intenseCreated = 0;
        }
    }

    // A card can receive Intense only if it has at least one stat that the Intense bonus
    // can actually modify via the powered hook — i.e. a DamageVar or BlockVar with
    // ValueProp.Move (the flag set by WithDamage/WithBlock, which satisfies IsPoweredAttack
    // and IsPoweredCardOrMonsterMoveBlock). Cards with no Damage/Block var (e.g. Performance,
    // Intention) or with unpowered props are excluded.
    public static bool CanApplyTo(CardModel card)
    {
        if (card.Keywords.Contains(UnderstudyKeywords.Stable)) return false;
        bool hasPoweredDamage = card.DynamicVars.TryGetValue("Damage", out var dmgVar)
            && ((DamageVar)dmgVar).Props.IsPoweredAttack();
        bool hasPoweredBlock = card.DynamicVars.TryGetValue("Block", out var blkVar)
            && ((BlockVar)blkVar).Props.IsPoweredCardOrMonsterMoveBlock();
        return hasPoweredDamage || hasPoweredBlock;
    }

    public static void Apply(CardModel card, ICombatState combat, IEnumerable<CardModel> allCards)
    {
        MaybeResetForCombat(combat);

        if (!card.TryGetModifier<IntenseModifier>(out var mod))
        {
            // First application to this card: count it as a new Intense card.
            CardModifier.AddModifier<IntenseModifier>(card);
            card.TryGetModifier<IntenseModifier>(out mod);
            _intenseCreated++;
        }

        mod!.Stacks++;
        // No explicit BaseValue update needed — ModifyDamageAdditive / ModifyBlockAdditive
        // inject the bonus at calculation time (same mechanism as Strength / Dexterity).
    }

    // ── Strength/Dexterity-style hook overrides ──────────────────────────────────────────────
    // These are called by Hook.ModifyDamage / Hook.ModifyBlock at card preview and play time,
    // causing the card to display and deal/grant the bonus automatically, with green/red coloring
    // when the final value differs from the base value (same as how Strength renders on cards).

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != Owner) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        return Stacks * _intenseCreated;
    }

    public override decimal ModifyBlockAdditive(
        Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != Owner) return 0m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 0m;
        return Stacks * _intenseCreated;
    }

    // ── Instance overrides ───────────────────────────────────────────────────────────────────

    // UnderstudyKeywords.Intense is NOT added here — that would create a second un-numbered
    // "Intense" badge alongside ModifyDescription's "Intense N." text.
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return false;
    }

    // Prepended BEFORE the card description (e.g. "Intense 2.\nDeal N damage.")
    // so the stack count appears above the main card text, matching the user's
    // "Intense should be before description" requirement.
    public override void ModifyDescription(Creature? creature, ref string description)
    {
        if (Stacks <= 0) return;
        description = $"[gold]Intense {Stacks}[/gold].\n" + description;
    }

    public override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Card == Owner)
        {
            IsSpent = true;
            if (!Owner!.TryGetModifier<UnplayableModifier>(out _))
                CardModifier.AddModifier<UnplayableModifier>(Owner!);
        }
        await Task.CompletedTask;
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.IntProperties["spent"] = IsSpent ? 1 : 0;
        save.IntProperties["stacks"] = Stacks;
    }

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("spent", out int v)) IsSpent = v != 0;
        if (save.IntProperties.TryGetValue("stacks", out int s)) Stacks = s;
    }
}
