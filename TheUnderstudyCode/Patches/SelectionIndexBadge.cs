using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Draws a small numbered badge ("#1", "#2", ...) on selected cards during a Planned selection so the
// player can see the order they picked. Driven by the two selection patches, which call Render() with
// (card, number) pairs on every change and ClearAll() when the screen closes. The `number` is the real
// visual Planned index the card will end up with (existing plan slots + selection position), computed
// by the patches — not just the 1..N position within this one selection.
//
// Attach mechanism mirrors OrderOverlayPatch: a Control added into NCard.OverlayContainer, sized/placed
// off the sibling "Frame" node (OverlayContainer and its ancestors report Size=(0,0); every visible
// card element is instead centered on CardContainer's origin via Position = Frame.Position). The number
// is a plain Label whose font we set to the base game's Kreon display face (card name/number type) and whose
// color we set gold — a code-created MegaRichTextLabel can't be used here: its _Ready() throws unless a
// theme font override was baked in by a scene (a Godot-engine-bug workaround). The chip background is a
// code-built StyleBoxFlat — no textures/scenes, so no PCK load-order risk.
public static class SelectionIndexBadge
{
    // Base-game Kreon Bold — the display typeface used for card names and cost/number text, so the
    // index badge matches the game's UI numbers rather than the serif card-body font.
    private const string KreonBoldPath = "res://fonts/kreon_bold.ttf";

    private const float BadgeSize = 54f;
    private const float Inset = 10f;
    private const int FontSize = 30;
    private static readonly Vector2 FallbackCardSize = new(300, 422);

    // Gold to match Planned's [gold] description text; dark translucent fill for contrast.
    private static readonly Color Gold = new(1f, 0.784f, 0f);
    private static readonly Color DarkFill = new(0.07f, 0.07f, 0.10f, 0.92f);

    // Per-card attached badge (ConditionalWeakTable so a freed NCard doesn't leak). Tracked is the
    // strong ordered list of cards currently badged, used to diff against each Render call.
    private static readonly ConditionalWeakTable<NCard, Control> Badges = new();
    private static readonly List<NCard> Tracked = new();

    // Renders a "#number" badge over each (card, number) pair and removes badges from any card no longer
    // present. Safe to call every time the selection changes.
    public static void Render(IReadOnlyList<(NCard card, int number)> items)
    {
        for (int i = Tracked.Count - 1; i >= 0; i--)
        {
            if (!items.Any(it => it.card == Tracked[i]))
            {
                RemoveFrom(Tracked[i]);
                Tracked.RemoveAt(i);
            }
        }

        foreach (var (card, number) in items)
        {
            if (card == null) continue;
            SetNumber(GetOrCreate(card), number);
            if (!Tracked.Contains(card)) Tracked.Add(card);
        }
    }

    // Removes every badge. Called when a selection screen completes/closes so nothing lingers on cards
    // returning to hand or on freed grid nodes.
    public static void ClearAll()
    {
        foreach (var card in Tracked) RemoveFrom(card);
        Tracked.Clear();
    }

    private static void RemoveFrom(NCard card)
    {
        if (!Badges.TryGetValue(card, out var badge)) return;
        if (GodotObject.IsInstanceValid(badge))
        {
            badge.GetParent()?.RemoveChild(badge);
            badge.QueueFree();
        }
        Badges.Remove(card);
    }

    private static Control GetOrCreate(NCard card)
    {
        if (Badges.TryGetValue(card, out var existing))
        {
            if (GodotObject.IsInstanceValid(existing)) return existing;
            Badges.Remove(card);
        }

        var frame = card.OverlayContainer.GetParent()?.GetNodeOrNull<Control>("Frame");
        var framePos = frame?.Position ?? -FallbackCardSize / 2f;
        var frameSize = frame?.Size ?? FallbackCardSize;
        // Top-right corner — top-left already holds the energy/star cost icons.
        var position = new Vector2(framePos.X + frameSize.X - BadgeSize - Inset, framePos.Y + Inset);

        var style = new StyleBoxFlat
        {
            BgColor = DarkFill,
            BorderColor = Gold,
            BorderWidthTop = 3,
            BorderWidthBottom = 3,
            BorderWidthLeft = 3,
            BorderWidthRight = 3,
            CornerRadiusTopLeft = (int)(BadgeSize / 2f),
            CornerRadiusTopRight = (int)(BadgeSize / 2f),
            CornerRadiusBottomLeft = (int)(BadgeSize / 2f),
            CornerRadiusBottomRight = (int)(BadgeSize / 2f),
        };

        var panel = new Panel
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new Vector2(BadgeSize, BadgeSize),
            Position = position,
            ZIndex = 100,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var label = new Label
        {
            Name = "Number",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new Vector2(BadgeSize, BadgeSize),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        // Use Kreon — the base game's display typeface for card names and numbers — so the "#N"
        // index reads as a game UI number, not card body text. (The card description font pulled from
        // the theme is a serif body face, which is what previously made this look wrong.) Fall back to
        // that description font if the base-game Kreon resource can't be loaded for any reason.
        Font? numberFont = ResourceLoader.Exists(KreonBoldPath)
            ? GD.Load<Font>(KreonBoldPath)
            : card.GetThemeFont(ThemeConstants.RichTextLabel.NormalFont, "RichTextLabel");
        if (numberFont != null) label.AddThemeFontOverride("font", numberFont);
        label.AddThemeColorOverride("font_color", Gold);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0));
        label.AddThemeConstantOverride("outline_size", 6);
        label.AddThemeFontSizeOverride("font_size", FontSize);
        panel.AddChild(label);

        card.OverlayContainer.AddChild(panel);
        Badges.AddOrUpdate(card, panel);
        return panel;
    }

    private static void SetNumber(Control badge, int number)
    {
        if (badge.GetNodeOrNull<Label>("Number") is { } label)
            label.Text = $"#{number}";
    }
}
