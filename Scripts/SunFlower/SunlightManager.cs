using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using KazamiYuka.Scripts.Relics;

namespace KazamiYuka.Scripts;

public static class SunlightManager
{
	[Obsolete("Use GetSunlight(player) for gameplay. CurrentSunlight kept for compatibility only.")]
	public static int CurrentSunlight { get; private set; } = 0;

	[Obsolete("Use SunlightChangedForPlayer. Global event kept for compatibility only.")]
	public static event Action<int, int>? SunlightChanged;
	public static event Action<Player, int, int>? SunlightChangedForPlayer;
	public static event Action<Player, int>? SunlightGained;
	public static event Action<Player, int>? SunlightSpent;

	public static bool IsYukaPlayer(Player player)
	{
		if (player?.Relics == null) return false;
		foreach (var relic in player.Relics)
		{
			if (relic is KazamiYukaStarterRelic or OldSleepingCap_Yuka)
				return true;
		}
		return false;
	}

	public static int GetFierceSunlight(Player player)
	{
		if (player == null) return 0;
		int actualSunlight = GetSunlight(player);
		if (actualSunlight > 0) return actualSunlight;
		return player.Character is KazamiYukaCharacter ? 0 : 3;
	}

	public static void ResetForCombat()
	{
		Dictionary<Player, int> retainedSunlight = _playerSunlight
			.Where(pair => pair.Key.Relics?.Any(relic => relic is YoukaiUmbrella_Yuka) == true)
			.ToDictionary(pair => pair.Key, pair => pair.Value);
		_playerSunlight.Clear();
		foreach (var pair in retainedSunlight)
		{
			_playerSunlight[pair.Key] = pair.Value;
		}
		_sunlightGainedThisTurn.Clear();
#pragma warning disable CS0618
		int old = CurrentSunlight;
		CurrentSunlight = 0;
		SunlightChanged?.Invoke(old, CurrentSunlight);
#pragma warning restore CS0618
	}

	private static Dictionary<Player, int> _playerSunlight = new Dictionary<Player, int>();
	private static readonly Dictionary<Player, int> _sunlightGainedThisTurn = new();

	public static void ResetSunlightGainedThisTurn(Player player)
	{
		if (player != null)
		{
			_sunlightGainedThisTurn[player] = 0;
		}
	}

	public static int GetSunlightGainedThisTurn(Player player)
	{
		return player != null && _sunlightGainedThisTurn.TryGetValue(player, out int amount)
			? amount
			: 0;
	}

	public static void GainSunlight(Player player, int amount)
	{
		if (player == null) return;
		
		int oldPersonal = GetSunlight(player);
		SetSunlight(player, oldPersonal + amount);

		if (amount <= 0) return;

		if (!_sunlightGainedThisTurn.ContainsKey(player))
			_sunlightGainedThisTurn[player] = 0;
		_sunlightGainedThisTurn[player] += amount;
#pragma warning disable CS0618
		int old = CurrentSunlight;
		CurrentSunlight += amount;
		SunlightChanged?.Invoke(old, CurrentSunlight);
#pragma warning restore CS0618
		SunlightChangedForPlayer?.Invoke(player, oldPersonal, _playerSunlight[player]);
		SunlightGained?.Invoke(player, amount);
	}

	public static bool HasEnoughSunlight(Player player, int amount)
	{
		if (player == null) return false;

		return GetSunlight(player) >= amount;
	}

	public static int GetSunlight(Player player)
	{
		if (player == null)
		{
			return 0;
		}

		if (_playerSunlight.TryGetValue(player, out int amount))
		{
			return amount;
		}

		YoukaiUmbrella_Yuka? umbrella = player.Relics.OfType<YoukaiUmbrella_Yuka>().FirstOrDefault();
		if (umbrella == null)
		{
			return 0;
		}

		_playerSunlight[player] = umbrella.StoredSunlight;
		return umbrella.StoredSunlight;
	}

	private static void SetSunlight(Player player, int amount)
	{
		_playerSunlight[player] = amount;
		foreach (YoukaiUmbrella_Yuka umbrella in player.Relics.OfType<YoukaiUmbrella_Yuka>())
		{
			umbrella.StoredSunlight = amount;
		}
	}

	public static int SpendAllSunlight(Player player)
	{
		int amount = GetSunlight(player);
		if (amount <= 0 || !TrySpendSunlight(player, amount))
		{
			return 0;
		}

		return amount;
	}

	public static bool TrySpendSunlight(Player player, int amount)
	{
		if (player == null) return false;

		int oldPersonal = GetSunlight(player);
		if (oldPersonal >= amount)
		{
			SetSunlight(player, oldPersonal - amount);
#pragma warning disable CS0618
			int old = CurrentSunlight;
			CurrentSunlight -= amount;
			SunlightChanged?.Invoke(old, CurrentSunlight);
#pragma warning restore CS0618
			SunlightChangedForPlayer?.Invoke(player, oldPersonal, _playerSunlight[player]);
			SunlightSpent?.Invoke(player, amount);
			return true;
		}
		
		return false;
	}
}
