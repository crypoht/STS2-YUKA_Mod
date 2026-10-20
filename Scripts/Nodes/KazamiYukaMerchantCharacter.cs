using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace KazamiYuka.Scripts.Nodes
{
	public partial class KazamiYukaMerchantCharacter : NMerchantCharacter
	{
		public override void _Ready()
		{
			this.RunWhenSpineReady(new MegaSprite(GetChild(0)), _ => PlayAnimation("idle_loop", true));
		}
	}
}
