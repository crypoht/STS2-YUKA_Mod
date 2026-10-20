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
		return IsYukaPlayer(player) ? 0 : 3;
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
		
		if (!_playerSunlight.ContainsKey(player))
			_playerSunlight[player] = 0;

		int oldPersonal = _playerSunlight[player];
		_playerSunlight[player] += amount;

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

		if (!_playerSunlight.ContainsKey(player))
		{
			return amount <= 0; 
		}

		return _playerSunlight[player] >= amount;
	}

	public static int GetSunlight(Player player)
	{
		if (player == null)
		{
			return 0;
		}

		return _playerSunlight.TryGetValue(player, out int amount) ? amount : 0;
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

		if (!_playerSunlight.ContainsKey(player))
		{
			_playerSunlight[player] = 0;
		}

		if (_playerSunlight[player] >= amount)
		{
			int oldPersonal = _playerSunlight[player];
			_playerSunlight[player] -= amount;
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
