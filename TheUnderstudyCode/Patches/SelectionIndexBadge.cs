using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Draws a small numbered badge ("1", "2", "3" ...) on selected cards during a Planned selection so the
// player can see the order they picked. Driven by the two selection patches, which call Render() with
// the currently-selected NCards in click order on every change and ClearAll() when the screen closes.
//
// Attach mechanism mirrors OrderOverlayPatch: a Control added into NCard.OverlayContainer, sized/placed
// off the sibling "Frame" node (OverlayContainer and its ancestors report Size=(0,0); every visible
// card element is instead centered on CardContainer's origin via Position = Frame.Position). Everything
// here is built in code (StyleBoxFlat + Label) — no textures/scenes, so no PCK load-order risk and no
// GPU shader pipeline to warm up.
public static class SelectionIndexBadge
{
    private const float BadgeSize = 54f;
    private const float Inset = 10f;
    private static readonly Vector2 FallbackCardSize = new(300, 422);

    // Gold to match Planned's [gold] description text; dark translucent fill for contrast.
    private static readonly Color GoldBorder = new(1f, 0.784f, 0f);
    private static readonly Color DarkFill = new(0.07f, 0.07f, 0.10f, 0.92f);

    // Per-card attached badge (ConditionalWeakTable so a freed NCard doesn't leak). Tracked is the
    // strong ordered list of cards currently badged, used to diff against each Render call.
    private static readonly ConditionalWeakTable<NCard, Control> Badges = new();
    private static readonly List<NCard> Tracked = new();

    // Renders badges numbered 1..N over `orderedCards` (in order) and removes badges from any card no
    // longer present. Safe to call every time the selection changes.
    public static void Render(IReadOnlyList<NCard> orderedCards)
    {
        for (int i = Tracked.Count - 1; i >= 0; i--)
        {
            if (!orderedCards.Contains(Tracked[i]))
            {
                RemoveFrom(Tracked[i]);
                Tracked.RemoveAt(i);
            }
        }

        for (int i = 0; i < orderedCards.Count; i++)
        {
            var card = orderedCards[i];
            if (card == null) continue;
            SetNumber(GetOrCreate(card), i + 1);
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
            BorderColor = GoldBorder,
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
        label.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0));
        label.AddThemeConstantOverride("outline_size", 6);
        label.AddThemeFontSizeOverride("font_size", 32);
        panel.AddChild(label);

        card.OverlayContainer.AddChild(panel);
        Badges.AddOrUpdate(card, panel);
        return panel;
    }

    private static void SetNumber(Control badge, int number)
    {
        if (badge.GetNodeOrNull<Label>("Number") is { } label)
            label.Text = number.ToString();
    }
}
