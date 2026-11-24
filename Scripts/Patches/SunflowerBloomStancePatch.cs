using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch]
internal static class SunflowerBloomStancePatch
{
    private const string BloomStanceId = "KOMEIJIKOISHI-BLOOM_STANCE_POWER";
    private const string ExternalAttackMethodName = "TryTriggerExternalAttackAsync";
    private static readonly AsyncLocal<bool> IsResolvingBloom = new();
    private static readonly MethodInfo? DamageMethod = AccessTools.Method(
        typeof(CreatureCmd), nameof(CreatureCmd.Damage),
        [typeof(PlayerChoiceContext), typeof(IEnumerable<DamageTargetData>), typeof(decimal),
            typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay), typeof(ModifyDamageHookType)]);

    private delegate Task<bool> ExternalBloomAttack(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        Creature attacker,
        DamageResult result,
        decimal baseDamage,
        ValueProp props,
        ModifyDamageHookType modifyDamageHookType);

    private static bool Prepare() => DamageMethod != null;

    private static MethodBase? TargetMethod() => DamageMethod;

    private static ExternalBloomAttack? FindExternalBloomAttack(PowerModel bloom)
    {
        MethodInfo? method = bloom.GetType().GetMethod(
            ExternalAttackMethodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding,
            null,
            [typeof(PlayerChoiceContext), typeof(CardPlay), typeof(Creature), typeof(DamageResult),
                typeof(decimal), typeof(ValueProp), typeof(ModifyDamageHookType)],
            null);

        return method?.ReturnType == typeof(Task<bool>)
            ? method.CreateDelegate<ExternalBloomAttack>(bloom)
            : null;
    }

    private static void Postfix(
        PlayerChoiceContext choiceContext,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay,
        ModifyDamageHookType modifyDamageHookType,
        ref Task<IEnumerable<DamageResult>> __result)
    {
        if (IsResolvingBloom.Value || dealer is not { Monster: KazamiYukaSunflower } ||
            cardSource is not { Type: CardType.Attack } || cardPlay == null || cardPlay.Card != cardSource ||
            !props.IsPoweredAttack() || cardSource.Owner?.Creature is not { } attacker ||
            cardSource.CombatState is not { } combat ||
            attacker.Powers.FirstOrDefault(power => power.Id.Entry == BloomStanceId) is not { } bloom)
        {
            return;
        }

        __result = ResolveBloom(__result, choiceContext, combat, attacker, bloom,
            dealer, cardSource, cardPlay, amount, props, modifyDamageHookType);
    }

    private static async Task<IEnumerable<DamageResult>> ResolveBloom(
        Task<IEnumerable<DamageResult>> damageTask,
        PlayerChoiceContext choiceContext,
        ICombatState combat,
        Creature attacker,
        PowerModel bloom,
        Creature sunflower,
        CardModel cardSource,
        CardPlay cardPlay,
        decimal amount,
        ValueProp props,
        ModifyDamageHookType modifyDamageHookType)
    {
        IEnumerable<DamageResult> results = await damageTask;
        ExternalBloomAttack? externalBloomAttack = FindExternalBloomAttack(bloom);
        HashSet<Creature> forwardedTargets = new(ReferenceEqualityComparer.Instance);
        IsResolvingBloom.Value = true;
        try
        {
            foreach (DamageResult hit in results)
            {
                if (!CombatManager.Instance.IsInProgress || !attacker.IsAlive || !attacker.Powers.Contains(bloom))
                {
                    break;
                }

                if (hit.TotalDamage <= 0 || hit.OriginalTarget.Side == attacker.Side ||
                    hit.Receiver.Side == attacker.Side || !forwardedTargets.Add(hit.OriginalTarget))
                {
                    continue;
                }

                if (externalBloomAttack != null)
                {
                    await externalBloomAttack(choiceContext, cardPlay, sunflower, hit, amount, props, modifyDamageHookType);
                    continue;
                }

                List<Creature> targets = combat.GetOpponentsOf(attacker).Where(enemy => enemy.IsAlive).ToList();
                if (targets.Count == 0)
                {
                    break;
                }

                Creature? target = cardSource.Owner.RunState.Rng.CombatTargets.NextItem(targets);
                if (target == null)
                {
                    break;
                }

                await using AttackContext attack = await AttackCommand.CreateContextAsync(combat, choiceContext, cardPlay);
                decimal damage = Hook.ModifyDamage(cardSource.Owner.RunState, combat, target, sunflower,
                    amount, props, cardSource, cardPlay,
                    modifyDamageHookType & (ModifyDamageHookType.Additive | ModifyDamageHookType.Multiplicative),
                    CardPreviewMode.None, out _);
                attack.AddHit(await CreatureCmd.Damage(choiceContext, new[] { target }, damage,
                    ValueProp.Unpowered, attacker, null, null, ModifyDamageHookType.Cap));
            }
        }
        finally
        {
            IsResolvingBloom.Value = false;
        }

        return results;
    }
}
