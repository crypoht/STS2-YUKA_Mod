using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot; 
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
namespace KazamiYuka.Scripts;

[RegisterMonster]
public class KazamiYukaSunflower : ModMonsterTemplate
{
    public override int MinInitialHp => 10;
    public override int MaxInitialHp => 10;
    public override bool IsInteractable => Creature.IsAlive;

    public override MonsterAssetProfile AssetProfile => new(
        $"res://mods/KazamiYuka/scenes/{GetType().Name}.tscn"
    );

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState moveState = new MoveState("NOTHING_MOVE", (_) => Task.CompletedTask, Array.Empty<AbstractIntent>());
        moveState.FollowUpState = moveState;
        return new MonsterMoveStateMachine(new[] { moveState }, moveState);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("idle", true);
        AnimState hit = new AnimState("hit", false);
        AnimState attack = new AnimState("attack", false);
        AnimState die = new AnimState("die", false);

        idle.AddBranch("Hit", hit, null);
        hit.NextState = idle;
        attack.NextState = idle;

        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack, null);
        animator.AddAnyState("Hit", hit, null);
        animator.AddAnyState("Dead", die, null);

        return animator;
    }
}
