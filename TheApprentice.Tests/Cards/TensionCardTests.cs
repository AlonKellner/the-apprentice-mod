using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class TensionCardTests
{
    // CardId constants are static — safe to verify without instantiation restrictions.

    [Fact] public void Staccato_CardId()        => Assert.Equal("TheApprentice:Staccato",        Staccato.CardId);
    [Fact] public void Brass_CardId()           => Assert.Equal("TheApprentice:Brass",           Brass.CardId);
    [Fact] public void Tutti_CardId()           => Assert.Equal("TheApprentice:Tutti",           Tutti.CardId);
    [Fact] public void Motif_CardId()           => Assert.Equal("TheApprentice:Motif",           Motif.CardId);
    [Fact] public void Strings_CardId()         => Assert.Equal("TheApprentice:Strings",         Strings.CardId);
    [Fact] public void Tremolo_CardId()         => Assert.Equal("TheApprentice:Tremolo",         Tremolo.CardId);
    [Fact] public void Score_CardId()           => Assert.Equal("TheApprentice:Score",           Score.CardId);
    [Fact] public void Coda_CardId()            => Assert.Equal("TheApprentice:Coda",            Coda.CardId);
    [Fact] public void Reprise_CardId()         => Assert.Equal("TheApprentice:Reprise",         Reprise.CardId);
    [Fact] public void Vibrato_CardId()         => Assert.Equal("TheApprentice:Vibrato",         Vibrato.CardId);
    [Fact] public void Marcato_CardId()         => Assert.Equal("TheApprentice:Marcato",         Marcato.CardId);
    [Fact] public void Buildup_CardId()         => Assert.Equal("TheApprentice:Buildup",         Buildup.CardId);
    [Fact] public void DeceptiveCadence_CardId()=> Assert.Equal("TheApprentice:DeceptiveCadence",DeceptiveCadence.CardId);
    [Fact] public void Dynamics_CardId()        => Assert.Equal("TheApprentice:Dynamics",        Dynamics.CardId);
    [Fact] public void Refrain_CardId()         => Assert.Equal("TheApprentice:Refrain",         Refrain.CardId);
    [Fact] public void Attunement_CardId()      => Assert.Equal("TheApprentice:Attunement",      Attunement.CardId);
    [Fact] public void Suspension_CardId()      => Assert.Equal("TheApprentice:Suspension",      Suspension.CardId);
    [Fact] public void Tuning_CardId()          => Assert.Equal("TheApprentice:Tuning",          Tuning.CardId);
    [Fact] public void Climax_CardId()          => Assert.Equal("TheApprentice:Climax",          Climax.CardId);
    [Fact] public void Triumph_CardId()         => Assert.Equal("TheApprentice:Triumph",         Triumph.CardId);
    [Fact] public void Tragedy_CardId()         => Assert.Equal("TheApprentice:Tragedy",         Tragedy.CardId);
    [Fact] public void Fortissimo_CardId()      => Assert.Equal("TheApprentice:Fortissimo",      Fortissimo.CardId);
    [Fact] public void Cadence_CardId()         => Assert.Equal("TheApprentice:Cadence",         Cadence.CardId);
}
