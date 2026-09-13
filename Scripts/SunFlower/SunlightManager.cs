using System;
using MegaCrit.Sts2.Core.Entities.Players;

namespace KazamiYuka.Scripts;

public static class SunlightManager
{
    // 当前拥有的阳光数量
    public static int CurrentSunlight { get; private set; } = 0;

    // 广播频道：当阳光数量改变时，通知 UI 去更新画面 (参数：旧值, 新值)
    public static event Action<int, int>? SunlightChanged;
    public static event Action<Player, int>? SunlightSpent;

    // 在每场战斗开始时清零
    public static void ResetForCombat()
    {
        int old = CurrentSunlight;
        CurrentSunlight = 0;
        _playerSunlight.Clear(); // 👇 必须加上这句，把玩家专属池也彻底倒空！
        SunlightChanged?.Invoke(old, CurrentSunlight);
    }

    // 获得阳光的方法

    private static Dictionary<Player, int> _playerSunlight = new Dictionary<Player, int>();
    public static void GainSunlight(Player player, int amount)
    {

        if (player == null) return;
        
        if (!_playerSunlight.ContainsKey(player))
            _playerSunlight[player] = 0;

        _playerSunlight[player] += amount;

        if (amount <= 0) return;
        int old = CurrentSunlight;
        CurrentSunlight += amount;
        SunlightChanged?.Invoke(old, CurrentSunlight);
    }

    public static bool HasEnoughSunlight(Player player, int amount)
    {
        if (player == null) return false;

        // 如果玩家连记录都没有，且需要的阳光大于0，肯定不够
        if (!_playerSunlight.ContainsKey(player))
        {
            return amount <= 0; 
        }

        return _playerSunlight[player] >= amount;
    }

    // 修改：要求必须传入是哪个 Player 在花阳光
    public static bool TrySpendSunlight(Player player, int amount)
    {
        if (player == null) return false;

        // 确保字典里有这个玩家，防止报错
        if (!_playerSunlight.ContainsKey(player))
        {
            _playerSunlight[player] = 0;
        }

        // 检查该名玩家的专属阳光是否足够
        if (_playerSunlight[player] >= amount)
        {
            int old = CurrentSunlight; // 保留为了给UI发送广播
            
            // 完美扣除：同时扣除玩家专属池和全局池
            _playerSunlight[player] -= amount;
            CurrentSunlight -= amount;
            
            SunlightChanged?.Invoke(old, CurrentSunlight);
            SunlightSpent?.Invoke(player, amount);
            return true;
        }
        
        return false; // 阳光不够，施法失败
    }
}
