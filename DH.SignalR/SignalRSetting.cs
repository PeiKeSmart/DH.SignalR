﻿using System.ComponentModel;

using NewLife.Configuration;

using Pek.Configs;

namespace DH.SignalR;

/// <summary>SignalR配置</summary>
[DisplayName("SignalR配置")]
[Config("SignalR")]
public class SignalRSetting : Config<SignalRSetting>
{
    /// <summary>是否启用SignalR实时通讯</summary>
    [Description("是否启用SignalR实时通讯")]
    public Boolean IsAllowSignalR { get; set; } = true;

    /// <summary>
    /// SignalR服务端地址
    /// </summary>
    [Description("SignalR服务端地址")]
    public String SignalRAddress { get; set; } = "http://localhost:9100/notify-hub";

    /// <summary>SignalR用户缓存前缀</summary>
    [Description("SignalR用户缓存前缀")]
    public String SignalRPrefixUser { get; set; } = "signalr_u_";

    /// <summary>SignalR组缓存前缀</summary>
    [Description("SignalR组缓存前缀")]
    public String SignalRPrefixGroup { get; set; } = "signalr_g_";

    #region 方法

    /// <summary>实例化</summary>
    public SignalRSetting() { }

    /// <summary>加载时触发</summary>
    protected override void OnLoaded()
    {
        SignalRPrefixUser = $"{RedisSetting.Current.CacheKeyPrefix}signalr_u_";
        SignalRPrefixUser = $"{RedisSetting.Current.CacheKeyPrefix}signalr_g_";

        base.OnLoaded();
    }

    #endregion

}
