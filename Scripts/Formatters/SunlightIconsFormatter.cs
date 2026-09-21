using System;
using System.Linq;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Formatters
{
    [RegisterSmartFormatter]
    public class SunlightIconsFormatter : IFormatter
    {
        public string Name 
        { 
            get => "sunlightIcons"; 
            set => throw new NotImplementedException(); 
        }
        
        public bool CanAutoDetect { get; set; }

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            try
            {
                int count = 1;
                object? val = formattingInfo.CurrentValue;

                // 1. 如果有括号参数，优先使用 (例如 {:sunlightIcons(3)})
                if (!string.IsNullOrEmpty(formattingInfo.FormatterOptions))
                {
                    if (int.TryParse(formattingInfo.FormatterOptions, out int parsed)) count = parsed;
                }
                // 2. 否则，解析传进来的变量 (例如 {SunAmount:sunlightIcons})
                // 2. 否则，解析传进来的变量 (例如 {SunAmount:sunlightIcons})
                else if (val != null)
                {
                    // 👇 终极绝杀：优先识别我们自己写的 SunlightCostVar！
                    if (val is SunlightCostVar sunVar)
                    {
                        // 直接呼叫实时方法，如果这卡被药水砸中，这里会瞬间得到 0！
                        count = sunVar.GetRealTimeDisplayCost();
                    }
                    else if (val is DynamicVar dynVar) 
                    {
                        count = Convert.ToInt32(dynVar.PreviewValue);
                    }
                    else 
                    {
                        string rawStr = val.ToString() ?? "1";
                        string cleanStr = Regex.Replace(rawStr, "<.*?>", "");
                        if (int.TryParse(cleanStr, out int strInt)) count = strInt;
                    }
                }

                string iconTag = "[img width=16 height=16]res://mods/KazamiYuka/images/sunlight_icon.png[/img]";
                string result = string.Concat(Enumerable.Repeat(iconTag, count));

                formattingInfo.Write(result);
                // 必须返回 true，告诉引擎“我处理成功了”
                return true;
            }
            catch (Exception ex)
            {
                formattingInfo.Write($"[FORMAT_ERROR:{ex.Message}]");
                return true; 
            }
        }
    }
}
